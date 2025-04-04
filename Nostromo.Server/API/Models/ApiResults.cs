﻿using Microsoft.AspNetCore.Http;
namespace Nostromo.Server.API.Models;

public class SuccessResponse<T>
{
    public string ApiVersion { get; set; }
    public T Data { get; set; }

    public SuccessResponse(string apiVersion, T data)
    {
        ApiVersion = apiVersion;
        Data = data;
    }
}

public class ErrorResponse
{
    public string ApiVersion { get; set; }
    public ApiError Error { get; set; }

    public ErrorResponse(string apiVersion, ApiError error)
    {
        ApiVersion = apiVersion;
        Error = error;
    }
}

public static class ApiResults
{
    private const string ApiVersion = "1.0";
    private static IResult Response<T>(T data, int statusCode, string? location = null)
    {
        var response = TypedResults.Json(
            new SuccessResponse<T>(ApiVersion, data),
            statusCode: statusCode
        );
        if (location != null)
        {
            return new HeaderResult(response, location);
        }
        return response;
    }

    private static IResult Error(int statusCode, string message) =>
        ErrorResponse(new ApiError(statusCode, message), statusCode);

    private static IResult ErrorResponse(ApiError error, int statusCode) =>
        TypedResults.Json(
            new ErrorResponse(ApiVersion, error),
            statusCode: statusCode
        );

    // Success responses
    public static IResult Success<T>(T data) =>
        Response(data, StatusCodes.Status200OK);

    public static IResult SuccessCollection<T>(IEnumerable<T>? items, int? total = null) =>
        Success(new ApiCollection<T>(items ?? Array.Empty<T>(), total));

    public static IResult Created<T>(T data, string? uri = null) =>
        Response(data, StatusCodes.Status201Created, uri);

    public static IResult PhysicalFile(string path, string contentType) =>
        TypedResults.PhysicalFile(path, contentType);

    // Error responses
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