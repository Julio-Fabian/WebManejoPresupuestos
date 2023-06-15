using Dapper;
using Microsoft.Data.SqlClient;
using WebManejoPresupuestos.Models;

namespace WebManejoPresupuestos.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Crear(Cuenta cuenta);
    }

    public class RepositorioCuentas: IRepositorioCuentas
    {
        private readonly string connectionString;

        public RepositorioCuentas(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        public async Task Crear(Cuenta cuenta)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                int id = await connection.QuerySingleAsync<int>(
                    @"INSERT INTO Cuentas (Nombre, TipoCuentaId, Descripcion, Balance)
                      VALUES (@Nombre, @TipoCuentaId, @Descripcion, @Balance);
                      SELECT SCOPE_IDENTITY();",
                    cuenta
                );

                cuenta.Id = id;
            }
        }
    }
}