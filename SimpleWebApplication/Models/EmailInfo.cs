namespace SimpleWebApplication.Models;

public class EmailInfo
{
    public string Subject { get; set; }
    public string Message { get; set; }
    public string Bcc { get; set; }
    public string Cc { get; set; }
    public string To { get; set; }
}