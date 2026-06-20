using System.Threading.RateLimiting;

namespace PayOnMap.API.Middleware;

/// <summary>
/// تنظیمات Rate Limiting برای جلوگیری از Brute Force و Abuse
/// </summary>
public static class RateLimitingExtensions
{
    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Rate Limit برای Auth Endpoints
            options.AddPolicy("AuthRateLimit", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetSlidingWindowLimiter(ip,
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,              // حداکثر ۱۰ درخواست
                        Window = TimeSpan.FromMinutes(1), // در هر ۱ دقیقه
                        SegmentsPerWindow = 5,
                        QueueLimit = 2
                    });
            });

            // Rate Limit عمومی
            options.AddPolicy("GeneralRateLimit", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(ip,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            // پاسخ در صورت Rate Limit
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    message = "تعداد درخواست‌ها بیش از حد مجاز است. لطفاً بعداً تلاش کنید."
                }, cancellationToken);
            };
        });

        return services;
    }
}