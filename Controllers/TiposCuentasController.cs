using Microsoft.AspNetCore.Mvc;
using WebManejoPresupuestos.Models;

namespace WebManejoPresupuestos.Controllers
{
    public class TiposCuentasController : Controller
    {
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(TiposCuentas cuenta)
        {
            if(!ModelState.IsValid) // si un campo no es valido regresamos.
            {
                return View(cuenta);
            }

            return View();
        }
    }
}
