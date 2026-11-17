
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intermediario.Models
{
    [Table("transacciones")]
    public class Transaccion
    {
        [Key]
        public int id { get; set; }

        public int producto_id { get; set; }

        public int comprador_id { get; set; }

        public int vendedor_id { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal monto_producto { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal comision_intermediario { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal monto_total { get; set; }

        [MaxLength(40)]
        public string? estado_transaccion { get; set; }
            = "PagoRetenidoEnGarantia";

        [MaxLength(100)]
        public string? codigo_rastreo_envio { get; set; }

        public DateTime? fecha_creacion { get; set; }

        public DateTime? fecha_actualizacion { get; set; }

        // Relaciones
        [ForeignKey(nameof(producto_id))]
        public Producto? Producto { get; set; }

        [ForeignKey(nameof(comprador_id))]
        public Usuario? Comprador { get; set; }

        [ForeignKey(nameof(vendedor_id))]
        public Usuario? Vendedor { get; set; }
    }
}
