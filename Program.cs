using BIsm2.Services;
using BIsm2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
//using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Debug: print connection string
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Connection string: {conn ?? "NOT FOUND"}");

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(conn));
//builder.Configuration.GetConnectionString("DefaultConnection"))
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Product/Login";   // where unauthenticated users are redirected
    options.AccessDeniedPath = "/Product/AccessDenied"; // optional
});

//builder.Services.AddDefaultIdentity<Users>(options => options.SignIn.RequireConfirmedAccount = true)
// .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddTransient<IEmailSender, EmailSender>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentityDataSeeder.SeedRolesAsync(services);
    await IdentityDataSeeder.SeedDefaultAdminAsync(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
