
using Dapper;
using Microsoft.Data.SqlClient;
using WebManejoPresupuestos.Models;

namespace WebManejoPresupuestos.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task ActualizarTransaccion(Transaccion transaccion, decimal montoAnterior, int idAnterior);
        Task Crear(Transaccion t);
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

        public async Task ActualizarTransaccion(Transaccion transaccion, decimal montoAnterior, int idAnterior)
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
                        idAnterior
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );
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
    }
}