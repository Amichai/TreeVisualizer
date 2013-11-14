using Roslyn.Compilers;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace ExecutionEngine {
    public class ExecutionEngine {
        public ExecutionEngine() {
            this.loadScriptEngine();
        }

        private static Session session;
        private static ScriptEngine engine;
        private void loadScriptEngine() {
            engine = new ScriptEngine();
            engine.AddReference(typeof(System.Linq.Enumerable).Assembly.Location);
            engine.AddReference(typeof(System.Xml.XmlElement).Assembly.Location);
              //engine.AddReference(typeof(Url).Assembly.Location);
            //engine.AddReference(typeof(JObject).Assembly.Location);
            engine.AddReference(typeof(XElement).Assembly.Location);
            engine.AddReference(typeof(UIElement).Assembly.Location);
            engine.AddReference(typeof(DependencyObject).Assembly.Location);
            engine.AddReference(typeof(TextBlock).Assembly.Location);
            engine.AddReference(typeof(System.ComponentModel.ISupportInitialize).Assembly.Location);
            engine.AddReference(typeof(System.Windows.Markup.IQueryAmbient).Assembly.Location);

            //engine.AddReference(typeof(DependencyObject).Assembly.Location);
            //engine.AddReference(typeof(FrameworkElement).Assembly.Location);
            //engine.AddReference(typeof(MethodInfo).Assembly.Location);
            //engine.AddReference(typeof(System.Windows.Application).Assembly.Location);
            //engine.AddReference(typeof(System.Windows.UIElement).Assembly.Location);
            //engine.AddReference(typeof(System.Windows.DependencyObject).Assembly.Location);


            //engine.AddReference(typeof(System.Drawing.Color).Assembly.Location);
            //engine.AddReference(typeof(ILScene).Assembly.Location);


            //engine.AddReference(typeof(FunctionLibrary).Assembly.Location);
            
            ///Untested
            //engine.AddReference(typeof(Uri).Assembly.Location);
            //engine.AddReference(typeof(XmlAttribute).Assembly.Location);

            //engine.AddReference(new MetadataFileReference(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.dll"));
            //engine.AddReference(new MetadataFileReference(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.Xml.dll"));
            //engine.AddReference(new MetadataFileReference(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.Xaml.dll"));
            var path = System.IO.Path.GetFullPath(@"..\..\..\TreeLib\bin\debug\TreeLib.dll");
            
            engine.AddReference(new MetadataFileReference(path));
            engine.ImportNamespace("System");
            //engine.ImportNamespace("System.Reflection");
            engine.ImportNamespace("System.Windows");
            engine.ImportNamespace("System.Windows.Controls");

            engine.ImportNamespace("System.Collections.Generic");
            engine.ImportNamespace("System.Linq");
            engine.ImportNamespace("System.Xml.Linq");

            engine.ImportNamespace("System.Text");
            engine.ImportNamespace("TreeLib");

            //engine.ImportNamespace("System.Diagnostics");
            //engine.ImportNamespace("Newtonsoft.Json.Linq");

            /*
             * using ILNumerics;
using ILNumerics.Drawing;
using ILNumerics.Drawing.Plotting;
             */
            session = engine.CreateSession();
            //var r = session.Execute("new Node(\"test\", \"test\")");  
        }


        public object Execute(string inputText) {
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
                return result;
            } catch (Exception ex) {
                return ex.Message + " " + ex.InnerException;
            }
        }
    }
}
