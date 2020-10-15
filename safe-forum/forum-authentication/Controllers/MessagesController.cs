using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using forum_authentication.Dtos;
using forum_authentication.Entities;
using forum_authentication.Helpers;
using forum_authentication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace forum_authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private IMessageService _messageService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public MessagesController(ILogger<UsersController> logger, IMessageService messageService,
                                  IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _messageService = messageService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }


        [JwtAuthorize]
        [HttpPost("send")]
        public IActionResult Send([FromBody]SendMessageDto sendMessageDto)
        {
            var user = HttpContext.Items["User"] as User;
            if (user == null || string.IsNullOrEmpty(sendMessageDto.Body) || string.IsNullOrEmpty(sendMessageDto.Recipent))
            {
                return BadRequest();
            }
            try
            {
                _messageService.SaveMessage(sendMessageDto, user.Username);
            }
            catch (ApplicationException e)
            {
                return NotFound();
            }
            return Ok();
        }

        [JwtAuthorize]
        [HttpGet]
        public IActionResult Messages([FromQuery] string from)
        {
            var user = HttpContext.Items["User"] as User;
            Message[] messages = null;
            try
            {
                messages = _messageService.GetMessages(from, user.Username);
            }
            catch(ApplicationException e)
            {
                return NotFound();
            }
            return Ok(messages);
        }


    }
}