using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SurveyBasket.Controllers;
using SurveyBasket.Dtos.Polls;
using SurveyBasket.Entities;
using SurveyBasket.Persistence;
using SurveyBasket.Tests.Helpers;

namespace SurveyBasket.Tests;

public class PollsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _context;

    public PollsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        var scope = factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Arrange
        await SeedPolls();

        // Act
        var response = await _client.GetAsync("/api/Polls");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var polls = await response.Content.ReadFromJsonAsync<IEnumerable<PollResponse>>();
        polls.Should().NotBeNull();
        polls.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task Get_WithValidId_ReturnsOk()
    {
        // Arrange
        var poll = new Poll
        {
            Title = "Test Poll",
            Summary = "Test Summary",
            StartsAt = DateOnly.FromDateTime(DateTime.UtcNow),
            EndsAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            IsPublished = true
        };
        _context.Polls.Add(poll);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/Polls/{poll.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pollResponse = await response.Content.ReadFromJsonAsync<PollResponse>();
        pollResponse.Should().NotBeNull();
        pollResponse!.Title.Should().Be(poll.Title);
    }

    [Fact]
    public async Task Get_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/Polls/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Add_WithValidData_ReturnsCreated()
    {
        // Arrange
        var pollRequest = new PollRequest(
            "New Poll",
            "New Summary",
            false,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/Polls", pollRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var location = response.Headers.Location;
        location.Should().NotBeNull();

        var poll = await response.Content.ReadFromJsonAsync<Poll>();
        poll.Should().NotBeNull();
        poll!.Title.Should().Be(pollRequest.Title);
    }

    [Fact]
    public async Task Update_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var poll = new Poll
        {
            Title = "Original Title",
            Summary = "Original Summary",
            StartsAt = DateOnly.FromDateTime(DateTime.UtcNow),
            EndsAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            IsPublished = false
        };
        _context.Polls.Add(poll);
        await _context.SaveChangesAsync();

        var updateRequest = new PollRequest(
            "Updated Title",
            "Updated Summary",
            false,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14))
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Polls/{poll.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var updatedPoll = await _context.Polls.FindAsync(poll.Id);
        updatedPoll.Should().NotBeNull();
        updatedPoll!.Title.Should().Be(updateRequest.Title);
        updatedPoll.Summary.Should().Be(updateRequest.Summary);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = new PollRequest(
            "Updated Title",
            "Updated Summary",
            false,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14))
        );

        // Act
        var response = await _client.PutAsJsonAsync("/api/Polls/99999", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var poll = new Poll
        {
            Title = "Poll To Delete",
            Summary = "Summary",
            StartsAt = DateOnly.FromDateTime(DateTime.UtcNow),
            EndsAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            IsPublished = false
        };
        _context.Polls.Add(poll);
        await _context.SaveChangesAsync();
        var pollId = poll.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/Polls/{pollId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var deletedPoll = await _context.Polls.FindAsync(pollId);
        deletedPoll.Should().BeNull();
    }

    [Fact]
    public async Task Delete_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/Polls/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TogglePublish_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var poll = new Poll
        {
            Title = "Test Poll",
            Summary = "Test Summary",
            StartsAt = DateOnly.FromDateTime(DateTime.UtcNow),
            EndsAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            IsPublished = false
        };
        _context.Polls.Add(poll);
        await _context.SaveChangesAsync();
        var originalStatus = poll.IsPublished;

        // Act
        var response = await _client.PutAsync($"/api/Polls/{poll.Id}/togglePublish", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify toggle
        await _context.Entry(poll).ReloadAsync();
        poll.IsPublished.Should().Be(!originalStatus);
    }

    [Fact]
    public async Task TogglePublish_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.PutAsync("/api/Polls/99999/togglePublish", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task SeedPolls()
    {
        if (await _context.Polls.AnyAsync())
            return;

        var polls = new List<Poll>
        {
            new Poll
            {
                Title = "Poll 1",
                Summary = "Summary 1",
                StartsAt = DateOnly.FromDateTime(DateTime.UtcNow),
                EndsAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                IsPublished = true
            },
            new Poll
            {
                Title = "Poll 2",
                Summary = "Summary 2",
                StartsAt = DateOnly.FromDateTime(DateTime.UtcNow),
                EndsAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
                IsPublished = false
            }
        };

        _context.Polls.AddRange(polls);
        await _context.SaveChangesAsync();
    }
}

