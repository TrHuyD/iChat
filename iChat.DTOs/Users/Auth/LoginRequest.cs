using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [RegularExpression(@"\S+", ErrorMessage = "Username cannot be empty or whitespace")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"\S+", ErrorMessage = "Password cannot be empty or whitespace")]
        public string Password { get; set; } = string.Empty;
    }
}
