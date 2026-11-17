
namespace Intermediario.Models
{
    public class Comprador : Usuario
    {
        // El comprador confirma que recibió su producto
        public bool ConfirmarRecepcion(Transaccion transaccion)
        {




            if (transaccion.estado_transaccion != "Enviado")
            {
                return false;
            }

            transaccion.estado_transaccion = "RecibidoConforme";

            return true;
    
        
        }


        public override string ObtenerTipoUsuario()
        {
            return "Comprador";
        }



    }


}
