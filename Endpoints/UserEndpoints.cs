using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuickChat.Server.Data;
using QuickChat.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuickChat.Server.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this WebApplication app, IConfiguration configuration)
        {
            app.MapGet("/api/users", async (AppDbContext db) =>
            {
                var users = await db.Users.Select(u => new { u.Id, u.Username, u.IsOnline }).ToListAsync();
                return Results.Ok(users);
            }).RequireAuthorization();

            app.MapPut("/api/users/{userId}", async (Guid userId, AppDbContext db, HttpContext http, [FromBody] UpdateUsernameRequest request) =>
            {
                var currentUserId = Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (userId != currentUserId)
                    return Results.Forbid();

                var user = await db.Users.FindAsync(userId);
                if (user == null)
                    return Results.NotFound("User not found");

                if (await db.Users.AnyAsync(u => u.Username == request.NewUsername && u.Id != userId))
                    return Results.BadRequest("Username already exists");

                var oldUsername = user.Username;
                user.Username = request.NewUsername;

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                };
                var token = new JwtSecurityToken(
                    configuration["Jwt:Issuer"],
                    configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: creds);

                await db.SaveChangesAsync();
                return Results.Ok(new { user.Id, user.Username, Token = new JwtSecurityTokenHandler().WriteToken(token) });
            }).RequireAuthorization();
        }
    }
}