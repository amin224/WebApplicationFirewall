namespace WebApplicationFirewallUE.Static;

public class Procedures
{
    public static readonly string GetIpCategory = $"{ApplicationConfiguration.DbConnectionSchemaName}.get_ip_category";

    public static readonly string AddNewCategory = $"{ApplicationConfiguration.DbConnectionSchemaName}.add_new_category";
}