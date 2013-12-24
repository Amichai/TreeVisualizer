using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;

namespace SlnViz {
    public class ResultLogString : INotifyPropertyChanged {
        public ResultLogString(string text, string eval) {
            this.Text = text;
            this.TargetResult = eval;
            this.Created = DateTime.Now;
            this.Results.Add(this.Created, eval);
        }

        public ResultLogString(string text) {
            this.Text = text;
        }

        public string Text { get; private set; }
        public DateTime Created { get; private set; }

        Dictionary<DateTime, string> Results = new Dictionary<DateTime, string>();

        public DateTime LastEval {
            get {
                return Results.Last().Key;
            }
        }

        public bool Passed {
            get {
                return LatestResult == TargetResult;
            }
        }

        private string _TargetResult;
        public string TargetResult {
            get { return _TargetResult; }
            set {
                if (_TargetResult != value) {
                    _TargetResult = value;
                    OnPropertyChanged("TargetResult");
                    OnPropertyChanged("Passed");
                    OnPropertyChanged("BackgroundColor");
                }
            }
        }

        public Brush BackgroundColor {
            get {
                if (this.Passed) {
                    return Brushes.Green;
                } else {
                    return Brushes.Red;
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


        public string Comments { get; set; }
        public string LatestResult {
            get {
                return Results.Last().Value;
            }
        }

        public XElement ToXml() {
            XElement root = new XElement("ResultLog");
            root.Add(new XAttribute("Text", this.Text));
            root.Add(new XAttribute("Created", this.Created));
            root.Add(new XAttribute("TargetResult", this.TargetResult));
            root.Add(new XAttribute("Comments", this.Comments ?? ""));

            XElement pastResults = new XElement("PastResults");
            foreach (var a in this.Results) {
                XElement result = new XElement("Result");
                result.Add(new XAttribute("Time", a.Key));
                result.Add(new XAttribute("Value", a.Value));
                pastResults.Add(result);
            }
            root.Add(pastResults);
            return root;
        }

        internal static ResultLogString Deserialize(XElement r) {
            var text = r.Attribute("Text").Value;
            ResultLogString resultLog = new ResultLogString(text);
            resultLog.Created = DateTime.Parse(r.Attribute("Created").Value);
            resultLog.TargetResult = r.Attribute("TargetResult").Value;
            resultLog.Comments = r.Attribute("Comments").Value;
            Dictionary<DateTime, string> results = new Dictionary<DateTime, string>();
            foreach (var a in r.Element("PastResults").Elements("Result")) {
                var time = DateTime.Parse(a.Attribute("Time").Value);
                var value = a.Attribute("Value").Value;
                results[time] = value;
            }
            resultLog.Results = results;

            return resultLog;
        }

        public int TestsRun {
            get {
                return this.Results.Count;
            }
        }

        internal void Test(string eval) {
            this.Results.Add(DateTime.Now, eval);
            OnPropertyChanged("TestsRun");
        }
    }
}
