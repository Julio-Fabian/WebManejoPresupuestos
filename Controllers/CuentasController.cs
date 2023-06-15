using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebManejoPresupuestos.Models;
using WebManejoPresupuestos.Servicios;

namespace WebManejoPresupuestos.Controllers
{
    public class CuentasController: Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios repositorioServicioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;

        public CuentasController(IRepositorioTiposCuentas repositorioTiposCuentas,
                                 IServicioUsuarios repositorioServicioUsuarios,
                                 IRepositorioCuentas repositorioCuentas) 
        {
            this.repositorioServicioUsuarios = repositorioServicioUsuarios;
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.repositorioCuentas = repositorioCuentas;
        }


        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = repositorioServicioUsuarios.ObtenerUsuarioId();
            var modelo = new CuentaCreacionViewModel();

            // X.Nombre --> nombre en la lista, x.Id.toString() --> 'id del elemento value en html'
            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
            return View(modelo);
        }


        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            var usuarioId = repositorioServicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuenta.TipoCuentaId, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            if (!ModelState.IsValid)
            {
                cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }

            await repositorioCuentas.Crear(cuenta);
            return RedirectToAction("Index");
        }


        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            // X.Nombre --> nombre en la lista, x.Id.toString() --> 'id del elemento value en html'
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
    }
}