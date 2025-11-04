using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SurveyBasket.Dtos.Auth;
using SurveyBasket.Entities;
using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<Application_User> userManager;
        private readonly IConfiguration configuration;

        public AuthService(UserManager<Application_User> userManager, IConfiguration configuration)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<AuthResponse?> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return null; 
            var isPasswordValid = await userManager.CheckPasswordAsync(user, loginDto.PassWord);
            if (!isPasswordValid)
                return null; 

            var token = await CreateTokenAsync(user);

            return new AuthResponse(
                user.Id,
                user.Email,
                user.FristName,
                user.LastName,
                token,
                3600 
            );
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
        {
            var existingUser = await userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null) return null;

            var user = new Application_User
            {
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                FristName = registerDto.FirstName,
                LastName = registerDto.LastName
            };

            var result = await userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded) return null;

         

            var token = await CreateTokenAsync(user);

            return new AuthResponse(
                user.Id,
                user.Email,
                user.FristName,
                user.LastName,
                token,
                3600
            );
        }


        
        private async Task<string> CreateTokenAsync(Application_User user)
        {
            var claims = new List<Claim>
    {
    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
    new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty)
    };

            var roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = configuration["JwtOptions:secretKey"]
                      ?? throw new InvalidOperationException("JWT secretKey is missing!");
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));




            var creds = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["JwtOptions:issuer"],
                audience: configuration["JwtOptions:audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
