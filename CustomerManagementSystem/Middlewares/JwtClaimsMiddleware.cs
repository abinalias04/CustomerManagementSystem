using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

public class JwtClaimsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _memoryCache;

    public JwtClaimsMiddleware(RequestDelegate next, IConfiguration config, IMemoryCache memoryCache)
    {
        _next = next;
        _config = config;
        _memoryCache = memoryCache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string? token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            try
            {
                //  Validate the token
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero 
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value
                          ?? jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var _sid = jwtToken.Claims.FirstOrDefault(c => c.Type == "sid")?.Value;

                //var sid = _memoryCache.Get($"sid_{userId}");

                //if (_sid != sid)
                //    throw new UnauthorizedAccessException();

                var cachedSid = _memoryCache.Get<string>($"sid_{userId}");
                if (string.IsNullOrEmpty(cachedSid) || _sid != cachedSid)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid or expired token (single-login enforcement)");
                    return;
                }


                if (!string.IsNullOrEmpty(userId))
                    context.Items["UserId"] = userId;

                if (!string.IsNullOrEmpty(role))
                    context.Items["UserRole"] = role;
            }
            catch
            {
                //  Invalid or expired token
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or expired token");
                return; // stop pipeline here
            }
        }

        await _next(context);
    }
}
