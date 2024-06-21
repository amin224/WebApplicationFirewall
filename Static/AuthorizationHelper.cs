using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WebApplicationFirewallUE.Models;

namespace WebApplicationFirewallUE.Static;

public class AuthorizationHelper
{
    private readonly IConfiguration _configuration;

    public AuthorizationHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthTokenResponse GenerateJwtToken(string username)
    {
        if (username == "admin")
        {
            var authTokenResponse = new AuthTokenResponse();

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["JwtSettings:TokenExpiryHours"])),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            authTokenResponse.tokenString = tokenHandler.WriteToken(token);
            authTokenResponse.Expires = tokenDescriptor.Expires;
            authTokenResponse.Result = "Success";

            return authTokenResponse;
        }

        throw new UnauthorizedAccessException("Access is only allowed for the 'admin' user.");
    }
}