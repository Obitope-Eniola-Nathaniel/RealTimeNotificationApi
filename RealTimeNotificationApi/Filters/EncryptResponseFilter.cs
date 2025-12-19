using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RealTimeNotificationApi.Security;
using System.Text.Json;

namespace RealTimeNotificationApi.Filters
{
    // Encrypts any ObjectResult and wraps it as { encrypted: "<cipher>" }
    public class EncryptResponseFilter : IAsyncResultFilter
    {
        private readonly EncryptionService _encryptionService;

        public EncryptResponseFilter(EncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }

        public async Task OnResultExecutionAsync(
            ResultExecutingContext context,
            ResultExecutionDelegate next)
        {
            // Only handle object results
            if (context.Result is ObjectResult objectResult &&
                objectResult.Value is not null)
            {
                // Convert object to JSON string
                var json = JsonSerializer.Serialize(objectResult.Value);

                // Encrypt JSON
                var cipher = _encryptionService.Encrypt(json);

                // Replace result with encrypted payload
                context.Result = new OkObjectResult(new
                {
                    encrypted = cipher
                });
            }

            await next();
        }
    }
}
