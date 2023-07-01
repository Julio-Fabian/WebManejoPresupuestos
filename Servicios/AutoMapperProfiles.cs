using AutoMapper;
using WebManejoPresupuestos.Models;

namespace WebManejoPresupuestos.Servicios
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles() 
        { 
            // con esto podemos mapear una cuenta y transformarla a
            // cuenta creacion view model.
            CreateMap<Cuenta, CuentaCreacionViewModel>();
        }
    }
}
