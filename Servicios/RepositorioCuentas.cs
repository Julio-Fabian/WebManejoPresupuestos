using Dapper;
using Microsoft.Data.SqlClient;
using WebManejoPresupuestos.Models;

namespace WebManejoPresupuestos.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Crear(Cuenta cuenta);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
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


        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsync<Cuenta>(
                    @"SELECT Cuentas.Id, Cuentas.Nombre, Balance, tc.Nombre AS TipoCuenta
                      FROM Cuentas INNER JOIN TiposCuentas tc
                      ON tc.Id = Cuentas.TipoCuentaId
                      WHERE tc.UsuarioId = @UsuarioId
                      ORDER BY tc.Orden;",
                      new { usuarioId }
                );
            }
        }


        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId) 
        {
            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<Cuenta>(
                    @"SELECT Cuentas.Id, Cuentas.Nombre, Balance, Descripcion, tc.TipoCuentaId
                      FROM Cuentas INNER JOIN TiposCuentas tc
                      ON tc.Id = Cuentas.TipoCuentaId
                      WHERE tc.UsuarioId = @UsuarioId AND Cuentas.Id = @Id",
                      new {id, usuarioId} // objeto anonimo
                );
            }
        }
    }
}