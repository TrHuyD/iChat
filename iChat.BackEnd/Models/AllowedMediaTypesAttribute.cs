using System.ComponentModel.DataAnnotations;

namespace iChat.BackEnd.Models
{
    public class AllowedMediaTypesAttribute : ValidationAttribute
    {
        private static readonly string[] _allowedMediaMimeTypes = new[]
{
        "image/jpeg", "image/png", "image/webp", "image/gif",
        "video/mp4", "video/webm", "video/quicktime",
        "audio/mpeg", "audio/ogg", "audio/wav"
    };

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file == null)
                return ValidationResult.Success; 

            if (!_allowedMediaMimeTypes.Contains(file.ContentType.ToLower()))
            {
                return new ValidationResult($"Unsupported media type: {file.ContentType}");
            }

            return ValidationResult.Success;
        }
    }
}
