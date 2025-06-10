namespace QuickChat.Server.Models
{
    public class Chat
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public bool IsGroup { get; set; }
        public List<UserChat> UserChats { get; set; } = new();
        public List<Message> Messages { get; set; } = new();
    }
}
