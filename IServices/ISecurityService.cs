namespace WebApplicationFirewallUE.IServices;

public interface ISecurityService
{
    bool IsSqlInjection(string input);
    bool IsXss(string message);
}