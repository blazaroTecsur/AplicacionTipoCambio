namespace TasaCambio.Application.Comun.Dtos;

public sealed record ResponseDto<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];

    public static ResponseDto<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ResponseDto<T> Fail(string error) =>
        new() { Success = false, Errors = [error] };

    public static ResponseDto<T> Fail(IReadOnlyList<string> errors) =>
        new() { Success = false, Errors = errors };
}
