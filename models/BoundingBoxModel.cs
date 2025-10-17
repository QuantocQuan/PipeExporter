using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExporterPipe.models
{
    internal class BoundingBoxModel
    {
        public XYZModel Min { get; set; }
        public XYZModel Max { get; set; }

        public BoundingBoxModel() { }

        public BoundingBoxModel(BoundingBoxXYZ box)
        {
            if (box != null)
            {
                Min = new XYZModel(box.Min);
                Max = new XYZModel(box.Max);
            }
        }
    }
}
