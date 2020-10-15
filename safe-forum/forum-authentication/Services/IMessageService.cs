using forum_authentication.Dtos;
using forum_authentication.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace forum_authentication.Services
{
    public interface IMessageService
    {
        void SaveMessage(SendMessageDto message, string sender);
        Message[] GetMessages(string from, string requestor);
    }
}
