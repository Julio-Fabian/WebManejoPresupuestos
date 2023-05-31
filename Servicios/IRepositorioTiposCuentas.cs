using WebManejoPresupuestos.Models;

namespace WebManejoPresupuestos.Servicios
{
    public interface IRepositorioTiposCuentas
    {
        Task Crear(TiposCuentas tiposCuentas);

        Task<bool> Existe(string nombre, int usuario);

        Task<IEnumerable<TiposCuentas>> Obtener(int usuarioId);
    }
}