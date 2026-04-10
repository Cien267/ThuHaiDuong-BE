namespace ThuHaiDuong.Application.Payloads.Responses;

public class ResponseErrorObject : Exception
{
    public int StatusCode { get; }
    public Dictionary<string, string[]>? Errors { get; }

    public ResponseErrorObject(
        string message,
        int statusCode,
        Dictionary<string, string[]>? errors = null
    ) : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}