using Dapper;
using Microsoft.Data.SqlClient;
using WebManejoPresupuestos.Models;

namespace WebManejoPresupuestos.Servicios
{
    public interface IRepositorioCategorias
    {
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId);
        Task<Categoria> ObtenerPorId(int usuarioId, int id);
        Task Actualizar(Categoria categoria);
        Task Borrar(int id);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId);
    }

    public class RepositorioCategorias : IRepositorioCategorias
    {
        private readonly string _ConnectionString;

        public RepositorioCategorias(IConfiguration configuration)
        {
            _ConnectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public async Task Crear(Categoria categoria)
        {
            using (var connection = new SqlConnection(_ConnectionString))
            {
                var id = await connection.QuerySingleAsync<int>(
                    @"INSERT INTO Categorias (Nombre, TipoOperacionId, UsuarioId)
                     VALUES (@Nombre, @TipoOperacionId, @UsuarioId);
                     SELECT SCOPE_IDENTITY();",
                    categoria
                );

                categoria.Id = id;
            }
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId)
        {
            using (var connection = new SqlConnection(_ConnectionString))
            {
                return await connection.QueryAsync<Categoria>(
                    @"SELECT * FROM Categorias WHERE UsuarioId = @usuarioId", 
                    new {usuarioId}
                );
            }
        }


        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using (var connection = new SqlConnection(_ConnectionString))
            {
                return await connection.QueryAsync<Categoria>(
                    @"SELECT * FROM Categorias WHERE UsuarioId = @usuarioId AND TipoOperacionId = @tipoOperacionId",
                    new { usuarioId, tipoOperacionId }
                );
            }
        }


        public async Task<Categoria> ObtenerPorId(int id, int usuarioId)
        {
            using (var connection = new SqlConnection(_ConnectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<Categoria>(
                    @"SELECT * FROM Categorias WHERE Id = @Id AND UsuarioId = @UsuarioId",
                    new {id, usuarioId}
                );
            }
        }


        public async Task Actualizar(Categoria categoria)
        {
            using (var connection = new SqlConnection(_ConnectionString))
            {
                await connection.ExecuteAsync(
                    @"UPDATE Categorias SET Nombre = @Nombre, TipoOperacionId = @TipoOperacionId
                     WHERE Id = @Id;",
                     categoria
                );
            }
        }


        public async Task Borrar(int id)
        {
            using (var connection = new SqlConnection(_ConnectionString))
            {
                await connection.ExecuteAsync("DELETE Categorias WHERE Id = @Id", new {id});
            }
        }

    }
}
