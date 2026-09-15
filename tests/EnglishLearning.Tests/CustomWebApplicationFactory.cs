using System.Data.Common;
using EnglishLearning.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace EnglishLearning.Tests;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration(
            (_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        [
                            "ConnectionStrings:" +
                            "DefaultConnection"
                        ] =
                            "Server=localhost;" +
                            "Database=EAppTests;" +
                            "User=test;" +
                            "Password=test;"
                    });
            });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<
                DbContextOptions<
                    ApplicationDbContext>>();

            services.RemoveAll<
                ApplicationDbContext>();

            services.RemoveAll<DbConnection>();

            services.AddSingleton<DbConnection>(
                _ =>
                {
                    var connection =
                        new SqliteConnection(
                            "Data Source=:memory:");

                    connection.Open();

                    return connection;
                });

            services.AddDbContext<
                ApplicationDbContext>(
                (serviceProvider, options) =>
                {
                    var connection =
                        serviceProvider
                            .GetRequiredService<
                                DbConnection>();

                    options.UseSqlite(connection);
                });
        });

        builder.ConfigureTestServices(services =>
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        TestAuthHandler.SchemeName;

                    options.DefaultChallengeScheme =
                        TestAuthHandler.SchemeName;

                    options.DefaultForbidScheme =
                        TestAuthHandler.SchemeName;
                })
                .AddScheme<
                    AuthenticationSchemeOptions,
                    TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ =>
                    {
                    });
        });
    }

    protected override IHost CreateHost(
        IHostBuilder builder)
    {
        var host =
            base.CreateHost(builder);

        using var scope =
            host.Services.CreateScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<
                    ApplicationDbContext>();

        db.Database.EnsureCreated();

        return host;
    }
}