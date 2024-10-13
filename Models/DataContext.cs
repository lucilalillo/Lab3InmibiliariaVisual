using Microsoft.EntityFrameworkCore;

namespace Lab3InmibiliariaVisual.Models{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<Propietario> Propietarios { get; set; }
        public DbSet<Inquilino> Inquilinos { get; set; }
        public DbSet<Inmueble> Inmuebles { get; set; }
        public DbSet<Pago> Pagos { get; set;  }
        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<Tipo> Tipos {get; set;}
        
    }
}