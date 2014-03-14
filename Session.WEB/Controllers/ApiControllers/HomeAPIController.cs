 using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Session.WEB.Models;
using System.Collections;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Session.WEB.Controllers.ApiControllers {
    public class HomeAPIController : ApiController {
        public IEnumerable<string> Get() {
            return new string[] { "value1", "value2" };
        }

        public string Get(int id) {
            return "value";
        }

        public static RoslynSession session = new RoslynSession();

        [HttpPost]
        public bool Save(string type) {
            var db = new Data.CodeBaseEntities();
            var matches = db.PresentationLogics.Where(i => i.Type == type);
            Data.PresentationLogic newLogic;
            if (matches.Count() == 0) {
                newLogic = new Data.PresentationLogic() { Type = type };
                newLogic.Presentation = postBody();
                db.PresentationLogics.Add(newLogic);
                db.SaveChanges();
            } else {
                newLogic = matches.First();
                newLogic.Presentation = postBody();
                db.SaveChanges();
            }
            return true;
        }

        [HttpPost]
        public bool ResetSession() {
            try {
                session = new RoslynSession();
                return true;
            } catch (Exception ex) {
                return false;
            }
        }

        [HttpPost]
        public JObject Execute(int lineNumber) {
            var toExecute = postBody();
            bool exceptionThrown;
            var result = session.AppendCSharp(toExecute, lineNumber, out exceptionThrown);
            var resultString = "";
            if (exceptionThrown) {
                resultString = handleException(result.ToString(), toExecute, lineNumber);
            } else {
                int recursionCounter = 0;
                resultString = displayResult(result, session, ref recursionCounter);
            }
            var j = new JObject();
            j["htmlresult"] = resultString;
            j["exceptionThrown"] = exceptionThrown;
            j["javascript"] = @"
var margin = {top: 10, right: 10, bottom: 100, left: 40},
    margin2 = {top: 430, right: 10, bottom: 20, left: 40},
    width = 960 - margin.left - margin.right,
    height = 500 - margin.top - margin.bottom,
    height2 = 500 - margin2.top - margin2.bottom;

var data = [{
    'Wed Jan 23 00:00:00 IST 2013': 3383387
}, {
    'Thu Jan 24 00:00:00 IST 2013': 3883387
}, {
    'Fri Jan 25 00:00:00 IST 2013': 4383387
}, {
    'Sat Jan 26 00:00:00 IST 2013': 2383387
}, {
    'Sun Jan 27 00:00:00 IST 2013': 5383387
}, {
    'Mon Jan 28 00:00:00 IST 2013': 2283387
}];

var format = d3.time.format(""%a %b %d %H:%M:%S IST %Y"");

var parseDate = d3.time.format(""%b %Y"").parse;

var x = d3.time.scale().range([0, width]),
    x2 = d3.time.scale().range([0, width]),
    y = d3.scale.linear().range([height, 0]),
    y2 = d3.scale.linear().range([height2, 0]);

var xAxis = d3.svg.axis().scale(x).orient(""bottom""),
    xAxis2 = d3.svg.axis().scale(x2).orient(""bottom""),
    yAxis = d3.svg.axis().scale(y).orient(""left"");

var brush = d3.svg.brush()
    .x(x2)
    .on(""brush"", brush);

var area = d3.svg.area()
    .interpolate(""monotone"")
    .x(function(d) { return x(format.parse(d3.keys(d)[0])); })
    .y0(height)
    .y1(function(d) { return y(d3.values(d)[0]); });

var area2 = d3.svg.area()
    .interpolate(""monotone"")
    .x(function(d) { return x2(format.parse(d3.keys(d)[0])); })
    .y0(height2)
    .y1(function(d) { return y2(d3.values(d)[0]); });

var svg = d3.select(""body"").append(""svg"")
    .attr(""width"", width + margin.left + margin.right)
    .attr(""height"", height + margin.top + margin.bottom);

svg.append(""defs"").append(""clipPath"")
    .attr(""id"", ""clip"")
  .append(""rect"")
    .attr(""width"", width)
    .attr(""height"", height);

var focus = svg.append(""g"")
    .attr(""transform"", ""translate("" + margin.left + "","" + margin.top + "")"");

var context = svg.append(""g"")
    .attr(""transform"", ""translate("" + margin2.left + "","" + margin2.top + "")"");

  x.domain(d3.extent(data.map(function(d) { return format.parse(d3.keys(d)[0]); })));
  y.domain([0, d3.max(data.map(function(d) { return d3.values(d)[0]; }))]);
  x2.domain(x.domain());
  y2.domain(y.domain());

  focus.append(""path"")
      .datum(data)
      .attr(""clip-path"", ""url(#clip)"")
      .attr(""d"", area)
  .attr(""style"", ""fill:steelblue;"");

  focus.append(""g"")
      .attr(""class"", ""x axis"")
      .attr(""transform"", ""translate(0,"" + height + "")"")
      .call(xAxis);

  focus.append(""g"")
      .attr(""class"", ""y axis"")
      .call(yAxis);

  context.append(""path"")
      .datum(data)
      .attr(""d"", area2).attr(""style"", ""fill:steelblue;"");

  context.append(""g"")
      .attr(""class"", ""x axis"")
      .attr(""transform"", ""translate(0,"" + height2 + "")"")
      .call(xAxis2);

  context.append(""g"")
      .attr(""class"", ""x brush"")
  .attr(""style"", ""fill-opacity:.125;shape-rendering:crispEdges"")
      .call(brush)
    .selectAll(""rect"")
      .attr(""y"", -6)
      .attr(""height"", height2 + 7);
      
    var circlegroup = focus.append(""g"");
    circlegroup.attr(""clip-path"", ""url(#clip)"");
    circlegroup.selectAll('.dot')
    .data(data)
    .enter().append(""circle"")
    .attr('class', 'dot')
    .attr(""cx"",function(d){ return x(format.parse(d3.keys(d)[0]));})
    .attr(""cy"", function(d){ return y(d3.values(d)[0]);})
    .attr(""r"", function(d){ return 4;})
    .on('mouseover', function(d){ d3.select(this).attr('r', 8)})
    .on('mouseout', function(d){ d3.select(this).attr('r', 4)});    

function brush() {
  x.domain(brush.empty() ? x2.domain() : brush.extent());
    focus.select(""path"").attr(""d"", area);
  focus.select("".x.axis"").call(xAxis);
  circlegroup.selectAll("".dot"").attr(""cx"",function(d){ return x(format.parse(d3.keys(d)[0]));}).attr(""cy"", function(d){ return y(d3.values(d)[0]);});
} svg"

                ;
            return j;
        }

        private string postBody() {
            var r = Request.Content.ReadAsStreamAsync().Result;
            StreamReader sr = new StreamReader(r);
            var toExecute = sr.ReadToEnd();
            return toExecute;
        }

        private string handleException(string exceptionText, string input, int lineNumber) {
            ///Try to get type information from the object
            bool ex;
            var result = session.AppendCSharp("typeof(" + input + ")", lineNumber, out ex);
            if (!ex) {
                return (result as Type).TypeInfo();
            } else {
                return exceptionText;
            }
        }

        private string displayResult(object result, RoslynSession session,
            ref int recursionCounter,
            Data.CodeBaseEntities db = null) {
                recursionCounter++;
                if (recursionCounter > 5) {
                    return "";
                }
            if(db == null){
                db = new Data.CodeBaseEntities();
            }
            var typeStringFull = result.GetType().FullName;
            var custom = customResult(session, ref recursionCounter, db, typeStringFull);
            if (custom != null) {
                return custom;
            }

            var typeStringShort = result.GetType().Name;
            custom = customResult(session, ref recursionCounter, db, typeStringShort);
            if (custom != null) {
                return custom;
            }
            
            if (result is string) {
                return  encodeString(result as string);
            } else if (result is IEnumerable) {
                string enumerableResult = "";
                foreach (var i in (result as IEnumerable)) {
                    enumerableResult += displayResult(i, session, ref recursionCounter, db) + ",";
                }
                return enumerableResult;
            } 
            var asString = result.ToString();
            return encodeString(asString);
        }

        private string encodeString(string s) {
            return s.Replace("\n", "<br />").Replace("\r", "");
        }

        private string customResult(RoslynSession session,  ref int recursionCounter, Data.CodeBaseEntities db, string typeStringFull) {
            var t = db.PresentationLogics.Where(i => i.Type == typeStringFull).FirstOrDefault();
            string toReturn = null;
            if (t != null && !string.IsNullOrWhiteSpace(t.Presentation)) {
                toReturn = displayResult(session.Execute(t.Presentation), session, ref recursionCounter, db);
            }
            return toReturn;
        }

        public void Post([FromBody]string value) {
        }

        // PUT api/homeapi/put?id=5
        public void Put(int id, [FromBody]string value) {
        }

        // DELETE api/homeapi/delete?id=5
        public void Delete(int id) {
        }
    }
}
