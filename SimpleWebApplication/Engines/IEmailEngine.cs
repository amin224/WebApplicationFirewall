using SimpleWebApplication.Models;

namespace SimpleWebApplication.Engines;

public interface IEmailEngine
{
    Task SendEmailAsync(EmailInfo emailInfo);
}