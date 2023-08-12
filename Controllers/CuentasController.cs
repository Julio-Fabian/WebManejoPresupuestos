using AutoMapper;
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
        private readonly IMapper mapper;
        private readonly IRepositorioTransacciones repositorioTransacciones;

        public CuentasController(IRepositorioTiposCuentas repositorioTiposCuentas,
                                 IServicioUsuarios repositorioServicioUsuarios,
                                 IRepositorioCuentas repositorioCuentas,
                                 IMapper mapper,
                                 IRepositorioTransacciones repositorioTransacciones) 
        {
            this.repositorioServicioUsuarios = repositorioServicioUsuarios;
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.repositorioCuentas = repositorioCuentas;
            this.mapper = mapper;
            this.repositorioTransacciones = repositorioTransacciones;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarioId = repositorioServicioUsuarios.ObtenerUsuarioId();
            var cuentasConTipoCuenta = await repositorioCuentas.Buscar(usuarioId);

            var modelo = cuentasConTipoCuenta
                .GroupBy(c => c.TipoCuenta) // agrupamos por tipo cuenta.
                .Select(grupo => new IndiceCuentasViewModel 
                { // grupos de este tipo de modelo.
                    // tipo de cuenta del grupo.
                    TipoCuenta = grupo.Key,
                    // listado de cuentas del mismo tipo.
                    Cuentas = grupo.AsEnumerable()
                }).ToList();
            
            return View(modelo);
        }


        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = repositorioServicioUsuarios.ObtenerUsuarioId();
            var modelo = new CuentaCreacionViewModel();
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

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = repositorioServicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            // usamos el mapper y le indicamos que mapee la el objeto cuenta a un objeto CuentaCreacionViewModel
            var modelo = mapper.Map<CuentaCreacionViewModel>(cuenta);

            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CuentaCreacionViewModel cuentaEditar)
        {
            var usuarioId = repositorioServicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(cuentaEditar.Id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuentaEditar.TipoCuentaId, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuentas.Actualizar(cuentaEditar);

            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = repositorioServicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(cuenta);
        }


        [HttpPost]
        public async Task<IActionResult> BorrarCuenta(int id)
        {
            var usuarioId = repositorioServicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuentas.Borrar(id);
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Detalle(int id, int mes, int anio)
        {
            var usuarioId = repositorioServicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            DateTime fechaInicio;
            DateTime fechaFin;

            // si la informacion de la fecha de inicio esta fuera de rangos para una fecha real
            // tomamos como fecha inicial el mes y año actual y lo situamos el primer dia del mes.
            if (mes<= 0 || mes > 12 || anio <= 1900) 
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(anio, mes, 1);
            }

            // fecha fin es un mes despues -1, (ultimo dia del mes)
            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            // creamos un objeto que usaremos para consultar la informacion con la
            // base de datos (solo se usa para la consulta).
            var obtenerTransaccionesPorCuenta = new ObtenerTransaccionesPorCuenta()
            {
                CuentaId = id,
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await repositorioTransacciones.ObtenerPorCuentaId(obtenerTransaccionesPorCuenta);

            // Este modelo se enviara a la vista y contendra toda la informacion encesaria
            // para visualizar las transacciones de la cuenta.
            var modelo = new ReporteTransaccionesDetalladas();
            ViewBag.Cuenta = cuenta.Nombre;

            // ordenamos las transacciones por fecha y las agregamos al modelo, junto a sus
            // fechas de inicio y fin.
            var transaccionesPorFecha = transacciones.OrderByDescending(x => x.FechaTransaccion)
                                        .GroupBy(x => x.FechaTransaccion)
                                        .Select(grupo => new ReporteTransaccionesDetalladas.TransaccionesPorFecha()
                                        {
                                            FechaTransaccion = grupo.Key,
                                            Transacciones = grupo.AsEnumerable()
                                        });

            modelo.TransaccionesAgrupadas = transaccionesPorFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;

            return View(modelo);
        }


        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            // X.Nombre --> nombre en la lista, x.Id.toString() --> 'id del elemento value en html'
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
    }
}