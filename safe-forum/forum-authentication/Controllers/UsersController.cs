using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using forum_authentication.Dtos;
using forum_authentication.Entities;
using forum_authentication.Helpers;
using forum_authentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace forum_authentication.Controllers
{
    [ApiController]
    [Route("api")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public UsersController(ILogger<UsersController> logger, IUserService userService,
            IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]UserDto userDto)
        {
            var user = _userService.Authenticate(userDto.Username, userDto.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            HttpContext.Response.Cookies.Append("token", tokenString,
            new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(30),
                Secure = true
            });

            // return basic user info (without password) and token to store client side
            return Ok(new
            {
                //user.Id,
                user.Username
            });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody]UserDto userDto)
        {
            if( string.IsNullOrEmpty(userDto.Username) 
                || string.IsNullOrEmpty(userDto.Password) 
                || string.IsNullOrEmpty(userDto.Certificate))
            {
                return BadRequest("Missing registration data)");
            }

            try
            {
                // save 
                _userService.Create(userDto);
                return Ok();
            }
            catch (ApplicationException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [JwtAuthorize]
        [HttpGet("certificate")]
        public IActionResult GetUserCertificate([FromQuery]string username)
        {
            string certificate = null;
            try
            {
                certificate = _userService.GetUserCertificate(username);
            }
            catch (ApplicationException e)
            {
                BadRequest(e.Message);
            }
            return Ok(new CertificateDto() { Certificate = certificate } );
        }
    }
}
