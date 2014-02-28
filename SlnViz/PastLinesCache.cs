using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlnViz {
    class PastLinesCache {
        public PastLinesCache() {
            this.Lines = new List<string>();
            this.Index = -1;
        }
        public int Index { get; private set; }
        public List<string> Lines { get; private set; }
        public string Forward() {
            if (this.Index < 0) {
                this.Index = 0;
            }
            if (this.Index == this.Lines.Count()) {
                return null;
            }
            var toReturn = this.Lines[this.Index];
            this.Index++;
            return toReturn;
        }

        public string Backward() {
            if (this.Index >= this.Lines.Count()) {
                this.Index = this.Lines.Count() - 1;
            }
            if (this.Index == -1) {
                return null;
            }
            
            var toReturn = this.Lines[this.Index];
            this.Index--;
            return toReturn;
        }

        private int indexOfLast {
            get {
                return this.Lines.Count() - 1;
            }
        }


        public void Add(string input) {
            if(string.IsNullOrWhiteSpace(input)){
                return;
            }
            if (this.Index != this.indexOfLast) {
                this.Lines = this.Lines.Take(this.Index).ToList();
            }
            this.Lines.Add(input);
            this.Index++;
        }

    }
}
