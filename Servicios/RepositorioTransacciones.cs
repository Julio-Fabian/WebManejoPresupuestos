
using Dapper;
using Microsoft.Data.SqlClient;
using WebManejoPresupuestos.Models;

namespace WebManejoPresupuestos.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task ActualizarTransaccion(Transaccion transaccion, decimal montoAnterior, int idAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion t);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<Transaccion> ObtenerTransaccionPorId(int id, int usuarioId);
    }

    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string _ConnectionString;

        public RepositorioTransacciones(IConfiguration configuration)
        {
            _ConnectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion t)
        {
            using (var connection = new SqlConnection(_ConnectionString))
            {
                int id = await connection.QuerySingleAsync<int>(
                    @"SP_Transacciones_Insertar",
                    new 
                    {
                        t.UsuarioId,
                        t.FechaTransaccion,
                        t.Monto,
                        t.CategoriaId,
                        t.CuentaId,
                        t.Nota
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );

                t.Id = id;
            }
        }

        public async Task ActualizarTransaccion(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using (var connection = new SqlConnection(_ConnectionString))
            {
                await connection.ExecuteAsync(
                    "SP_actualizar_transaccion",
                    new
                    {
                        transaccion.Id,
                        transaccion.FechaTransaccion,
                        transaccion.Monto,
                        transaccion.CategoriaId,
                        transaccion.CuentaId,
                        transaccion.Nota,
                        montoAnterior,
                        cuentaAnteriorId
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );
            }
        }

        public async Task Borrar(int id)
        {
            using (var connection = new SqlConnection(_ConnectionString))
            {
                await connection.ExecuteAsync("SP_borrar_transaccion", new {id}, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public async Task<Transaccion> ObtenerTransaccionPorId(int id, int usuarioId)
        {
            using (var connection = new SqlConnection(_ConnectionString))
            {
                return await connection.QueryFirstAsync<Transaccion>(
                    @"SELECT Transacciones.*, cat.TipoOperacionId
                    FROM Transacciones INNER JOIN Categorias cat ON cat.Id = Transacciones.CategoriaId
                    WHERE Transacciones.Id = @Id AND Transacciones.UsuarioId = @UsuarioId;", 
                    new {id, usuarioId}
                );
            }
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using (var connection = new SqlConnection(_ConnectionString))
            {
                return await connection.QueryAsync<Transaccion>(
                    @"SELECT
	                    t.Id, t.Monto, t.FechaTransaccion, c.Nombre AS Categoria,
	                    cu.Nombre AS Cuenta, c.TipoOperacionId
                     FROM 
	                    Transacciones t INNER JOIN Categorias c ON c.Id = t.CategoriaId
	                    INNER JOIN Cuentas cu ON cu.Id = t.Cuentaid
                     WHERE
	                    t.Cuentaid = @CuentaId AND t.UsuarioId = @UsuarioId
	                    AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin;",
                    modelo
                );
            }
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using (var connection = new SqlConnection(_ConnectionString))
            {
                return await connection.QueryAsync<Transaccion>(
                    @"SELECT
	                    t.Id, t.Monto, t.FechaTransaccion, c.Nombre AS Categoria,
	                    cu.Nombre AS Cuenta, c.TipoOperacionId
                     FROM 
	                    Transacciones t INNER JOIN Categorias c ON c.Id = t.CategoriaId
	                    INNER JOIN Cuentas cu ON cu.Id = t.Cuentaid
                     WHERE
	                    t.UsuarioId = @UsuarioId
	                    AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                     ORDER BY t.FechaTransaccion DESC;",
                    modelo
                );
            }
        }
    }
}