
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Intermediario.Data;
using Intermediario.Models;

namespace Intermediario.Pages
{
    public class IndexModel : PageModel
    {
        private readonly MarketplaceContext _context;

        private const string ClaveCarrito = "DANIS_CARRITO";

        public IndexModel(MarketplaceContext context)
        {
            _context = context;
        }

        public IList<Producto> Productos { get; set; } = new List<Producto>();

        // CARGAR PRODUCTOS DEL CATÁLOGO
        public async Task OnGetAsync()
        {
            await CargarProductosAsync();
        }

        // AGREGAR PRODUCTO AL CARRITO
        public async Task<IActionResult> OnPostAgregarAlCarritoAsync(int productoId)
        {
            if (productoId <= 0)
            {
                return RedirectToPage("/Index");
            }

            // Verificar que el producto exista y esté disponible.
            var productoExiste = await _context.Productos
                .AnyAsync(p =>
                    p.id == productoId &&
                    p.estado_producto == "Disponible");

            if (!productoExiste)
            {
                return RedirectToPage("/Index");
            }

            // Recuperar el carrito de la sesión.
            var json = HttpContext.Session.GetString(ClaveCarrito);

            List<int> carrito;

            try
            {
                carrito = string.IsNullOrWhiteSpace(json)
                    ? new List<int>()
                    : JsonSerializer.Deserialize<List<int>>(json)
                        ?? new List<int>();
            }
            catch (JsonException)
            {
                carrito = new List<int>();
            }

            // Evitar agregar dos veces el mismo producto.
            if (!carrito.Contains(productoId))
            {
                carrito.Add(productoId);
            }

            // Guardar el carrito.
            HttpContext.Session.SetString(
                ClaveCarrito,
                JsonSerializer.Serialize(carrito)
            );

            // Ir directamente al carrito.
            return RedirectToPage("/Carrito/Index");
        }

        // CONSULTAR PRODUCTOS E IMÁGENES
        private async Task CargarProductosAsync()
        {
            Productos = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Imagenes)
                .Include(p => p.Vendedor)
                .OrderByDescending(p => p.fecha_publicacion)
                .ToListAsync();
        }
    }
}
