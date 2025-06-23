using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class RawBucketResult
    {
        public int bucket_id { get; set; }
        public long channel_id { get; set; }
        public long message_count { get; set; }

        public string messages { get; set; }
    }
}
