using Roslyn.Compilers;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;
using Roslyn.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
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
    /// Interaction logic for ExecutionEngine.xaml
    /// </summary>
    public partial class ExecutionEngine : UserControl, INotifyPropertyChanged {
        private static Session session;
        private static ScriptEngine engine;

        public ExecutionEngine() {
            InitializeComponent();
            AppendNewLine(new PageLine());
            setFocus();
        }

        List<string> importedNamespaces = new List<string>();
        List<string> importedRefs = new List<string>();

        private void addReference(MetadataReference r) {
            string refName = r.Display.Split('\\').Last();
            if (!importedRefs.Contains(refName)) {
                engine.AddReference(r);
                Debug.Print("Ref: " + r.Display);
                importedRefs.Add(refName);
            } 
        }

        public void Init(string filepath, List<SyntaxNode> nodes) {
            IWorkspace workspace = Workspace.LoadSolution(filepath);
            ISolution sln = workspace.CurrentSolution;
            engine = new ScriptEngine();
            foreach (var proj in sln.Projects) {

                var assemblyName = proj.AssemblyName;
                importNamespace(assemblyName);
                var binPath = System.IO.Path.Combine(new System.IO.FileInfo(proj.FilePath).Directory.FullName, "bin", "debug", string.Format("{0}.exe", assemblyName));
                if (System.IO.File.Exists(binPath)) {
                    Debug.Print("Bin path: " + binPath);
                    engine.AddReference(new MetadataFileReference(binPath));
                }

                binPath = System.IO.Path.Combine(new System.IO.FileInfo(proj.FilePath).Directory.FullName, "bin", "debug", string.Format("{0}.dll", assemblyName));
                if (System.IO.File.Exists(binPath)) {
                    Debug.Print("Bin path: " + binPath);
                    engine.AddReference(new MetadataFileReference(binPath));
                }

                foreach (var r in proj.MetadataReferences) {
                    addReference(r);
                }
            }

            foreach (var n in nodes) {
                var namespaces = n.GetNamespaces();
                foreach (var space in namespaces) {
                    importNamespace(space);
                }
            }

            foreach (var n in standardNamespaces) {
                importNamespace(n);
            }
            session = engine.CreateSession();
        }

        private List<string> standardNamespaces = new List<string>() { "System", "System.Linq", "System.Collections", "System.Collections.Generic" };

        private void importNamespace(string space) {
            space = space.TrimEnd();
            if (!this.importedNamespaces.Contains(space)) {
                engine.ImportNamespace(space);
                importedNamespaces.Add(space);
                Debug.Print("space: " + space);
            } 
        }

        public void CSharpAssign(string result, int lineNumber) {
            if (result == null) {
                return;
            }
            string lastValName = "_" + lineNumber.ToString();
            var escapedString = result.Replace("\"", "\"\"");
            var assign = "var " + lastValName + " = @\"" + escapedString + "\";";
            session.Execute(assign);
            session.Execute("var _ = " + lastValName + ";");
        }

        public void CSharpAssign(string inputText, string result, int lineNumber) {

            string lastValName = "_" + lineNumber.ToString();
            try {
                session.Execute(@"var " + lastValName + " = " + inputText + ";");
                session.Execute(@"var _" + " = " + lastValName + ";");

            } catch {
                var escapedString = result.Replace("\"", "\"\"");
                var assign = "var " + lastValName + " = @\"" + escapedString + "\";";
                session.Execute(assign);
            }
        }

        public object AppendCSharp(string inputText, int lineNumber) {
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
                CSharpAssign(inputText, result.ToString(), lineNumber);
                if (inputText.Last() == ';') {
                    return "";
                }
                return result;
            } catch (Exception ex) {
                return ex.Message + " " + ex.InnerException;
            }
        }

        public static object Execute(string inputText) {
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) {
            var eh = PropertyChanged;
            if (eh != null) {
                eh(this, new PropertyChangedEventArgs(name));
            }
        }

        ////NEW OUTPUT STACK CODE:

        void setFocus() {
            var count = this.outputStack.Children.Count;
            var line = this.outputStack.Children[count - 1] as PageLine;
            line.SetFocus();
        }

        public void AppendNewLine(PageLine newLine) {
            selectedIndex++;
            Observable.FromEventPattern(newLine.del, "Click").Select(i => i.Sender).Subscribe(sl => {
                var lineToDelete = (sl as Button).Tag as UIElement;
                if (lineToDelete is PageLine) {
                    ///TODO: still buggy...
                    //foreach (var l in (lineToDelete as PageLine).GetDependentLineIndices()) {
                    //    this.allLines.Children.RemoveAt(l);
                    //}
                }
                this.outputStack.Children.Remove(lineToDelete);
            });
            newLine.NewUIResult.Subscribe(i => {
                var smartGrid = new GridSplitter.SmartGrid();
                smartGrid.Add(i);
                this.outputStack.Children.Add(smartGrid);
                newLine.AddDependentLineIndex(selectedIndex);
                selectedIndex++;
            });

            Observable.FromEventPattern(newLine.input, "PreviewKeyDown").Subscribe(i => {
                var sender = ((i.Sender as TextBox).Tag as PageLine);
                var e = (i.EventArgs as KeyEventArgs);
                if (e.Key == Key.Enter && (Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift))) {
                    var result = AppendCSharp(newLine.input.Text, newLine.LineNumber);
                    newLine.SetResult(result);
                    AppendNewLine(new PageLine());
                    e.Handled = true;
                } else if (e.Key == Key.Enter && (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))) {
                    var result = sender.Result;
                    if (result == "") {
                        result = sender.GetText();
                    }
                    CSharpAssign(result, sender.LineNumber);
                    AppendNewLine(new PageLine());
                    e.Handled = true;
                }
            });
            this.outputStack.Children.Add(newLine);
            setFocus();
        }

        private int selectedIndex = 0;

        ///TODO: thisi fails, because all lines contains result controls which aren't PageLines anymore
        private void setTextFromIndex(int index) {
            var count = this.outputStack.Children.Count;
            var active = this.outputStack.Children[count - 1] as PageLine;
            if (active.input.Text.Contains('\n')) {
                return;
            }

            var pl = (this.outputStack.Children[index] as PageLine);
            if (pl == null) {
                return;
            }
            var text = pl.input.Text;
            (active as PageLine).input.Text = text;
        }

        private void Window_PreviewKeyDown_1(object sender, KeyEventArgs e) {
            if (e.Key == Key.Up) {
                if (selectedIndex < 1) {
                    return;
                }
                selectedIndex--;
                if (selectedIndex > this.outputStack.Children.Count - 1) {
                    return;
                }
                setTextFromIndex(selectedIndex);
            } else if (e.Key == Key.Down) {
                if (selectedIndex > this.outputStack.Children.Count - 2) {
                    return;
                }
                selectedIndex++;
                setTextFromIndex(selectedIndex);
            }
        }

        ///TODO: serialize and save to xml
        ///TODO: undo stack, up arrow
        ///TODO: delete result button

        private void Save_Click_1(object sender, RoutedEventArgs e) {
            XElement root = new XElement("AllLines");
            foreach (var l in this.outputStack.Children) {
                if ((l as PageLine) == null) {
                    continue;
                }
                var lineXml = (l as PageLine).ToXml();
                root.Add(lineXml);
            }
            var dialog = new Microsoft.Win32.SaveFileDialog();
            var success = dialog.ShowDialog().Value;
            if (success) {
                root.Save(dialog.FileName);
            }
        }

        private void resetContent() {
            this.outputStack.Children.Clear();
            this.selectedIndex = 0;
            PageLine.ResetLineCounter();
        }

    }

}
