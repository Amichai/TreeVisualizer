using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;
using Roslyn.Services;
using System;
using System.Collections.Generic;
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
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            string path = @"..\..\..\TreeViz.sln";
            IWorkspace workspace = Workspace.LoadSolution(path);

            ISolution sln = workspace.CurrentSolution;
            this.execution.Init(path);
            //this.NodeTypes.ItemsSource = Enum.GetValues(typeof(SyntaxNode.NodeType)).Cast<SyntaxNode.NodeType>();
            //this.NodeTypes.SelectionChanged += NodeTypes_SelectionChanged;
            List<SyntaxNode> nodes = new List<SyntaxNode>();
            foreach (var p in sln.Projects) {
                nodes.AddRange(p.Documents.Select(i => new SyntaxNode(i.GetSyntaxTree().GetRoot())));
            }
            this.tree.ItemsSource = nodes;
            //var proj = sln.Projects.First();

            //foreach (var proj in sln.Projects) {
            //ScriptEngine engine = new ScriptEngine();
            //var assemblyName = proj.AssemblyName;
            //engine.ImportNamespace(assemblyName);
            //var binPath = System.IO.Path.Combine(new System.IO.FileInfo(proj.FilePath).Directory.FullName, "bin", "debug", string.Format("{0}.exe", assemblyName));
            //engine.AddReference(new MetadataFileReference(binPath));
            //foreach (var r in proj.MetadataReferences) {
            //    engine.AddReference(r);
            //}
            //var l = engine.CreateSession();
            ////var result= l.Execute("new MainWindow();");
            //var result = l.Execute("new MvvmTextEditor()");
            //this.tree.ItemsSource = proj.Documents.Select(i => new SyntaxNode(i.GetSyntaxTree().GetRoot()));
            //}

        }

        void NodeTypes_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e) {


        }
    }
}
