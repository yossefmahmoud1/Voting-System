using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SurveyBasket.Abstraction;
using SurveyBasket.Dtos.Auth;
using SurveyBasket.Entities;
using SurveyBasket.Errors;
using SurveyBasket.Repositeryes.Interfaces;
using SurveyBasket.Services.Interfaces;
using SurveyBasket.Services.OptionsPattern;

namespace SurveyBasket.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtOptions _jwtOptions;

        public AuthService(IUserRepository userRepository, IOptions<JwtOptions> jwtoptions)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtOptions = jwtoptions?.Value ?? throw new ArgumentNullException(nameof(jwtoptions));
        }


        // -------------------- Login --------------------
        public async Task<Result<AuthResponse>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email, cancellationToken);
            if (user == null)
                return Result.Fail<AuthResponse>(UserErrors.InvalidCredentials);

            var isPasswordValid = await _userRepository.CheckPasswordAsync(user, loginDto.PassWord);
            if (!isPasswordValid)
                return Result.Fail<AuthResponse>(UserErrors.InvalidCredentials);

            var token = await CreateTokenAsync(user);

            var refreshToken = GenerateRefreshToken();
            user.refreshTokens.Add(refreshToken);
            await _userRepository.UpdateAsync(user, cancellationToken);

            var response = new AuthResponse(
            user.Id,
                user.Email,
                user.FristName,
                user.LastName,
                token,
                _jwtOptions.expiresInHours * 3600,
                refreshToken.Token,
                refreshToken.Expireson
            );
            return Result.Success(response);
        }

        // -------------------- Register --------------------
        public async Task<object?> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
        {
            // تحقق من Email أولاً
            if (await _userRepository.ExistsByEmailAsync(registerDto.Email, cancellationToken))
                return new RegistrationErrorResponse("Email already exists");

            // تحقق من UserName بعد الإيميل
            bool usernameExists = await _userRepository.ExistsByUserNameAsync(registerDto.UserName, cancellationToken);
            if (usernameExists)
            {
                // اقتراحات UserName فريدة
                var suggestions = new List<string>();
                int counter = 1;

                while (suggestions.Count < 3)
                {
                    string newName = $"{registerDto.UserName}{counter}";
                    bool exists = await _userRepository.ExistsByUserNameAsync(newName, cancellationToken);
                    if (!exists)
                        suggestions.Add(newName);
                    counter++;
                }

                return new UsernameTakenResponse(
                    "Username already exists",
                    suggestions
                );
            }

            // إنشاء المستخدم
            var user = new Application_User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                FristName = registerDto.FirstName,
                LastName = registerDto.LastName
            };

            try
            {
                await _userRepository.CreateAsync(user, registerDto.Password, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                // User creation failed due to validation errors
                return new RegistrationErrorResponse(ex.Message);
            }
            catch (Exception)
            {
                // Unexpected error occurred
                return new RegistrationErrorResponse("Unexpected error while creating the user");
            }

            // توليد JWT
            var token = await CreateTokenAsync(user);

            // توليد RefreshToken
            var refreshToken = GenerateRefreshToken();
            user.refreshTokens.Add(refreshToken);
            await _userRepository.UpdateAsync(user, cancellationToken);

            // رجع AuthResponse
            return new AuthResponse(
                user.Id,
                user.Email,
                user.FristName,
                user.LastName,
                token,
                _jwtOptions.expiresInHours * 3600,
                refreshToken.Token,
                refreshToken.Expireson
            );
        }
        // -------------------- Refresh Token --------------------
        public async Task<AuthResponse?> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            var handler = new JwtSecurityTokenHandler();
            string? userId;

            try
            {
                var jwtToken = handler.ReadJwtToken(token);
                userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return null;
            }
            catch
            {
                return null;
            }

            var user = await _userRepository.GetByIdWithRefreshTokensAsync(userId, cancellationToken);
            if (user == null)
                return null;

            var userRefreshToken = user.refreshTokens.SingleOrDefault(x => x.Token == refreshToken);
            if (userRefreshToken == null)
                return null;

            // التحقق من أن الريفريش توكن نشط وغير منتهي الصلاحية
            if (!userRefreshToken.Isactive)
                return null;

            // إلغاء الريفريش توكن القديم
            userRefreshToken.RevokedOn = DateTime.UtcNow;

            // توليد Access + Refresh Token جديدين
            var newToken = await CreateTokenAsync(user);
            var newRefreshToken = GenerateRefreshToken();
            user.refreshTokens.Add(newRefreshToken);

            await _userRepository.UpdateAsync(user, cancellationToken);

            return new AuthResponse(
                user.Id,
                user.Email,
                user.FristName,
                user.LastName,
                newToken,
                _jwtOptions.expiresInHours * 3600,
                newRefreshToken.Token,
                newRefreshToken.Expireson
            );
        }

        // -------------------- Revoke Refresh Token --------------------
        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken);
            if (user == null)
                return false;

            var token = user.refreshTokens.SingleOrDefault(t => t.Token == refreshToken && t.Isactive);
            if (token == null)
                return false;

            token.RevokedOn = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);

            return true;
        }


        // -------------------- JWT Token --------------------
        private async Task<string> CreateTokenAsync(Application_User user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
        new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
        new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
        new Claim(ClaimTypes.GivenName, user.FristName ?? string.Empty)
    };

            var roles = await _userRepository.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = _jwtOptions.secretKey ?? throw new InvalidOperationException("JWT secretKey is missing!");
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var expiresInHours = _jwtOptions.expiresInHours;

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.issuer,
                audience: _jwtOptions.audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiresInHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                Expireson = DateTime.UtcNow.AddDays(10), 
                Createdon = DateTime.UtcNow
            };
        }


    }
}
