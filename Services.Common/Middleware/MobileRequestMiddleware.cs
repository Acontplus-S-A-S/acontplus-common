using Microsoft.AspNetCore.Http;

namespace Services.Common.Middleware
{
    public class MobileRequestMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var isMobileUserAgent = IsMobileUserAgent(userAgent);

            // Optional: Additional check for custom header
            var isMobileHeader = context.Request.Headers["X-Is-Mobile"].ToString()
                .Equals("true", StringComparison.OrdinalIgnoreCase);

            context.Items["IsMobileRequest"] = isMobileUserAgent || isMobileHeader;

            await next(context);
        }

        private static bool IsMobileUserAgent(string userAgent)
        {
            var mobileKeywords = new[]
            {
                "Android", "iPhone", "iPad", "iPod", "Windows Phone", "IEMobile", "BlackBerry", "BB10",
                "Opera Mini", "Mobile", "Silk"
            };

            return mobileKeywords.Any(keyword => userAgent.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
    }
}
