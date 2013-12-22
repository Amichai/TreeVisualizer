using Roslyn.Compilers;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;
using Roslyn.Services;
using System;
using System.Collections;
using System.Collections.Generic;
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
    /// Interaction logic for ExecutionEngine.xaml
    /// </summary>
    public partial class ExecutionEngine : UserControl, INotifyPropertyChanged {
        private Session session;
        private ScriptEngine engine;

        public ExecutionEngine() {
            InitializeComponent();
        }

        public void Init(string filepath) {
            IWorkspace workspace = Workspace.LoadSolution(filepath);
            ISolution sln = workspace.CurrentSolution;
            this.engine = new ScriptEngine();
            foreach (var proj in sln.Projects) {

                var assemblyName = proj.AssemblyName;
                engine.ImportNamespace(assemblyName);
                var binPath = System.IO.Path.Combine(new System.IO.FileInfo(proj.FilePath).Directory.FullName, "bin", "debug", string.Format("{0}.exe", assemblyName));
                if (System.IO.File.Exists(binPath)) {
                    engine.AddReference(new MetadataFileReference(binPath));
                }

                binPath = System.IO.Path.Combine(new System.IO.FileInfo(proj.FilePath).Directory.FullName, "bin", "debug", string.Format("{0}.dll", assemblyName));
                if (System.IO.File.Exists(binPath)) {
                    engine.AddReference(new MetadataFileReference(binPath));
                }
                foreach (var r in proj.MetadataReferences) {
                    engine.AddReference(r);
                }
            }
            //var proj = sln.Projects.First();
            //foreach (var proj in sln.Projects) {
            this.session = engine.CreateSession();
            //var result= l.Execute("new MainWindow();");
            //var result = this.session.Execute("new MvvmTextEditor()");
        }

        public object Execute(string inputText) {
            if (string.IsNullOrWhiteSpace(inputText)) {
                return "";
            }
            try {
                if (session == null) {
                    return "No C# session available.";
                }
                var result = session.Execute(inputText);
                if (result == null) {
                    return "";
                }
                return result;
            } catch (Exception ex) {
                return ex.Message + " " + ex.InnerException;
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

        private void stringResult(string input) {
            TextBlock t = new TextBlock() { HorizontalAlignment = System.Windows.HorizontalAlignment.Left};
            t.Text = input;
            this.outputStack.Children.Add(t);
        }

        internal void SetResult(object result) {
            if (result is string) {
                var asString = result as string;
                stringResult(asString);
            } else if (result is Window) {
                var asWindow = (result as Window);
                var target = asWindow.Content;
                asWindow.Content = null;
                this.outputStack.Children.Add(target as UIElement);
            } else if (result is UIElement) {
                this.outputStack.Children.Add(result as UIElement);
            } else if (result is IEnumerable) {
                foreach (var r in (result as IEnumerable)) {
                    SetResult(r);
                }
            } else {
                stringResult(result.ToString());
            }
        }


        private void TextBox_PreviewKeyDown_1(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                var result = Execute(this.InputText);
                SetResult(result);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) {
            var eh = PropertyChanged;
            if (eh != null) {
                eh(this, new PropertyChangedEventArgs(name));
            }
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e) {
            this.InputText = (sender as TextBox).Text;
        }
    }
}
