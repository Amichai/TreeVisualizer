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
using SessionHelper;

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
            var j = new JObject();

            //if (result is IGenerateHtml) {
            //    j["htmlresult"] = (result as IGenerateHtml).Html();
            //    return j;
            //}
            var resultString = "";
            if (exceptionThrown) {
                resultString = handleException(result.ToString(), toExecute, lineNumber);
            } else {
                int recursionCounter = 0;
                resultString = displayResult(result, session, ref recursionCounter);
            }
            j["htmlresult"] = resultString;
            j["textResult"] = exceptionThrown;
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
                //return  encodeString(result as string);
                return result as string;
            } else if (result is IEnumerable) {
                string enumerableResult = "";
                foreach (var i in (result as IEnumerable)) {
                    recursionCounter = 0;
                    enumerableResult += displayResult(i, session, ref recursionCounter, db) + ",";
                }
                return enumerableResult;
            } 
            var asString = result.ToString();
            return encodeString(asString);
        }

        private string encodeString(string s) {
            if (s == null) {
                return "<i>null</i>";
            }
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
