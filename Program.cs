
using Microsoft.EntityFrameworkCore;
using Intermediario.Data;

var builder = WebApplication.CreateBuilder(args);

// Registrar Razor Pages
builder.Services.AddRazorPages();

// Carrito temporal por sesión del navegador
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Obtener cadena de conexion
var connectionString = builder.Configuration
    .GetConnectionString("ConexionMySQL");

// Registrar conexion MySQL
builder.Services.AddDbContext<MarketplaceContext>(options =>
    options.UseMySQL(connectionString!));

// Construir aplicacion
var app = builder.Build();

// HABILITAR ARCHIVOS ESTATICOS (CSS, JS E IMAGENES)
app.UseStaticFiles();

// Comprobar conexion a MySQL
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<MarketplaceContext>();

    try
    {
        bool conectado = await db.Database.CanConnectAsync();

        if (conectado)
        {
            Console.WriteLine("==============================");
            Console.WriteLine("CONEXION EXITOSA A MYSQL");
            Console.WriteLine("Base: intermediariodb");
            Console.WriteLine("==============================");
        }
        else
        {
            Console.WriteLine("NO SE PUDO CONECTAR A MYSQL");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("ERROR DE CONEXION:");
        Console.WriteLine(ex.Message);
    }
}

// Ruta para comprobar conexion desde navegador
app.MapGet("/prueba-db", async (MarketplaceContext db) =>
{
    try
    {
        bool conectado = await db.Database.CanConnectAsync();

        if (conectado)
        {
            return Results.Ok(
                "CONEXION EXITOSA A MYSQL - intermediariodb");
        }

        return Results.Problem("No se pudo conectar a MySQL");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Configuracion HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// La sesión debe estar activa antes de ejecutar las páginas Razor
app.UseSession();

app.UseAuthorization();

// Mantener Razor Pages y recursos estaticos
app.MapStaticAssets();
app.MapRazorPages();

app.Run();
