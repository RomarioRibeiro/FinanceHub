using FinanceHub.Data;
using FinanceHub.ModelBinders;
using FinanceHub.Models.Options;
using FinanceHub.Repositories;
using FinanceHub.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddDbContext<FinanceHubContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("FinanceHubContext")
        ?? throw new InvalidOperationException("Connection string 'FinanceHubContext' not found.")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection(EmailOptions.SectionName));

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Conta/Login";
        options.AccessDeniedPath = "/Conta/AcessoNegado";
        options.Cookie.Name = "FinanceHub.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync(
            "Muitas tentativas de login. Aguarde 15 minutos antes de tentar novamente.",
            cancellationToken);
    };

    options.AddPolicy("login", httpContext =>
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

        return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(15),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});

builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
});
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<AutenticacaoService>();
builder.Services.AddScoped<UsuarioAtualService>();
builder.Services.AddScoped<PerfilService>();
builder.Services.AddScoped<ContaService>();
builder.Services.AddScoped<SaldoService>();
builder.Services.AddScoped<TransacaoService>();
builder.Services.AddScoped<MetaService>();
builder.Services.AddScoped<NotificacaoMetaService>();
builder.Services.AddScoped<MetaLembreteProcessorService>();
builder.Services.AddScoped<IEmailMetaSender, SmtpEmailMetaSender>();
builder.Services.AddScoped<LancamentoRecorrenteService>();
builder.Services.AddScoped<RelatorioService>();
builder.Services.AddScoped<RecorrenciaProcessorService>();
builder.Services.AddHostedService<RecorrenciaBackgroundService>();
builder.Services.AddHostedService<MetaLembreteBackgroundService>();

var app = builder.Build();

var supportedCultures = new[]
{
    new CultureInfo("pt-BR")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<FinanceHubContext>();
    await DevelopmentDataSeeder.SeedAsync(context);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
