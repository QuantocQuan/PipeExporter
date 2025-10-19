using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using ExporterPipe.models;
using ExporterPipe.viewmodels;
using ExporterPipe.views;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ExporterPipe.actions
{
    internal class PipeExporterAction
    {
        private UIDocument _uiDocument;
        private Document _document;
        private readonly PipeExporterView _pipeExporterView;
        private readonly PipeExporterViewModel _pipeExporterViewModel;

        public PipeExporterAction(UIDocument uiDocument)
        {
            _uiDocument = uiDocument;
            _document = _uiDocument.Document;
            _pipeExporterView = new PipeExporterView();
            _pipeExporterViewModel = new PipeExporterViewModel();

            _pipeExporterView.DataContext = _pipeExporterViewModel;

            _pipeExporterViewModel.ExportPipes = new RelayCommand(ExportPipes);
            _pipeExporterViewModel.ImportPipes = new RelayCommand(ImportPipes);
        }

        public void Excute()
        {
            _pipeExporterView.ShowDialog();
        }

        private void ImportPipes()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                Title = "Chọn file JSON cần import"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                ImportPipesHelper(filePath);
                MessageBox.Show($"Đã đọc dữ liệu từ:\n{filePath}\n",
                    "Import thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ImportPipesHelper(String inputPath)
        {
            List<ElementInforModel> elementInfos = JsonConvert.DeserializeObject<List<ElementInforModel>>(File.ReadAllText(inputPath));
            // Lấy danh sách tất cả Level hiện có trong document
            var existingLevels = new FilteredElementCollector(_document)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToDictionary(l => l.Name, l => l);

            // Lấy danh sách Level duy nhất (theo tên + cao độ)
            var levelInfos = elementInfos
                .Where(e => e.Level != null)
                .Select(e => e.Level)
                .GroupBy(l => l.LevelName) // tránh trùng tên
                .Select(g => g.First())    // lấy 1 cái đại diện
                .ToList();

            using (Transaction t = new Transaction(_document, "Create Levels"))
            {
                t.Start();

                foreach (var lvl in levelInfos)
                {
                    if (!existingLevels.ContainsKey(lvl.LevelName))
                    {
                        // Tạo Level với cao độ thật từ dữ liệu
                        Level newLevel = Level.Create(_document, lvl.Elevation);
                        newLevel.Name = lvl.LevelName;

                        existingLevels.Add(lvl.LevelName, newLevel);
                    }
                }

                t.Commit();
            }
            using (Transaction trans = new Transaction(_document, "Import Pipes"))
            {
                trans.Start();

                foreach (var elem in elementInfos)
                {
                    try
                    {
                        //Lấy Level
                        Level level = new FilteredElementCollector(_document)
                            .OfClass(typeof(Level))
                            .FirstOrDefault(x => x.Name == elem.Level.LevelName) as Level;

                        if (level == null)
                        {
                            //TaskDialog.Show("Warning", $"Không tìm thấy Level: {elem.LevelName}. Bỏ qua phần tử {elem.ElementId}");
                            continue;
                        }


                        if (elem.Category.Equals("Pipes"))
                        {
                            XYZ start = new XYZ(elem.StartPoint.X, elem.StartPoint.Y, elem.StartPoint.Z);
                            XYZ end = new XYZ(elem.EndPoint.X, elem.EndPoint.Y, elem.EndPoint.Z);

                            PipeType pipeType = new FilteredElementCollector(_document)
                           .OfClass(typeof(PipeType))
                           .Cast<PipeType>()
                           .FirstOrDefault(x => x.Name == elem.TypeName);
                            if (pipeType == null)
                            {
                                TaskDialog.Show("Warning", $"Không tìm thấy Pipe Type: {elem.TypeName}. Bỏ qua.");
                                continue;
                            }


                            MEPSystemType sysType = new FilteredElementCollector(_document)
                            .OfClass(typeof(MEPSystemType))
                            .Cast<MEPSystemType>()
                            .FirstOrDefault(x => x.Name == elem.SystemType);

                            if (sysType == null)
                            {
                                sysType = new FilteredElementCollector(_document)
                                    .OfClass(typeof(MEPSystemType))
                                    .Cast<MEPSystemType>()
                                    .FirstOrDefault();

                            }
                            Pipe newPipe = Pipe.Create(_document, sysType.Id, pipeType.Id, level.Id, start, end);
                            newPipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(elem.Diameter);
                        }

                    }
                    catch (Exception ex)
                    {
                        //TaskDialog.Show("Error", $"Lỗi tại phần tử {elem.ElementId}: {ex.Message}");
                    }
                }

                trans.Commit();
            }
        }

        private void ExportPipes()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                Title = "Chọn nơi lưu file JSON",
                FileName = "pipes.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                string jsonData = ExportPipesHelper();
                File.WriteAllText(filePath, jsonData);

                MessageBox.Show($"Đã lưu file JSON tại:\n{filePath}",
                    "Export thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private String ExportPipesHelper()
        {
            String result = null;
      
            // Thu thập tất cả Pipe và Fitting
            var pipes = new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_PipeCurves)
                .WhereElementIsNotElementType()
                .ToList();

            var fittings = new FilteredElementCollector(_document)
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
                    ElementType type = _document.GetElement(element.GetTypeId()) as ElementType;
                    elemInfo.FamilyName = type?.FamilyName ?? "";
                    elemInfo.TypeName = type?.Name ?? element.Name;

                    // Level
                    Level level = _document.GetElement(element.LevelId) as Level;
                    double elevation = level?.Elevation ?? 0;
                    elemInfo.Level = new LevelModel
                    {
                        LevelName = level?.Name
                        ?? element.LookupParameter("Reference Level")?.AsValueString()
                        ?? element.LookupParameter("Level")?.AsValueString()
                        ?? "",
                        Elevation = elevation
                    };


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
                    else if (element.Location is LocationPoint locPoint)
                    {
                        elemInfo.Location = new XYZModel(locPoint.Point);
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

            result = JsonConvert.SerializeObject(elementInfos, Formatting.Indented);

            return result;
        }
    }
}
