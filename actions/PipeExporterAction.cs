using Autodesk.Revit.UI;
using ExporterPipe.viewmodels;
using ExporterPipe.views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExporterPipe.actions
{
    internal class PipeExporterAction
    {
        private UIDocument _uiDocument;
        private readonly PipeExporterView _pipeExporterView;
        private readonly PipeExporterViewModel _pipeExporterViewModel;

        public PipeExporterAction(UIDocument uIDocument)
        {
            _uiDocument = uIDocument;
            _pipeExporterView = new PipeExporterView();
            _pipeExporterViewModel = new PipeExporterViewModel();
        }

        public void Excute()
        {
            _pipeExporterView.ShowDialog();
        }

    }
}
