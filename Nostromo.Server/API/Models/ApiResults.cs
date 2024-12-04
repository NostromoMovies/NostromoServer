using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nostromo.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.API.Models;

public static class ApiResults
{
    public static IResult Success<T>(T data) =>
        Results.Ok(new { data });

    public static IResult NotFound(string message) =>
        Results.NotFound(new
        {
            error = new { code = 404, message }
        });

    public static IResult ServerError(string message = "Error occurred") =>
        Results.Json(
            new { error = new { code = 500, message } },
            statusCode: 500
        );

    public static IResult BadRequest(string message) =>
        Results.BadRequest(new
        {
            error = new { code = 400, message }
        });
}