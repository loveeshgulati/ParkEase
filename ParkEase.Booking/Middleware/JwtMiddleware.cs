using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ParkEase.Booking.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        var token = authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
            ? authHeader["Bearer ".Length..].Trim()
            : authHeader?.Trim();

        if (!string.IsNullOrEmpty(token))
            AttachUserToContext(context, token);

        await _next(context);
    }

    private void AttachUserToContext(HttpContext context, string token)
    {
        try
        {
            var jwtSecret = (_configuration["JWT_SECRET"] ?? _configuration["Jwt:Secret"]!).Trim();
            var key = Encoding.UTF8.GetBytes(jwtSecret);
            var handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JWT_ISSUER"] ?? _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JWT_AUDIENCE"] ?? _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            context.Items["UserId"] = int.Parse(
                jwtToken.Claims.First(c => c.Type == "userId").Value);
        }
        catch { }
    }
}
