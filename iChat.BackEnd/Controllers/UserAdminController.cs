using Microsoft.AspNetCore.Mvc;
using iChat.BackEnd.Services;
using iChat.BackEnd.Models;
using System.Threading.Tasks;
using iChat.BackEnd.Services.Users.Auth;
using Microsoft.AspNetCore.Identity;
using iChat.Data.Entities.Users;
using iChat.ViewModels.Users;
using Microsoft.EntityFrameworkCore;

namespace iChat.BackEnd.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserAdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;

        public UserAdminController(IUserService userService,UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _userService = userService;
        }
        [HttpGet("Paging")]
        public async Task<IActionResult> GetUsers(int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _userManager.Users
                .AsNoTracking()
                .Select(u => new UserVM
                {
                    Id = u.Id,
                    FirstName = u.Name,
                   // LastName = u.LastName
                });

            var users = await query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize + 1)
                .ToListAsync();
            bool hasNextPage = users.Count > pageSize;
            if (hasNextPage)
                users.RemoveAt(users.Count - 1);

            return Ok(new
            {
                Users = users,
                HasNextPage = hasNextPage,
                CurrentPage = page,
                PageSize = pageSize
            });
        }



    }
}
