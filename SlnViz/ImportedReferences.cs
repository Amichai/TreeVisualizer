using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SlnViz {
    public class ImportedReferences : INotifyPropertyChanged{
        public ImportedReferences(ExecutionEngine execution) {
            this.execution = execution;
            this.Namespaces = new ObservableCollection<string>();
            this.Binaries = new ObservableCollection<string>();


            this.execution.NamespaceImported.Subscribe(i => {
                this.Namespaces.Add(i);
            });
            this.execution.BinaryImported.Subscribe(i => {
                this.Binaries.Add(i);
            });
        }

        private ExecutionEngine execution;

        private ObservableCollection<string> _Namespaces;
        public ObservableCollection<string> Namespaces {
            get { return _Namespaces; }
            set {
                if (_Namespaces != value) {
                    _Namespaces = value;
                    OnPropertyChanged("Namespaces");
                }
            }
        }

        private ObservableCollection<string> _Binaries;
        public ObservableCollection<string> Binaries {
            get { return _Binaries; }
            set {
                if (_Binaries != value) {
                    _Binaries = value;
                    OnPropertyChanged("Binaries");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) {
            var eh = PropertyChanged;
            if (eh != null) {
                eh(this, new PropertyChangedEventArgs(name));
            }
        }

        internal void AddNamespace(string i) {
            this.execution.ImportNamespace(i);
        }

        internal void AddBinary(string i) {
            throw new NotImplementedException();
        }
    }
}
