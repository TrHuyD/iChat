using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.BackEnd.Services.Users.Infra.Neo4j;
using iChat.Data.Entities.Users;
using iChat.DTOs.Users.Auth;
using Microsoft.AspNetCore.Identity;

namespace iChat.BackEnd.Services.Users.Auth
{
    public class CreateUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly UserIdService _idGen;
        private readonly Neo4jCreateUserService _neo4j;
        public CreateUserService(UserManager<AppUser> userManager,UserIdService IdGen,Neo4jCreateUserService neo4J)
        {
            _userManager = userManager;
            _idGen = IdGen;
            _neo4j=neo4J;
        }
        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            var user = new AppUser
            {
                Id=_idGen.GenerateId(),
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            _ = _neo4j.CreateUserNode(user.Id);
            return true;
        }
    }
}
