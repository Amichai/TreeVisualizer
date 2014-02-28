using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for Notepad.xaml
    /// </summary>
    public partial class Notepad : UserControl {
        public Notepad() {
            InitializeComponent();
            this.pastLinesCache = new PastLinesCache();
        }

        private bool exectuablePosition(out string toExecute, out int lineNumber, out int insertionIdx) {
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

        private int getLineNumber(int caretPos) {
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
            if (!ex) {
                return (result as Type).TypeInfo();
            } else {
                return exceptionText;
            }
        }

        private void MvvmTextEditor_PreviewKeyDown_1(object sender, KeyEventArgs e) {
            var key = e.Key;
            string toAppend;
            int lineNumber;
            switch (key) {
                case Key.Enter:
                    string toExecute;
                    int insertionIdx;
                    if (this.exectuablePosition(out toExecute, out lineNumber, out insertionIdx)) {
                        bool exceptionThrown;
                        var result = this.engine.AppendCSharp(toExecute, lineNumber, out exceptionThrown).ToString();
                        this.pastLinesCache.Add(toExecute);
                        if (exceptionThrown) {
                            toAppend = "\n" + handleException(result, toExecute, lineNumber);
                        } else {
                            toAppend = "\n" + result;
                        }
                        appendToEditor(toAppend, insertionIdx);
                    }
                    break;
                case Key.Up:
                    if (!this.caretAtStartOfLine()) {
                        return;
                    }
                    toAppend = this.pastLinesCache.Backward();
                    lineNumber = this.getLineNumber(this.caretPosition);
                    this.overwriteLine(toAppend, lineNumber);
                    e.Handled = true;
                    break;
                case Key.Down:
                    if (!this.caretAtStartOfLine()) {
                        return;
                    }
                    toAppend = this.pastLinesCache.Forward();
                    lineNumber = this.getLineNumber(this.caretPosition);
                    this.overwriteLine(toAppend, lineNumber);
                    e.Handled = true;
                    break;
            }
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
    }
}
