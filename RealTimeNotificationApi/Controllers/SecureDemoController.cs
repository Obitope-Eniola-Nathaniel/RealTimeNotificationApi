using Microsoft.AspNetCore.Mvc;
using RealTimeNotificationApi.Filters;

namespace RealTimeNotificationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/secureDemo
    public class SecureDemoController : ControllerBase
    {
        // GET /api/secureDemo/secret
        // Response will be encrypted by EncryptResponseFilter
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
