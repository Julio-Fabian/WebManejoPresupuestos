using Dapper;
using Microsoft.Data.SqlClient;
using WebManejoPresupuestos.Models;

namespace WebManejoPresupuestos.Servicios
{
    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string connectionStr;
        public RepositorioTiposCuentas(IConfiguration config)
        {
            connectionStr = config.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TiposCuentas tiposCuentas) // Task --> void asincrono
        {
            using (var conn = new SqlConnection(connectionStr)) 
            {
                var id = await conn.QuerySingleAsync<int>(
                    $@"INSERT INTO TiposCuentas (Nombre, UsuarioId, Orden) VALUES (@Nombre, @UsuarioId, 0);
                    SELECT SCOPE_IDENTITY();", 
                    tiposCuentas);

                tiposCuentas.Id = id;
            }
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            int id;

            using (var conn = new SqlConnection(connectionStr)) 
            {
                id = await conn.QueryFirstOrDefaultAsync<int>(
                    @"SELECT 1 FROM TiposCuentas
                      WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId;",
                      new {nombre, usuarioId});

                
            }

            return id == 1;
        }

        public async Task<IEnumerable<TiposCuentas>> Obtener(int usuarioId)
        {
            using (var conn = new SqlConnection(connectionStr))
            {
                return await conn.QueryAsync<TiposCuentas>
                    (@"SELECT Id, Nombre, Orden FROM TiposCuentas
                       WHERE UsuarioId = @UsuarioId", new { usuarioId });
            }
        }
    }
}