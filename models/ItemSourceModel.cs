using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExporterPipe.models

{
    class ItemSource
    {
        public String PipeType { get; set; }
        public String PipeSpec { get; set; }
        public String FittingSpec { get; set; }
        public bool Exclusion { get; set; }

        public ItemSource(String fittingSpec)
        {
            FittingSpec = fittingSpec;
            PipeType = "";
            PipeSpec = "";
            Exclusion = false;
        }

    }
}
