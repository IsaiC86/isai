
namespace Intermediario.Models
{
    public class Administrador : Usuario
    {
        // El administrador resuelve una disputa
        public bool ResolverDisputa(
            Disputa disputa,
            string resolucion)
        {
            if (disputa.estado_disputa != "Abierta" &&
                disputa.estado_disputa != "EnRevision")
            {
                return false;
            }

            if (resolucion != "ResueltaComprador" &&
                resolucion != "ResueltaVendedor")
            {
                return false;
            }

            disputa.estado_disputa = resolucion;
            disputa.fecha_resolucion = DateTime.Now;
            return true;

        }


        public override string ObtenerTipoUsuario()
        {
            return "Administrador";
        }
    }
}
