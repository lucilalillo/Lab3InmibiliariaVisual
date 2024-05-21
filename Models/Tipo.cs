using System.ComponentModel.DataAnnotations;

namespace Lab3InmibiliariaVisual.Models
{
    public class Tipo
    {
        [Key]
		[Display(Name = "Código Interno")]
		public int Id { get; set; }
		[Required]
		public string? Descripcion { get; set; }

       /* public override string ToString()
		{
			return $"{Descripcion}";
		}*/
    }
}