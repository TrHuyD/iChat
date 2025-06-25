using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class BucketingReport
    {
        public BucketingReport() { }
        public long channel_id { get; set; } 
        public int latest_bucket_id { get; set; }
        public long last_message_id { get; set; }
}
}
