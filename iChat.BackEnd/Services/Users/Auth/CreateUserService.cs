using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.BackEnd.Services.Users.Infra.Neo4jService;
using iChat.Data.Entities.Users;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Auth;
using Microsoft.AspNetCore.Identity;

namespace iChat.BackEnd.Services.Users.Auth
{
    public class CreateUserService : IRegisterService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly UserIdService _idGen;

        public CreateUserService(UserManager<AppUser> userManager,UserIdService IdGen)
        {
            _userManager = userManager;
            _idGen = IdGen;

        }
        public async Task<OperationResult> RegisterAsync(RegisterRequest request)
        {
            var user = new AppUser
            {
                Id = _idGen.GenerateId().Id,
                UserName = request.UserName,
                Email = request.Email,
                Name = request.Name,
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.ToList();
                foreach (var error in errors)
                {
                    if (error.Code == "DuplicateUserName")
                        return OperationResult.Fail("username_exists", "The username is already taken.");

                    if (error.Code == "DuplicateEmail")
                        return OperationResult.Fail("email_exists", "An account with this email already exists.");
                }
                var combined = string.Join(", ", errors.Select(e => e.Description));
                return OperationResult.Fail("identity_error", combined);
            }

            return OperationResult.Ok();
        }

    }
}
