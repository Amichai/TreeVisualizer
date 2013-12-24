using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlnViz {
    public class ResultLogString {
        public ResultLogString(string text, string eval) {
            this.Text = text;
            this.TargetResult = eval;
            this.Created = DateTime.Now;
            this.Results.Add(this.Created, eval);

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

        public string TargetResult { get; set; }
        public string Comments { get; set; }
        public string LatestResult {
            get {
                return Results.Last().Value;
            }
        }
    }
}
