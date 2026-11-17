
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Intermediario.Data;
using Intermediario.Models;

namespace Intermediario.Pages.Productos
{
    public class CrearModel : PageModel
    {
        private readonly MarketplaceContext _context;
        private readonly IWebHostEnvironment _environment;

        public CrearModel(
            MarketplaceContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public Producto Producto { get; set; } = new();

        [BindProperty]
        public List<IFormFile> Fotos { get; set; } = new();

        public List<SelectListItem> Vendedores { get; set; } = new();

        public async Task OnGetAsync()
        {
            await CargarVendedores();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Producto.Vendedor");
            ModelState.Remove("Producto.Imagenes");

            // Validar cantidad de fotografías
            if (Fotos.Count > 5)
            {
                ModelState.AddModelError(
                    "Fotos",
                    "Solo puedes subir un máximo de 5 imágenes.");
            }

            string[] extensionesPermitidas =
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            string[] tiposPermitidos =
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            // Validar formato y tamaño
            foreach (var foto in Fotos)
            {
                if (foto.Length == 0)
                    continue;

                string extension =
                    Path.GetExtension(foto.FileName).ToLowerInvariant();

                string tipo = foto.ContentType.ToLowerInvariant();

                if (!extensionesPermitidas.Contains(extension) ||
                    !tiposPermitidos.Contains(tipo))
                {
                    ModelState.AddModelError(
                        "Fotos",
                        "Solo se permiten imágenes JPG, PNG o WEBP.");

                    break;
                }

                if (foto.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "Fotos",
                        "Cada imagen debe pesar máximo 5 MB.");

                    break;
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarVendedores();
                return Page();
            }

            // Comprobar que exista el vendedor
            bool vendedorExiste = await _context.Usuarios
                .AnyAsync(u => u.id == Producto.VendedorId);

            if (!vendedorExiste)
            {
                ModelState.AddModelError(
                    "Producto.VendedorId",
                    "Selecciona un vendedor válido.");

                await CargarVendedores();
                return Page();
            }

            Producto.fecha_publicacion = DateTime.Now;
            Producto.estado_producto = "Disponible";

            // Preparar carpeta de imágenes
            string carpetaWeb = _environment.WebRootPath
                ?? Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot"
                );

            string carpetaImagenes = Path.Combine(
                carpetaWeb,
                "uploads",
                "products"
            );

            Directory.CreateDirectory(carpetaImagenes);

            List<string> archivosGuardados = new();

            try
            {
                // Iniciar transacción de base de datos
                await using var transaccion =
                    await _context.Database.BeginTransactionAsync();

                // Guardar producto
                _context.Productos.Add(Producto);

                await _context.SaveChangesAsync();

                int numeroImagen = 0;

                // Guardar fotografías
                foreach (var foto in Fotos)
                {
                    if (foto.Length == 0)
                        continue;

                    string extension =
                        Path.GetExtension(foto.FileName).ToLowerInvariant();

                    string nombreArchivo =
                        Guid.NewGuid().ToString("N") + extension;

                    string rutaFisica = Path.Combine(
                        carpetaImagenes,
                        nombreArchivo
                    );

                    using (var stream = new FileStream(
                        rutaFisica,
                        FileMode.CreateNew))
                    {
                        await foto.CopyToAsync(stream);
                    }

                    archivosGuardados.Add(rutaFisica);

                    ProductoImagen imagen = new ProductoImagen
                    {
                        ProductoId = Producto.id,
                        ruta = "/uploads/products/" + nombreArchivo,
                        EsPrincipal = numeroImagen == 0,
                        fecha_subida = DateTime.Now
                    };

                    _context.ProductoImagenes.Add(imagen);

                    numeroImagen++;
                }

                // Guardar relaciones de las imágenes
                await _context.SaveChangesAsync();

                // Confirmar transacción
                await transaccion.CommitAsync();

                return RedirectToPage("/Index");
            }
            catch (Exception)
            {
                // Eliminar archivos guardados si ocurre un error
                foreach (string archivo in archivosGuardados)
                {
                    if (System.IO.File.Exists(archivo))
                    {
                        System.IO.File.Delete(archivo);
                    }
                }

                ModelState.AddModelError(
                    string.Empty,
                    "Ocurrió un error al guardar el producto o sus imágenes. Verifica la conexión con MySQL y la tabla producto_imagenes.");

                await CargarVendedores();

                return Page();
            }
        }

        private async Task CargarVendedores()
        {
            Vendedores = await _context.Usuarios
                .AsNoTracking()
                .Select(u => new SelectListItem
                {
                    Value = u.id.ToString(),
                    Text = u.NombreCompleto
                })
                .ToListAsync();
        }
    }
}
