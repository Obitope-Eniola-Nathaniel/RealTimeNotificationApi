using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace RealTimeNotificationApi.Filters
{
    // Runs before and after controller actions
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

            // Before controller
            _logger.LogInformation("Incoming {Method} {Path}",
                http.Request.Method,
                http.Request.Path);

            // Execute controller
            var executedContext = await next();

            // After controller
            _logger.LogInformation("Outgoing {StatusCode}",
                executedContext.HttpContext.Response.StatusCode);
        }
    }
}
