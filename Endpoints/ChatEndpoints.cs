using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickChat.Server.Data;
using QuickChat.Server.Models;
using System.Security.Claims;

namespace QuickChat.Server.Endpoints
{
    public static class ChatEndpoints
    {
        public static void MapChatEndpoints(this WebApplication app)
        {
            app.MapGet("/api/chats", async (AppDbContext db, HttpContext http) =>
            {
                var userId = Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var chats = await db.UserChats
                    .Where(uc => uc.UserId == userId)
                    .Select(uc => new
                    {
                        uc.Chat.Id,
                        uc.Chat.Name,
                        uc.Chat.IsGroup,
                        UserIds = uc.Chat.UserChats.Select(uc2 => uc2.UserId).ToArray()
                    })
                    .ToListAsync();
                return Results.Ok(chats);
            }).RequireAuthorization();

            app.MapPost("/api/chats", async (AppDbContext db, HttpContext http, [FromBody] CreateChatRequest request) =>
            {
                var userId = Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var chat = new Chat
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    IsGroup = request.IsGroup
                };
                db.Chats.Add(chat);
                db.UserChats.Add(new UserChat { UserId = userId, ChatId = chat.Id });
                if (request.OtherUserId.HasValue)
                    db.UserChats.Add(new UserChat { UserId = request.OtherUserId.Value, ChatId = chat.Id });
                await db.SaveChangesAsync();

                return Results.Ok(new
                {
                    chat.Id,
                    chat.Name,
                    chat.IsGroup
                });
            }).RequireAuthorization();
        }
    }
}