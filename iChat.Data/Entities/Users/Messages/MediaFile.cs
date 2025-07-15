using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Users.Messages
{
    public class MediaFile
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(64)]
        public string Hash { get; set; } = null!;

        [Required]
        [MaxLength(512)]
        public string Url { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string ContentType { get; set; } = null!;

        public int? Width { get; set; }

        public int? Height { get; set; }

        public int SizeBytes { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public long? UploaderUserId { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
