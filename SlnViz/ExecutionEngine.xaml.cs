using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;
using Roslyn.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Reflection.Emit;
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
    /// Interaction logic for ExecutionEngine.xaml
    /// </summary>
    public partial class ExecutionEngine : UserControl, INotifyPropertyChanged {
        private static Session session;
        private static ScriptEngine engine;

        public ExecutionEngine() {
            InitializeComponent();
            this.NamespaceImported = new Subject<string>();
            this.BinaryImported = new Subject<string>();
            AppendNewLine(new PageLine());
            setFocus();
        }


        public Subject<string> NamespaceImported;
        public Subject<string> BinaryImported;

        List<string> importedNamespaces = new List<string>();
        List<string> importedRefs = new List<string>();

        private void clearAllImports() {
            this.importedNamespaces.Clear();
            this.importedRefs.Clear();
        }

        private void addReference(MetadataReference r) {
            string refName = r.Display.Split('\\').Last();
            if (!importedRefs.Contains(refName)) {
                engine.AddReference(r);
                Debug.Print("Ref: " + r.Display);
                importedRefs.Add(refName);
                this.BinaryImported.OnNext(r.Display);
            }
        }

        public void Init(ISolution sln, List<SyntaxNodeWrapper> nodes) {
            this.clearAllImports();
            engine = new ScriptEngine();
            foreach (var proj in sln.Projects) {

                AddProject(proj);
            }

            foreach (var n in nodes) {
                var namespaces = n.GetNamespaces();
                foreach (var space in namespaces) {
                    ImportNamespace(space);
                }
            }

            foreach (var n in standardNamespaces) {
                ImportNamespace(n);
            }
            session = engine.CreateSession();
        }

        public void Init2(string filepath, List<SyntaxNodeWrapper> nodes) {
            IWorkspace workspace = Workspace.LoadSolution(filepath);
            ISolution sln = workspace.CurrentSolution;
            engine = new ScriptEngine();
            foreach (var proj in sln.Projects) {
                AddProject(proj);
            }
            foreach (var n in nodes) {
                var namespaces = n.GetNamespaces();
                foreach (var space in namespaces) {
                    ImportNamespace(space);
                }
            }

            foreach (var n in standardNamespaces) {
                ImportNamespace(n);
            }
            session = engine.CreateSession();
        }


        

        public void AddProject(IProject proj) {
            bool errors;
            string assemblyName = string.Format(proj.AssemblyName + "{0}.exe", assemblyCounter++);
            var fullAssemblyName = getFullPath(assemblyName);
            var comp = proj.GetCompilation();
            
            var compilation = Compilation.Create(assemblyName, 
                new CompilationOptions(proj.CompilationOptions.OutputKind), comp.SyntaxTrees.Select(i => (SyntaxTree)i), comp.References, null, null);

            try {
                createAssembly(assemblyName, compilation, out errors);
                addReference(new MetadataFileReference(fullAssemblyName));
                foreach (var r in proj.MetadataReferences) {
                    addReference(r);
                }
            } catch {
                Debug.Print("Failed to reference project: " + proj.Name);
            }           
        }

        private static string getFullPath(string assemblyName) {
            var fullAssemblyName = System.IO.Path.Combine(Directory.GetCurrentDirectory(), assemblyName);
            return fullAssemblyName;
        }
        public void AddProject2(IProject proj) {
            var assemblyName = proj.AssemblyName;
            ImportNamespace(assemblyName);
            var binPath = System.IO.Path.Combine(new System.IO.FileInfo(proj.FilePath).Directory.FullName, "bin", "debug", string.Format("{0}.exe", assemblyName));
            if (System.IO.File.Exists(binPath)) {
                Debug.Print("Bin path: " + binPath);
                AddReference(binPath);
            }

            binPath = System.IO.Path.Combine(new System.IO.FileInfo(proj.FilePath).Directory.FullName, "bin", "debug", string.Format("{0}.dll", assemblyName));
            if (System.IO.File.Exists(binPath)) {
                Debug.Print("Bin path: " + binPath);
                addReference(new MetadataFileReference(binPath));
            }

            foreach (var r in proj.MetadataReferences) {
                addReference(r);
            }
        }

        public void AddReference(string binPath) {
            addReference(new MetadataFileReference(binPath));
        }

        private List<string> standardNamespaces = new List<string>() { "System", "System.Linq", "System.Collections", "System.Collections.Generic" };

        public void ImportNamespace(string space) {
            space = space.TrimEnd();
            if (!this.importedNamespaces.Contains(space)) {
                engine.ImportNamespace(space);
                importedNamespaces.Add(space);
                this.NamespaceImported.OnNext(space);
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

        public object AppendCSharp(string inputText, int lineNumber, out bool exceptionThrown) {
            exceptionThrown = false;
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
                exceptionThrown = true;
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
            TextBlock t = new TextBlock() { HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
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
                    bool exceptionThrown;
                    var result = AppendCSharp(newLine.input.Text, newLine.LineNumber, out exceptionThrown);
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

        private static int assemblyCounter = 0;

        public void Launch(IProject i, SyntaxTree[] trees) {
            string assemblyName;
            bool errors;
            Assembly a;
            getAssembly(i, trees, out assemblyName, out errors);
            //AppDomain.CurrentDomain.ExecuteAssembly(assemblyName);
            Process.Start(assemblyName);
        }

        internal void AddCompilation(IProject i, SyntaxTree[] trees) {
            string assemblyName;
            bool errors;
            Assembly a;
            getAssembly(i, trees, out assemblyName, out errors);

            if (errors) {
                return;
            }
            var fullAssemblyName = getFullPath(assemblyName);
            //var a2 = AssemblyIdentity.FromAssemblyDefinition(a);
            addReference(new MetadataFileReference(fullAssemblyName));
            
            //engine.AddReference(a);
        }

        private static void getAssembly(IProject i, SyntaxTree[] trees, out string assemblyName, out bool errors) {
            assemblyName = string.Format(@"Greeter{0}.dll", ++assemblyCounter);
            //var syntaxTree = SyntaxTree.ParseText(text);
            foreach (var t in trees) {
                var text = t.GetText().ToString();
                Debug.Print("Text: " + text);
            }
            var comp = Compilation.Create(assemblyName,
            syntaxTrees: trees,
                     references:
                i.MetadataReferences,
                //},
                  options: new CompilationOptions(OutputKind.DynamicallyLinkedLibrary));


            //var exeName = "testingComp.exe";
            //var sucesss = comp.Emit(exeName);
            //Process.Start(exeName);

            createAssembly(assemblyName, comp, out errors);
        }

        private static void createAssembly(string assemblyName, Compilation comp, out bool errors) {
            //AssemblyBuilder ab = new AssemblyBuilder()
            //var s = comp.CreateDefaultWin32Resources(true, false, null, null);
            
            EmitResult result;
            using (var file = new FileStream(assemblyName, FileMode.Create)) {
                //result = comp.Emit(outputStream: file, outputName:null , pdbFileName: null, pdbStream: null, xmlDocStream: null, cancellationToken: CancellationToken.None ,win32ResourcesInRESFormat: s,manifestResources: null);
                result = comp.Emit(outputStream: file);
                file.Flush();
            }
            
            
            errors = false;

            if (result.Diagnostics.Count() > 0) {
                foreach (var d in result.Diagnostics) {
                    Debug.Print(d.Info.GetMessage());
                }
                errors = true;
            }
            if (errors) {
                Debug.Print("DIAGNOSTIC ERRORS IN THE EXECUTION ENGINE");
            }
            
        }

        internal void UpdateSession() {
            session = engine.CreateSession();
        }
    }
}
