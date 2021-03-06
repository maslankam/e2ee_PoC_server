﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace forum_authentication.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Certificate { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
    }
}
