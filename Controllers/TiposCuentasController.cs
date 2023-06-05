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
        private readonly IServicioUsuarios repositorioServicioUsuarios;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas,
            IServicioUsuarios repositorioServicioUsuarios) 
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.repositorioServicioUsuarios = repositorioServicioUsuarios;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = repositorioServicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);

            return View(tiposCuentas);
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

            cuenta.UsuarioId = 
                repositorioServicioUsuarios.ObtenerUsuarioId();

            bool cuentaExiste = await repositorioTiposCuentas
                                      .Existe(cuenta.Nombre, cuenta.UsuarioId);

            if (cuentaExiste) 
            {
                ModelState.AddModelError(nameof(cuenta.Nombre), 
                $"La cuenta {cuenta.Nombre} ya existe.");
                return View(cuenta);
            }
            
            // si el modelo es valido y no existe la cuenta insertamos asincronamente
            await repositorioTiposCuentas.Crear(cuenta);

            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Editar(int id) 
        {
            var usuarioId = repositorioServicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }


        [HttpPost]
        public async Task<ActionResult> Editar(TiposCuentas tipoCuenta)
        {
            var usuarioId = repositorioServicioUsuarios.ObtenerUsuarioId();
            var tipoCuentaExiste = await repositorioTiposCuentas
                                         .ObtenerPorId(tipoCuenta.Id, usuarioId);
            
            if (tipoCuentaExiste is null) 
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }


        [HttpGet] // con este metodo validamos sin pulsar enviar.
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre) 
        {
            var usuarioId = repositorioServicioUsuarios.ObtenerUsuarioId();
            
            var yaExiste = await repositorioTiposCuentas.Existe(nombre, usuarioId);

            if (yaExiste) 
            {
                return Json($"La cuenta {nombre} ya existe.");
            }

            return Json(true);
        }
    }
}
