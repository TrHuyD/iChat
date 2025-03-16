using System.Security.Claims;

namespace iChat.BackEnd.Models.User
{
    public class UserJwtClaims
    {
        public string userID { get; set; }
        public UserJwtClaims(ClaimsPrincipal claims)
        {
            userID = claims.FindFirstValue(ClaimTypes.NameIdentifier);
        }
         

    }
}
