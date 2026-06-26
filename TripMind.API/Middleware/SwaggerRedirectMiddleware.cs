using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TripMind.API.Middleware
{
    public class SwaggerRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public SwaggerRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/")
            {
                context.Response.Redirect("/swagger");
                return;
            }

            await _next(context);
        }
    }
}