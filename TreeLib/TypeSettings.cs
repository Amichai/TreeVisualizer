using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml.Linq;
//using System.Threading.Tasks;

namespace TreeLib {
    public class TypeSettings {
        public TypeSettings() {

        }
        //public TypeSettings(string type, XElement xml) {
        //    this.TypeName = type;
        //    var visualization = xml.Element("Visualizations").Elements("Visualization")
        //        .Where(i => i.Attribute("Type").Value == type).Single();
        //    this.ToStringFunction = visualization.Attribute("AsString").Value;
        //    this.ToUIElement = visualization.Attribute("ToUIElement").Value;
        //}
        public string TypeName { get; set; }
        public string ToStringFunction { get; set; }
        
        public string ToUIElement { get; set; }
        public XElement ToXml() {
            var vis = new XElement("Visualization");
            vis.Add(new XAttribute("Type", this.TypeName));
            vis.Add(new XAttribute("AsString", this.ToStringFunction));
            vis.Add(new XAttribute("ToUIElement", this.ToUIElement));
            return vis;
        }

        public static TypeSettings FromXml(XElement xml) {
            TypeSettings toReturn = new TypeSettings();
            toReturn.TypeName = xml.Attribute("Type").Value;
            //var visualization = xml.Element("Visualizations").Elements("Visualization").Where(i => i.Attribute("Type").Value == type).Single();
            toReturn.ToStringFunction = xml.Attribute("AsString").Value;
            toReturn.ToUIElement = xml.Attribute("ToUIElement").Value;
            return toReturn;
        }
    }
}
