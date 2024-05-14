using System.ComponentModel.DataAnnotations;

namespace Lab3InmibiliariaVisual.Models{
    public class Login
    {
         [Required]
            [DataType(DataType.EmailAddress)]
            public String Email { get; set; }
            [Required]
            [DataType(DataType.Password)]
            public String Clave { get; set; }
    }
}