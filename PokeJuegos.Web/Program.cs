using System.Globalization;
using Microsoft.AspNetCore.Localization;
using PokeJuegos.Web.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
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

// The API's email links carry ?lang=en|es; otherwise the browser's Accept-Language decides. English by default.
CultureInfo[] cultures = [new("en"), new("es")];
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = cultures,
    SupportedUICultures = cultures,
    RequestCultureProviders =
    [
        new QueryStringRequestCultureProvider { QueryStringKey = "lang", UIQueryStringKey = "lang" },
        new AcceptLanguageHeaderRequestCultureProvider(),
    ],
});

app.UseStaticFiles();
app.MapRazorPages();

app.Run();
