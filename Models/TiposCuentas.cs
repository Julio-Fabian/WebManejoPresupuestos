using System.ComponentModel.DataAnnotations;
using WebManejoPresupuestos.Validaciones;

namespace WebManejoPresupuestos.Models
{
    public class TiposCuentas
    {
        public int Id { get; set; }

        [Required (ErrorMessage = "El campo {0}, es obligatorio.")]
        [StringLength (maximumLength: 50, 
            MinimumLength = 3, ErrorMessage = "El campo {0} debe tener un rango de {2} y {1} caracteres.")]
        [Display(Name = "Nombre")]
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; } //Nombre es obligatorio.
        
        public int UsuarioId { get; set; }
        
        public int Orden { get; set; }
    }
}
