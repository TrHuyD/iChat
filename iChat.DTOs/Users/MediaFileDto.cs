using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users
{
    public class MediaFileDto
    {
        public int Id { get; set; }
        public string Hash { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int SizeBytes { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
