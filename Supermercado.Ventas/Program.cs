using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using Supermercado.Datos.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration
    .GetConnectionString("MySql")
    ?? throw new InvalidOperationException(
        "No existe la cadena de conexión MySql.");

builder.Services.AddDbContext<SupermercadoDbContext>(
    options => options.UseMySQL(connectionString));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();