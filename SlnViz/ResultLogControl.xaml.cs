using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SlnViz {
    /// <summary>
    /// Interaction logic for ResultLogControl.xaml
    /// </summary>
    public partial class ResultLogControl : UserControl, INotifyPropertyChanged {

        private ObservableCollection<ResultLogString> _ResultLog;
        public ObservableCollection<ResultLogString> ResultLog {
            get { return _ResultLog; }
            set {
                if (_ResultLog != value) {
                    _ResultLog = value;
                    OnPropertyChanged("ResultLog");
                }
            }
        }

        public ResultLogControl() {
            InitializeComponent();
            this.ResultLog = new ObservableCollection<ResultLogString>();

            ///Load the result log from disk
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) {
            var eh = PropertyChanged;
            if (eh != null) {
                eh(this, new PropertyChangedEventArgs(name));
            }
        }

        private string _InputText;
        public string InputText {
            get { return _InputText; }
            set {
                if (_InputText != value) {
                    _InputText = value;
                    OnPropertyChanged("InputText");
                }
            }
        }

        private void Add_Click_1(object sender, RoutedEventArgs e) {
            this.ResultLog.Add(
                new ResultLogString(this.InputText,
                ExecutionEngine.Execute(this.InputText).ToString()
                ));
        }
    }
}
