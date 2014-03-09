using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Session.WEB.Controllers {
    public class HomeController : Controller {
        public ActionResult Index() {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            var type = "";
                        var code = @"using ILNumerics;
ILMath.rand(5,4)

using MathNet.Numerics.LinearAlgebra.Double;
var matrixA = DenseMatrix.OfArray(new[,] { { 1.0, 2.0, 3.0 }, { 4.0, 5.0, 6.0 }, { 7.0, 8.0, 9.0 } });
var matrixB = DenseMatrix.OfArray(new[,] { { 1.0, 3.0, 5.0 }, { 2.0, 4.0, 6.0 }, { 3.0, 5.0, 7.0 } });

matrixA.Transpose() * matrixB

using MathNet.Numerics.Integration;
Integrate.OnClosedInterval(x => Math.Exp(-x / 5) * (2 + Math.Sin(2 * x)), 0, 100)";
            return View("../Home/Index", new Tuple<string, string>(code, type));
        }

        public ActionResult About() {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact() {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
