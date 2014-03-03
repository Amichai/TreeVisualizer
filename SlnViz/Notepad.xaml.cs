using ILNumerics.Drawing;
using ILNumerics.Drawing.Plotting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SlnViz {
    /// <summary>
    /// Interaction logic for Notepad.xaml
    /// </summary>
    public partial class Notepad : System.Windows.Controls.UserControl, INotifyPropertyChanged {
        public Notepad() {
            InitializeComponent();            
            this.pastLinesCache = new PastLinesCache();

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastFile)) {
                this.open(Properties.Settings.Default.LastFile);
            }
        }

        private bool caretAtEndOfLine(out string toExecute, out int lineNumber, out int insertionIdx) {
            toExecute = "";
            lineNumber = -1;
            insertionIdx = -1;
            var text = this.editor.Text;
            var pos = this.editor.CaretOffset;

            ///Extract the line from the caret offset
            int totalLength = 0;
            for (int i = 0; i < this.allLines.Count(); i++) {
                var l = allLines[i];
                totalLength += l.Count();
                if (totalLength >= pos) {
                    if (totalLength == pos) {
                        toExecute = l;
                        lineNumber = i;
                        insertionIdx = totalLength;
                        return true;
                    }
                    return false;
                }
                totalLength++;
            }
            return false;
        }

        private int getLineNumberOfCaret(int caretPos) {
            ///Extract the line from the caret offset
            int totalLength = 0, lineNumber = -1;
            for (int i = 0; i < this.allLines.Count(); i++) {
                var l = allLines[i];
                totalLength += l.Count();
                if (totalLength >= caretPos) {
                    lineNumber = i;
                    break;
                }
                totalLength++;
            }
            return lineNumber;
        }

        private PastLinesCache pastLinesCache;

        private List<string> allLines {
            get {
                return this.editor.Text.Split('\n').ToList();
            }
        }

        private bool caretAtStartOfLine() {
            var idx = this.caretPosition - 1;
            if (idx >= this.editor.Text.Length || idx < 0) {
                return false;
            }
            return this.editor.Text[idx] == '\n';
        }

        private string handleException(string exceptionText, string input, int lineNumber) {
            ///Try to get type information from the object
            bool ex;
            var result = this.engine.AppendCSharp("typeof(" + input + ")", lineNumber, out ex);
            if (exceptionText.Contains("is a namespace but is used like a variable ")) {
                string @namespace = input;
                var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.Namespace == @namespace
                        select t;
                var toReturn = string.Concat(q.ToList().Select(i => i + "\n"));
                return toReturn;
            } else if (!ex) {
                return (result as Type).TypeInfo();
            } else {
                return exceptionText;
            }
        }

        private void MvvmTextEditor_PreviewKeyDown_1(object sender, System.Windows.Input.KeyEventArgs e) {
            var key = e.Key;
            string toAppend;
            int lineNumber;
            switch (key) {
                case Key.Enter:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl)) {
                        e.Handled = false;
                        return;
                    }
                    string toExecute;
                    int insertionIdx;
                    
                    var selectedLines = this.editor.SelectedText;
                    bool textSelected = !string.IsNullOrWhiteSpace(selectedLines);
                    if (this.caretAtEndOfLine(out toExecute, out lineNumber, out insertionIdx) || textSelected) {
                        bool exceptionThrown;
                        if (textSelected) {
                            toExecute = selectedLines;
                        }
                        Debug.Print(string.Format("To Execute: {0}", toExecute));
                        var result = this.engine.AppendCSharp(toExecute, lineNumber, out exceptionThrown); ///Render non-string types in a new window
                        this.pastLinesCache.Add(toExecute);
                        if (result.ToString().Contains(": error CS1002: ; expected ") && !textSelected) {
                            e.Handled = false;
                            return;
                        }

                        handleResult(lineNumber, toExecute, ref insertionIdx, exceptionThrown, result);
                        Debug.Print("Key down handled");
                        e.Handled = true;
                    }
                    break;
                case Key.Up:
                    if (!this.caretAtStartOfLine()) {
                        return;
                    }
                    if(!Keyboard.IsKeyDown(Key.LeftCtrl)){
                        return;
                    }
                    toAppend = this.pastLinesCache.Backward();
                    lineNumber = this.getLineNumberOfCaret(this.caretPosition);
                    this.overwriteLine(toAppend, lineNumber);
                    e.Handled = true;
                    break;
                case Key.Down:
                    if (!this.caretAtStartOfLine()) {
                        return;
                    }
                    if(!Keyboard.IsKeyDown(Key.LeftCtrl)){
                        return;
                    }
                    toAppend = this.pastLinesCache.Forward();
                    lineNumber = this.getLineNumberOfCaret(this.caretPosition);
                    this.overwriteLine(toAppend, lineNumber);
                    e.Handled = true;
                    break;
            }
        }

        internal void SetResult(object result, int insertionIdx) {
            bool toStringOverriden = false;
            try {
                var thisType = result.GetType();
                var methodInfo = thisType.GetMethod("ToString", new Type[] {});
                toStringOverriden = methodInfo.DeclaringType == thisType;
            } catch {

            }

            if (result is string) {
                var asString = result as string;
                //stringResult(asString);
                appendToEditor("\n" + asString, insertionIdx);
            } else if (result is ILNumerics.ILBaseArray) {
                appendToEditor("\n" + result.ToString(), insertionIdx);                
            } else if (toStringOverriden) {
                appendToEditor("\n" + result.ToString(), insertionIdx);            
            } else if (result is Window) {
                (result as Window).Show();
            } else if (result is ILPanel) {
                ILPanel p = result as ILPanel;
                System.Windows.Forms.Integration.WindowsFormsHost e = new System.Windows.Forms.Integration.WindowsFormsHost();
                e.Child = p;
                Grid g = new Grid();
                g.Children.Add(e);
                SetResult(g, insertionIdx);
            } else if (result is ILScene) {
                ILPanel p = new ILPanel();
                p.Scene.Add(result as ILScene);
                System.Windows.Forms.Integration.WindowsFormsHost e = new System.Windows.Forms.Integration.WindowsFormsHost();
                e.Child = p;
                Grid g = new Grid();
                g.Children.Add(e);
                SetResult(g, insertionIdx);

            } else if (result is ILPlotCube) {
                ILPanel p = new ILPanel();
                p.Scene.Add(result as ILPlotCube);
                System.Windows.Forms.Integration.WindowsFormsHost e = new System.Windows.Forms.Integration.WindowsFormsHost();
                e.Child = p;
                Grid g = new Grid();
                g.Children.Add(e);
                SetResult(g, insertionIdx);
            } else if (result is FrameworkElement) {
                Window w = new Window();
                w.Content = (result as FrameworkElement);
                w.Show();
            } else if (result is IEnumerable) {
                foreach (var r in (result as IEnumerable)) {
                    SetResult(r, insertionIdx);
                }
            } else {
                //stringResult(result.ToString());
                Debug.Print("New result: " + result.ToString());
                appendToEditor("\n" + result.ToString(), insertionIdx);
            }
        }

        private void handleResult(int lineNumber, string toExecute, ref int insertionIdx, bool exceptionThrown, object result) {
            string toAppend;
            if (insertionIdx == -1) {
                    insertionIdx = editor.Text.Length;
                }
            if (exceptionThrown) {
                toAppend = "\n" + handleException(result.ToString(), toExecute, lineNumber);
                appendToEditor(toAppend, insertionIdx);
            } else {
                try {
                    this.SetResult(result, insertionIdx);
                } catch (Exception ex) {
                    this.SetResult("Failed to render result", insertionIdx);
                }
                //toAppend = "\n" + result.ToString();
            }
            //appendToEditor(toAppend, insertionIdx);
        }

        private string getLines(int start, int end) {
            var toReturn = string.Concat(this.allLines.Skip(start).Take(end - start).Select(i => i + "\n"));
            Debug.Print(string.Format("Start: {0}, end: {1} acquired: {2}", start, end, toReturn));
            return toReturn;
        }

        private int caretPosition {
            get {
                return this.editor.CaretOffset;
            }
        }

        private void overwriteLine(string text, int lineNumber) {
            if (text == null) {
                return;
            }
            StringBuilder adjustedText = new StringBuilder();
            for (int i = 0; i < allLines.Count; i++) {
                string toAppend;
                if (i == lineNumber) {
                    toAppend = text;
                } else {
                    toAppend = allLines[i];
                }

                if (i == allLines.Count() - 1) {
                    adjustedText.Append(toAppend.Trim('\r'));
                } else {
                    adjustedText.AppendLine(toAppend.Trim('\r'));
                }
            }
            var asString = adjustedText.ToString();
            
            setText(asString);
        }

        private void setText(string text) {
            var c = this.caretPosition + 4;
            this.editor.Text = text;
            this.editor.CaretOffset = (int)Math.Min(c, text.Length);
        }


        private bool appendToEditor(string resultToAppend, int insertionIdx) {
            if (resultToAppend == null) {
                return false;
            }
            int originalCaretPosition = this.caretPosition;
            this.editor.Text = this.editor.Text.Insert(insertionIdx, resultToAppend);
            this.editor.CaretOffset = originalCaretPosition + resultToAppend.Count();
            return true;
        }

        private ExecutionEngine engine;

        internal void SetExecutionEngine(ExecutionEngine executionEngine) {
            this.engine = executionEngine;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) {
            var eh = PropertyChanged;
            if (eh != null) {
                eh(this, new PropertyChangedEventArgs(name));
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            if (!File.Exists(this.Filepath)) {
                this.saveAs();
            } else {
                File.WriteAllText(this.Filepath, this.editor.Text);
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e) {
            saveAs();
        }

        private void saveAs() {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.ShowDialog();
            this.Filepath = sfd.FileName;
            File.WriteAllText(this.Filepath, this.editor.Text);
            Properties.Settings.Default.LastFile = this.Filepath;
            Properties.Settings.Default.Save();
        }

        private void Open_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            open(ofd.FileName);
        }

        private void open(string filename) {
            this.Filepath = filename;
            this.editor.Text = File.ReadAllText(this.Filepath);
            Properties.Settings.Default.LastFile = this.Filepath;
            Properties.Settings.Default.Save();
        }

        private string _Filepath = "Untitled";
        public string Filepath {
            get { return _Filepath; }
            set {
                if (_Filepath != value) {
                    _Filepath = value;
                    OnPropertyChanged("Filepath");
                }
            }
        }

        private void New_Click(object sender, RoutedEventArgs e) {
            this.Filepath = "Untiled";
            this.editor.Text = "";
        }
    }
}
