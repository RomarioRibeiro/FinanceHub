using FinanceHub.Data;
using FinanceHub.ModelBinders;
using FinanceHub.Repositories;
using FinanceHub.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FinanceHubContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("FinanceHubContext")
        ?? throw new InvalidOperationException("Connection string 'FinanceHubContext' not found.")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Conta/Login";
        options.AccessDeniedPath = "/Conta/AcessoNegado";
        options.Cookie.Name = "FinanceHub.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
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
builder.Services.AddScoped<LancamentoRecorrenteService>();
builder.Services.AddScoped<RelatorioService>();
builder.Services.AddScoped<RecorrenciaProcessorService>();
builder.Services.AddHostedService<RecorrenciaBackgroundService>();

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
