namespace WebApplicationFirewallUE.Models;

public class AuthTokenResponse
{
    public string Result { get; set; }
    public string tokenString { get; set; }
    public DateTime? Expires { get; set; }
}