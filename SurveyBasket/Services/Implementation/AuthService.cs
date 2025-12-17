using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SurveyBasket.Abstraction;
using SurveyBasket.Abstraction.Consts;
using SurveyBasket.Dtos.Auth;
using SurveyBasket.Entities;
using SurveyBasket.Errors;
using SurveyBasket.Helpers;
using SurveyBasket.Repositeryes.Implementation;
using SurveyBasket.Repositeryes.Interfaces;
using SurveyBasket.Services.Interfaces;
using SurveyBasket.Services.OptionsPattern;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace SurveyBasket.Services.Implementation
{
    public class AuthService(
    ILogger<AuthService> logger,IUserRepository userRepository,SignInManager<Application_User> signInManager,IOptions<JwtOptions> jwtOptions ,IEmailSender emailSender , ITokenClaimsBuilder claimsBuilder,
IHttpContextAccessor httpContextAccessor , ApplicationDbContext applicationDbContext) : IAuthService
    {
        private readonly ILogger<AuthService> _logger = logger;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly SignInManager<Application_User> _signInManager = signInManager;
        private readonly IOptions<JwtOptions> jwtOptions = jwtOptions;
        private readonly IEmailSender emailSender = emailSender;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
        private readonly ApplicationDbContext applicationDbContext = applicationDbContext;
        private readonly JwtOptions _jwtOptions = jwtOptions.Value;
        private readonly ITokenClaimsBuilder _claimsBuilder = claimsBuilder;




        // -------------------- Login --------------------
        public async Task<Result<AuthResponse>> LoginAsync(
            LoginDto loginDto,
            CancellationToken cancellationToken = default)
        {
            if (await _userRepository.GetByEmailAsync(loginDto.Email, cancellationToken) is not { } user)
                return Result.Fail<AuthResponse>(UserErrors.InvalidCredentials);
            //disabled HIS account
            if (user.IsDisabled)
                return Result.Fail<AuthResponse>(UserErrors.UserDisabled);

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                loginDto.PassWord,
                lockoutOnFailure: true
            );
            //locked out HIS account
            if (result.IsLockedOut)
                return Result.Fail<AuthResponse>(UserErrors.UserLockedOut);
            //not confirmed HIS email
            if (result.IsNotAllowed)
                return Result.Fail<AuthResponse>(UserErrors.EmailNotConfirmed);
            //invalid credentials
            if (!result.Succeeded)
                return Result.Fail<AuthResponse>(UserErrors.InvalidCredentials);

            var token = await CreateTokenAsync(user);

            var refreshToken = GenerateRefreshToken();
            user.refreshTokens.Add(refreshToken);
            //remove old refresh tokens
            await _userRepository.UpdateAsync(user, cancellationToken);
            //log HIS login
            return Result.Success(new AuthResponse(
                user.Id,
                user.Email,
                user.FristName,
                user.LastName,
                token,
                _jwtOptions.expiresInHours * 3600,
                refreshToken.Token,
                refreshToken.Expireson
            ));
        }




        // -------------------- Register --------------------
        public async Task<Result> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default)
        {
            var emailIsExists = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken);

            if (emailIsExists)
                return Result.Fail(UserErrors.EmailAlreadyExists);

            var usernameIsExists = await _userRepository.ExistsByUserNameAsync(request.UserName, cancellationToken);

            if (usernameIsExists)
            {
                var suggestions = await GenerateUsernameSuggestionsAsync(request.UserName, cancellationToken);
                return Result.Fail(UserErrors.UsernameTaken(suggestions));
            }

            var user = new Application_User
            {
                Email = request.Email,
                UserName = request.UserName,
                FristName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = false
            };

            try
            {
                await _userRepository.CreateAsync(user, request.Password, cancellationToken);

                var code = await _userRepository.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                _logger.LogInformation("User registered successfully. UserId: {UserId}, ConfirmationCode: {code}", user.Id, code);
             await SendConfirmationEmailAsync(user , code);
                return Result.Success("Account created successfully. Please confirm your email.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Failed to create user: {Email}", request.Email);
                return Result.Fail(UserErrors.RegisterFailed(ex.Message));
            }
        }



        // -------------------- Confirm Email --------------------
        public async Task<Result> ConfirmEmailAsync(ConfirmEmailDto dto, CancellationToken cancellationToken = default)
        {
            // 1) نجيب المستخدم بالمعرف
            var user = await _userRepository.GetByIdAsync(dto.Id, cancellationToken);
            if (user == null)
                return Result.Fail(UserErrors.UserNotFound);
            if (user.EmailConfirmed)
                return Result.Fail(UserErrors.DuplicatedConfirmation);
            // 2) نفك التوكن من Base64
            string decodedToken;
            try
            {
                var decodedBytes = WebEncoders.Base64UrlDecode(dto.Code);
                decodedToken = Encoding.UTF8.GetString(decodedBytes);
            }
            catch
            {
                return Result.Fail(UserErrors.InvalidConfirmationToken);
            }

            // 3) نعمل Confirm Email
            var result = await _userRepository.ConfirmEmailAsync(user, decodedToken);

            if (!result)
                return Result.Fail(UserErrors.InvalidConfirmationToken);

            await _userRepository.AddToRoleAsync(user, DefaultRoles.Member);

            return Result.Success();
        }

        // -------------------- Refresh Token --------------------
        public async Task<Result<AuthResponse>> RefreshTokenAsync(
     string token,
     string refreshToken,
     CancellationToken cancellationToken = default)
        {
            var handler = new JwtSecurityTokenHandler();
            string? userId;

            try
            {
                var jwtToken = handler.ReadJwtToken(token);
                userId = jwtToken.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                    ?.Value;

                if (userId == null)
                    return Result.Fail<AuthResponse>(UserErrors.InvalidToken);
            }
            catch
            {
                return Result.Fail<AuthResponse>(UserErrors.InvalidToken);
            }

            var user = await _userRepository
                .GetByIdWithRefreshTokensAsync(userId, cancellationToken);

            if (user == null)
                return Result.Fail<AuthResponse>(UserErrors.UserNotFound);

            if (user.IsDisabled)
                return Result.Fail<AuthResponse>(UserErrors.UserDisabled);
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                return Result.Fail<AuthResponse>(UserErrors.UserLockedOut); ;


            var userRefreshToken = user.refreshTokens
                .SingleOrDefault(x => x.Token == refreshToken);

            if (userRefreshToken == null)
                return Result.Fail<AuthResponse>(UserErrors.InvalidConfirmationToken);

            // التحقق من أن الريفريش توكن نشط وغير منتهي الصلاحية
            if (!userRefreshToken.Isactive ||
                userRefreshToken.Expireson < DateTime.UtcNow)
                return Result.Fail<AuthResponse>(UserErrors.InvalidConfirmationToken);

            // إلغاء الريفريش توكن القديم
            userRefreshToken.RevokedOn = DateTime.UtcNow;

            var newToken = await CreateTokenAsync(user);
            var newRefreshToken = GenerateRefreshToken();

            user.refreshTokens.Add(newRefreshToken);

            await _userRepository.UpdateAsync(user, cancellationToken);

            return Result.Success(new AuthResponse(
                user.Id,
                user.Email,
                user.FristName,
                user.LastName,
                newToken,
                _jwtOptions.expiresInHours * 3600,
                newRefreshToken.Token,
                newRefreshToken.Expireson
            ));
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

        // -------------------- ResendConfirm Email Validator --------------------
        public async Task<Result> ResendConfirmationEmail(ResendConfirmEmailDto dto, CancellationToken cancellationToken = default)
        {
            // 1) نجيب المستخدم بالمعرف
           var user=  await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);  
            if (user == null)
                return Result.Fail(UserErrors.UserNotFound);
            if (user.EmailConfirmed)
                return Result.Fail(UserErrors.DuplicatedConfirmation);
            // 2) نولد كود جديد
            var code = await _userRepository.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            _logger.LogInformation("Resent confirmation email. UserId: {UserId}, ConfirmationCode: {code}", user.Id, code);
           await SendConfirmationEmailAsync(user , code);

            return Result.Success();
        }



        public async Task<Result> SendResetPasswordCodeAsync(string Email)
        {
            if(await _userRepository.GetByEmailAsync(Email) is not { } user)
                return Result.Success();
            if(!user.EmailConfirmed)
                return Result.Fail(UserErrors.EmailNotConfirmed);
            var code = await _userRepository.GenratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            _logger.LogInformation("Sent password reset code. UserId: {UserId}, ResetCode: {code}", user.Id, code);
            await SendResetPasswordByEmail(user , code);

            return Result.Success();
        }
        public async Task<Result> ResetPassWordAsync(ResetPasswordRequest resetPasswordRequest)
        {
            var user = await _userRepository.GetByEmailAsync(resetPasswordRequest.Email);
            if (user is null || !user.EmailConfirmed)
                return Result.Fail(UserErrors.InvalidConfirmationToken);
            IdentityResult result;
            try
            {
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordRequest.Code));
                result = await _userRepository.ResetPasswordAsync(user, code, resetPasswordRequest.NewPassword);
            }
            catch (FormatException)
            {
                result = IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidToken",
                    Description = "Invalid or expired reset token"
                });
            }
            if (result.Succeeded)
            {
                return Result.Success();
            }
            var error=result.Errors.First();
            return Result.Fail(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));
        }





        //    // -------------------- JWT Token --------------------
        private async Task<string> CreateTokenAsync(Application_User user)
        {
            var claims = await _claimsBuilder.BuildClaimsAsync(user);

            var secretKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.secretKey!)
            );

            var creds = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.issuer,
                audience: _jwtOptions.audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtOptions.expiresInHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // -------------------- Refresh Token --------------------

        private static RefreshToken GenerateRefreshToken()

        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expireson = DateTime.UtcNow.AddDays(10), 
                Createdon = DateTime.UtcNow
            };
        }

 
        // -------------------- Username Suggestions --------------------
        private async Task<List<string>> GenerateUsernameSuggestionsAsync(string baseUsername, CancellationToken cancellationToken = default)
        {
            var suggestions = new List<string>();
            var random = new Random();
            
            // Generate suggestions by appending numbers
            for (int i = 1; i <= 5; i++)
            {
                var suggestion = $"{baseUsername}{random.Next(100, 999)}";
                if (!await _userRepository.ExistsByUserNameAsync(suggestion, cancellationToken))
                {
                    suggestions.Add(suggestion);
                    if (suggestions.Count >= 3) break;
                }
            }

            // If we don't have enough suggestions, try with different patterns
            if (suggestions.Count < 3)
            {
                for (int i = 1; i <= 5 && suggestions.Count < 3; i++)
                {
                    var suggestion = $"{baseUsername}_{random.Next(1000, 9999)}";
                    if (!await _userRepository.ExistsByUserNameAsync(suggestion, cancellationToken))
                    {
                        suggestions.Add(suggestion);
                    }
                }
            }

            // If still not enough, add timestamp-based suggestions
            if (suggestions.Count < 3)
            {
                var timestamp = DateTime.UtcNow.Ticks % 10000;
                var suggestion = $"{baseUsername}{timestamp}";
                if (!await _userRepository.ExistsByUserNameAsync(suggestion, cancellationToken))
                {
                    suggestions.Add(suggestion);
                }
            }

            return suggestions;
        }

        private async  Task SendConfirmationEmailAsync(Application_User user, string code)
        {
            var Origin = httpContextAccessor.HttpContext?.Request.Headers.Origin;
            var EmailBody = EmailBodyBuilder.GenrateEmailBody("EmailConfirmation", new Dictionary<string, string>
{
    { "{{name}}", user.FristName },
    { "{{action_url}}", $"{Origin}/auth/email/emailConfiramtion?userId={user.Id}&code={code}" }
});

            BackgroundJob.Enqueue(()=>emailSender.SendEmailAsync(user.Email!, "Confirm your email", EmailBody));

            await Task.CompletedTask;
        }

        private async Task SendResetPasswordByEmail(Application_User user, string code)
        {
            var Origin = httpContextAccessor.HttpContext?.Request.Headers.Origin;
            var EmailBody = EmailBodyBuilder.GenrateEmailBody("EmailConfirmation", new Dictionary<string, string>
{
    { "{{name}}", user.FristName },
    { "{{action_url}}", $"{Origin}/auth/email/ForgetPassword?userId={user.Email}&code={code}" }
});

            BackgroundJob.Enqueue(() => emailSender.SendEmailAsync(user.Email!, "Change your Password", EmailBody));

            await Task.CompletedTask;
        }


    }
}
