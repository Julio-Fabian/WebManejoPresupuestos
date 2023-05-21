using System.ComponentModel.DataAnnotations;
using WebManejoPresupuestos.Validaciones;

namespace WebManejoPresupuestos.Models
{
    public class TiposCuentas //: IValidatableObject
    {
        public int Id { get; set; }

        // validaciones del campo "Nombre"
        [Required (ErrorMessage = "El campo {0}, es obligatorio.")]
        [PrimeraLetraMayuscula] // se sobreentiende 'PrimeraLetraMayusculaAttribute'
        [Display(Name = "Nombre cuenta")]
        public string Nombre { get; set; } //Nombre es obligatorio.
        
        public int UsuarioId { get; set; }
        
        public int Orden { get; set; }

        /*

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Nombre != null && Nombre.Length > 0)
            {
                var letra = Nombre[0].ToString();

                if (letra != letra.ToUpper()) 
                {
                    yield return new ValidationResult("La primer letra debe ser mayuscula",
                        new[] { nameof(Nombre) } );
                }
            }
        } 
        
        */
    }
}
