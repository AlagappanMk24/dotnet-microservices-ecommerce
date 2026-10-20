namespace Ecommerce.SharedLibrary.Responses;

public sealed record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data,
    string? TraceId = null)
{
    public static ApiResponse<T> Ok(
        T data,
        string message = "Success",
        string? traceId = null)
    {
        return new ApiResponse<T>(
            true, message, data, traceId);
    }

    public static ApiResponse<T> Fail(
        string message,
        string? traceId = null)
    {
        return new ApiResponse<T>(
            false, message, default, traceId);
    }
}