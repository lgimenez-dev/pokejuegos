using PokeJuegos.Web.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    // Paths used by the links in the API's emails (App:FrontendUrl + /activate, /reset-password).
    options.Conventions.AddPageRoute("/ResetPassword", "reset-password");
});
builder.Services.AddHttpClient<AuthApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(15);
});

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.MapRazorPages();

app.Run();
