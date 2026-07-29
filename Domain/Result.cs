using Microsoft.AspNetCore.Mvc;

namespace SIGRA.Domain;

public enum ErrorType
{
    NotFound,
    Conflict,
    Unprocessable,
    BadRequest
}

public class Result
{
    public bool IsSuccess { get; private set; }
    public string? Error { get; private set; }
    public ErrorType? ErrorType { get; private set; }

    private Result(bool isSuccess, string? error, ErrorType? errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string error, ErrorType errorType) => new(false, error, errorType);

    public IActionResult ToHttpResult()
    {
        return ErrorType switch
        {
            Domain.ErrorType.NotFound => new NotFoundObjectResult(Error),
            Domain.ErrorType.Conflict => new ConflictObjectResult(Error),
            Domain.ErrorType.Unprocessable => new UnprocessableEntityObjectResult(Error),
            Domain.ErrorType.BadRequest => new BadRequestObjectResult(Error),
            _ => new BadRequestObjectResult(Error)
        };
    }
}