using AzerothCoreIntegration.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddScoped<SiteUserService>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<CharacterStatisticsService>();
builder.Services.AddScoped<CharacterService>();

builder.Services.AddHttpClient<AzerothCoreSoapClient>();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddScoped<AuthenticationService>();

builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
{
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
});
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var siteUserService = scope.ServiceProvider.GetRequiredService<SiteUserService>();
    await siteUserService.EnsureTableExistsAsync();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
