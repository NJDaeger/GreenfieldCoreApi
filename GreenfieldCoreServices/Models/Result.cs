namespace GreenfieldCoreServices.Models;

/// <summary>
/// Represents an untyped result
/// </summary>
/// <param name="Success">Whether this result was successful or not</param>
/// <param name="Message">The message to go along with this result. May be null.</param>
public record Result(bool Success, string? Message = null)
{
    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <param name="message">Message to return</param>
    /// <returns></returns>
    public static Result Ok(string? message = null) => new Result(true, message);

    /// <summary>
    /// Creates a failed result
    /// </summary>
    /// <param name="message">Message to return</param>
    /// <returns></returns>
    public static Result Fail(string? message = null) => new Result(false, message);
    
}

/// <summary>
/// Represents a typed result
/// </summary>
/// <param name="Success">Whether this result was successful or not</param>
/// <param name="Data">The data to go along with this result</param>
/// <param name="Message">The message to go along with this result. May be null.</param>
/// <typeparam name="T"></typeparam>
public record Result<T>(bool Success, T Data, string? Message = null) : Result(Success, Message)
{
    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <param name="data">Data to return</param>
    /// <param name="message">Message to return</param>
    /// <returns></returns>
    public static Result<T> Ok(T data = default!, string? message = null) => new(true, data, message);
    
    /// <summary>
    /// Creates a failed result
    /// </summary>
    /// <param name="data">Data to return</param>
    /// <param name="message">Message to return</param>
    /// <returns></returns>
    public static Result<T> Fail(T data = default!, string? message = null) => new(false, data, message);
}