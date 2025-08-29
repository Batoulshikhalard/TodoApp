using Microsoft.AspNetCore.Authentication.Cookies;
using TodoApp.Web.Middleware;
using TodoApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAntiforgery(); 


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;

        // Cookie security settings
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    })
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["ApiSettings:BaseUrl"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:SecretKey"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });


builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IApiService, ApiService>();
builder.Services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();

builder.Services.AddScoped<IApiService, ApiService>();


// Memory cache for rate limiting
builder.Services.AddMemoryCache();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Security middleware
app.UseSecurityHeaders();
app.UseEnhancedRateLimiting();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // add this line
app.UseAuthorization();

// Add XSRF token to response cookies
app.Use(async (context, next) =>
{
    var tokens = app.Services.GetRequiredService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>().GetAndStoreTokens(context);
    context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken,
        new CookieOptions { HttpOnly = false, Secure = true, SameSite = SameSiteMode.Strict });
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
