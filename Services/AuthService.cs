using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WorkoutTracker.Data;
using WorkoutTracker.Dtos;
using WorkoutTracker.Entities;

namespace WorkoutTracker.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ServiceResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var allowRegistration = _configuration.GetValue<bool>("AllowRegistration", false);
        if (!allowRegistration)
            return new ServiceResponse<AuthResponse> { Success = false, Message = "Registration is currently disabled." };

        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower()))
            return new ServiceResponse<AuthResponse> { Success = false, Message = "Username is already taken." };

        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new ServiceResponse<AuthResponse> { Data = new AuthResponse { Token = CreateToken(user) }, Message = "User registered successfully." };
    }

    public async Task<ServiceResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return new ServiceResponse<AuthResponse> { Success = false, Message = "Invalid username or password." };

        return new ServiceResponse<AuthResponse> { Data = new AuthResponse { Token = CreateToken(user) }, Message = "Login successful." };
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration.GetSection("JwtSettings:Secret").Value!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(30), // Token valid for 30 days
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}