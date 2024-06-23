using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebFirewall
{
    public class CsrfSecurity
    {
        public async Task<bool> CheckRequestAsync(HttpContext context)
        {
            // write your own CSRF protection logic here
            
            // Example form
            //<form method="post" action="/submit">
            //    <input type="hidden" name="__RequestVerificationToken" value="A6YD*!fV?NN89B0" />
            //    <input type="text" name="data" />
            //    <button type="submit">Submit</button>
            //</form>

            // Example logic for blocking
            if (context.Request.Method == HttpMethods.Post ||
                context.Request.Method == HttpMethods.Put ||
                context.Request.Method == HttpMethods.Delete)
            {
                if (!context.Request.Headers.ContainsKey(Settings.CsrfToken))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync(Messages.CsrfTokenNotFound);
                    return false;
                }
            }

            return true;
        }
    }
}