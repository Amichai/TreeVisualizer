using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Session.WEB.Controllers.ApiControllers {
    public class HomeAPIController : ApiController {
        public IEnumerable<string> Get() {
            return new string[] { "value1", "value2" };
        }

        public string Get(int id) {
            return "value";
        }

        private static RoslynSession session = new RoslynSession();

        [HttpPost]
        public object Execute() {
            var r = Request.Content.ReadAsStreamAsync().Result;
            StreamReader sr = new StreamReader(r);
            var toExecute = sr.ReadToEnd();

            bool exceptionThrown;
            var result = session.AppendCSharp(toExecute, 0, out exceptionThrown);
            return handleResult(result);
        }

        private string handleResult(object result) {
            return result.ToString();
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
