using System.Text.Json.Serialization;

namespace Backend.API.Common.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaginationDto? Pagination { get; set; }

    public static ApiResponse<T> SuccessResponse(
        string? message = null,
        T? data = default,
        PaginationDto? pagination = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Pagination = pagination
        };
    }
}