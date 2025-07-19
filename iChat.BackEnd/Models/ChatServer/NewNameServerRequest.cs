using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace iChat.BackEnd.Models.ChatServer
{
    public class NewNameServerRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 32 characters.")]
        [RegularExpression(@"^[\p{L}0-9 _\-]{3,50}$", ErrorMessage = "Name can only contain letters, numbers, spaces, underscores, or hyphens.")]
        [FromForm(Name = "name")]
        public string Name { get; set; }
        [FromForm(Name = "id")]
        public string ServerId { get; set; }
    }
}
