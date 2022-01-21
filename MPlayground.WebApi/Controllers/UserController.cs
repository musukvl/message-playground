using Microsoft.AspNetCore.Mvc;
using MPlayground.WebApi.Services;
using MPlayground.WebApi.WebApiContract.User;

namespace MPlayground.WebApi.Controllers;

[ApiController]
[Route("/api/user")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger, UserService userService)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost]
    [Route("update")]
    public async Task<ActionResult<UpdateUserResponse>> UpdateUser(UpdateUserRequest request)
    {
        await _userService.UpdateUser(request);
        _logger.LogInformation("User updated");
        return Ok(new UpdateUserResponse());
    }
}