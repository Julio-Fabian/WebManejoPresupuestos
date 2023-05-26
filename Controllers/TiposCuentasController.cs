using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using WebManejoPresupuestos.Models;
using WebManejoPresupuestos.Servicios;

namespace WebManejoPresupuestos.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas) 
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TiposCuentas cuenta)
        {
            // los metodos que utilicen metodos 'await async' tambien deben ser 'await async'
            // usamos 'await async' de abajo hacia arriba.

            if(!ModelState.IsValid) // si un campo no es valido regresamos.
            {
                return View(cuenta);
            }

            cuenta.UsuarioId = 1; // agregamos el usuario de prueba

            bool cuentaExiste = await repositorioTiposCuentas.Existe(cuenta.Nombre, cuenta.UsuarioId);

            if (cuentaExiste) 
            {
                ModelState.AddModelError(nameof(cuenta.Nombre), 
                $"La cuenta {cuenta.Nombre} ya existe.");
                return View(cuenta);
            }
            
            // si el modelo es valido y no existe la cuenta insertamos asincronamente
            await repositorioTiposCuentas.Crear(cuenta);

            return View();
        }
    }
}
