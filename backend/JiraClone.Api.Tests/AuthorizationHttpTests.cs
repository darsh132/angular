using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using JiraClone.Api.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace JiraClone.Api.Tests;

public sealed class AuthorizationHttpTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;
    public AuthorizationHttpTests(WebApplicationFactory<Program> factory) => this.factory = factory;

    [Fact]
    public async Task Anonymous_request_to_protected_endpoint_returns_401()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/projects");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_endpoint_allows_anonymous_access()
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("", ""));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_can_read_project_but_cannot_create_issue()
    {
        using var client = await AuthenticatedClientAsync("priya@example.com", "demo123");
        var projects = await client.GetFromJsonAsync<List<ProjectResponse>>("/api/projects");
        Assert.NotNull(projects);
        Assert.Contains(projects!, x => x.Key == "ACME");

        var response = await client.PostAsJsonAsync("/api/issues", new CreateIssueRequest(1, "Viewer cannot create", "authorization test"));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Member_can_create_issue()
    {
        using var client = await AuthenticatedClientAsync("aarav@example.com", "demo123");
        var response = await client.PostAsJsonAsync("/api/issues", new CreateIssueRequest(1, "Member can create", "authorization test"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Admin_can_view_project_without_project_membership()
    {
        using var client = await AuthenticatedClientAsync("darshan@example.com", "demo123");
        var response = await client.GetAsync("/api/projects");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<HttpClient> AuthenticatedClientAsync(string email, string password)
    {
        var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        login.EnsureSuccessStatusCode();
        var result = await login.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(result);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);
        return client;
    }
}
