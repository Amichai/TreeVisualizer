using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlnViz {
    public class CodeStat {
        public CodeStat(string key) {
            this.Key = key;
        }
        public CodeStat(string key, object value) {
            this.Key = key;
            this.Value = value;
        }
        public string Key { get; set; }
        public object Value { get; set; }
    }
}
