using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EnglishLearning.Tests;

public class AuthorizationTests(
    CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task AnonymousUserCannotOpenAdmin()
    {
        using var client = CreateClient();

        var response =
            await client.GetAsync("/Admin");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task StudentCannotOpenAdmin()
    {
        using var client =
            CreateClient("Student");

        var response =
            await client.GetAsync("/Admin");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task StaffCanOpenContentDashboard()
    {
        using var client =
            CreateClient("Staff");

        var response =
            await client.GetAsync("/Admin");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task StaffCannotManageUsers()
    {
        using var client =
            CreateClient("Staff");

        var response =
            await client.GetAsync("/Admin/Users");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task StaffCannotImportCsv()
    {
        using var client =
            CreateClient("Staff");

        var response =
            await client.GetAsync(
                "/Admin/ImportVocabularies");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task AdminCanManageUsers()
    {
        using var client =
            CreateClient("Admin");

        var response =
            await client.GetAsync("/Admin/Users");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task UnknownRouteReturnsFriendly404Page()
    {
        using var client = CreateClient();

        var response =
            await client.GetAsync(
                "/this-page-does-not-exist");

        var content =
            await response.Content.ReadAsStringAsync();

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        Assert.Contains(
            "Không tìm thấy trang",
            content);
    }

    [Fact]
    public async Task ErrorEndpointReturnsFriendly500Page()
    {
        using var client = CreateClient();

        var response =
            await client.GetAsync("/Home/Error");

        var content =
            await response.Content.ReadAsStringAsync();

        Assert.Equal(
            HttpStatusCode.InternalServerError,
            response.StatusCode);

        Assert.Contains(
            "Không thể xử lý yêu cầu",
            content);
    }

    private HttpClient CreateClient(
        string? role = null)
    {
        var client =
            factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });

        if (!string.IsNullOrWhiteSpace(role))
        {
            client.DefaultRequestHeaders.Add(
                TestAuthHandler.RoleHeader,
                role);
        }

        return client;
    }
}