using Microsoft.AspNetCore.Http;

namespace Nostromo.Server.API.Models;

// ApiResults class
// ex usage:
//      var movie = await _tmdbService.GetMovieById(id)
//      return ApiResults.Success(movie);
public static class ApiResults
{
    private const string ApiVersion = "1.0";

    private static IResult Response<T>(T data, int statusCode, string? location = null)
    {
        var response = TypedResults.Json(
            new { apiVersion = ApiVersion, data },
            statusCode: statusCode
        );

        if (location != null)
        {
            return new HeaderResult(response, location);
        }

        return response;
    }

    private static IResult Error(int statusCode, string message) =>
        Response(new ApiError(statusCode, message), statusCode);

// --------------------------------------------------------------------------------------------------------------------------
// Success responses
// --------------------------------------------------------------------------------------------------------------------------
    public static IResult Success<T>(T data) =>
        Response(data, StatusCodes.Status200OK);

    public static IResult SuccessCollection<T>(IEnumerable<T>? items, int? total = null) =>
    Success(new ApiCollection<T>(items ?? Array.Empty<T>(), total));

    public static IResult Created<T>(T data, string? uri = null) =>
        Response(data, StatusCodes.Status201Created, uri);

    // File response (passthrough to TypedResults)
    public static IResult PhysicalFile(string path, string contentType) =>
        TypedResults.PhysicalFile(path, contentType);

// --------------------------------------------------------------------------------------------------------------------------
// Error responses
// --------------------------------------------------------------------------------------------------------------------------
    public static IResult NotFound(string message) =>
        Error(StatusCodes.Status404NotFound, message);

    public static IResult Unauthorized(string message) =>
        Error(StatusCodes.Status401Unauthorized, message);

    public static IResult Forbidden(string message) =>
        Error(StatusCodes.Status403Forbidden, message);

    public static IResult ServerError(string message = "Error occurred") =>
        Error(StatusCodes.Status500InternalServerError, message);

    public static IResult BadRequest(string message) =>
        Error(StatusCodes.Status400BadRequest, message);


    // Custom IResult implementation for handling Location header
    private class HeaderResult : IResult
    {
        private readonly IResult _result;
        private readonly string _locationValue;

        public HeaderResult(IResult result, string locationValue)
        {
            _result = result;
            _locationValue = locationValue;
        }

        public async Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.Headers.Location = _locationValue;
            await _result.ExecuteAsync(httpContext);
        }
    }
}
// ApiCollection class for results with multiple items
public class ApiCollection<T>
{
    public int TotalItems { get; }
    public IEnumerable<T> Items { get; }
    public ApiCollection(IEnumerable<T> items, int? total = null)
    {
        Items = items ?? Array.Empty<T>();
        TotalItems = total ?? Items.Count();
    }
}

// ApiError class for consistent error response structure
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

