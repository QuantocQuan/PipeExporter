using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExporterPipe.models
{
    internal class ConnectorInforModel
    {
        public XYZModel Origin { get; set; }
        public List<int> ConnectedTo { get; set; } = new List<int>();
    }
}
