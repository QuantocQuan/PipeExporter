using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
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
    internal class PipeImporterCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            string inputPath = @"D:\pipes_export_full.json";
            if (!File.Exists(inputPath))
            {
                TaskDialog.Show("Error", "File JSON không tồn tại.");
                return Result.Failed;
            }


            List<ElementInforModel> elementInfos = JsonConvert.DeserializeObject<List<ElementInforModel>>(File.ReadAllText(inputPath));
            // Lấy danh sách tất cả Level hiện có trong document
            var existingLevels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToDictionary(l => l.Name, l => l);

            // Duyệt qua tất cả Level trong JSON
            var levelNames = elementInfos.Select(e => e.LevelName).Distinct();
            using (Transaction t = new Transaction(doc, "Create Levels"))
            {
                t.Start();

                foreach (var lvlName in levelNames)
                {
                    if (!existingLevels.ContainsKey(lvlName))
                    {
                        // Giả sử tạo Level mới với chiều cao 0 (có thể chỉnh lại tùy project)
                        Level newLevel = Level.Create(doc, 0);
                        newLevel.Name = lvlName;
                        existingLevels.Add(lvlName, newLevel);
                    }
                }

                t.Commit();
            }
            using (Transaction trans = new Transaction(doc, "Import Pipes"))
            {
                trans.Start();

                foreach (var elem in elementInfos)
                {
                    try
                    {
                        // Lấy Level
                        Level level = new FilteredElementCollector(doc)
                            .OfClass(typeof(Level))
                            .FirstOrDefault(x => x.Name == elem.LevelName) as Level;

                        if (level == null)
                        {
                            TaskDialog.Show("Warning", $"Không tìm thấy Level: {elem.LevelName}. Bỏ qua phần tử {elem.ElementId}");
                            continue;
                        }

                        XYZ start = new XYZ(elem.StartPoint.X, elem.StartPoint.Y, elem.StartPoint.Z);
                        XYZ end = new XYZ(elem.EndPoint.X, elem.EndPoint.Y, elem.EndPoint.Z);

                        if (elem.Category.Contains("Pipe"))
                        {
                            // Lấy FamilySymbol Pipe
                            FamilySymbol pipeType = new FilteredElementCollector(doc)
                                .OfClass(typeof(FamilySymbol))
                                .OfCategory(BuiltInCategory.OST_PipeCurves)
                                .FirstOrDefault(x => x.Name == elem.TypeName) as FamilySymbol;

                            if (pipeType == null)
                            {
                                TaskDialog.Show("Warning", $"Không tìm thấy Pipe Type: {elem.TypeName}. Bỏ qua.");
                                continue;
                            }

                            if (!pipeType.IsActive) pipeType.Activate();

                            Pipe newPipe = Pipe.Create(doc, doc.GetDefaultElementTypeId(ElementTypeGroup.PipeType), pipeType.Id, level.Id, start, end);
                            newPipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(elem.Diameter);
                        }
                        else if (elem.Category.Contains("Fitting"))
                        {
                            // Fitting: cần kết nối với các pipe đã có
                            // Đây chỉ là ví dụ đơn giản, có thể dùng FamilyInstance nếu fitting có model
                        }
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Error", $"Lỗi tại phần tử {elem.ElementId}: {ex.Message}");
                    }
                }

                trans.Commit();
            }

            TaskDialog.Show("Revit", "✅ Import xong!");
            return Result.Succeeded;
        }
    }
}
