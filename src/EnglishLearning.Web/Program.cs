using EnglishLearning.Application.Interfaces;
using EnglishLearning.Application.Services;
using EnglishLearning.Infrastructure.Data;
using EnglishLearning.Infrastructure.Identity;
using EnglishLearning.Infrastructure.Repositories;
using EnglishLearning.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;

var builder =
    WebApplication.CreateBuilder(args);
var dataProtectionKeysPath =
    builder.Configuration[
        "DataProtection:KeysPath"];

if (!string.IsNullOrWhiteSpace(
        dataProtectionKeysPath))
{
    Directory.CreateDirectory(
        dataProtectionKeysPath);

    builder.Services
        .AddDataProtection()
        .PersistKeysToFileSystem(
            new DirectoryInfo(
                dataProtectionKeysPath))
        .SetApplicationName(
            "EnglishLearningPlatform");
}

var connection =
    builder.Configuration.GetConnectionString(
        "DefaultConnection")
    ?? throw new InvalidOperationException(
        "Thiếu DefaultConnection.");

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseMySql(
            connection,
            new MySqlServerVersion(
                new Version(8, 0, 0))));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(
        options =>
        {
            options.User.RequireUniqueEmail = true;

            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;

            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(15);
        })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(
    options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Denied";

        options.Cookie.Name = "EApp.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.Cookie.SecurePolicy =
            builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;

        options.ExpireTimeSpan =
            TimeSpan.FromHours(8);

        options.SlidingExpiration = true;
    });

builder.Services.AddScoped<
    ILearningRepository,
    LearningRepository>();

builder.Services.AddScoped<QuizService>();
builder.Services.AddScoped<ExperimentService>();
builder.Services.AddScoped<
    VocabularyImportService>();

builder.Services.AddHttpClient<
    DictionaryApiService>(
        client =>
        {
            client.BaseAddress =
                new Uri(
                    "https://api.dictionaryapi.dev/");

            client.Timeout =
                TimeSpan.FromSeconds(30);

            client.DefaultRequestHeaders
                .UserAgent
                .ParseAdd(
                    "EApp-EnglishLearning/1.0");
        });

builder.Services.AddControllersWithViews(
    options =>
    {
        options.Filters.Add(
            new AutoValidateAntiforgeryTokenAttribute());

        options.ModelMetadataDetailsProviders.Add(
            new EnglishLearning.Web.ViewModels
                .EmptyStringMetadataProvider());
    });

var app = builder.Build();

// Tự động áp dụng migration khi cấu hình được bật.
if (builder.Configuration.GetValue<bool>(
        "Database:MigrateOnStartup"))
{
    using var migrationScope =
        app.Services.CreateScope();

    var database =
        migrationScope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

    await database.Database.MigrateAsync();
}

// Đặt lại mật khẩu Admin bằng cấu hình trong .env.
if (args.Contains("--reset-admin-password"))
{
    using var resetScope =
        app.Services.CreateScope();

    var userManager =
        resetScope.ServiceProvider
            .GetRequiredService<
                UserManager<ApplicationUser>>();

    var adminEmail =
        builder.Configuration[
            "Seed:AdminEmail"];

    var adminPassword =
        builder.Configuration[
            "Seed:AdminPassword"];

    if (string.IsNullOrWhiteSpace(adminEmail) ||
        string.IsNullOrWhiteSpace(adminPassword))
    {
        throw new InvalidOperationException(
            "Thiếu Seed:AdminEmail hoặc " +
            "Seed:AdminPassword.");
    }

    var admin =
        await userManager.FindByEmailAsync(
            adminEmail);

    if (admin is null)
    {
        throw new InvalidOperationException(
            $"Không tìm thấy tài khoản {adminEmail}.");
    }

    var resetToken =
        await userManager
            .GeneratePasswordResetTokenAsync(
                admin);

    var resetResult =
        await userManager.ResetPasswordAsync(
            admin,
            resetToken,
            adminPassword);

    if (!resetResult.Succeeded)
    {
        var errors =
            string.Join(
                "; ",
                resetResult.Errors.Select(
                    error => error.Description));

        throw new InvalidOperationException(
            $"Không thể đặt lại mật khẩu: {errors}");
    }

    await userManager.SetLockoutEndDateAsync(
        admin,
        null);

    await userManager
        .ResetAccessFailedCountAsync(admin);

    Console.WriteLine(
        $"Đã đặt lại mật khẩu cho {adminEmail}.");

    return;
}

// Tạo roles, Admin và nội dung demo.
if (args.Contains("--seed"))
{
    using var seedScope =
        app.Services.CreateScope();

    await DbSeeder.SeedAsync(
        seedScope.ServiceProvider
            .GetRequiredService<
                ApplicationDbContext>(),

        seedScope.ServiceProvider
            .GetRequiredService<
                RoleManager<IdentityRole>>(),

        seedScope.ServiceProvider
            .GetRequiredService<
                UserManager<ApplicationUser>>(),

        builder.Configuration[
            "Seed:AdminEmail"],

        builder.Configuration[
            "Seed:AdminPassword"],

        builder.Configuration.GetValue<bool>(
            "Seed:DemoContent"));

    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();

    app.UseHttpsRedirection();
}

// HTTP security headers.
app.Use(
    async (context, next) =>
    {
        context.Response.Headers[
            "X-Content-Type-Options"] = "nosniff";

        context.Response.Headers[
            "X-Frame-Options"] = "DENY";

        context.Response.Headers[
            "Referrer-Policy"] =
            "strict-origin-when-cross-origin";

        context.Response.Headers[
            "Permissions-Policy"] =
            "camera=(), microphone=(), geolocation=()";

        await next();
    });

// Hiển thị trang lỗi thân thiện.
app.UseStatusCodePagesWithReExecute(
    "/Home/HttpStatus",
    "?code={0}");

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

// Health Check cho website và database.
app.MapGet(
        "/health",
        async Task<IResult>(
            ApplicationDbContext database) =>
        {
            try
            {
                var canConnect =
                    await database.Database
                        .CanConnectAsync();

                if (!canConnect)
                {
                    return Results.StatusCode(
                        StatusCodes
                            .Status503ServiceUnavailable);
                }

                return Results.Ok(
                    new
                    {
                        status = "Healthy"
                    });
            }
            catch
            {
                return Results.StatusCode(
                    StatusCodes
                        .Status503ServiceUnavailable);
            }
        })
    .AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Cho phép Integration Test truy cập Program.
public partial class Program
{
}