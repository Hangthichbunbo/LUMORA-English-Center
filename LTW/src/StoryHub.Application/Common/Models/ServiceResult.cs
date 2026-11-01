namespace StoryHub.Application.Common.Models;

public class ServiceResult
{
    public bool Succeeded { get; protected set; }
    public string Message { get; protected set; } = string.Empty;
    public string[] Errors { get; protected set; } = [];

    public static ServiceResult Success(string message = "Thành công") => new() { Succeeded = true, Message = message };
    public static ServiceResult Failure(params string[] errors) => new() { Succeeded = false, Errors = errors };
    public static ServiceResult Failure(string message, params string[] errors) => new() { Succeeded = false, Message = message, Errors = errors };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; private set; }

    public static ServiceResult<T> Success(T data, string message = "Thành công") => new() { Succeeded = true, Data = data, Message = message };
    public new static ServiceResult<T> Failure(params string[] errors) => new() { Succeeded = false, Errors = errors };
    public new static ServiceResult<T> Failure(string message, params string[] errors) => new() { Succeeded = false, Message = message, Errors = errors };
}
