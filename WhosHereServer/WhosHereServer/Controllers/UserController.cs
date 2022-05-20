using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WhosHereServer.Data;
using WhosHereServer.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WhosHereServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IConfiguration configuration;

        public UserController(UserManager<AppUser> userManager, 
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.configuration = configuration;
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: configuration["JWT:ValidIssuer"],
                audience: configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMonths(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Login(AppUserLoginDto login)
        {
            AppUser? user = await userManager.FindByEmailAsync(login.Email);
            if (user == null)
            {
                return NotFound();
            }

            if (await userManager.CheckPasswordAsync(user, login.Password))
            {
                var userRoles = await userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GetToken(authClaims);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }

            return Unauthorized();
        }

        [Authorize]
        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Profile([EmailAddress] string email)
        {
            return Ok();
        }

        /// <summary>
        /// Create and register a new user
        /// </summary>
        /// <param name="reg"><see cref="AppUserRegistrationDto"/> object</param>
        /// <returns>Ok</returns>
        /// <remarks></remarks>
        /// <response
        /// <response code="409">Returns if registration failed. Please check response content for reason.</response>
        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Register(AppUserRegistrationDto registration)
        {
            var user = new AppUser();
            user.UserName = registration.UserName;
            user.Email = registration.Email;
            //TODO: use IUserStore and IUsereMailStore
            //await userStore.SetUserNameAsync(user, registration.UserName, CancellationToken.None);
            //await userEmailStore.SetEmailAsync(user, registration.Email, CancellationToken.None);

            var result = await userManager.CreateAsync(user, registration.Password);
            
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(nameof(registration), error.Description);
                }
                return Conflict(ModelState);
            }
        }
    }
}
