namespace IdentityService.Api;

public interface IIdentityService
{
    Task<LoginResponseModel> Login(LoginRequestModel requestModel);
}
