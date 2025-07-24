using Microsoft.AspNetCore.Mvc;

namespace MMG_AdminTool.Controllers
{ 
    public class HomeController : Controller
    {
        public string Index() 
        {
            string? userId = Request.Query["user_id"];
            string? age = Request.Query["age"];

            return "응답입니다. " + userId + " " + age;
        }

        public IActionResult Test()
        {
            return View();
        }
    }
}
