using System.Net;
using System.Text.Json;
using MultiWarehouseInventory.Domain.Exceptions;

namespace MultiWarehouseInventory.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode, message) = MapException(exception);

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled server error");
        }
        else
        {
            _logger.LogWarning(exception, "Handled error: {ErrorCode}", errorCode);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            success = false,
            errorCode,
            message,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        }));
    }

    private static (int StatusCode, string ErrorCode, string Message) MapException(Exception exception)
    {
        return exception switch
        {
            UnauthorizedException unauthorized => (
                (int)HttpStatusCode.Unauthorized,
                unauthorized.ErrorCode,
                unauthorized.Message),

            NotFoundException notFound => (
                (int)HttpStatusCode.NotFound,
                notFound.ErrorCode,
                notFound.Message),

            BadRequestException badRequest => (
                (int)HttpStatusCode.BadRequest,
                "BAD_REQUEST",
                badRequest.Message),

            InvalidOperationException invalidOp when invalidOp.Message.Contains("JWT", StringComparison.OrdinalIgnoreCase) => (
                (int)HttpStatusCode.InternalServerError,
                "JWT_CONFIG_ERROR",
                "Lỗi cấu hình JWT trên máy chủ"),

            InvalidOperationException invalidOp => (
                (int)HttpStatusCode.BadRequest,
                "BAD_REQUEST",
                invalidOp.Message),

            InsufficientStockException insufficient => (
                (int)HttpStatusCode.BadRequest,
                "INSUFFICIENT_STOCK",
                insufficient.Message),

            WarehouseInventoryNotFoundException notFoundInv => (
                (int)HttpStatusCode.NotFound,
                "INVENTORY_NOT_FOUND",
                notFoundInv.Message),

            BaseException baseEx => (
                (int)HttpStatusCode.BadRequest,
                baseEx.ErrorCode,
                baseEx.Message),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR",
                ""),
        };
    }
}
