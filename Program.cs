using WebManejoPresupuestos.Servicios;

namespace WebManejoPresupuestos
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            // servicio para agregar un tipo cuenta.
            builder.Services.AddTransient<IRepositorioTiposCuentas, RepositorioTiposCuentas>();
            // servicio que retorna el ID de usuario.
            builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();
            // servicio de cuentas.
            builder.Services.AddTransient<IRepositorioCuentas, RepositorioCuentas>();
            // servicio de categorias.
            builder.Services.AddTransient<IRepositorioCategorias, RepositorioCategorias>();
            // Servicio de transacciones
            builder.Services.AddTransient<IRepositorioTransacciones, RepositorioTransacciones>();
            // servicio de reportes.
            builder.Services.AddTransient<IservicioReportes, ServicioReportes>();
            // servicio de http context
            builder.Services.AddHttpContextAccessor();
            // servicio de auto mapper.
            builder.Services.AddAutoMapper(typeof(Program));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. 
                // You may want to change this for production scenarios, 
                // see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            // controlador por defecto (pagina principal que carga).
            app.MapControllerRoute(name: "default", pattern: "{controller=Transacciones}/{action=Index}/{id?}");

            app.Run();
        }
    }
}