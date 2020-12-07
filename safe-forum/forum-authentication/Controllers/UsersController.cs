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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace forum_authentication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly ICertificateService _certificateService;

        public UsersController(ILogger<UsersController> logger, IUserService userService,
            IMapper mapper, IOptions<AppSettings> appSettings, ICertificateService certificateService)
        {
            _logger = logger;
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
            _certificateService = certificateService;
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
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            //TODO: Ciasteczko z Http only

            // return basic user info (without password) and token to store client side
            return Ok(new
            {
                Id = user.Id,
                Username = user.Username,
                Token = tokenString
            });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody]UserDto userDto)
        {
            // map dto to entity
            var user = _mapper.Map<User>(userDto);

            try
            {
                // save 
                _userService.Create(user, userDto.Password);
                return Ok();
            }
            catch (ApplicationException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [JwtAuthorize]
        [HttpPost("certificate")]
        [HttpPost]
        public IActionResult SignNewCertificate([FromBody]CsrDto csrDto)
        {
            var user = HttpContext.Items["User"] as User;
            string certificate = string.Empty;
            try
            {
                certificate = _certificateService.SignCsr(csrDto.Csr, user.Username);
            }
            catch(ApplicationException e)
            {
                BadRequest(e.Message);
            }

            try
            {
                _userService.UpdateUserCertificate(user.Username, certificate);
            }
            catch (ApplicationException e)
            {
                BadRequest(e.Message);
            }
            
            return Ok(certificate);
        }

        [JwtAuthorize]
        [HttpPost("certificate")]
        [HttpGet]
        public IActionResult GetUserCertificate([FromQuery]string username)
        {
            string certificate = null;
            try
            {
                _userService.GetUserCertificate(username);
            }
            catch (ApplicationException e)
            {
                BadRequest(e.Message);
            }
            return Ok(new CertificateDto() { Certificate = certificate } );
        }

        [JwtAuthorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_userService.GetAllUsernames());
        }

        //[JwtAuthorize]
        //[HttpGet("{id}")]
        //public IActionResult GetById(int id)
        //{
        //    var user = _userService.GetById(id);
        //    var userDto = _mapper.Map<UserDto>(user);
        //    return Ok(userDto);
        //}

        //[JwtAuthorize]
        //[HttpPut("{id}")]
        //public IActionResult Update(int id, [FromBody]UserDto userDto)
        //{
        //    // map dto to entity and set id
        //    var user = _mapper.Map<User>(userDto);
        //    user.Id = id;

        //    try
        //    {
        //        // save 
        //        _userService.Update(user, userDto.Password);
        //        return Ok();
        //    }
        //    catch (ApplicationException ex)
        //    {
        //        // return error message if there was an exception
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}

        //[JwtAuthorize]
        //[HttpDelete("{id}")]
        //public IActionResult Delete(int id)
        //{
        //    _userService.Delete(id);
        //    return Ok();
        //}
    }
}
