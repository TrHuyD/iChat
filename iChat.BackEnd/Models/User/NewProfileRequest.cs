using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.BackEnd.Models.User
{
    public class NewProfileRequest
    {
        [Required(ErrorMessage = "Nickname is required.")]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "Nickname must be between 3 and 32 characters.")]
        [RegularExpression(@"^[\p{L}0-9 _\-]{3,50}$", ErrorMessage = "Nickname can only contain letters, numbers, spaces, underscores, or hyphens.")]
        [FromForm(Name ="newName")]
        public string UserName { get; set; }
        [FromForm(Name ="file")]
        public IFormFile File { get; set; }
    }
}
