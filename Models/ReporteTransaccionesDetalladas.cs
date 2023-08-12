namespace WebManejoPresupuestos.Models
{
    public class ReporteTransaccionesDetalladas
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public IEnumerable<TransaccionesPorFecha> TransaccionesAgrupadas { get; set; }
        public decimal BalanceDepositos => TransaccionesAgrupadas.Sum(x => x.BalanceDepositos);
        public decimal BalanceRetiros => TransaccionesAgrupadas.Sum(x => x.BalanceRetiros);
        public decimal Total => BalanceDepositos - BalanceRetiros;

        // clase interna para manejar las transacciones por fecha.
        // solo se utilizara en este modelo.
        public class TransaccionesPorFecha
        {
            public DateTime FechaTransaccion { get; set; }
            public IEnumerable<Transaccion> Transacciones { get; set; }
            public decimal BalanceDepositos => this.Transacciones
                                                        .Where(x => x.TipoOperacionId == TipoOperacion.Ingreso)
                                                        .Sum(m => m.Monto);
            public decimal BalanceRetiros => this.Transacciones
                                                     .Where(x => x.TipoOperacionId == TipoOperacion.Gasto)
                                                     .Sum(m => m.Monto);
        }
    }
}
