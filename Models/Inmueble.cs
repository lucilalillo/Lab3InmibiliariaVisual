using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Lab3InmibiliariaVisual.Models
{
    public class Inmueble
    {
        [Key]
        [Display(Name = "Código Interno")]
		public int Id { get; set; }
		[Required]
		[Display(Name = "Dirección")]
		public string? Direccion { get; set; }
		[Required]
		public int Ambientes { get; set; }
		[Required]
		public int Superficie { get; set; }
		public decimal Latitud { get; set; }

        public string? Uso {get; set; }
		public decimal Longitud { get; set; }
		
		public int? PropietarioId { get; set; }

		[ForeignKey(nameof(PropietarioId))]
        public Propietario? Duenio { get; set; }

		public int  TipoId {get; set;}
        
		public int? Importe {get; set;}

		public bool Disponible {get; set;}

		public string? imgUrl {get; set;}
       
	    [NotMapped]
        public IFormFile imagen { get; set;}

		[ForeignKey(nameof(TipoId))]
		public Tipo? Tipo {get; set;}

		//public string? Disp {get; set;}
    }
}