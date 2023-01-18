using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApiSecurity.Models;
using MyApiSecurity.Services;

namespace MyApiSecurity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(RegisterModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authService.RegisterAsync(model);

            if (!result.isAuthentcated)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetToken(TokenRequistModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.GetTokenAsync(model);
            if (!result.isAuthentcated)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var reslut = await _authService.GetAllUsers();
            if (reslut == null)
                return BadRequest("No User In This Data");

            return Ok(reslut);
        }

        [HttpPost("AddRole")]
        public async Task<IActionResult> AddRoleAsync(AddRoleModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reslut = await _authService.AddRoleAsync(model);
            if (!string.IsNullOrEmpty(reslut))
                return BadRequest(reslut);

            return Ok(model);
        } 
    }
}
