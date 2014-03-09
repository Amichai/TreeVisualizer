using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Session.WEB.Controllers
{
    public class OutputController : Controller
    {
        //
        // GET: /Output/

        public ActionResult Index(string type)
        {
            var db = new Data.CodeBaseEntities();
            var matches = db.PresentationLogics.Where(i => i.Type == type);
            string code;
            if (matches.Count() == 0) {
                code = "";
            } else {
                code = matches.First().Presentation;
            }
            return View("../Home/Index", new Tuple<string, string>(code, type));
        }

    }
}
