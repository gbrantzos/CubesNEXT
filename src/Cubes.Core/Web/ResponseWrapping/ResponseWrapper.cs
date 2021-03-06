using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Cubes.Core.Base;
using Cubes.Core.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Cubes.Core.Web.ResponseWrapping
{
    public class ResponseWrapper
    {
        private static readonly HashSet<string> Excluded = new HashSet<string>
        {
            "/api/system/version"
        };

        private static readonly List<string> ContentType2Handle = new List<string>
        {
            "application/json",
            "application/problem+json"
        };

        private readonly RequestDelegate next;
        private readonly IApiResponseBuilder responseBuilder;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly string includePath;
        private readonly string excludePath;

        public ResponseWrapper(RequestDelegate next,
            IApiResponseBuilder responseBuilder,
            IConfiguration configuration,
            JsonSerializerSettings jsonSerializerSettings)
        {
            this.next = next;
            this.responseBuilder = responseBuilder;
            this.jsonSerializerSettings = jsonSerializerSettings;

            this.includePath = configuration.GetValue(CubesConstants.Config_HostWrapPath, "/api/");
            this.excludePath = configuration.GetValue(CubesConstants.Config_HostWrapPathExclude, "");
        }

        // https://stackoverflow.com/a/47183053/3410871
        public async Task Invoke(HttpContext context)
        {
            // Honor include and exclude paths
            if (ShouldSkip(context))
            {
                await this.next(context);
                return;
            }

            // Keep a reference to original body
            var originalBody = context.Response.Body;
            try
            {
                await using var memStream = new MemoryStream();
                // Replace stream for actual calls
                context.Response.Body = memStream;

                // Continue the pipeline
                await next(context);

                // Check if we are about to return JSON data
                var responseContentType = context.Response.ContentType ?? String.Empty;
                if (ContentTypeIsHandled(responseContentType))
                {
                    // Memory stream now hold the response data
                    // Reset position to read data stored in response stream
                    memStream.Position = 0;
                    var responseBody = await new StreamReader(memStream).ReadToEndAsync();
                    var data = JsonConvert.DeserializeObject<dynamic>(responseBody);

                    // Create wrapper response and convert to JSON
                    var apiResponse = responseBuilder
                        .Create()
                        .WithStatusCode(context.Response.StatusCode)
                        .WithData((object)data);

                    // Copy wrapped response to original body
                    var buffer = Encoding.UTF8.GetBytes(apiResponse.AsJson(jsonSerializerSettings));
                    context.Response.ContentLength = null; // buffer.Length;
                    await using var output = new MemoryStream(buffer);
                    await output.CopyToAsync(originalBody);
                }
                else
                {
                    // No need to wrap, just copy original stream
                    memStream.Position = 0;
                    await memStream.CopyToAsync(originalBody);
                }
            }
            finally
            {
                // Finally, reset the stream for downstream calls
                context.Response.Body = originalBody;
            }
        }

        private bool ShouldSkip(HttpContext context)
        {
            var requestPath = context.Request.Path.Value;
            var shouldSkip = !requestPath.StartsWith(this.includePath) ||
                             (!String.IsNullOrEmpty(this.excludePath) && requestPath.StartsWith(this.excludePath)) ||
                             Excluded.Contains(requestPath);
            return shouldSkip;
        }

        private bool ContentTypeIsHandled(string content)
        {
            foreach (var knownType in ContentType2Handle)
            {
                if (content.Contains(knownType, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }

    public static class ResponseWrapperExtensions
    {
        public static IApplicationBuilder UseResponseWrapper(this IApplicationBuilder builder)
            => builder.UseMiddleware<ResponseWrapper>();
    }
}
