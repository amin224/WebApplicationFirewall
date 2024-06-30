using System.Net;
using System.Text;
using Audit.WebApi;
using Ganss.Xss;
using Newtonsoft.Json;
using SimpleWebApplication.Helpers;
using SimpleWebApplication.Models;

namespace SimpleWebApplication.WebFirewall
{
    public class AntiXssSecurity
    {
        private readonly AuditConfiguration _auditConfiguration;
        private ErrorResponse _error;
        private readonly int _statusCode = (int)HttpStatusCode.BadRequest;

        public AntiXssSecurity(AuditConfiguration auditConfiguration)
        {
            _auditConfiguration = auditConfiguration;
        }

        [AuditApi]
        public async Task<bool> CheckRequestAsync(HttpContext context)
        {
            var originalBody = context.Request.Body;
            try
            {
                var content = await ReadRequestBody(context);

                var sanitiser = new HtmlSanitizer();
                var sanitised = sanitiser.Sanitize(content);
                if (content != sanitised.Replace("&amp;", "&"))
                {
                    var log = new LogTraceOperation(true, "XSS");
                    _auditConfiguration.AuditCustomFields(log);

                    if (context.Request.ContentType?.Contains("application/json") == true)
                    {
                        await RespondWithJsonError(context).ConfigureAwait(false);
                    }
                    else
                    {
                        await RespondWithPageError(context).ConfigureAwait(false);
                    }
                    
                    return false;
                }

                return true;
            }
            finally
            {
                // context.Request.Body = originalBody;
            }
        }

        private static async Task<string> ReadRequestBody(HttpContext context)
        {
            var buffer = new MemoryStream();
            await context.Request.Body.CopyToAsync(buffer);
            context.Request.Body = buffer;
            buffer.Position = 0;

            var encoding = Encoding.UTF8;

            var requestContent = await new StreamReader(buffer, encoding).ReadToEndAsync();
            context.Request.Body.Position = 0;

            return requestContent;
        }

        // this method checks xss attack for the requests and return a response as json format 
        private async Task RespondWithJsonError(HttpContext context)
        {
            context.Response.Clear();
            context.Response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            // context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = _statusCode;

            if (_error == null)
            {
                _error = new ErrorResponse
                {
                    Description = Messages.XssBanned,
                    ErrorCode = 500
                };
            }

            await context.Response.WriteAsync(JsonConvert.SerializeObject(_error));
        }

        private async Task RespondWithPageError(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync(Messages.Banned);
        }
    }
}
