
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Intermediario.Data;
using Intermediario.Models;

namespace Intermediario.Pages.Usuarios
{
    public class CrearModel : PageModel
    {
        private readonly MarketplaceContext _context;

        public CrearModel(MarketplaceContext context)
        {
            _context = context;
        }

        [BindProperty]
        public RegistroDatos Datos { get; set; } = new();

        public class RegistroDatos
        {
            [Required(ErrorMessage = "El nombre es obligatorio.")]
            [StringLength(100)]
            public string Nombre { get; set; } = string.Empty;

            [Required(ErrorMessage = "El correo es obligatorio.")]
            [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
            [StringLength(100)]
            public string Correo { get; set; } = string.Empty;

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [MinLength(8, ErrorMessage = "Mínimo 8 caracteres.")]
            public string Password { get; set; } = string.Empty;

            public int? Telefono { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            bool correoExiste = await _context.Usuarios
                .AnyAsync(u => u.correo == Datos.Correo);

            if (correoExiste)
            {
                ModelState.AddModelError(
                    "Datos.Correo",
                    "Este correo ya está registrado.");

                return Page();
            }

            var usuario = new Usuario();

            usuario.Registrar(
                Datos.Nombre,
                Datos.Correo,
                Datos.Password,
                Datos.Telefono
            );

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            return RedirectToPage("/Index");
        }
    }
}
