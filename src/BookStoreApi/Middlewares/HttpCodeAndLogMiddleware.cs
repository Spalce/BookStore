using System.Net;
using System.Net.Mime;
using System.Security.Authentication;
using System.Text;
using BookStore.Core.Core.Exceptions;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using BookStore.Core.Exceptions;

namespace BookStoreApi.Api.Middlewares
{
    public static class HttpStatusCodeExceptionMiddlewareExtension
    {
        public static IApplicationBuilder UseHttpCodeAndLogMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HttpCodeAndLogMiddleware>();
        }
    }

    public class HttpCodeAndLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpCodeAndLogMiddleware> _logger;

        public HttpCodeAndLogMiddleware(RequestDelegate next, ILogger<HttpCodeAndLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                return;
            }

            try
            {
                context.Request.EnableBuffering();
                await _next(context);
            }
            catch (Exception exception)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                switch (exception)
                {
                    case ApiException e:
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        await WriteAndLogResponseAsync(exception, context, HttpStatusCode.BadRequest, LogLevel.Error,
                            "Bad Request Exception!" + e.Message);
                        break;
                    case NotFoundException e:
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        await WriteAndLogResponseAsync(exception, context, HttpStatusCode.NotFound, LogLevel.Error,
                            "Not Found!" + e.Message);
                        break;
                    case ValidationException e:
                        context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                        await WriteAndLogResponseAsync(exception, context, HttpStatusCode.UnprocessableEntity, LogLevel.Error,
                            "Validation Exception!" + e.Message);
                        break;
                    case AuthenticationException e:
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        await WriteAndLogResponseAsync(exception, context, HttpStatusCode.Unauthorized, LogLevel.Error,
                            "Authentication Exception!" + e.Message);
                        break;
                    default:
                        await WriteAndLogResponseAsync(exception, context, HttpStatusCode.InternalServerError,
                            LogLevel.Error, "Server Error!");
                        break;
                }
            }
        }

        private async Task WriteAndLogResponseAsync(Exception exception, HttpContext context, HttpStatusCode code,
            LogLevel level, string alternateMessage = null)
        {
            string requestBody = string.Empty;
            if (context.Request.Body.CanSeek)
            {
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                using (var sr = new StreamReader(context.Request.Body))
                {
                    requestBody = JsonConvert.SerializeObject(sr.ReadToEndAsync());
                }

                StringValues authorization;
                context.Response.Headers.TryGetValue("Authorization", out authorization);

                var customDetails = new StringBuilder();
                customDetails
                    .AppendFormat("\n Service URL     :").Append(context.Request.Path.ToString())
                    .AppendFormat("\n Request Method  :").Append(context.Request?.Method)
                    .AppendFormat("\n Request Body    :").Append(requestBody)
                    .AppendFormat("\n Authorization   :").Append(authorization)
                    .AppendFormat("\n Content Type    :").Append(context.Request?.Headers["Content-Type"].ToString())
                    .AppendFormat("\n Cookie          :").Append(context.Request?.Headers["Cookie"].ToString())
                    .AppendFormat("\n Host            :").Append(context.Request?.Headers["Host"].ToString())
                    .AppendFormat("\n Referer         :").Append(context.Request?.Headers["Referer"].ToString())
                    .AppendFormat("\n Origin          :").Append(context.Request?.Headers["Origin"].ToString())
                    .AppendFormat("\n User Agent      :").Append(context.Request?.Headers["User Agent"].ToString())
                    .AppendFormat("\n Error Message   :").Append(exception.Message);

                _logger.Log(level, exception, customDetails.ToString());
            }

            if (context.Response.HasStarted)
            {
                _logger.LogError("The response has already started, the http status code middleware will not be executed.");
            }

            string responseMessage = JsonConvert.SerializeObject(new
            {
                Message = string.IsNullOrWhiteSpace(exception.Message) ? alternateMessage : exception.Message
            });

            context.Response.Clear();
            context.Response.ContentType = MediaTypeNames.Application.Json;
            context.Response.StatusCode = (int)code;
            await context.Response.WriteAsync(responseMessage, Encoding.UTF8);
        }
    }
}
