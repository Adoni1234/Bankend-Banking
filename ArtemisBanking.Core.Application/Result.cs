using ArtemisBanking.Core.Application.Dtos.User;

namespace ArtemisBanking.Core.Application
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        
        public string? GeneralError { get; }
        public List<string>? Errors { get; }


        protected Result(bool isSuccess, string? generalError, List<string>? errors)
        {
            IsSuccess = isSuccess;
            GeneralError = generalError;
            Errors = errors;
        }

        public static Result Ok() => new(true, null, null);
        public static Result Fail(List<string> errors) => new(false, null, errors);
        public static Result Fail(string generalError) => new(false, generalError, null);
    }

    public class Result<T> : Result
    {
        public T? Value { get; }

        public Result(bool isSuccess, T? value, string? generalError, List<string>? errors) :
            base(isSuccess, generalError, errors)
        {
            Value = value;
        }

        public static Result<T> Ok(T value) => new(true, value, null, null);
        public new static Result<T> Fail(List<string> errors) => new(false, default, null , errors);
        public new static Result<T> Fail(string generalError) => new(false, default, generalError, null);
    }
}