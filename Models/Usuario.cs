using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intermediario.Models
{
    [Table("usuarios")]
    public class Usuario
    {



        // Método virtual para aplicar polimorfismo
        public virtual string ObtenerTipoUsuario()
        {
            return "Usuario general";
        }



        [Key]
        public int id { get; private set; }

        [Required]
        [Column("nombre_completo")]
        [MaxLength(100)]
        public string NombreCompleto { get; private set; }
            = string.Empty;

        [Required]
        [MaxLength(100)]
        public string correo { get; private set; }
            = string.Empty;

        [MaxLength(150)]
        public string? passwordHash { get; private set; }

        public int? telefono { get; private set; }

        [MaxLength(20)]
        public string EVerificacion { get; private set; }
            = "Pendiente";

        public DateTime FechaRegistro { get; private set; }

        // Constructor para Entity Framework
        public Usuario()
        {
        }

        // Actualizar información del usuario
        public void ActualizarPerfil(
            string nombre,
            string nuevoCorreo,
            int? nuevoTelefono)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(nuevoCorreo))
                throw new ArgumentException("El correo es obligatorio.");

            NombreCompleto = nombre;
            correo = nuevoCorreo;
            telefono = nuevoTelefono;
        }

        // Cambiar estado de verificación
        public void CambiarVerificacion(string estado)
        {
            if (estado != "Pendiente" &&
                estado != "Verificado" &&
                estado != "Rechazado")
            {
                throw new ArgumentException(
                    "Estado de verificación no válido.");
            }

            EVerificacion = estado;
        }





        public void Registrar(
            string nombre,
            string email,
            string password,
            int? numeroTelefono)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El correo es obligatorio.");

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                throw new ArgumentException(
                    "La contraseña debe tener al menos 8 caracteres.");

            NombreCompleto = nombre;
            correo = email;
            telefono = numeroTelefono;

            // Guardar la contraseña mediante un hash seguro
            var hasher = new PasswordHasher<Usuario>();
            passwordHash = hasher.HashPassword(this, password);

            EVerificacion = "Pendiente";
            FechaRegistro = DateTime.Now;
        }

    }



}



