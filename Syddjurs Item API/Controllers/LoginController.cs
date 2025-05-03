using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Syddjurs_Item_API.Data;
using Syddjurs_Item_API.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Syddjurs_Item_API.Controllers
{


    [ApiController]
    [Route("[controller]")]
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoginController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDbContext context, IConfiguration configuration)
        {
            _context = context;
             _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto )
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);
                        // await _userManager.FindByEmailAsync(loginDto.Username);

           

            if (user == null)
            {
                return Unauthorized("Ugyldig brugernavn eller kodeord");
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);

            if (!signInResult.Succeeded)
            {
                return Unauthorized("Invalid username or password");
            }

            var token = await GenerateJwtToken(user);

            return Ok(new { Token = token });
        }

      

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                 var errors = result.Errors.Select(e => e.Description).ToList();
            
                return BadRequest(new { Errors = errors }); // ✅ Proper 400 Bad Request
            }

            // Optional: Assign default role
            // await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { Message = "User registered successfully" });
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {

            var listclaim = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                 new Claim(ClaimTypes.Email, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            var currentUserRoles = await _userManager.GetRolesAsync(user);
            foreach (var currentUserRole in currentUserRoles)
            {
                var claim = new Claim(ClaimTypes.Role, currentUserRole);
                listclaim.Add(claim);
            }

            var claims = listclaim.ToArray();

            //var claims = new[]
            //{
            //  new Claim(ClaimTypes.Name, user.UserName),
            //     new Claim(ClaimTypes.Email, user.UserName),
            //    new Claim(ClaimTypes.NameIdentifier, user.Id),
            //    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }     
   }
}

