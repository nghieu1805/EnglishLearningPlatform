using EnglishLearning.Application.Interfaces;
using EnglishLearning.Application.Services;
using EnglishLearning.Infrastructure.Data;
using EnglishLearning.Infrastructure.Identity;
using EnglishLearning.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
var builder=WebApplication.CreateBuilder(args);
var connection=builder.Configuration.GetConnectionString("DefaultConnection")??throw new InvalidOperationException("Thiếu DefaultConnection.");
builder.Services.AddDbContext<ApplicationDbContext>(o=>o.UseMySql(connection,new MySqlServerVersion(new Version(8,0,0))));
builder.Services.AddIdentity<ApplicationUser,IdentityRole>(o=>{
 o.User.RequireUniqueEmail=true;o.Password.RequiredLength=8;
 o.Lockout.MaxFailedAccessAttempts=5;o.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(15);
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o=>{o.LoginPath="/Account/Login";o.AccessDeniedPath="/Account/Denied";o.Cookie.HttpOnly=true;o.Cookie.SameSite=SameSiteMode.Lax;});
builder.Services.AddScoped<ILearningRepository,LearningRepository>();
builder.Services.AddScoped<QuizService>();
builder.Services.AddControllersWithViews(o=>{o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());o.ModelMetadataDetailsProviders.Add(new EnglishLearning.Web.ViewModels.EmptyStringMetadataProvider());});
var app=builder.Build();
if(args.Contains("--seed")){
 using var scope=app.Services.CreateScope();
 await DbSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),builder.Configuration["Seed:AdminEmail"],builder.Configuration["Seed:AdminPassword"],builder.Configuration.GetValue<bool>("Seed:DemoContent"));
 return;
}
if(!app.Environment.IsDevelopment()){app.UseExceptionHandler("/Home/Error");app.UseHsts();app.UseHttpsRedirection();}
app.UseStaticFiles();app.UseRouting();app.UseAuthentication();app.UseAuthorization();
app.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}");
app.Run();
