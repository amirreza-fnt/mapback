namespace PayOnMap.API.Middleware;

/// <summary>
/// Middleware افزودن Security Headers
///  محافظت در برابر:
/// - XSS (Content-Security-Policy)
/// - Clickjacking (X-Frame-Options)
/// - MIME Sniffing (X-Content-Type-Options)
/// - Information Leakage (Server Header)
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        //  جلوگیری از Clickjacking
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        //  جلوگیری از MIME Sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        //  Content Security Policy
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self'; " +
            "connect-src 'self' https://apiweb-payonmap.sabzevar.ir");

        //  Referrer Policy
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        //  Permissions Policy
        context.Response.Headers.Append("Permissions-Policy",
            "camera=(), microphone=(), geolocation=()");

        //  حذف Server Header
        context.Response.Headers.Remove("Server");

        //  Strict Transport Security (HSTS)
        if (context.Request.IsHttps)
        {
            context.Response.Headers.Append("Strict-Transport-Security",
                "max-age=31536000; includeSubDomains; preload");
        }

        await _next(context);
    }
}