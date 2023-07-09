using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }


    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
    {
        var result = await _identityService.Login(model);

        return Ok(result);
    }
}
