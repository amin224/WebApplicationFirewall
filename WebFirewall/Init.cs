using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Xml;

namespace WebFirewall
{
    public class Init
    {
        private readonly RequestDelegate _next;
        private readonly DDoSSecurity _dDoSSecurity;
        private readonly CustomHeaderSecurity _customHeaderSecurity;
        private readonly FloodSecurity _floodSecurity;
        private readonly SqlInjectionSecurity _sqlInjectionSecurity;
        private readonly XssSecurity _xssSecurity;
        private readonly CsrfSecurity _csrfSecurity;
        private readonly FileInclusionSecurity _fileInclusionSecurity;

        public Init(RequestDelegate next)
        {
            _next = next;
            _dDoSSecurity = new DDoSSecurity();
            _customHeaderSecurity = new CustomHeaderSecurity();
            _floodSecurity = new FloodSecurity();
            _sqlInjectionSecurity = new SqlInjectionSecurity();
            _xssSecurity = new XssSecurity();
            _csrfSecurity = new CsrfSecurity();
            _fileInclusionSecurity = new FileInclusionSecurity();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (Settings.isDDoSSecurityActive)
                {
                    if (!await _dDoSSecurity.CheckRequestAsync(context)) return;
                }

                if (Settings.isFloodSecurityActive)
                {
                    if (!await _floodSecurity.CheckRequestAsync(context)) return;
                }

                if (Settings.isSqlInjectionSecurityActive)
                {
                    if (!await _sqlInjectionSecurity.CheckRequestAsync(context)) return;
                }

                if (Settings.isXssSecurityActive)
                {
                    if (!await _xssSecurity.CheckRequestAsync(context)) return;
                }

                if (Settings.isCsrfSecurityActive)
                {
                    if (!await _csrfSecurity.CheckRequestAsync(context)) return;
                }

                if (Settings.isFileInclusionSecurityActive)
                {
                    if (!await _fileInclusionSecurity.CheckRequestAsync(context)) return;
                }

                // Call the target method if it has passed all security checks above
                // this line forward the request to target address/method
                await _next(context);
            }
            catch
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("There is an internal error. Try again later");

            }
        }
    }
}