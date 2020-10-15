using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace forum_authentication.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Body { get; set; }
    }
}
