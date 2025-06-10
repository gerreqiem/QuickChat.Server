namespace QuickChat.Server.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime LastOnline { get; set; }
        public bool IsOnline { get; set; }
        public List<UserChat> UserChats { get; set; } = new();
        public List<Message> MessagesSent { get; set; } = new();
    }
}
