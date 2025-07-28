using Microsoft.AspNetCore.Mvc;

namespace MMG_AdminTool.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
