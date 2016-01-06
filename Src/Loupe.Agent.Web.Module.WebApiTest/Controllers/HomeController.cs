using System.Web.Mvc;

namespace Loupe.Agent.Web.Module.MVCTest.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.SessionId = ControllerContext.HttpContext.Items["LoupeSessionId"].ToString();

            return View();
        }
    }
}