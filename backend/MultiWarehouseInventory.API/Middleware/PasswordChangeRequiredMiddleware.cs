using System.Security.Claims;
using System.Text.Json;

namespace MultiWarehouseInventory.API.Middleware;

public sealed class PasswordChangeRequiredMiddleware
{
    private readonly RequestDelegate _next;

    public PasswordChangeRequiredMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requiresPasswordChange = context.User.Identity?.IsAuthenticated == true &&
            string.Equals(context.User.FindFirstValue("requires_password_change"), "true", StringComparison.Ordinal);
        var isPasswordChangeRequest = context.Request.Path.StartsWithSegments("/api/auth/change-password");

        if (requiresPasswordChange && !isPasswordChangeRequest)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                errorCode = "PASSWORD_CHANGE_REQUIRED",
                message = "Change_password",
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            return;
        }

        await _next(context);
    }
}
