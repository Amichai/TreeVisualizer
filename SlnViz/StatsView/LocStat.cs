using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlnViz {
    public class LocStat : List<CodeStat> {
        
        public LocStat(SyntaxNode.NodeType nodeType, List<SyntaxNode> nodes) {
            int instanceCount = 0;
            double lineCount = 0;
            foreach (var n in nodes) {
                int nodeLineCount;
                instanceCount += n.CountSelfAndChildren(nodeType, out nodeLineCount);
                lineCount += nodeLineCount;
            }

            var name1 = nodeType.ToString() + " count";
            this.Add(new CodeStat(name1, instanceCount));

            var name2 = nodeType.ToString() + " average length";
            this.Add(new CodeStat(name2, Math.Round(lineCount / instanceCount, 2)));

        }
    }
}
