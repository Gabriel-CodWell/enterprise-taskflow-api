using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Enterprise.TaskFlow.API.Controllers;
using Enterprise.TaskFlow.Application.Commands.CreateTask;
using FluentAssertions;
using Xunit;

namespace Enterprise.TaskFlow.IntegrationTests;

public class TasksControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TasksControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private record AuthResponse(string Token);

    private async Task<string> GetJwtTokenAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("testuser", "password"));
        loginResponse.EnsureSuccessStatusCode();
        var content = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return content!.Token;
    }

    [Fact]
    public async Task Post_CreateTask_WithValidCommand_ReturnsOk()
    {
        // Arrange
        var token = await GetJwtTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var command = new CreateTaskCommand("Integration Test Task", "Testing full flow.");

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
        var taskId = jsonResponse.GetProperty("id").GetGuid();
        taskId.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task Post_CreateTask_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var command = new CreateTaskCommand("Unauthorized Task", "Should fail.");

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
