using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _context;

    public AuthController(IConfiguration config, ApplicationDbContext context)
    {
        _config = config;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginRequest request)
    {
        if (request.Username != "admin" || request.Password != "admin123")
            return Unauthorized("Invalid credentials");

        var accessToken = GenerateAccessToken(request.Username);
        var refreshToken = await GenerateRefreshToken(request.Username);

        return Ok(new
        {
            accessToken,
            refreshToken = refreshToken.Token,
            expiresIn = 900 // 15 phút
        });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> Refresh(RefreshRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (storedToken == null)
            return Unauthorized("Refresh token not found");

        if (storedToken.IsRevoked)
            return Unauthorized("Refresh token has been revoked");

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            return Unauthorized("Refresh token has expired");

        // Revoke token cũ
        storedToken.IsRevoked = true;

        // Tạo token mới
        var newAccessToken = GenerateAccessToken(storedToken.Username);
        var newRefreshToken = await GenerateRefreshToken(storedToken.Username);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken.Token,
            expiresIn = 900
        });
    }

    [HttpPost("revoke")]
    public async Task<ActionResult> Revoke(RefreshRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (storedToken == null)
            return NotFound();

        storedToken.IsRevoked = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private string GenerateAccessToken(string username)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshToken(string username)
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Username = username,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
