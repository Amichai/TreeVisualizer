using Roslyn.Scripting.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Roslyn.Scripting;
using Roslyn.Services;
using Roslyn.Compilers.CSharp;
using System.IO;
using System.Diagnostics;
using Roslyn.Compilers;
using System.Net;


namespace Session.WEB {
    public class RoslynSession {
        private Roslyn.Scripting.Session session;
        private ScriptEngine engine;

        public RoslynSession() {
            this.importedNamespaces = new List<string>();
            this.ImportedRefs = new List<string>();
            var path = @"C:\Users\Amichai\Documents\Visual Studio 2012\Projects\ComputationalPhysics\computationalPhysics.sln";
            IWorkspace workspace = Workspace.LoadSolution(path);
            ISolution sln = workspace.CurrentSolution;
            this.Init(sln);
            this.session.AddReference(typeof(WebClient).Assembly.Location);
        }

        public List<string> GetImportedNamespaces() {
            return this.importedNamespaces;
        }

        private List<string> importedNamespaces { get; set; }
        public List<string> ImportedRefs { get; set; }

        private void clearAllImports() {
            this.importedNamespaces.Clear();
            this.ImportedRefs.Clear();
        }
        private static string getFullPath(string assemblyName) {
            var fullAssemblyName = System.IO.Path.Combine(Directory.GetCurrentDirectory(), assemblyName);
            return fullAssemblyName;
        }

        private int assemblyCounter = 0;


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
        public void AddProject2(IProject proj) {
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

        private List<string> standardNamespaces = new List<string>() { "System", "System.Linq", "System.Collections", "System.Collections.Generic" };


        public void ImportNamespace(string space) {
            space = space.TrimEnd();
            if (!this.importedNamespaces.Contains(space)) {
                engine.ImportNamespace(space);
                importedNamespaces.Add(space);
                Debug.Print("space: " + space);
                session = engine.CreateSession();
            }
        }

        private void addReference(MetadataReference r) {
            string refName = r.Display.Split('\\').Last();
            if (!ImportedRefs.Contains(refName)) {
                engine.AddReference(r);
                Debug.Print("Ref: " + r.Display);
                ImportedRefs.Add(refName);
            }
        }
        public void AddReference(string binPath) {
            addReference(new MetadataFileReference(binPath));
        }

        public void AddProject(IProject proj) {
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


        public void Init(ISolution sln) {
            this.clearAllImports();
            engine = new ScriptEngine();
            foreach (var proj in sln.Projects) {
                AddProject(proj);
            }

            foreach (var n in standardNamespaces) {
                ImportNamespace(n);
            }
            session = engine.CreateSession();
        }

        public string ExecuteToString(string input) {
            return session.Execute(input).ToString();
        }

        public object Execute(string inputText) {
            return session.Execute(inputText);
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

    }
}