using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExporterPipe.models
{
    internal class XYZModel
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public XYZModel() { }

        public XYZModel(XYZ point)
        {
            if (point != null)
            {
                X = point.X;
                Y = point.Y;
                Z = point.Z;
            }
        }
    }
}
