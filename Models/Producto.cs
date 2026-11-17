
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intermediario.Models
{
    [Table("productos")]
    public class Producto
    {
        [Key]
        public int id { get; set; }

        [Column("vendedor_id")]
        public int VendedorId { get; set; }

        [Required]
        [MaxLength(150)]
        public string titulo { get; set; } = string.Empty;

        public string? descripcion { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal precio { get; set; }

        [MaxLength(30)]
        public string? estado_producto { get; set; } = "Disponible";

        public DateTime? fecha_publicacion { get; set; }

        // Relación con el usuario vendedor
        [ForeignKey(nameof(VendedorId))]
        public Usuario? Vendedor { get; set; }

        // Relación con las imágenes del producto
        public List<ProductoImagen> Imagenes { get; set; } = new();
    }
}
