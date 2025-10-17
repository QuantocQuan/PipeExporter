using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using ExporterPipe.models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExporterPipe.commands
{
    [Transaction(TransactionMode.Manual)]
    internal class PipeExporterCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Thu thập tất cả Pipe và Fitting
            var pipes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_PipeCurves)
                .WhereElementIsNotElementType()
                .ToList();

            var fittings = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_PipeFitting)
                .WhereElementIsNotElementType()
                .ToList();

            List<ElementInforModel> elementInfos = new List<ElementInforModel>();

            foreach (var element in pipes.Concat(fittings))
            {
                try
                {
                    ElementInforModel elemInfo = new ElementInforModel
                    {
                        ElementId = element.Id.IntegerValue,
                        UniqueId = element.UniqueId,
                        Category = element.Category?.Name ?? ""
                    };

                    // Family & Type
                    ElementType type = doc.GetElement(element.GetTypeId()) as ElementType;
                    elemInfo.FamilyName = type?.FamilyName ?? "";
                    elemInfo.TypeName = type?.Name ?? element.Name;

                    // Level
                    Level level = doc.GetElement(element.LevelId) as Level;
                    elemInfo.LevelName = level?.Name
                        ?? element.LookupParameter("Reference Level")?.AsValueString()
                        ?? element.LookupParameter("Level")?.AsValueString()
                        ?? "";

                    // System Type
                    string systemType = element.LookupParameter("System Type")?.AsValueString();

                    if (string.IsNullOrEmpty(systemType))
                    {
                        if (element is MEPCurve m)
                        {
                            systemType = m.MEPSystem?.Name;
                        }
                        else if (element is FamilyInstance fiSys)
                        {
                            var mepModel = fiSys.MEPModel;
                            if (mepModel != null && mepModel.ConnectorManager != null)
                            {
                                ConnectorSet connectors = mepModel.ConnectorManager.Connectors;
                                if (connectors != null)
                                {
                                    foreach (Connector c in connectors)
                                    {
                                        if (c.MEPSystem != null)
                                        {
                                            systemType = c.MEPSystem.Name;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    elemInfo.SystemType = systemType ?? "";

                    // Location (Start, End, Length, Direction)
                    if (element.Location is LocationCurve locCurve)
                    {
                        elemInfo.StartPoint = new XYZModel(locCurve.Curve.GetEndPoint(0));
                        elemInfo.EndPoint = new XYZModel(locCurve.Curve.GetEndPoint(1));
                        elemInfo.Length = locCurve.Curve.Length;
                        elemInfo.Direction = (locCurve.Curve.GetEndPoint(1) - locCurve.Curve.GetEndPoint(0)).Normalize();
                    }

                    // Diameter
                    Parameter diaParam = element.LookupParameter("Diameter")
                        ?? element.LookupParameter("Nominal Diameter");
                    elemInfo.Diameter = diaParam?.AsDouble() ?? 0;

                    // Connectors (cho cả Pipe và Fitting)
                    elemInfo.Connectors = new List<ConnectorInforModel>();

                    ConnectorSet connectorSet = null;

                    // Trường hợp là MEPCurve (Pipe, Duct, Conduit,...)
                    if (element is MEPCurve mepCurve)
                    {
                        connectorSet = mepCurve.ConnectorManager?.Connectors;
                    }
                    // Trường hợp là Fitting hoặc thiết bị MEP (FamilyInstance có MEPModel)
                    else if (element is FamilyInstance fi && fi.MEPModel != null)
                    {
                        connectorSet = fi.MEPModel?.ConnectorManager?.Connectors;
                    }

                    // Kiểm tra connectorSet khác null
                    if (connectorSet != null)
                    {
                        foreach (Connector c in connectorSet)
                        {
                            if (c == null) continue;

                            List<int> connectedIds = new List<int>();
                            try
                            {
                                // AllRefs chứa tất cả connector đang kết nối với connector này
                                var allRefs = c.AllRefs;
                                if (allRefs != null)
                                {
                                    foreach (Connector refConn in allRefs)
                                    {
                                        if (refConn?.Owner != null && refConn.Owner.Id != element.Id)
                                        {
                                            connectedIds.Add(refConn.Owner.Id.IntegerValue);
                                        }
                                    }
                                }
                            }
                            catch { }

                            var conn = new ConnectorInforModel
                            {
                                Origin = new XYZModel(c.Origin),
                                ConnectedTo = connectedIds
                            };

                            elemInfo.Connectors.Add(conn);
                        }
                    }

                    // Parameters (tùy chọn, lấy tất cả hoặc chỉ vài cái)
                    //elemInfo.Parameters = element.Parameters
                    //    .Cast<Parameter>()
                    //    .Select(p => new ParameterInforModel
                    //    {
                    //        Name = p.Definition?.Name,
                    //        Value = SafeParamValue(p)
                    //    })
                    //    .ToList();

                    // BoundingBox
                    elemInfo.BoundingBox = new BoundingBoxModel(element.get_BoundingBox(null));

                    elementInfos.Add(elemInfo);
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Revit Export Error", $"Lỗi tại phần tử ID {element.Id}: {ex.Message}");
                }
            }

            // Xuất JSON
            string outputPath = @"D:\pipes_export_full.json";
            File.WriteAllText(outputPath, JsonConvert.SerializeObject(elementInfos, Formatting.Indented));

            TaskDialog.Show("Revit", $"✅ Export thành công!\nTổng cộng: {elementInfos.Count} phần tử\nĐường dẫn: {outputPath}");
            return Result.Succeeded;
        }

        private string SafeParamValue(Parameter p)
        {
            try
            {
                switch (p.StorageType)
                {
                    case StorageType.Double:
                        return p.AsValueString() ?? p.AsDouble().ToString();
                    case StorageType.Integer:
                        return p.AsInteger().ToString();
                    case StorageType.String:
                        return p.AsString() ?? "";
                    case StorageType.ElementId:
                        return p.AsElementId().IntegerValue.ToString();
                    default:
                        return "";
                }
            }
            catch { return ""; }
        }
    }
}
