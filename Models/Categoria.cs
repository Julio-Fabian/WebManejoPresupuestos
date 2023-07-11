using System.ComponentModel.DataAnnotations;

namespace WebManejoPresupuestos.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        [Display(Name = "Nombre de la categoria:")]
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(maximumLength:50, ErrorMessage = "No puede ser mayor a {0} caracteres.")]
        public string Nombre { get; set; }
        [Display(Name = "Tipo operacion:")]
        public TipoOperacion TipoOperacionId { get; set; }
        public int UsuarioId { get; set; }
    }
}
