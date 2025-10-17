using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExporterPipe.models
{
    internal class SystemPipeModel
    {
        public List<Level> Levels { get; set; }
        public List<Element> Pipes { get; set; }
        public List<Element> Fittings { get; set; }
    }
}
