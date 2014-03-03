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
    /// Interaction logic for StatsView.xaml
    /// </summary>
    public partial class StatsView : UserControl, INotifyPropertyChanged {
        public StatsView() {
            InitializeComponent();
            this.Stats = new ObservableCollection<CodeStat>();
            
        }

        private string _Title;
        public string Title {
            get { return _Title; }
            set {
                if (_Title != value) {
                    _Title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        private string _Path;
        public string Path {
            get { return _Path; }
            set {
                if (_Path != value) {
                    _Path = value;
                    OnPropertyChanged("Path");
                }
            }
        }

        public void SetNodes(List<SyntaxNodeWrapper> nodes, string path) {
            this.Stats.Clear();
            this.Path = path;
            this.Title = path.Split('\\').Last();
            var lineCount = 0;

            //Get average length
            foreach (var n in nodes) {
                lineCount += n.Node.GetText().Lines.Count();
            }

            this.Stats.Add(new CodeStat("Lines of code", lineCount));


            foreach (var s in new LocStat(SyntaxNodeWrapper.NodeType.Class, nodes)) {
                this.Stats.Add(s);
            }

            foreach (var s in new LocStat(SyntaxNodeWrapper.NodeType.Property, nodes)) {
                this.Stats.Add(s);
            }

            foreach (var s in new LocStat(SyntaxNodeWrapper.NodeType.File, nodes)) {
                this.Stats.Add(s);
            }

            foreach (var s in new LocStat(SyntaxNodeWrapper.NodeType.Method, nodes)) {
                this.Stats.Add(s);
            }

            this.Stats.Add(new CodeStat("Cyclomatic complexity"));

            this.Stats.Add(new CodeStat("Compiler Errors", 0));
            this.Stats.Add(new CodeStat("Warnings", 7));




            //this.Stats.Add(new CodeStat("Files count", filesCount));
            //this.Stats.Add(new CodeStat("Class count", classCount));
            //this.Stats.Add(new CodeStat("Property count", propertyCount));
            //this.Stats.Add(new CodeStat("Method count", methodCount));


        }

        private ObservableCollection<CodeStat> _Stats;
        public ObservableCollection<CodeStat> Stats {
            get { return _Stats; }
            set {
                if (_Stats != value) {
                    _Stats = value;
                    OnPropertyChanged("Stats");
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
    }
}
