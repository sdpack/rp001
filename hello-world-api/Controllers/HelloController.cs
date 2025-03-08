using Microsoft.AspNetCore.Mvc;

namespace hello_world_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Hello World!" });
    }
}
