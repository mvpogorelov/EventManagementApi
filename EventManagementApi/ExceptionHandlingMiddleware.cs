using EventManagement.Domain.Exceptions;
using EventManagement.Presentation.Contracts;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace EventManagement.Presentation;

/// <summary>
/// Глобальный обработчик исключений API
/// </summary>
/// <param name="next"></param>
/// <param name="logger"></param>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleException(httpContext, ex);
        }
    }

    private async Task HandleException(HttpContext httpContext, Exception ex)
    {
        logger.LogError(
            ex,
            "Необработанное исключение. Method={Method}, Path={Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        if (httpContext.Response.HasStarted)
        {
            return;
        }

        var statusCode = MapStatusCode(ex);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var error = new ApiResultDto
        {
            Success = false,
            StatusCode = (HttpStatusCode)statusCode,
            Message = ex.Message
        };

        await httpContext.Response.WriteAsJsonAsync(error);
    }

    private static int MapStatusCode(Exception ex) =>
        ex switch
        {
            ArgumentOutOfRangeException aore => StatusCodes.Status400BadRequest,
            ArgumentException ae => StatusCodes.Status400BadRequest,
            ValidationException ve => StatusCodes.Status400BadRequest,
            NoAvailableSeatsException nas => StatusCodes.Status409Conflict,
            NotFoundException nfe => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };
}
