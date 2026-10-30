using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using Supermercado.Datos.Data;
using Supermercado.Bodega.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration
    .GetConnectionString("MySql")
    ?? throw new InvalidOperationException(
        "No existe la cadena de conexión MySql.");

builder.Services.AddDbContext<SupermercadoDbContext>(
    options => options.UseMySQL(connectionString));
builder.Services.AddScoped<InventarioService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Bodegas}/{action=Index}/{id?}");

app.Run();
