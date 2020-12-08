using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace forum_authentication.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Certificate { get; set; }
    }
}
