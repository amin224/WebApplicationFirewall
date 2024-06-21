using WebApplicationFirewallUE.Models;

namespace WebApplicationFirewallUE.IServices;

public interface IFileInclusionService
{
    Task<bool> CheckFileInclusion(IFormFile formFile);
    Task<bool> SafeRemoteFileInclude(string remoteUrl);
}