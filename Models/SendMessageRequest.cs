﻿namespace QuickChat.Server.Models
{
    public class SendMessageRequest
    {
        public Guid ChatId { get; set; }
        public string Text { get; set; }
    }
}
