using WebManejoPresupuestos.Models;

namespace WebManejoPresupuestos.Servicios
{
    public interface IservicioReportes
    {
        Task<ReporteTransaccionesDetalladas> ObtenerReporteDetallado(int usuarioId, int mes, int año, dynamic ViewBag);
        Task<ReporteTransaccionesDetalladas> ObtenerReportePorCuenta(int usuarioId, int cuentaId, int mes, int año, dynamic viewBag);
    }
    public class ServicioReportes : IservicioReportes
    {
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly HttpContext httpContext;

        #region Constructores

        public ServicioReportes(IRepositorioTransacciones repositorioTransacciones,
                                IHttpContextAccessor httpContextAccessor)
        {
            this.repositorioTransacciones = repositorioTransacciones;
            this.httpContext = httpContextAccessor.HttpContext;
        }

        #endregion

        #region MetodosPublicos

        /// <summary>
        /// Obtiene el reporte de transacciones detalladas por cuenta de usuario.
        /// </summary>
        /// <param name="usuarioId"></param>
        /// <param name="cuentaId"></param>
        /// <param name="mes"></param>
        /// <param name="año"></param>
        /// <param name="viewBag"></param>
        /// <returns></returns>
        public async Task<ReporteTransaccionesDetalladas> ObtenerReportePorCuenta(int usuarioId, 
                                                          int cuentaId, 
                                                          int mes, 
                                                          int año, 
                                                          dynamic viewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioFechaFin(mes, año);

            // creamos un objeto que usaremos para consultar la informacion con la
            // base de datos (solo se usa para la consulta).
            var obtenerTransaccionesPorCuenta = new ObtenerTransaccionesPorCuenta()
            {
                CuentaId = cuentaId,
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await repositorioTransacciones
                                      .ObtenerPorCuentaId(obtenerTransaccionesPorCuenta);

            var modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);

            AsignarValoresViewBag(viewBag, fechaInicio);

            return modelo;
        }

        public async Task<ReporteTransaccionesDetalladas> ObtenerReporteDetallado(int usuarioId,
                                                          int mes,
                                                          int año,
                                                          dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioFechaFin(mes, año);

            // parametro de la vista.
            var parametro = new ParametroObtenerTransaccionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaFin = fechaFin,
                FechaInicio = fechaInicio
            };

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(parametro);

            var modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);

            AsignarValoresViewBag(ViewBag, fechaInicio);

            return modelo;

        }


        #endregion

        // Metodos privados de simplificacion de operaciones.
        private (DateTime fechaInicio, DateTime fechaFin) GenerarFechaInicioFechaFin(int mes, int año)
        {
            DateTime fechaInicio;
            DateTime fechaFin;

            // si la informacion de la fecha de inicio esta fuera de rangos para una fecha real
            // tomamos como fecha inicial el mes y año actual y lo situamos el primer dia del mes.
            if (mes <= 0 || mes > 12 || año <= 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(año, mes, 1);
            }

            // fecha fin es un mes despues -1, (ultimo dia del mes)
            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            return (fechaInicio, fechaFin);
        }

        private ReporteTransaccionesDetalladas GenerarReporteTransaccionesDetalladas(DateTime fechaInicio, 
                                               DateTime fechaFin, 
                                               IEnumerable<Transaccion> transacciones)
        {
            // Este modelo se enviara a la vista y contendra toda la informacion encesaria
            // para visualizar las transacciones de la cuenta.
            var modelo = new ReporteTransaccionesDetalladas();
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

            return modelo;
        }

        private void AsignarValoresViewBag(dynamic viewBag, DateTime fechaInicio)
        {
            viewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            viewBag.yearAnterior = fechaInicio.AddMonths(-1).Year;

            viewBag.mesPosterior = fechaInicio.AddMonths(1).Month;
            viewBag.yearPosterior = fechaInicio.AddMonths(1).Year;
            // obtenemos la url en donde nos encontramos:
            viewBag.urlRetorno = httpContext.Request.Path + httpContext.Request.QueryString;
        }
    }
}
