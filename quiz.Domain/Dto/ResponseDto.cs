namespace quiz.Domain.Dto;

public class ResponseDto
{
    public int StatusCode { get; set; } = 200; 
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }

    public ResponseDto(bool isSuccess, string? message = null, object? data = null, int statusCode = 200)
    {
        StatusCode = statusCode;
        IsSuccess = isSuccess;
        Message = message;
        Data = data;
    }
   
}
