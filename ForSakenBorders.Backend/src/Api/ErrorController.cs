using System;
using System.Collections.Generic;
using ForSakenBorders.Backend.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ForSakenBorders.Backend.Api
{
    /// <summary>
    /// API Controller used to intercept and handle all unexpected exception
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ErrorController : ControllerBase
    {
        private readonly BackendContext _database;
        public ErrorController(BackendContext database) => _database = database;

        /// <summary>
        /// Action that will be invoked for any call to this Controller in order to handle the current error
        /// </summary>
        /// <returns>A generic error formatted as JSON because we are in a REST API app context</returns>
        [HttpGet, HttpPost, HttpHead, HttpDelete, HttpPut, HttpOptions, HttpPatch]
        public IActionResult Handle()
        {
            Exception exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
            Log log = new();
            log.Endpoint = HttpContext.Request.Path;
            if (Guid.TryParse(HttpContext.Request.Headers["Authorization"], out Guid userToken))
            {
                log.Token = userToken;
            }

            if (HttpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent))
            {
                log.UserAgent = userAgent;
            }
            log.Exception = exception;
            _database.Logs.Add(log);

            Request.HttpContext.Response.Headers["X-Error"] = "true";
            return StatusCode(500, "An internal server error occured. Please try again later, or try with different options.");
        }
    }
}