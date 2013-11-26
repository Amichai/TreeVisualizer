using Roslyn.Compilers;
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

            //this.NodeTypes.ItemsSource = Enum.GetValues(typeof(SyntaxNode.NodeType)).Cast<SyntaxNode.NodeType>();
            //this.NodeTypes.SelectionChanged += NodeTypes_SelectionChanged;

            //foreach(var proj in sln)
            proj = sln.Projects.First();
            //var a = proj.FileResolver;
            //var b = proj.FilePath;
            //var c = proj.Documents;
            //var d = proj.ProjectReferences;
            //var e = proj.AssemblyName;
            
            ScriptEngine engine = new ScriptEngine();
            //var libpath = System.IO.Path.GetFullPath(@"..\..\..\..\TreeViz\TreeViz\bin\debug\TreeViz.exe");
            //engine.AddReference(proj.GetCompilation().ToMetadataReference());
            engine.ImportNamespace("TreeViz");
            var binPath = System.IO.Path.Combine(new System.IO.FileInfo(proj.FilePath).Directory.FullName, "bin", "debug");
            
            //engine.AddReference(new MetadataFileReference(libpath));
            foreach (var r in proj.MetadataReferences) {
                engine.AddReference(r);
            }
            var loader = engine.AssemblyLoader;
            var comp = proj.GetCompilation();
            var namesp = comp.GlobalNamespace;
            //var emmited = comp.GetReferencedAssemblySymbol(comp.ToMetadataReference()).Identity;
            
            var l = engine.CreateSession();
            //var result= l.Execute("new MainWindow();");
            var result = l.Execute("new MvvmTextEditor()");
            this.tree.ItemsSource = proj.Documents.Select(i => new SyntaxNode(i.GetSyntaxTree().GetRoot()));            

        }

        IProject proj;

        void NodeTypes_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e) {


        }
    }
}
