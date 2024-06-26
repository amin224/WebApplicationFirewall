using AspNetCoreRateLimit;
using Microsoft.Extensions.Options;

namespace SimpleWebApplication.Customs;

public class RateLimitConfigurationService : RateLimitConfiguration
{
    public RateLimitConfigurationService(IOptions<IpRateLimitOptions> ipOptions, IOptions<ClientRateLimitOptions> clientOptions) : base(ipOptions, clientOptions)
    {
        
    }
    
    public override void RegisterResolvers()
    {
        base.RegisterResolvers();

        //ClientResolvers.Add(new ClientHeaderResolveContributor(ClientRateLimitOptions.ClientIdHeader));
        ClientResolvers.Add(new CustomClientResolver());
    }
}