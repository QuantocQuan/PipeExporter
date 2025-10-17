using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExporterPipe.models
{
    internal class ElementInforModel
    {
        public int ElementId { get; set; }
        public string UniqueId { get; set; }
        public string Category { get; set; }
        public string FamilyName { get; set; }
        public string TypeName { get; set; }
        public string LevelName { get; set; }
        public string SystemType { get; set; }
        public XYZModel StartPoint { get; set; }
        public XYZModel EndPoint { get; set; }
        public double Length { get; set; }
        public double Diameter { get; set; }
        public XYZ Direction { get; set; }
        public List<ConnectorInforModel> Connectors { get; set; }
        public List<ParameterInforModel> Parameters { get; set; }
        public BoundingBoxModel BoundingBox { get; set; }
    }
}
