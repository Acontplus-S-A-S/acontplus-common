using Microsoft.AspNetCore.Http;

namespace Services.Common.Extensions;

public interface IUserContext
{
    int GetUserId();
    int GetUserRoleId();
    string GetUserName();
    string GetEmail();
    string GetRoleName();
}

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public int GetUserId() { return httpContextAccessor.HttpContext!.User.GetUserId(); }
    public int GetUserRoleId() { return httpContextAccessor.HttpContext!.User.GetUserRoleId(); }
    public string GetUserName() { return httpContextAccessor.HttpContext?.User.GetUsername(); }
    public string GetEmail() { return httpContextAccessor.HttpContext?.User.GetEmail(); }
    public string GetRoleName() { return httpContextAccessor.HttpContext?.User.GetRoleName(); }
}
