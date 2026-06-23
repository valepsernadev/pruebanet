namespace RentasCortas.Common.Responses;

public class ApiResponse
{
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }

    public ApiResponse(string message, int statusCode)
    {
        Message = message;
        StatusCode = statusCode;
    }
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public ApiResponse(string message, int statusCode, T? data = default)
        : base(message, statusCode)
    {
        Data = data;
    }
}
