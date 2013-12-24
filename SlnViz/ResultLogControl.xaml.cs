using Microsoft.Win32;
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
using System.Xml.Linq;

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



        private string _Filepath;
        public string Filepath {
            get { return _Filepath; }
            set {
                if (_Filepath != value) {
                    _Filepath = value;
                    OnPropertyChanged("Filepath");
                }
            }
        }

        private void Add_Click_1(object sender, RoutedEventArgs e) {
            this.ResultLog.Add(
                new ResultLogString(this.InputText,
                ExecutionEngine.Execute(this.InputText).ToString()
                ));
        }

        private void Open_Click_1(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            this.Filepath = ofd.FileName;
            this.ResultLog.Clear();
            XElement root = XElement.Load(this.Filepath);
            foreach (var r in root.Elements("ResultLog")) {
                var l = ResultLogString.Deserialize(r);
                this.ResultLog.Add(l);
            }
            ///Deserialize this new file.
        }

        private void Save_Click_1(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(this.Filepath)) {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.ShowDialog();
                this.Filepath = sfd.FileName;
            }
            XElement resultLog = new XElement("ResultsLog");
            foreach (var r in this.ResultLog) {
                resultLog.Add(r.ToXml());
            }
            resultLog.Save(Filepath);
        }

        private void Test_Click_1(object sender, RoutedEventArgs e) {
            var eval = ExecutionEngine.Execute(this.InputText).ToString();
           ((sender as Button).Tag as ResultLogString).Test(eval);
        }
    }
}
