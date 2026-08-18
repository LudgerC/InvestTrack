using System.Linq;
using System.Threading.Tasks;
using InvestTrack.Model.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InvestTrack.Web.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthApiController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class RegisterRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string? FullName { get; set; }
        }

        public class LoginResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new LoginResponse { Success = false, Message = "E-mailadres en wachtwoord verplicht." });
            }

            var user = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (user == null)
            {
                return Unauthorized(new LoginResponse { Success = false, Message = "Onbekende gebruiker." });
            }

            var valid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!valid)
            {
                return Unauthorized(new LoginResponse { Success = false, Message = "Ongeldig wachtwoord." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            string primaryRole = roles.FirstOrDefault() ?? "Trader";

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Inloggen succesvol",
                UserId = user.Id,
                Email = user.Email ?? request.Email,
                Role = primaryRole
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new LoginResponse { Success = false, Message = "E-mailadres en wachtwoord zijn verplicht." });
            }

            var existing = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (existing != null)
            {
                return BadRequest(new LoginResponse { Success = false, Message = "Dit e-mailadres is al in gebruik." });
            }

            var user = new ApplicationUser
            {
                UserName = request.Email.Trim(),
                Email = request.Email.Trim(),
                FullName = string.IsNullOrWhiteSpace(request.FullName) ? request.Email.Trim() : request.FullName.Trim()
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = string.Join("; ", result.Errors.Select(e => e.Description))
                });
            }

            string targetRole = "Trader";
            if (!await _roleManager.RoleExistsAsync(targetRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(targetRole));
            }

            await _userManager.AddToRoleAsync(user, targetRole);

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Registratie succesvol",
                UserId = user.Id,
                Email = user.Email ?? request.Email,
                Role = targetRole
            });
        }
    }
}
