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
        public string Sender { get; set; }
        public string Recipent { get; set; }
        public string Body { get; set; }
        public string Signature { get; set; }
    }
}
