using ContactManager.Data;
using ContactManager.Services;
using ContactManager.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<ContactService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddScoped<UserState>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped(sp => sp.GetRequiredService<System.Net.Http.IHttpClientFactory>().CreateClient());
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Minimal API endpoints for account actions
app.MapPost("/api/account/register", async (RegisterRequest req, UserService userService, HttpContext http) =>
{
    if (string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Missing fields");

    var existing = await userService.GetByEmailAsync(req.Email);
    if (existing != null) return Results.Conflict("Email already registered");

    var byName = await userService.GetByUserNameAsync(req.UserName);
    if (byName != null) return Results.Conflict("Username already taken");

    var user = new ContactManager.Models.User
    {
        UserName = req.UserName,
        Email = req.Email,
        PasswordHash = ContactManager.Utilities.PasswordHasher.Hash(req.Password)
    };

    await userService.CreateAsync(user);

    var claims = new[] { new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? "") };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return Results.Ok();
});

app.MapPost("/api/account/login", async (LoginRequest req, UserService userService, HttpContext http) =>
{
    if (string.IsNullOrWhiteSpace(req.UserOrEmail) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Missing fields");

    var user = await userService.ValidateCredentialsAsync(req.UserOrEmail, req.Password);
    if (user == null) return Results.Unauthorized();

    var claims = new[] { new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? "") };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return Results.Ok();
});

app.MapPost("/api/account/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
});

// Request DTOs are defined in Models/AuthRequests.cs
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
