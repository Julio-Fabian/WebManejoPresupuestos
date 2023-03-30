using System.ComponentModel.DataAnnotations;

namespace WebManejoPresupuestos.Validaciones
{
    public class PrimeraLetraMayusculaAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var primerLetra = value.ToString()[0].ToString();

            if (primerLetra != primerLetra.ToUpper())
            {
                return new ValidationResult("La palabra debe iniciar con mayuscula.");
            }

            return ValidationResult.Success;
        }
    }
}
