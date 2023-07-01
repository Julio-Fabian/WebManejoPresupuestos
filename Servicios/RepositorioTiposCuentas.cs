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
                    "SP_TiposCuentas_Insertar",  // asi se llama un SP con Drapper.
                    // objeto anonimo (El Sp no necesita todos los parametros del objeto)
                    new {
                        UsuarioId = tiposCuentas.UsuarioId,
                        Nombre = tiposCuentas.Nombre
                    },

                    // tipo de ejecucion.
                    commandType: System.Data.CommandType.StoredProcedure
                );

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
                      new {nombre, usuarioId}
                      );
            }

            return id == 1;
        }

        public async Task<IEnumerable<TiposCuentas>> Obtener(int usuarioId)
        {
            using (var conn = new SqlConnection(connectionStr))
            {
                return await conn.QueryAsync<TiposCuentas>(
                    @"SELECT Id, Nombre, Orden FROM TiposCuentas
                    WHERE UsuarioId = @UsuarioId ORDER BY Orden", 
                    new { usuarioId }
                );
            }
        }


        public async Task Actualizar(TiposCuentas tipoCuenta) 
        {
            using (var conn = new SqlConnection(connectionStr))
            {
                // ExecuteAsync ejecuta un query que no retorna nada.
                await conn.ExecuteAsync(
                    @"UPDATE TiposCuentas
                    SET Nombre = @Nombre
                    WHERE Id = @Id",
                    tipoCuenta
                );
            }
        }


        public async Task<TiposCuentas> ObtenerPorId(int id, int usuarioId)
        {
            using (var conn = new SqlConnection(connectionStr))
            {
                return await conn.QueryFirstOrDefaultAsync<TiposCuentas>(
                    @"SELECT Id, Nombre, Orden
                      FROM TiposCuentas
                      WHERE Id = @Id AND UsuarioId = @UsuarioId",
                      new {id, usuarioId}
                );
            }            
        }

        public async Task Borrar(int Id) 
        {
            using (var connection = new SqlConnection(connectionStr))
            {
                await connection.ExecuteAsync(
                    @"DELETE TiposCuentas WHERE Id = @Id", new { Id }
                );
            }
        }

        public async Task Ordenar(IEnumerable<TiposCuentas> tiposCuentasOrdenados)
        {
            var query = @"UPDATE TiposCuentas SET Orden = @Orden WHERE Id = @Id";

            using (var connection = new SqlConnection(connectionStr))
            {
                await connection.ExecuteAsync(query, tiposCuentasOrdenados);
            }
        }
    }
}