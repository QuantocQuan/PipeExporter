using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using ExporterPipe.actions;
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
            PipeExporterAction action = new PipeExporterAction(uiDoc);
            action.Excute();
            return Result.Succeeded;
        }
    }
}
