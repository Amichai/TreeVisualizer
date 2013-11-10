using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml.Linq;
//using System.Threading.Tasks;

namespace TreeLib {
    public class TypeSettings {
        private static XElement xml = XElement.Load(@"..\..\DataSet.xml");

        public TypeSettings(string type) {
            this.TypeName = type;
            var visualization = xml.Element("Visualizations").Elements("Visualization").Where(i => i.Attribute("Type").Value == type).Single();
            this.ToStringFunction = visualization.Attribute("AsString").Value;
            this.ToUIElement = visualization.Attribute("ToUIElement").Value;
        }
        public string TypeName { get; set; }
        public string ToStringFunction { get; set; }
        public string ToUIElement { get; set; }
    }
}
