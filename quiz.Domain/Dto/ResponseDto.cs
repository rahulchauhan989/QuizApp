namespace quiz.Domain.Dto;

public class ResponseDto
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }

    public ResponseDto(bool isSuccess, string? message = null, object? data = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Data = data;
    }
}
