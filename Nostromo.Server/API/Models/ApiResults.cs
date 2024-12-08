using Microsoft.AspNetCore.Http;

namespace Nostromo.Server.API.Models;

public class ApiError
{
    public int Code { get; }
    public string Message { get; }

    public ApiError(int code, string message)
    {
        Code = code;
        Message = message;
    }
}

public class ApiCollection<T>
{
    public int TotalItems { get; }
    public IEnumerable<T> Items { get; }

    public ApiCollection(IEnumerable<T> items, int? total = null)
    {
        Items = items ?? Array.Empty<T>();
        TotalItems = Items.Count();
    }
}

public static class ApiResults
{
    private const string ApiVersion = "1.0";

    public static IResult Success<T>(T data) =>
        Results.Json(new { apiVersion = ApiVersion, data },
            statusCode: StatusCodes.Status200OK);

    public static IResult SuccessCollection<T>(IEnumerable<T>? items) =>
        Success(new ApiCollection<T>(items ?? Array.Empty<T>()));

    public static IResult NotFound(string message) =>
        Results.Json(
            new { apiVersion = ApiVersion, error = new ApiError(StatusCodes.Status404NotFound, message) },
            statusCode: StatusCodes.Status404NotFound);

    public static IResult Unauthorized(string message) =>
        Results.Json(
            new { apiVersion = ApiVersion, error = new ApiError(StatusCodes.Status401Unauthorized, message) },
            statusCode: StatusCodes.Status401Unauthorized);

    public static IResult ServerError(string message = "Error occurred") =>
        Results.Json(
            new { apiVersion = ApiVersion, error = new ApiError(StatusCodes.Status500InternalServerError, message) },
            statusCode: StatusCodes.Status500InternalServerError);

    public static IResult BadRequest(string message) =>
        Results.Json(
            new { apiVersion = ApiVersion, error = new ApiError(StatusCodes.Status400BadRequest, message) },
            statusCode: StatusCodes.Status400BadRequest);
    public static IResult PhysicalFile(string path, string contentType) =>
        TypedResults.PhysicalFile(path, contentType);
}