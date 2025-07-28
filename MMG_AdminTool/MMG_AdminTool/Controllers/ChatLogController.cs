using Microsoft.AspNetCore.Mvc;

namespace MMG_AdminTool.Controllers
{
    public class ChatLogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
