using SimpleWebApplication.Helpers;

namespace SimpleWebApplication.WebFirewall
{
    public class Init
    {
        private readonly RequestDelegate _next;
        private readonly DDoSSecurity _dDoSSecurity;
        private readonly SqlInjectionSecurity _sqlInjectionSecurity;
        private readonly AntiXssSecurity _antiXssSecurity;
        private readonly FileInclusionSecurity _fileInclusionSecurity;
        private readonly UserAgentFilteringSecurity _userAgentFilteringSecurity;

        public Init(RequestDelegate next, AuditConfiguration auditConfiguration)
        {
            _next = next;
            _dDoSSecurity = new DDoSSecurity(auditConfiguration);
            _userAgentFilteringSecurity = new UserAgentFilteringSecurity(auditConfiguration);
            _sqlInjectionSecurity = new SqlInjectionSecurity(auditConfiguration);
            _antiXssSecurity = new AntiXssSecurity(auditConfiguration);
            _fileInclusionSecurity = new FileInclusionSecurity(auditConfiguration);
        }

        public async Task InvokeAsync(HttpContext context)
        {

            // if request exist in ignored path list then bypass security check
            var requestPath = context.Request.Path.ToString();
            if (Settings.IgnoredPaths.Any(path => requestPath.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            try
            {
                if (Settings.isDDoSSecurityActive)
                {
                    if (!await _dDoSSecurity.CheckRequestAsync(context)) return;
                }

                if (Settings.isFileInclusionSecurityActive)
                {
                    if (!await _fileInclusionSecurity.CheckRequestAsync(context)) return;
                }

                if (Settings.isSqlInjectionSecurityActive)
                {
                    if (!await _sqlInjectionSecurity.CheckRequestAsync(context)) return;
                }

                if (Settings.isAntiXssSecurityActive)
                {
                    if (!await _antiXssSecurity.CheckRequestAsync(context)) return;
                }

                if (Settings.isUserAgentFilteringSecurityActive)
                {
                    if (!await _userAgentFilteringSecurity.CheckRequestAsync(context)) return;
                }

                // Call the target method if it has passed all security checks above
                // this line forward the request to target address/method
                await _next(context);
            }
            catch (Exception ex)
            {
                // context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                // await context.Response.WriteAsync(string.Format(Messages.Error, ex.Message));

                // forward to custom error page
                context.Items["Exception"] = string.Format(Messages.Error, ex.Message);
                context.Response.Redirect("/Error/CustomErrorPage");
            }
        }
    }
}