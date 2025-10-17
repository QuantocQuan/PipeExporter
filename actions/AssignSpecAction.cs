using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using ExporterPipe.models;
using ExporterPipe.viewmodels;
using ExporterPipe.views;
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
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ExporterPipe.actions
{
    internal class AssignSpecAction
    {
        private readonly AssignSpecView _assingSpecView;
        private readonly AssignSpecViewModel _assingSpecViewModel;

        public Element ElementPicked { get; set; }

        private UIDocument _uiDoc;
        private bool _isHide;
        private bool _isIsolate;

        public AssignSpecAction(UIDocument uiDoc, Element element)
        {
            _assingSpecView = new AssignSpecView();
            _assingSpecViewModel = new AssignSpecViewModel();

            LoadJsonData();
            _assingSpecViewModel.AssignCommand = new RelayCommand(
               AssignPecPipe
                );
            _assingSpecViewModel.ToggleElementVisibilityCommand = new RelayCommand(
                ToggleElementVisibility
                );
            _assingSpecViewModel.ToggleElementIsolateCommand = new RelayCommand(
                ToggleElementIsolate);
            ElementPicked = element;
            _uiDoc = uiDoc;
            _assingSpecViewModel.DataAssigned = new ObservableCollection<ItemSource>();
            _isHide = false;
            _isIsolate = false;
            _assingSpecView.DataContext = _assingSpecViewModel;
        }
        private void LoadJsonData()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("ExporterPipe.Resources.Speclib.json");
            var reader = new StreamReader(stream);
            string json = reader.ReadToEnd();
            var data = JObject.Parse(json); // parse JSON object
            var list = data["Pipe_Fitting_Spec"].ToObject<List<SpecModel>>(); // convert JArray sang List<SpecModel>
            _assingSpecViewModel.SpecList = new ObservableCollection<SpecModel>(list);
        }

        private void AssignPecPipe()
        {
            if (ElementPicked == null)
            {
                MessageBox.Show("Chưa chọn element");
                return;
            }
            if (_assingSpecViewModel.SelectedSpec == null)
            {
                MessageBox.Show("Chưa chọn data");
                return;
            }
            SetDataToElement(ElementPicked, _assingSpecViewModel.SelectedSpec);
            ItemSource itemSource = new ItemSource(_assingSpecViewModel.SelectedSpec.Name);
            _assingSpecViewModel.DataAssigned.Add(itemSource);
            MessageBox.Show("Gán data thành công cho element ID: " + ElementPicked.Id + " Data: " + _assingSpecViewModel.SelectedSpec.Name);

        }
        public void ToggleElementVisibility()
        {
            if (ElementPicked == null)
            {
                MessageBox.Show("Chưa chọn element");
                return;
            }
            using (Transaction t = new Transaction(_uiDoc.Document, "Hide Element"))
            {
                t.Start();
                if (!_isHide)
                {
                    _uiDoc.Document.ActiveView.HideElements(new List<ElementId> { ElementPicked.Id });
                    _isHide = true;
                }
                else
                {
                    _uiDoc.Document.ActiveView.UnhideElements(new List<ElementId> { ElementPicked.Id });
                    _isHide = false;
                }

                t.Commit();
            }
        }
        public void ToggleElementIsolate()
        {
            if (ElementPicked == null)
            {
                MessageBox.Show("Chưa chọn element");
                return;
            }
            using (Transaction t = new Transaction(_uiDoc.Document, "Hide Element"))
            {
                t.Start();
                if (!_isIsolate)
                {
                    _uiDoc.Document.ActiveView.IsolateElementsTemporary(new List<ElementId> { ElementPicked.Id });
                    _isIsolate = true;
                }
                else
                {
                    _uiDoc.Document.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                    _isIsolate = false;
                }

                t.Commit();
            }
        }
        public void Excute()
        {
            _assingSpecView.ShowDialog();
        }

        public void SetDataToElement<T>(Element element, T data)
        {
            Guid schemaGuid = new Guid("A1B2C3D4-E5F6-7890-1234-56789ABCDEF0");

            // Schema chỉ cần 1 field string để lưu JSON
            Schema schema = Schema.Lookup(schemaGuid);
            if (schema == null)
            {
                SchemaBuilder builder = new SchemaBuilder(schemaGuid);
                builder.SetSchemaName("SpecData");
                builder.AddSimpleField("SpecDataJson", typeof(string));
                schema = builder.Finish();
            }

            Entity entity = new Entity(schema);

            // Serialize object sang JSON
            string json = JsonConvert.SerializeObject(data);
            entity.Set("SpecDataJson", json);

            using (Transaction tx = new Transaction(element.Document, "Lưu Data"))
            {
                tx.Start();
                element.SetEntity(entity);
                tx.Commit();
            }
        }

    }
}
