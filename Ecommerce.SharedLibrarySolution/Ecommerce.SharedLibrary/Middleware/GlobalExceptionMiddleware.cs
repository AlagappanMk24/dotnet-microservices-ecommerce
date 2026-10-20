// File: Ecommerce.SharedLibrary/Middleware/GlobalExceptionMiddleware.cs
using Ecommerce.SharedLibrary.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.SharedLibrary.Middleware;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected or cancelled the request. 
            // Do not attempt to write to the response since the connection is closed.
            logger.LogInformation("Request was cancelled by the client. TraceId: {TraceId}", context.TraceIdentifier);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
                throw;

            var (status, title, detail) = exception switch
            {
                KeyNotFoundException =>
                    (StatusCodes.Status404NotFound,
                     "Resource not found",
                     exception.Message),

                ArgumentException =>
                    (StatusCodes.Status400BadRequest,
                     "Invalid request",
                     exception.Message),

                InvalidOperationException =>
                    (StatusCodes.Status409Conflict,
                     "Operation conflict",
                     exception.Message),

                DbUpdateException =>
                    (StatusCodes.Status409Conflict,
                     "Data conflict",
                     "The operation conflicts with existing data."),

                RequestValidationException validation =>
                    (StatusCodes.Status400BadRequest,
                    "Validation failed",
                    string.Join("; ", validation.Errors)),

                _ =>
                    (StatusCodes.Status500InternalServerError,
                     "Unexpected server error",
                     "An unexpected error occurred.")
            };

            if (status >= 500)
            {
                logger.LogError(
                    exception,
                    "Unhandled exception. TraceId: {TraceId}",
                    context.TraceIdentifier);
            }
            else
            {
                logger.LogWarning(
                    "Request failed with status {StatusCode}. TraceId: {TraceId}",
                    status,
                    context.TraceIdentifier);
            }

            context.Response.Clear();
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(
                      problem,
                      cancellationToken: CancellationToken.None);
        }
    }
}