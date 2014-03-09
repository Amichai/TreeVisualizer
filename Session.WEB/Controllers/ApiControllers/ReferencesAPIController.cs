using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Session.WEB.Controllers.ApiControllers
{
    public class ReferencesAPIController : ApiController
    {
        [HttpGet]
        public List<string> GetReferences() {
            var session = HomeAPIController.session;
            return session.ImportedRefs;
        }

        [HttpGet]
        public List<string> GetNamespaces() {
            var session = HomeAPIController.session;
            return session.ImportedNamespaces;
        }

        [HttpPost]
        public void ImportNamespace(string toImport) {

        }
    }
}
