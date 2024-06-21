using Audit.Core;
using Audit.PostgreSql.Configuration;
using Audit.WebApi;
using Microsoft.AspNetCore.Mvc;
using WebApplicationFirewallUE.Models;

namespace WebApplicationFirewallUE.Static;

public class AuditConfiguration(IHttpContextAccessor httpContextAccessor)
{
    // Enables audit log with a global Action Filter
    public static void AddAudit(MvcOptions mvcOptions)
    {
        mvcOptions.AddAuditFilter(config => config
            .LogAllActions()
            .WithEventType("{verb} {controller}.{action}")
            .IncludeHeaders()
            .IncludeRequestBody()
            .IncludeResponseBody()
            .IncludeModelState()
            .IncludeResponseHeaders()
        );
    }
    
    // Configures what and how is logged or is not logged
    public static void ConfigureAudit(IServiceCollection serviceCollection)
    {
        var test = Environment.GetEnvironmentVariable("DbConnection");
        Audit.Core.Configuration.Setup()
            .UsePostgreSql(config => config
                .ConnectionString(Environment.GetEnvironmentVariable("DbConnection"))
                .Schema("public")
                .TableName("Audit")
                .IdColumnName("Id")
                .DataColumn("Data", DataType.JSONB)
                .LastUpdatedColumnName("updated_date")
                .CustomColumn("Attack",
                    ev => ev.CustomFields.Any()
                        ? ev.CustomFields.Where(x => x.Key == "Attack").Select(x => x.Value).FirstOrDefault()!
                            .ToString()
                        : "0")
                .CustomColumn("Type",
                    ev => ev.CustomFields.Any()
                        ? ev.CustomFields.Where(x => x.Key == "Type").Select(x => x.Value).FirstOrDefault()!
                            .ToString()
                        : "0"));
        Audit.Core.Configuration.AddCustomAction(ActionType.OnEventSaving, scope =>
        {
            var auditAction = scope.Event.GetWebApiAuditAction();
            if (auditAction == null)
            {
                return;
            }

            //Removing sensitive headers
            /*auditAction.Headers.Remove("Authorization");
            auditAction.Headers.Remove("Host");
            auditAction.Headers.Remove("Referer");
            auditAction.Headers.Remove("Postman-Token");*/
        });
    }

    private static void SetCustomField(HttpContext context, string key, object value)
    {
        var auditScope = context.GetCurrentAuditScope();
        auditScope.SetCustomField(key, value);
    }

    public void AuditCustomFields(LogTraceOperation logTrace)
    {
        SetCustomField(httpContextAccessor.HttpContext!, "Attack", logTrace.Attack);
        SetCustomField(httpContextAccessor.HttpContext!, "Type", logTrace.Type);
    }
}