
using Dapper;
using Microsoft.Data.SqlClient;
using WebManejoPresupuestos.Models;

namespace WebManejoPresupuestos.Servicios
{
    public interface IRepositorioTransacciones
    {
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
    }
}