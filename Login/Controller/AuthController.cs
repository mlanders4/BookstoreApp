using Microsoft.AspNetCore.Mvc;
using BookstoreApp.Login.Contracts;

namespace BookstoreApp.Login.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserManager _userManager;

        public AuthController(IUserManager userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("signup")]
        public IActionResult SignUp([FromBody] UserDto dto)
        {
            var result = _userManager.SignUp(dto);
            if (result)
                return Ok(new { message = "Account created!" });

            return BadRequest(new { message = "User already exists." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserDto dto)
        {
            var result = _userManager.Login(dto);
            if (result)
                return Ok(new { message = "Login successful!" });

            return Unauthorized(new { message = "Invalid credentials." });
        }
    }
}
