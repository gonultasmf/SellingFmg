using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Api;

public class IdentityService : IIdentityService
{
    public Task<LoginResponseModel> Login(LoginRequestModel requestModel)
    {
        // Db'den kullanıcıya dair bilgilerin alabilirsiniz.

        var claims = new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, requestModel.UserName),
            new Claim(ClaimTypes.Name, "Mustafa Gönültaş"),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(" "));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.Now.AddDays(10);

        var token = new JwtSecurityToken(claims: claims, expires: expiry, signingCredentials: creds, notBefore: DateTime.Now);

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(token);

        LoginResponseModel response = new()
        {
            UserName = requestModel.UserName,
            UserToken = encodedJwt,
        };

        return Task.FromResult(response);
    }
}
