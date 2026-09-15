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
        using var client =
            CreateClient();

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