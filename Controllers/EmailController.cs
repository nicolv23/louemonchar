using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Projet_Final.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailSender _emailService;
        private readonly ILogger<EmailController> _logger;
        public EmailController(IEmailSender emailService,
             ILogger<EmailController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            await _emailService.SendEmailAsync("nicolaslv0018@gmail.com", "Test d'envoi de courriel", "Lien vers LoueMonChar");
            return Ok();
        }
    }
}
