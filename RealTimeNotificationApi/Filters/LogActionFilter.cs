using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace RealTimeNotificationApi.Filters
{
    public class LogActionFilter : IAsyncActionFilter
    {
        private readonly ILogger<LogActionFilter> _logger;

        public LogActionFilter(ILogger<LogActionFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var http = context.HttpContext;

            _logger.LogInformation("Incoming {Method} {Path}",
                http.Request.Method,
                http.Request.Path);

            var executedContext = await next();

            _logger.LogInformation("Outgoing {StatusCode}",
                executedContext.HttpContext.Response.StatusCode);
        }
    }
}
