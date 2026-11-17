
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Intermediario.Data;
using Intermediario.Models;

namespace Intermediario.Pages.Envio
{
    public class IndexModel : PageModel
    {
        private readonly MarketplaceContext _context;

        private const string ClaveCarrito = "DANIS_CARRITO";
        private const string ClaveConfirmacion = "DANIS_COMPRA_SIMULADA";
        private const decimal EnvioBase = 250m;

        public IndexModel(MarketplaceContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DatosEntrega Direccion { get; set; } = new();

        public List<Producto> Productos { get; set; } = new();

        public decimal Subtotal =>
            Productos.Sum(p => p.precio);

        public decimal Comision =>
            Productos.Sum(p =>
                p.precio <= 10000m
                    ? p.precio * 0.10m
                    : p.precio * 0.05m);

        public decimal CostoEnvio =>
            Productos.Count > 0 ? EnvioBase : 0m;

        public decimal Total =>
            Subtotal + Comision + CostoEnvio;

        public bool CompraConfirmada { get; set; }

        public string FolioConfirmacion { get; set; } =
            string.Empty;

        public decimal TotalConfirmado { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            CargarConfirmacionGuardada();

            if (CompraConfirmada)
                return Page();

            await CargarProductosAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmarAsync()
        {
            // Recuperar los productos actuales del carrito.
            await CargarProductosAsync();

            if (!Productos.Any())
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Tu carrito está vacío o los productos ya no están disponibles.");

                return Page();
            }

            if (!ModelState.IsValid)
                return Page();

            // Calcular el total nuevamente en el servidor.
            var confirmacion = new ConfirmacionSimulada
            {
                Folio = $"DANIS-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}",
                Total = Total,
                Fecha = DateTime.Now,
                Destinatario = Direccion.Nombre
            };

            // Guardar confirmación temporal en la sesión.
            HttpContext.Session.SetString(
                ClaveConfirmacion,
                JsonSerializer.Serialize(confirmacion));

            // Vaciar el carrito después de confirmar.
            HttpContext.Session.Remove(ClaveCarrito);

            CompraConfirmada = true;
            FolioConfirmacion = confirmacion.Folio;
            TotalConfirmado = confirmacion.Total;

            return Page();
        }

        private async Task CargarProductosAsync()
        {
            Productos = new List<Producto>();

            var json = HttpContext.Session.GetString(ClaveCarrito);

            if (string.IsNullOrWhiteSpace(json))
                return;

            List<int> ids;

            try
            {
                ids = JsonSerializer.Deserialize<List<int>>(json)
                      ?? new List<int>();
            }
            catch (JsonException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo leer el contenido del carrito. Vuelve a agregar tus productos.");

                return;
            }

            ids = ids.Distinct().ToList();

            if (!ids.Any())
                return;

            Productos = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Imagenes)
                .Where(p =>
                    ids.Contains(p.id) &&
                    p.estado_producto != null &&
                    p.estado_producto.ToLower() == "disponible")
                .ToListAsync();
        }

        private void CargarConfirmacionGuardada()
        {
            var json = HttpContext.Session.GetString(
                ClaveConfirmacion);

            if (string.IsNullOrWhiteSpace(json))
                return;

            try
            {
                var datos =
                    JsonSerializer.Deserialize<ConfirmacionSimulada>(json);

                if (datos == null)
                    return;

                CompraConfirmada = true;
                FolioConfirmacion = datos.Folio;
                TotalConfirmado = datos.Total;
            }
            catch (JsonException)
            {
                HttpContext.Session.Remove(ClaveConfirmacion);
            }
        }

        public class DatosEntrega
        {
            [Required(ErrorMessage = "Escribe el nombre del destinatario.")]
            [StringLength(100)]
            public string Nombre { get; set; } = string.Empty;

            [Required(ErrorMessage = "Escribe un teléfono de contacto.")]
            [Phone(ErrorMessage = "Escribe un teléfono válido.")]
            [StringLength(20)]
            public string Telefono { get; set; } = string.Empty;

            [Required(ErrorMessage = "Escribe la calle y número.")]
            [StringLength(180)]
            public string Calle { get; set; } = string.Empty;

            [Required(ErrorMessage = "Escribe la colonia.")]
            [StringLength(100)]
            public string Colonia { get; set; } = string.Empty;

            [Required(ErrorMessage = "Escribe el código postal.")]
            [RegularExpression(
                @"^\d{5}$",
                ErrorMessage = "El código postal debe tener 5 dígitos.")]
            public string CodigoPostal { get; set; } = string.Empty;

            [Required(ErrorMessage = "Escribe el municipio o ciudad.")]
            [StringLength(100)]
            public string Municipio { get; set; } = string.Empty;

            [Required(ErrorMessage = "Escribe el estado.")]
            [StringLength(100)]
            public string Estado { get; set; } = string.Empty;

            [StringLength(300)]
            public string? Referencias { get; set; }
        }

        private class ConfirmacionSimulada
        {
            public string Folio { get; set; } = string.Empty;

            public decimal Total { get; set; }

            public DateTime Fecha { get; set; }

            public string Destinatario { get; set; } = string.Empty;
        }
    }
}
