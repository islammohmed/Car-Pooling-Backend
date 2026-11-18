namespace CarPooling.Application.Common.Results
{
    public class Result
    {
        public bool IsSuccess { get; protected set; }
        public string Message { get; protected set; } = string.Empty;
        public List<string> Errors { get; protected set; } = new();

        public static Result Success(string message = "") =>
            new Result { IsSuccess = true, Message = message };

        public static Result Failure(string message) =>
            new Result { IsSuccess = false, Message = message };
    }
    public class Result<T> : Result
    {
        public T? Data { get; set; }
        public static Result<T> Success(T data, string message = "")
        {
            return new Result<T> { IsSuccess = true, Message = message, Data = data };
        }
        public static Result<T> Failure(string message)
        {
            return new Result<T> { IsSuccess = false, Message = message };
        }
        public static Result<T> FailureWithErrors(List<string> errors, string message = "")
        {
            return new Result<T> { IsSuccess = false, Message = message, Errors = errors };
        }
    }
}
