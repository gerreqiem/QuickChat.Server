namespace QuickChat.Server.Models
{
    public class CreateChatRequest
    {
        public string? Name { get; set; }
        public bool IsGroup { get; set; }
        public Guid? OtherUserId { get; set; }
    }
}
