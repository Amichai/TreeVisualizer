using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace TreeLib {
    public class XamlControl {
        public XamlControl(string name, string val) {
            this.Name = name;
            this.Val = val;
            this.Valid = false;
        }
        public string Val { get; set; }
        public bool Valid { get; set; }

        private string _Name;
        public string Name {
            get { return _Name; }
            set {
                _Name = value;
            }
        }
        

        public XElement ToXml() {
            var con = new XElement("XamlControl");
            con.Add(new XAttribute("Name", this.Name));
            con.Add(new XAttribute("Val", this.Val));
            return con;
        }

        public string ToExecute() {
            string toReturn = "";
            toReturn += "UIElement " + this.Name + "() {";

            toReturn += "return (UIElement)System.Windows.Markup.XamlReader.Parse(@\"";
            toReturn += this.Val.Replace("\"", "\"\"");
            toReturn += "\"); }";

            return toReturn;
        }

        public static XamlControl FromXml(XElement con) {
            var name = con.Attribute("Name").Value;
            var val = con.Attribute("Val").Value;
            return new XamlControl(name, val);
        }
    }
}
