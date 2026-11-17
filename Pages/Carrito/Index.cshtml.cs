
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Intermediario.Data;
using Intermediario.Models;

namespace Intermediario.Pages.Carrito
{
    public class IndexModel : PageModel
    {
        private readonly MarketplaceContext _context;

        private const string ClaveCarrito = "DANIS_CARRITO";

        public IndexModel(MarketplaceContext context)
        {
            _context = context;
        }

        public List<Producto> Productos { get; set; } = new();

        public decimal Subtotal =>
            Productos.Sum(p => p.precio);

        public decimal Comision =>
            Productos.Sum(p =>
                p.precio <= 10000m
                    ? p.precio * 0.10m
                    : p.precio * 0.05m);

        public decimal Envio => 250m;

        public decimal Total =>
            Subtotal + Comision + Envio;

        // CARGAR CARRITO
        public async Task OnGetAsync()
        {
            var ids = ObtenerIds();

            if (ids.Count == 0)
                return;

            Productos = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Imagenes)
                .Include(p => p.Vendedor)
                .Where(p => ids.Contains(p.id))
                .OrderBy(p => p.titulo)
                .ToListAsync();

            // Conservar únicamente productos disponibles.
            var disponibles = Productos
                .Where(p =>
                    string.Equals(
                        p.estado_producto,
                        "Disponible",
                        StringComparison.OrdinalIgnoreCase))
                .Select(p => p.id)
                .ToList();

            GuardarIds(disponibles);

            Productos = Productos
                .Where(p => disponibles.Contains(p.id))
                .ToList();
        }

        // QUITAR PRODUCTO
        public IActionResult OnPostQuitar(int productoId)
        {
            var ids = ObtenerIds();

            ids.Remove(productoId);

            GuardarIds(ids);

            TempData["MensajeCarrito"] =
                "Producto eliminado del carrito.";

            return RedirectToPage();
        }

        // VACIAR CARRITO
        public IActionResult OnPostVaciar()
        {
            GuardarIds(new List<int>());

            TempData["MensajeCarrito"] =
                "Carrito vaciado.";

            return RedirectToPage();
        }

        // OBTENER IDENTIFICADORES DE LA SESIÓN
        private List<int> ObtenerIds()
        {
            var json = HttpContext.Session.GetString(ClaveCarrito);

            if (string.IsNullOrWhiteSpace(json))
                return new List<int>();

            try
            {
                return JsonSerializer.Deserialize<List<int>>(json)
                    ?? new List<int>();
            }
            catch (JsonException)
            {
                return new List<int>();
            }
        }

        // GUARDAR IDENTIFICADORES EN LA SESIÓN
        private void GuardarIds(List<int> ids)
        {
            HttpContext.Session.SetString(
                ClaveCarrito,
                JsonSerializer.Serialize(ids)
            );
        }
    }
}
