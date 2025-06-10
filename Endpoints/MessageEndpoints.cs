using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickChat.Server.Data;
using QuickChat.Server.Models;
using System.Security.Claims;

namespace QuickChat.Server.Endpoints
{
    public static class MessageEndpoints
    {
        public static void MapMessageEndpoints(this WebApplication app)
        {
            app.MapGet("/api/messages/{chatId}", async (Guid chatId, AppDbContext db, HttpContext http, int page = 1, int size = 10) =>
            {
                var userId = Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var messages = await db.Messages
                    .Where(m => m.ChatId == chatId)
                    .OrderByDescending(m => m.SentAt)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .Select(m => new
                    {
                        m.Id,
                        m.ChatId,
                        m.SenderId,
                        m.Text,
                        m.SentAt,
                        m.IsRead
                    })
                    .ToListAsync();
                return Results.Ok(messages);
            }).RequireAuthorization();

            app.MapPost("/api/messages", async (AppDbContext db, HttpContext http, [FromBody] SendMessageRequest request) =>
            {
                var userId = Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var message = new Message
                {
                    Id = Guid.NewGuid(),
                    ChatId = request.ChatId,
                    SenderId = userId,
                    Text = request.Text,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };
                db.Messages.Add(message);
                await db.SaveChangesAsync();
                return Results.Ok(new { message.Id, message.IsRead });
            }).RequireAuthorization();
        }
    }
}