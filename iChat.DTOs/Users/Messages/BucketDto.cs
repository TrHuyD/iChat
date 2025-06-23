using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class BucketDto
    {
        public int BucketId { get; set; }
        public long MessageCount { get; set; }
        public long FirstSequence { get; set; }
        public long LastSequence { get; set; }
        public decimal BucketSizeMb { get; set; }
        public string CacheKey { get; set; } = string.Empty;
    }
}
