using System.ComponentModel.DataAnnotations;

namespace Lab3InmibiliariaVisual.Models{
    public class Contrato
    {
        [Key]
        [Display(Name = "Codigo")]
        public int Id { get; set; }

        public Inmueble? Inmueble { get; set; }

        [Required, Display (Name ="Direccion")]
        public int InmuebleId { get; set; }

        public Inquilino? Inquilino { get; set; }

        [Required, Display(Name ="Inquilino")]
        public int InquilinoId { get; set; }

        [Required, Display(Name ="Fecha Inicio Contrato")]
        public DateTime FecInicio { get; set; }

        [Required, Display(Name ="Fecha Fin contrato")]
        public DateTime FecFin { get; set; }

        public decimal monto { get; set; }

        public bool Estado { get; set; }
    }
}