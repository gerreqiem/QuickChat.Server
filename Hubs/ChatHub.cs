using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuickChat.Server.Data;
using QuickChat.Server.Models;
namespace QuickChat.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _db;

        public ChatHub(AppDbContext db)
        {
            _db = db;
        }

        public async Task SendMessage(Guid chatId, string message, bool isRead)
        {
            var userId = Guid.Parse(Context.UserIdentifier);
            var msg = new Message
            {
                Id = Guid.NewGuid(),
                ChatId = chatId,
                SenderId = userId,
                Text = message,
                SentAt = DateTime.UtcNow,
                IsRead = isRead
            };
            _db.Messages.Add(msg);
            await _db.SaveChangesAsync();
            Console.WriteLine($"Sent message: ID={msg.Id}, Sender={userId}, IsRead={isRead}");
            await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", msg.Id, userId, message, msg.SentAt, msg.IsRead);
        }

        public async Task JoinChat(Guid chatId)
        {
            var userId = Guid.Parse(Context.UserIdentifier);
            if (await _db.UserChats.AnyAsync(uc => uc.UserId == userId && uc.ChatId == chatId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
                var user = await _db.Users.FindAsync(userId);
                user.IsOnline = true;
                user.LastOnline = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                await Clients.OthersInGroup(chatId.ToString()).SendAsync("UserOnline", userId);

                var messages = await _db.Messages
                    .Where(m => m.ChatId == chatId && !m.IsRead)
                    .ToListAsync();
                foreach (var message in messages)
                {
                    message.IsRead = true;
                }
                await _db.SaveChangesAsync();
                if (messages.Any())
                {
                    await Clients.Group(chatId.ToString()).SendAsync("MessagesUpdated", messages.Select(m => m.Id).ToArray());
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Guid.Parse(Context.UserIdentifier);
            var user = await _db.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsOnline = false;
                user.LastOnline = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                var chats = await _db.UserChats.Where(uc => uc.UserId == userId).Select(uc => uc.ChatId).ToListAsync();
                foreach (var chatId in chats)
                {
                    await Clients.OthersInGroup(chatId.ToString()).SendAsync("UserOffline", userId);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
