using System.ComponentModel.DataAnnotations;
using WebManejoPresupuestos.Validaciones;

namespace WebManejoPresupuestos.Models
{
    public class TiposCuentas
    {
        public int Id { get; set; }

        // validaciones del campo "Nombre"
        [Required (ErrorMessage = "El campo {0}, es obligatorio.")]
        [Display(Name = "Nombre")]
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; } //Nombre es obligatorio.
        
        public int UsuarioId { get; set; }
        
        public int Orden { get; set; }
    }
}
