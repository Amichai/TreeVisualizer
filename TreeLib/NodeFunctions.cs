using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace TreeLib {
    public static class NodeFunctions {
        public static Dictionary<string, TypeSettings> KnownTypes = new Dictionary<string, TypeSettings>();
        public static List<XamlControl> XamlControls = new List<XamlControl>();
        public static List<Function> Functions = new List<Function>();

        private static ExecutionEngine.ExecutionEngine engine = new ExecutionEngine.ExecutionEngine();

        public static void AddXamlControl(XamlControl control) {
            ExecuteXamlControl(control);
            XamlControls.Add(control);
        }

        public static void AddFunction(Function p) {
            ExecuteFunction(p);
            Functions.Add(p);
        }

        public static void ExecuteFunction(Function p) {
            var toExecute = p.Val;
            engine.Execute(toExecute);
            p.Valid = true;
        }

        public static void ExecuteXamlControl(XamlControl control) {
            var toExecute = control.ToExecute();
            engine.Execute(toExecute);
            control.Valid = true;
        }

        public static string NodeToString(this Node node, int depth) {
            var type = node.Type;
            string toExecute1 = "var node = Node.FromString( @\"" + node.ToXml().ToString().Replace("\"", "\"\"") + "\");";

            var result1 = engine.Execute(toExecute1);

            string toExecute2 = "Func<Node, string> f = " + KnownTypes[type].ToStringFunction + ";";
            var result2 = engine.Execute(toExecute2);

            string toExecute3 = "f(node)";
            var result3 = engine.Execute(toExecute3);
            return result3 as string;
            //return string.Format("Type: {0}, value: {1}", node.Type, node.Value.ToString());
        }
        public static UIElement ToUIElement(this Node node, int depth = -1) {
            var type = node.Type;
            string toExecute1 = "var node = Node.FromString( @\"" + node.ToXml().ToString().Replace("\"", "\"\"") + "\");";

            var result1 = engine.Execute(toExecute1);

            string toExecute2 = "Func<Node, UIElement> f2 = " + KnownTypes[type].ToUIElement + ";";
            var result2 = engine.Execute(toExecute2);

            string toExecute3 = "f2(node)";
            var result3 = engine.Execute(toExecute3);

            if (!(result3 is UIElement)) {

            }
            return result3 as UIElement;
        }
        public static object Eval(this Node node, int depth) {
            var type = node.Type;
            return node.Value;
        }

        public static void RefreshLibraries() {
            foreach (var c in NodeFunctions.XamlControls.Where(i => !i.Valid)) {
                ExecuteXamlControl(c);
            }

            foreach (var f in NodeFunctions.Functions.Where(i => !i.Valid)) {
                ExecuteFunction(f);
            }
        }

        public static void LoadLibraries(XElement xml) {
            KnownTypes.Clear();
            var visualizations = xml.Element("Visualizations");
            foreach (var vis in visualizations.Elements("Visualization")) {
                KnownTypes.Add(vis.Attribute("Type").Value, TypeSettings.FromXml(vis));
            }

            XamlControls.Clear();
            var controls = xml.Element("XamlControls");
            if (controls != null) {
                foreach (var con in controls.Elements("XamlControl")) {
                    AddXamlControl(XamlControl.FromXml(con));
                }
            }

            Functions.Clear();
            var functions = xml.Element("Functions");
            if (functions != null) {
                foreach (var f in functions.Elements("Function")) {
                    AddFunction(Function.FromXml(f));
                }
            }
        }
    }
}
