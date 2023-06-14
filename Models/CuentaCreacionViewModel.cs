using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebManejoPresupuestos.Models
{
    public class CuentaCreacionViewModel : Cuenta
    {
        public IEnumerable<SelectListItem> TiposCuentas { get; set; }
    }
}