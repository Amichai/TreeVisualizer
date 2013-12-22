using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlnViz {
    public class ResultLogString {
        public ResultLogString(string text) {
            this.Text = text;
            this.Created = DateTime.Now;
        }
        public string Text { get; private set; }
        public DateTime Created { get; private set; }

        Dictionary<DateTime, string> Results = new Dictionary<DateTime, string>();

        public string TargetResult { get; set; }
        public string Comments { get; set; }
    }
}
