using System.Security.Claims;

public class UserClaimHelper
{
    ClaimsPrincipal _user;
    public UserClaimHelper(ClaimsPrincipal User)
    {
        _user = User;
    }
    public long GetUserId()
    {
        return long.Parse(_user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }
    public string GetUserIdStr()
    {
        return _user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}