using System.Net;
using System.Net.Http.Json;
using JiraClone.Api.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace JiraClone.Api.Tests;

public sealed class AuthorizationHttpTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public AuthorizationHttpTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Anonymous_request_to_protected_endpoint_returns_401()
    {
        var response = await client.GetAsync("/api/projects");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_endpoint_allows_anonymous_access()
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("", ""));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
