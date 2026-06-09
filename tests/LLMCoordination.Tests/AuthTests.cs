using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Tests;

public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RegisterTenant_CreatesTenantAndUser()
    {
        var email = $"admin-{Guid.NewGuid():N}@test.local";
        var response = await _client.PostAsJsonAsync("/api/auth/register-tenant", new RegisterTenantRequest
        {
            TenantName = "Test Tenant",
            DomainType = DomainType.Healthcare,
            AdminEmail = email,
            AdminPassword = "Test123!",
            AdminFullName = "Test Admin"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<RegisterTenantResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.TenantId);
        Assert.NotEqual(Guid.Empty, body.UserId);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        await _factory.SeedAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "admin@demo.local",
            Password = "Demo123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
        Assert.Equal("Admin", body.Role);
    }

    [Fact]
    public async Task Login_InvalidPassword_Returns401()
    {
        await _factory.SeedAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "admin@demo.local",
            Password = "WrongPassword!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TenantMe_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/tenant/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TenantMe_WithToken_ReturnsTenant()
    {
        await _factory.SeedAsync();
        var token = await LoginAsync("admin@demo.local", "Demo123!");

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/tenant/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TenantInfoDto>();
        Assert.NotNull(body);
        Assert.Equal("Demo Healthcare", body.Name);
    }

    private async Task<string> LoginAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }
}
