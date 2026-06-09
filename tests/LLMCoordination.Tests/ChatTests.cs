using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LLMCoordination.Application.Contracts;

namespace LLMCoordination.Tests;

public class ChatTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ChatTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SendMessage_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/chat/message", new ChatRequest
        {
            Message = "Give me the list of patients in card view",
            ViewPreference = "card"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_EmptyMessage_Returns400()
    {
        await _factory.SeedAsync();
        var token = await LoginAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/chat/message");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new ChatRequest { Message = "   " });
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_PatientList_ReturnsCardView()
    {
        await _factory.SeedAsync();
        var token = await LoginAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/chat/message");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new ChatRequest
        {
            Message = "Give me the list of patients in card view",
            ViewPreference = "card"
        });

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ChatResponse>();
        Assert.NotNull(body);
        Assert.Equal("success", body.Status);
        Assert.Equal("card", body.ViewType);
        Assert.Equal("patient.list", body.Intent);
        Assert.Equal("api.query.execute", body.Skill);

        var dataJson = JsonSerializer.Serialize(body.Data);
        Assert.Contains("cards", dataJson, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> LoginAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "admin@demo.local",
            Password = "Demo123!"
        });

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }
}
