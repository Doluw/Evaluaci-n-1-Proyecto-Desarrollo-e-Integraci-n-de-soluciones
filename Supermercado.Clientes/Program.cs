using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using Supermercado.Datos.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Misma base de datos que Ventas y Bodega (cadena "MySql").
var connectionString = builder.Configuration
    .GetConnectionString("MySql")
    ?? throw new InvalidOperationException(
        "No existe la cadena de conexión MySql.");

builder.Services.AddDbContext<SupermercadoDbContext>(
    options => options.UseMySQL(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Clientes}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
