using EnglishLearning.Application.Interfaces;
using EnglishLearning.Application.Services;
using EnglishLearning.Infrastructure.Data;
using EnglishLearning.Infrastructure.Identity;
using EnglishLearning.Infrastructure.Repositories;
using EnglishLearning.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder =
    WebApplication.CreateBuilder(args);

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

if (args.Contains("--seed"))
{
    using var scope =
        app.Services.CreateScope();

    await DbSeeder.SeedAsync(
        scope.ServiceProvider
            .GetRequiredService<
                ApplicationDbContext>(),

        scope.ServiceProvider
            .GetRequiredService<
                RoleManager<IdentityRole>>(),

        scope.ServiceProvider
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

// Thêm các HTTP security headers.
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

// Chuyển các lỗi HTTP như 404 sang trang thông báo thân thiện.
app.UseStatusCodePagesWithReExecute(
    "/Home/HttpStatus",
    "?code={0}");

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Cho phép project Integration Test truy cập Program.
public partial class Program
{
}