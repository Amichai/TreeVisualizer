using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TreeLib {
    public static class NodeFunctions {
        public static Dictionary<string, TypeSettings> KnownTypes = new Dictionary<string, TypeSettings>();

        private static ExecutionEngine.ExecutionEngine engine = new ExecutionEngine.ExecutionEngine();

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
    }
}
