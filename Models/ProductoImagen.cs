
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intermediario.Models
{
    [Table("producto_imagenes")]
    public class ProductoImagen
    {
        [Key]
        public int id { get; set; }

        [Column("producto_id")]
        public int ProductoId { get; set; }

        [Required]
        [MaxLength(500)]
        public string ruta { get; set; } = string.Empty;

        [Column("es_principal")]
        public bool EsPrincipal { get; set; }

        public DateTime fecha_subida { get; set; } = DateTime.Now;

        [ForeignKey(nameof(ProductoId))]
        public Producto? Producto { get; set; }
    }
}
