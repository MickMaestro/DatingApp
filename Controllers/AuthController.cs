using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc;
using DatingApp.API.Models;
using DatingApp.API.Dtos;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _configy;
        public AuthController(IAuthRepository repo, IConfiguration configy)
        {
            _configy = configy;
            _repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            //validate request
            userForRegisterDto.Username = userForRegisterDto.Username.ToUpper();
            if (await _repo.UserExists(userForRegisterDto.Username))
            {
                return BadRequest("Username exists. Sorry!");
            }

            var userToCreate = new User
            {
                Username = userForRegisterDto.Username
            };

            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            
            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToUpper(), userForLoginDto.Password);
            if (userFromRepo == null)
            {
                return Unauthorized();
            }

            //token will contain 2 claims
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            //key to sign token
            var key = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(_configy.GetSection("AppSettings:Token").Value));

            var credsy = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credsy
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokeny = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {
                tokeny = tokenHandler.WriteToken(tokeny) 
            });
        }
    }
}