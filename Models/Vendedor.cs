
namespace Intermediario.Models
{
    public class Vendedor : Usuario
    {
        // El vendedor registra el envío de un producto
        public bool RegistrarEnvio(
            Transaccion transaccion,
            string codigoRastreo)
        {
            if (transaccion.estado_transaccion != "PagoRetenidoEnGarantia")
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(codigoRastreo))
            {
                return false;
            }

            transaccion.codigo_rastreo_envio = codigoRastreo;
            transaccion.estado_transaccion = "Enviado";

            return true;
        }
    }
}
