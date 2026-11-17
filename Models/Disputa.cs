
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intermediario.Models
{
    [Table("disputas")]
    public class Disputa
    {
        [Key]
        public int id { get; set; }

        public int transaccion_id { get; set; }

        public int usuario_reporta_id { get; set; }

        [Required]
        [MaxLength(250)]
        public string motivo { get; set; } = string.Empty;

        // El nombre conserva exactamente el de MySQL
        [Column("descreipcion_detallada")]
        public string descreipcion_detallada { get; set; }
            = string.Empty;

        [MaxLength(30)]
        public string? estado_disputa { get; set; } = "Abierta";

        public DateTime? fecha_apertura { get; set; }

        public DateTime? fecha_resolucion { get; set; }

        // Relaciones
        [ForeignKey(nameof(transaccion_id))]
        public Transaccion? Transaccion { get; set; }

        [ForeignKey(nameof(usuario_reporta_id))]
        public Usuario? UsuarioReporta { get; set; }
    }
}
