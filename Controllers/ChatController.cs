using Microsoft.AspNetCore.Mvc;

namespace Projet_Final.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View("SupportChat");
        }
    }
}
