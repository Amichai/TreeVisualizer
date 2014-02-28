using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlnViz {
    public static class Util {
        public static string TypeInfo(this Type t) {
            StringBuilder sb = new StringBuilder();
            if (t.BaseType != null) {
                sb.AppendLine(string.Format("Base Type: {0}", t.BaseType.Name));
            }
            sb.AppendLine("PROPERTIES:");
            foreach (var p in t.GetProperties()) {
                sb.AppendLine(p.GetSignature());
            }

            sb.AppendLine("METHODS:");


            foreach (var p in t.GetMethods()) {
                
                sb.AppendLine(p.GetSignature());
            }
            return sb.ToString();
        }
    }
}
