using VotingSystem.Abstraction;

namespace VotingSystem.Abstraction
{
    public class Result
    {
        public Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != null)
            {
                throw new InvalidOperationException();
            }
            if (!isSuccess && error == null)
            {
                throw new InvalidOperationException();
            }
            IsSuccess = isSuccess;
            Error = error;
        }
        public bool IsSuccess { get;  }
        public bool IsFailiure => !IsSuccess;
        public Error Error { get; } = default!;

        public static Result Success() => new Result(true, null!);
        public static Result Fail(Error error) => new Result(false, error);
        public static Result<T> Success<T>(T value) => new(value, true, null!);
        public static Result<T> Fail<T>(Error error) => new (default , false, error);
    }
}

public class Result<T> : Result
{

    private readonly T? _value;
    public Result(T? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }
    public T Value
    {
        get
        {
            if (IsFailiure)
            {
                throw new InvalidOperationException("Failed Results Cannot Have Value");
            }
            return _value!;
        }
    }

}
