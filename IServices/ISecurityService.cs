using HtmlAgilityPack;
using WebApplicationFirewallUE.Models;

namespace WebApplicationFirewallUE.IServices;

public interface ISecurityService
{
    void updateUser(int userId);
    bool IsSqlInjection(string input);
    bool ExampleSQLInjection(Users myusers);
    void AnastasiaFunction(int amin);
    bool IsXSS(string message);
    bool SanitizeHtml(string message);
}