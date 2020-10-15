using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace forum_authentication.Dtos
{
    public class SendMessageDto
    {
        public string Body { get; set; }
        public string Recipent { get; set; }
    }
}
