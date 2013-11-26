using Roslyn.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace slnAnalyzer {
    class Program {
        static void Main(string[] args) {
            string path = @"..\..\..\TreeViz.sln";
            IWorkspace workspace = Workspace.LoadSolution(path);

            ISolution sln = workspace.CurrentSolution;

            var proj = sln.Projects.First();
            var comp = proj.GetCompilation();
            
            CancellationToken token = new CancellationToken(false);
            var entry = comp.GetEntryPoint(token);
            var diag = comp.GetDiagnostics();
            var doc = proj.Documents.First();
            var tree = doc.GetSyntaxTree();
            var model = doc.GetSemanticModel();

            
            ///Point to a solution file
        }
    }
}
