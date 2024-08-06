using Core.Error;

namespace Core;

public readonly struct Result<T>
{
    private readonly ResultState _state;

    public Result(T value)
    {
        Value = value;
        ErrorResponse = null;
        _state = ResultState.Success;
    }

    public Result(ErrorResponse errorResponse)
    {
        Value = default;
        ErrorResponse = errorResponse;
        _state = ResultState.Failure;
    }

    public enum ResultState
    {
        Null,
        Failure,
        Success
    }

    public T Value { get; }
    
    public bool IsSuccess => _state == ResultState.Success;
    public bool IsFailure => _state == ResultState.Failure;
    public bool IsNull => _state == ResultState.Null;

    public ErrorResponse? ErrorResponse { get; }

    public static implicit operator Result<T>(T value) => value is not null ? new Result<T>(value) : new Result<T>();
    public static implicit operator Result<T>(ErrorResponse errorResponse) => new(errorResponse);
}
