using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using WebManejoPresupuestos.Models;
using WebManejoPresupuestos.Servicios;

namespace WebManejoPresupuestos.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IMapper mapper;

        private readonly IservicioReportes servicioReportes;

        public TransaccionesController(IServicioUsuarios servicioUsuarios,
                                       IRepositorioCuentas repositorioCuentas,
                                       IRepositorioCategorias repositorioCategorias,
                                       IRepositorioTransacciones repositorioTransacciones,
                                       IMapper mapper, IservicioReportes servicioReportes)
        {
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCategorias = repositorioCategorias;
            this.repositorioTransacciones = repositorioTransacciones;
            this.mapper = mapper;
            this.servicioReportes = servicioReportes;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int mes, int a�o)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var modelo = await servicioReportes.ObtenerReporteDetallado(usuarioId, mes, a�o, ViewBag);

            return View(modelo);
        }

        public async Task<IActionResult> Semanal(int mes, int a�o)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            IEnumerable<ResultadoObtenerPorSemana> transaccionesPorSemana = 
                        await servicioReportes.ObtenerReporteSemanal(usuarioId, mes, a�o, ViewBag);

            // agrupacion de los registros (sin fechas)
            var agrupado = transaccionesPorSemana.GroupBy(t => t.Semana).Select(x =>
                new ResultadoObtenerPorSemana()
                {
                    Semana = x.Key,
                    Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                    Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault()
                }
            ).ToList();

            // agregando la fecha de los registros.
            if (a�o == 0 || mes == 0)
            {
                var hoy = DateTime.Today.Date;
                a�o = hoy.Year;
                mes = hoy.Month;
            }

            var fechaReferencia = new DateTime(a�o, mes, 1);
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);
            
            // guardamos los dias en un arreglo de arreglos donde cada sub arreglo tiene 7 dias
            // (una Semana) y en conjunto representan los dias del mes.
            var diasSegmentados = diasDelMes.Chunk(7).ToList();

            // iteramos el arreglo de arreglos.
            for (int i = 0; i < diasSegmentados.Count(); i++)
            {
                var semana = i + 1; // Semana 1 de Febrero
                var fechaInicio = new DateTime(a�o,mes, diasSegmentados[i].First()); // Lunes 1 de Febrero
                var fechaFin = new DateTime(a�o, mes, diasSegmentados[i].Last()); // Domingo 7 de Febrero

                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);

                if (grupoSemana == null)
                {
                    agrupado.Add(
                        new ResultadoObtenerPorSemana()
                        {
                            Semana = semana,
                            FechaInicio = fechaInicio,
                            FechaFin = fechaFin
                        }
                    );
                }
                else
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }
            }

            agrupado = agrupado.OrderBy(x => x.Semana).ToList();

            var modelo = new ReporteSemanalViewModel()
            {
                TransaccionesPorSemana = agrupado,
                FechaReferencia = fechaReferencia
            };

            return View(modelo);
        }

        public async Task<IActionResult> Mensual(int a�o)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            if (a�o == 0)
            {
                a�o = DateTime.Today.Year;
            }

            var transaccionesPorMes = await repositorioTransacciones.ObtenerPorMes(usuarioId, a�o);

            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes)
                    .Select(x => new ResultadoObtenerPorMes()
                    {
                        Mes = x.Key,
                        Ingreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso)
                                 .Select(x => x.Monto).FirstOrDefault(),
                        Gasto = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto)
                                 .Select(x => x.Monto).FirstOrDefault()
                    }
                ).ToList();

            for (int mes = 1; mes <= 12; mes++)
            {
                var transaccion = transaccionesAgrupadas.FirstOrDefault(t => t.Mes == mes);
                var fechaReferencia = new DateTime(a�o, mes, 1);

                if (transaccion == null)
                {
                    transaccionesAgrupadas.Add(
                            new ResultadoObtenerPorMes()
                            {
                                Mes = mes,
                                FechaReferencia = fechaReferencia
                            }
                        );
                }
                else
                {
                    transaccion.FechaReferencia = fechaReferencia;
                }
            }

            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();

            var modelo = new ReporteMensualViewModel();
            modelo.TransaccionesPorMes = transaccionesAgrupadas;

            return View(modelo);
        }

        public IActionResult ExcelReporte()
        {
            return View();
        }

        public IActionResult Calendario()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel modelo)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }

            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);

            if (cuenta == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if (categoria == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            modelo.UsuarioId = usuarioId;

            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
                modelo.Monto *= -1;

            await repositorioTransacciones.Crear(modelo);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id, string urlRetorno = null)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerTransaccionPorId(id, usuarioId);

            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            // transformamos la transaccion encontrada a un modelo.
            var modelo = mapper.Map<TransaccionActualizacionViewModel>(transaccion);

            modelo.MontoAnterior = modelo.Monto;

            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
                modelo.MontoAnterior = modelo.Monto * -1;

            // asignamos informacion faltante.
            modelo.CuentaAnteriorId = transaccion.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioId, transaccion.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.UrlRetorno = urlRetorno;

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizacionViewModel modelo)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            if (!ModelState.IsValid)
            {
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                return View(modelo);
            }

            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var transaccion = mapper.Map<Transaccion>(modelo);

            if (transaccion.TipoOperacionId == TipoOperacion.Gasto)
            {
                transaccion.Monto *= -1;
            }

            await repositorioTransacciones.ActualizarTransaccion(
                transaccion,
                modelo.MontoAnterior,
                modelo.CuentaAnteriorId
            );

            if (string.IsNullOrEmpty(modelo.UrlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(modelo.UrlRetorno); // investigar LocalRedirect
            }

        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerTransaccionPorId(id, usuarioId);

            if (transaccion is null) 
            { 
                return RedirectToAction("NoEncontrado", "Home"); 
            }

            await repositorioTransacciones.Borrar(id);

            if (string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(urlRetorno); // investigar LocalRedirect
            }
        }

        /// METODOS PRIVADOS/PUBLICOS PARA FUNCIONALIDADES O OBTENCION DE DATOS DEL SERVIDOR ///

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await repositorioCuentas.Buscar(usuarioId);
            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCategorias.Obtener(usuarioId, tipoOperacion);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }
    }
}