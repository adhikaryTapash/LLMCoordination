using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LLMCoordination.Application.Contracts;

namespace LLMCoordination.Tests;

public class SwaggerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SwaggerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SwaggerParser_ExtractsEndpoints()
    {
        var rawJson = await File.ReadAllTextAsync(GetFixturePath("sample-openapi.json"));
        var parser = new LLMCoordination.Application.Services.SwaggerParserService();
        var result = parser.Parse(rawJson);

        Assert.Equal(3, result.Endpoints.Count);
    }

    [Fact]
    public async Task RegisterSwagger_StoresInDatabase()
    {
        await _factory.SeedAsync();
        var token = await LoginAsync();
        var rawJson = await File.ReadAllTextAsync(GetFixturePath("sample-openapi.json"));

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/swagger/register");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new RegisterSwaggerRequest
        {
            Name = "Healthcare API",
            RawJson = rawJson,
            Domain = "healthcare"
        });

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var document = await response.Content.ReadFromJsonAsync<SwaggerDocumentDto>();
        Assert.NotNull(document);
        Assert.Equal(3, document.EndpointCount);
    }

    [Fact]
    public async Task InvalidJson_Returns400()
    {
        await _factory.SeedAsync();
        var token = await LoginAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/swagger/register");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new RegisterSwaggerRequest
        {
            Name = "Bad API",
            RawJson = "{ invalid json",
            Domain = "healthcare"
        });

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListEndpoints_FiltersByTenant()
    {
        await _factory.SeedAsync();
        var tokenA = await LoginAsync();
        var rawJson = await File.ReadAllTextAsync(GetFixturePath("sample-openapi.json"));

        var registerRequest = new HttpRequestMessage(HttpMethod.Post, "/api/swagger/register");
        registerRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        registerRequest.Content = JsonContent.Create(new RegisterSwaggerRequest
        {
            Name = "Tenant A API",
            RawJson = rawJson,
            Domain = "healthcare"
        });
        var registerResponse = await _client.SendAsync(registerRequest);
        var document = await registerResponse.Content.ReadFromJsonAsync<SwaggerDocumentDto>();

        var tenantBEmail = $"tenantb-{Guid.NewGuid():N}@test.local";
        var registerTenantResponse = await _client.PostAsJsonAsync("/api/auth/register-tenant", new RegisterTenantRequest
        {
            TenantName = "Tenant B",
            DomainType = Domain.Enums.DomainType.Healthcare,
            AdminEmail = tenantBEmail,
            AdminPassword = "Test123!",
            AdminFullName = "Tenant B Admin"
        });
        var tenantB = await registerTenantResponse.Content.ReadFromJsonAsync<RegisterTenantResponse>();

        var listRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/swagger/documents/{document!.Id}/endpoints");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tenantB!.Token);
        var listResponse = await _client.SendAsync(listRequest);

        Assert.Equal(HttpStatusCode.NotFound, listResponse.StatusCode);
    }

    private static string GetFixturePath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);

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
