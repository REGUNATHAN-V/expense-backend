namespace Backend.Application.Common.Results;

public class Result<T>
{
    public bool IsSuccess { get; private set; }

    public T? Data { get; private set; }

    public string? Error { get; private set; }

    public ResultErrorType ErrorType { get; private set; }

    private Result()
    {
    }

    public static Result<T> Success(T data)
    {
        return new Result<T>
        {
            IsSuccess = true,
            Data = data,
            ErrorType = ResultErrorType.None
        };
    }

    public static Result<T> Failure(
        string error,
        ResultErrorType errorType = ResultErrorType.Failure)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Error = error,
            ErrorType = errorType
        };
    }
}