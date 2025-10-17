using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ExporterPipe.actions;
using ExporterPipe.viewmodels;

namespace ExporterPipe.commands
{
    [Transaction(TransactionMode.Manual)]
    public class AssignSpecCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Reference r = uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            Document doc = uiDoc.Document;
            Element element = doc.GetElement(r);
            AssignSpecAction action = new AssignSpecAction(uiDoc, element);
            action.Excute();


            return Result.Succeeded;
        }
    }
}
