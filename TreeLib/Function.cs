using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TreeLib {
    public class Function {
        public Function() {
            this.Val = "";
            this.Valid = false;
        }
        public string Val { get; set; }
        public bool Valid { get; set; }

        public XElement ToXml() {
            var f = new XElement("Function");
            f.Add(new XAttribute("Val", this.Val));
            return f;
        }

        public static Function FromXml(XElement xml) {
            Function f = new Function();
            f.Val = xml.Attribute("Val").Value;
            return f;
        }
    }
}
