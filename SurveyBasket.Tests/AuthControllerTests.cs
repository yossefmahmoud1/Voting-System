using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SurveyBasket.Controllers;
using SurveyBasket.Dtos.Auth;
using SurveyBasket.Entities;
using SurveyBasket.Persistence;
using SurveyBasket.Tests.Helpers;

namespace SurveyBasket.Tests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Application_User> _userManager;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        var scope = factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _userManager = scope.ServiceProvider.GetRequiredService<UserManager<Application_User>>();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsOk()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "Test@123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Email.Should().Be(registerDto.Email);
        authResponse.Token.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var user = new Application_User
        {
            UserName = "existinguser",
            Email = "existing@example.com",
            FristName = "Existing",
            LastName = "User"
        };
        await _userManager.CreateAsync(user, "Password@123");

        var registerDto = new RegisterDto
        {
            UserName = "newuser",
            Email = "existing@example.com",
            FirstName = "New",
            LastName = "User",
            Password = "Test@123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsBadRequestWithSuggestions()
    {
        // Arrange
        var user = new Application_User
        {
            UserName = "testuser",
            Email = "test@example.com",
            FristName = "Test",
            LastName = "User"
        };
        await _userManager.CreateAsync(user, "Password@123");

        var registerDto = new RegisterDto
        {
            UserName = "testuser",
            Email = "new@example.com",
            FirstName = "New",
            LastName = "User",
            Password = "Test@123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<UsernameTakenResponse>();
        result.Should().NotBeNull();
        result!.Suggestions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        // Arrange
        var user = new Application_User
        {
            UserName = "testuser",
            Email = "test@example.com",
            FristName = "Test",
            LastName = "User"
        };
        await _userManager.CreateAsync(user, "Test@123456");

        var loginDto = new LoginDto("test@example.com", "Test@123456");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Email.Should().Be(loginDto.Email);
        authResponse.Token.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto("nonexistent@example.com", "Test@123456");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var user = new Application_User
        {
            UserName = "testuser",
            Email = "test@example.com",
            FristName = "Test",
            LastName = "User"
        };
        await _userManager.CreateAsync(user, "CorrectPassword@123");

        var loginDto = new LoginDto("test@example.com", "WrongPassword@123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithValidTokens_ReturnsOk()
    {
        // Arrange - First login to get tokens
        var user = new Application_User
        {
            UserName = "testuser",
            Email = "test@example.com",
            FristName = "Test",
            LastName = "User"
        };
        await _userManager.CreateAsync(user, "Test@123456");

        var loginDto = new LoginDto("test@example.com", "Test@123456");

        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var refreshRequest = new RefreshTokenRequest(authResponse!.Token, authResponse.RefreshToken);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/Refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var newAuthResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        newAuthResponse.Should().NotBeNull();
        newAuthResponse!.Token.Should().NotBeNullOrEmpty();
        newAuthResponse.RefreshToken.Should().NotBeNullOrEmpty();
        newAuthResponse.RefreshToken.Should().NotBe(authResponse.RefreshToken); // Should be new token
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest("invalid.token.here", "invalid-refresh-token");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/Refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RevokeToken_WithValidToken_ReturnsOk()
    {
        // Arrange - First login to get refresh token
        var user = new Application_User
        {
            UserName = "testuser",
            Email = "test@example.com",
            FristName = "Test",
            LastName = "User"
        };
        await _userManager.CreateAsync(user, "Test@123456");

        var loginDto = new LoginDto("test@example.com", "Test@123456");

        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var revokeRequest = new RevokeTokenRequest(authResponse!.RefreshToken);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/Revoke", revokeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RevokeToken_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var revokeRequest = new RevokeTokenRequest("invalid-refresh-token");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/Revoke", revokeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

