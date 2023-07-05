using Dapper;
using Microsoft.Data.SqlClient;
using WebManejoPresupuestos.Models;

namespace WebManejoPresupuestos.Servicios
{
    public interface IRepositorioCategorias
    {
        Task Crear(Categoria categoria);
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
        
    }
}
