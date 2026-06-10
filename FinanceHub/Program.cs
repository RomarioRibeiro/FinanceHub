using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FinanceHub.Data;
using FinanceHub.Repositories;
using FinanceHub.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<FinanceHubContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FinanceHubContext") ?? throw new InvalidOperationException("Connection string 'FinanceHubContext' not found.")));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<PerfilService>();
builder.Services.AddScoped<ContaService>();
builder.Services.AddScoped<SaldoService>();
builder.Services.AddScoped<TransacaoService>();
builder.Services.AddScoped<MetaService>();
builder.Services.AddScoped<LancamentoRecorrenteService>();
builder.Services.AddScoped<RelatorioService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
