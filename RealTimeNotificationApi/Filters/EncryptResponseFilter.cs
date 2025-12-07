using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RealTimeNotificationApi.Security;
using System.Text.Json;

namespace RealTimeNotificationApi.Filters
{
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
            if (context.Result is ObjectResult objectResult &&
                objectResult.Value is not null)
            {
                // Serialize
                var json = JsonSerializer.Serialize(objectResult.Value);

                // Encrypt
                var cipher = _encryptionService.Encrypt(json);

                // Replace result
                context.Result = new OkObjectResult(new
                {
                    encrypted = cipher
                });
            }

            await next();
        }
    }
}
