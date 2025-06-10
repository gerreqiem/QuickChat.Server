using BCrypt.Net;
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
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app, IConfiguration configuration)
        {
            app.MapPost("/api/auth/register", async (AppDbContext db, [FromBody] RegisterRequest request) =>
            {
                if (await db.Users.AnyAsync(u => u.Username == request.Username))
                    return Results.BadRequest("Username already exists");

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = request.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    LastOnline = DateTime.UtcNow,
                    IsOnline = false
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();
                return Results.Ok();
            });

            app.MapPost("/api/auth/login", async (AppDbContext db, [FromBody] LoginRequest request) =>
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                    return Results.Unauthorized();

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
                return Results.Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
            });
        }
    }
}