﻿using forum_authentication.Dtos;
using forum_authentication.Entities;
using forum_authentication.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace forum_authentication.Services
{
    public class MessageService : IMessageService
    {
        private DataContext _context;

        public MessageService(DataContext context)
        {
            _context = context;
        }

        public Message[] GetMessages(string from, string requestor, int limit)
        {
            return _context.Messages.Where(m => m.Sender == from && m.Recipent == requestor)
                .OrderBy(x => x.Timestamp)
                //.TakeLast(limit)
                .ToArray();
        }

        public void SaveMessage(SendMessageDto sendMessageDto, string sender)
        {
            if(!_context.Users.Any(u => u.Username == sendMessageDto.Recipent))
            {
                throw new ApplicationException("Sender not found");
            }
            var message = new Message
            {
                Body = sendMessageDto.Body,
                Recipent = sendMessageDto.Recipent,
                Sender = sender,
                Timestamp = DateTime.UtcNow,
                Signature = sendMessageDto.Signature
            };

            _context.Messages.Add(message);
            _context.SaveChanges();
        }
    }
}
