using Microsoft.Win32;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;
using Roslyn.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        public MainWindow() {
            InitializeComponent();
            //string path = @"..\..\..\TreeViz.sln";
            //this.Filepath = @"C:\Users\Amichai\Documents\Visual Studio 2012\Projects\ComputationalPhysics\computationalPhysics.sln";
            this.Filepath = @"C:\Users\Amichai\Documents\Visual Studio 2012\Projects\EquationEditor\EquationEditor.sln";
            loadSln(this.Filepath);
        }

        private void loadSln(string path) {
            IWorkspace workspace = Workspace.LoadSolution(path);
            ISolution sln = workspace.CurrentSolution;
            List<SyntaxNode> nodes = new List<SyntaxNode>();
            foreach (var p in sln.Projects) {
                var selectedNodes = p.Documents.Select(i => new SyntaxNode(i.GetSyntaxTree().GetRoot()));
                nodes.AddRange(selectedNodes);
            }

            this.execution.Init(path, nodes);
            this.tree.ItemsSource = nodes;
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

        void NodeTypes_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void Window_Loaded_1(object sender, RoutedEventArgs e) { }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) {
            var eh = PropertyChanged;
            if (eh != null) {
                eh(this, new PropertyChangedEventArgs(name));
            }
        }

        private void Open_Click_1(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog();
            ofd.ShowDialog();
            var newPath = ofd.FileName;
            if (newPath != "") {
                this.loadSln(newPath);
                this.Filepath = newPath;
            }
        }

        private void Reload_Click_1(object sender, RoutedEventArgs e) {
            this.loadSln(this.Filepath);
        }
    }
}
