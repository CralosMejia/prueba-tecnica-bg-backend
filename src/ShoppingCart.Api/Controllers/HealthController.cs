using Microsoft.AspNetCore.Mvc;


namespace ShoppingCart.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "Healthy", message = "Service is running smoothly." });
        }
    }
}