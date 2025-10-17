using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ExporterPipe.models;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ExporterPipe.viewmodels
{
    class AssignSpecViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<SpecModel> SpecList { get; set; }
        public ObservableCollection<ItemSource> DataAssigned { get; set; }

        public SpecModel SelectedSpec { get; set; }
        public ICommand AssignCommand { get; set; }
        public ICommand ToggleElementVisibilityCommand { get; set; }

        public ICommand ToggleElementIsolateCommand { get; set; }



        public AssignSpecViewModel() { }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "") =>
       PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
