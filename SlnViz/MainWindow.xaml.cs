using Microsoft.Win32;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;
using Roslyn.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using TreeViz;

namespace SlnViz {
    /// <summary>
    /// Interaction logic for MainWindow.xaml   
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        public MainWindow() {
            InitializeComponent();
            //string path = @"..\..\..\TreeViz.sln";
            this.Filepath = @"C:\Users\Amichai\Documents\Visual Studio 2012\Projects\ComputationalPhysics\computationalPhysics.sln";
            //this.Filepath = @"C:\Users\Amichai\Documents\Visual Studio 2012\Projects\EquationEditor\EquationEditor.sln";
            //this.Filepath = @"C:\Users\Amichai\Documents\Visual Studio 2012\Projects\SolutionBuilder\SolutionBuilder.sln";
            //this.Filepath = @"C:\Users\Amichai\Documents\Visual Studio 2012\Projects\TreeViz\TreeViz.sln";
            //this.Filepath = @"C:\Users\Amichai\Documents\Projects\S1102 MoMath Exhibits\Robot Swarm\RobotManager\RobotManager.sln";
            this.Imported = new ImportedReferences(this.execution);
            loadSln(this.Filepath);
        }

        private ImportedReferences _Imported;
        public ImportedReferences Imported {
            get { return _Imported; }
            set {
                if (_Imported != value) {
                    _Imported = value;
                    OnPropertyChanged("Imported");
                }
            }
        }

        ISolution inspectionSln;

        private void loadSln(string path) {
            if (path == "") {
                return;
            }
            IWorkspace workspace = Workspace.LoadSolution(path);
            ISolution sln = workspace.CurrentSolution;
            this.inspectionSln = sln;

            List<SyntaxNodeWrapper> nodes = loadISolution(sln);
            this.stats.SetNodes(nodes, path);
        }

        private List<SyntaxNodeWrapper> loadISolution(ISolution sln) {
            List<SyntaxNodeWrapper> nodes = new List<SyntaxNodeWrapper>();

            //SyntaxNode.ProjectUpdated.Subscribe(i => {
            //    //this.execution.AddProject(i);
            //    //this.execution.AddCompilation(i.Project, i.Tree);
            //    this.execution.AddCompilation(i.Project, new SyntaxTree[] { i.Tree });
            //});

            foreach (var p in sln.Projects) {
                var selectedNodes = p.Documents.Select(i => {
                    var root = i.GetSyntaxTree().GetRoot();
                    var sn = new SyntaxNodeWrapper(root, i);
                    return sn;
                });
                nodes.AddRange(selectedNodes);
            }
            this.tree.ItemsSource = nodes;
            this.execution.Init(sln, nodes);
            //this.execution.Init(sln.FilePath, nodes);
            this.notepad.SetExecutionEngine(this.execution);
            return nodes;
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
            reloadSln();
        }

        private void reloadSln() {
            this.loadSln(this.Filepath);
        }

        private string _NewNamespace;
        public string NewNamespace {
            get { return _NewNamespace; }
            set {
                if (_NewNamespace != value) {
                    _NewNamespace = value;
                    OnPropertyChanged("NewNamespace");
                }
            }
        }

        private void AddNewNamespace_Click(object sender, RoutedEventArgs e) {
            this.Imported.AddNamespace(this.NewNamespace);
            this.NewNamespace = "";
        }

        private void GetTrivia_Click(object sender, RoutedEventArgs e) {
            var node = ((sender as Button).Tag as SyntaxNodeWrapper).Node;
            try {
                var trivia = node.GetLeadingTrivia().Single(t => t.Kind == (int)SyntaxKind.DocumentationCommentTrivia);
                var xml = trivia.GetStructure();
                Debug.Print(xml.ToFullString());

            } catch (Exception ex){
                Debug.Print("Exception " + ex.Message);                
            }
        }

        private void AddMethod_Click(object sender, RoutedEventArgs e) {
            
            var m = Syntax.PropertyDeclaration(Syntax.ParseTypeName("string"), "Ticker2");
            var t = Syntax.Token(SyntaxKind.PublicKeyword);
            m = m.AddModifiers(t);
            m = m.AddAccessorListAccessors(
            Syntax.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Syntax.Token(SyntaxKind.SemicolonToken)),
            Syntax.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(Syntax.Token(SyntaxKind.SemicolonToken)));
            var syntaxNode = ((sender as Button).Tag as SyntaxNodeWrapper);

            var node = syntaxNode.Node;
            var newNode = (node as ClassDeclarationSyntax).AddMembers(m);
            var documentRootNode = syntaxNode.documentRootNode;
            var document = syntaxNode.document;

            documentRootNode = documentRootNode.ReplaceNode(node, newNode).NormalizeWhitespace();
            document = document.UpdateSyntaxRoot(documentRootNode);

            var toReturn = new ProjectUpdated() {
                Project = document.Project,
                Tree = SyntaxTree.ParseText(document.GetText().ToString())
            };

            this.inspectionSln = this.inspectionSln.UpdateDocument(document);
            this.loadISolution(this.inspectionSln);
        }

        private void ExectueAssembly(object sender, RoutedEventArgs e) {
            var syntaxNode = ((sender as Button).Tag as SyntaxNodeWrapper);

            var node = syntaxNode.Node;
            var documentRootNode = syntaxNode.documentRootNode;
            var document = syntaxNode.document;

            var toReturn = new ProjectUpdated() {
                Project = document.Project,
                Tree = SyntaxTree.ParseText(document.GetText().ToString())
            };

            this.inspectionSln = this.inspectionSln.UpdateDocument(document);
            //this.loadISolution(this.thisSln);
//            this.execution.Launch(toReturn.Project, new SyntaxTree[] { toReturn.Tree });

        }

        private void MvvmTextEditor_TextChanged_1(object sender, EventArgs e) {
            var editor = sender as MvvmTextEditor;
            var syntaxNode = (editor.Tag as SyntaxNodeWrapper);
            syntaxNode.TextEditorText = editor.Text;
        }

        private void ReloadNode_Click(object sender, RoutedEventArgs e) {
            var btn = sender as Button;
            var syntaxNode = (btn.Tag as SyntaxNodeWrapper);

            var node = syntaxNode.Node;
            var newNode = SyntaxTree.ParseText(syntaxNode.TextEditorText).GetRoot();
            
            var documentRootNode = syntaxNode.documentRootNode;
            var document = syntaxNode.document;

            documentRootNode = documentRootNode.ReplaceNode(node, newNode).NormalizeWhitespace();
            document = document.UpdateSyntaxRoot(documentRootNode);

            var toReturn = new ProjectUpdated() {
                Project = document.Project,
                Tree = SyntaxTree.ParseText(document.GetText().ToString())
            };

            this.inspectionSln = this.inspectionSln.UpdateDocument(document);
            this.loadISolution(this.inspectionSln);
        }

        private void AddReference_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();

            this.execution.AddReference(ofd.FileName);
            this.execution.UpdateSession();
        }
    }
}
