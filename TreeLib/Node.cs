using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TreeLib
{
    public class Node {
        public string Type { get; set; }
        public object Value { get; set; }
        public List<Node> Children { get; set; }
        public int MaxChildren { get; set; }
        public int MinChildren { get; set; }
        public Node(string type, object value) {
            this.Children = new List<Node>();
            this.Type = type;
            this.Value = value;
            this.MinChildren = 0;
            this.MaxChildren = int.MaxValue;
        }

        public string AsString {
            get {
                return this.NodeToString(1);
            }
        }

        public XElement ToXml() {
            var node = new XElement("Node");
            node.Add(new XAttribute("Type", this.Type));
            node.Add(new XAttribute("Value", this.Value));
            node.Add(new XAttribute("MaxChildren", this.MaxChildren));
            node.Add(new XAttribute("MinChildren", this.MinChildren));
            foreach (var n in this.Children) {
                node.Add(n.ToXml());
            }
            return node;
        }

        public static Node FromString(string xml) {
            return FromXml(XElement.Parse(xml));

        }

        public static Node FromXml(XElement xml) {
            var type = xml.Attribute("Type").Value;
            var value = xml.Attribute("Value").Value;
            Node node = new Node(type, value);
            var maxChildren = xml.Attribute("MaxChildren");
            if (maxChildren != null) {
                node.MaxChildren = int.Parse(maxChildren.Value);
            }
            var minChildren = xml.Attribute("MinChildren");
            if (minChildren != null) {
                node.MinChildren = int.Parse(minChildren.Value);
            }
            foreach (var n in xml.Elements("Node")) {
                node.Add(Node.FromXml(n));
            }
            return node;
        }

        public void Add(Node n) {
            this.Children.Add(n);
        }

        //public Func<string> ToString = null;
        //public Func<object> Eval = null;
    }
}
