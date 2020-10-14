using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace forum_authentication.Controllers
{
    [ApiController]

    [Route("[controller]")]
    public class LogInController : ControllerBase
    {
        private readonly ILogger<LogInController> _logger;

        public LogInController(ILogger<LogInController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Authenticate()
        {
            return Ok;
        }
    }
}
