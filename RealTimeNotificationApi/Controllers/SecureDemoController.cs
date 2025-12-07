using Microsoft.AspNetCore.Mvc;
using RealTimeNotificationApi.Filters;

namespace RealTimeNotificationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecureDemoController : ControllerBase
    {
        [HttpGet("secret")]
        [ServiceFilter(typeof(EncryptResponseFilter))]
        public IActionResult GetSecret()
        {
            var data = new
            {
                Message = "This is sensitive data",
                Timestamp = DateTime.UtcNow
            };

            return Ok(data);
        }
    }
}
