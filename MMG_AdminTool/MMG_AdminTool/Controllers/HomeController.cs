using Microsoft.AspNetCore.Mvc;

namespace MMG_AdminTool.Controllers
{ 
    public class HomeController : Controller
    {
        public IActionResult Index() 
        {
            return View();
        }

        public IActionResult Test()
        {
            ViewData["MyMsg"] = "Hello response";
            //ViewBag.MyTest = new List<string> { "abc", "def", "ton" };
            //ViewBag.MyNum = 5;

            var list = new List<string> { "abc", "def", "ton" };

            return View(list);
        }
    }
}
