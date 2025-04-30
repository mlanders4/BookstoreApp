using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Bookstore.Checkout.Infrastructure.Logging
{
    public class RequestLogger
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLogger> _logger;

        public RequestLogger(RequestDelegate next, ILogger<RequestLogger> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                ?? Guid.NewGuid().ToString();
            
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["RequestPath"] = context.Request.Path
            }))
            {
                var stopwatch = Stopwatch.StartNew();
                
                try
                {
                    context.Response.Headers.Add("X-Correlation-ID", correlationId);
                    await _next(context);
                    stopwatch.Stop();

                    _logger.LogInformation(
                        "Request completed in {ElapsedMs}ms with status {StatusCode}",
                        stopwatch.ElapsedMilliseconds,
                        context.Response.StatusCode);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(
                        ex,
                        "Request failed after {ElapsedMs}ms",
                        stopwatch.ElapsedMilliseconds);
                    throw;
                }
            }
        }
    }
}
