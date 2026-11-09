namespace Backend.API.Common.Responses;

public class ApiErrorResponse
{
    public bool Success { get; set; } = false;
    public string Error { get; set; } = string.Empty;
    
    public static ApiErrorResponse FailureResponse(string error)
    {
        return new ApiErrorResponse
        {
            Success = false,
            Error = error
        };
    }
}