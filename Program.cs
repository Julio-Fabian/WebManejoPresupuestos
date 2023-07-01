using WebManejoPresupuestos.Servicios;

namespace WebManejoPresupuestos
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services
                   .AddControllersWithViews();
            // servicio para agregar un tipo cuenta.
            builder.Services
                   .AddTransient<IRepositorioTiposCuentas, RepositorioTiposCuentas>();
            builder.Services
                   .AddTransient<IServicioUsuarios, ServicioUsuarios>();
            // servicio de cuentas.
            builder.Services
                   .AddTransient<IRepositorioCuentas, RepositorioCuentas>();
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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}