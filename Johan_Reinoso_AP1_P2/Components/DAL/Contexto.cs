using Johan_Reinoso_AP1_P2.Components.Models;
using Microsoft.EntityFrameworkCore;

namespace Johan_Reinoso_AP1_P2.Components.DAL;

public class Contexto : DbContext
{
    public Contexto(DbContextOptions<Contexto> options) : base(options) { }

    public DbSet<Productos> productos { get; set; } 
                                                   
    public DbSet<Entradas> entradas { get; set; }
    public DbSet<EntradasDetalles> entradasDetalles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Productos>().HasData(
            new Productos 
            {
                ProductoId = 1,
                Descripcion = "Maní",
                Peso = 0,
                Existencia = 100,
                EsCompuesto = false
            },
            new Productos
            {
                ProductoId = 2,
                Descripcion = "Pistachos",
                Peso = 0,
                Existencia = 100,
                EsCompuesto = false
            },
            new Productos
            {
                ProductoId = 3,
                Descripcion = "Almendras",
                Peso = 0,
                Existencia = 100,
                EsCompuesto = false
            },
            new Productos
            {
                ProductoId = 4,
                Descripcion = "Frutos Mixtos 200gr",
                Peso = 200.00m,
                Existencia = 0,
                EsCompuesto = true
            },
            new Productos
            {
                ProductoId = 5,
                Descripcion = "Frutos Mixtos 400gr",
                Peso = 400.00m,
                Existencia = 0,
                EsCompuesto = true
            },
            new Productos
            {
                ProductoId = 6,
                Descripcion = "Frutos Mixtos 600gr",
                Peso = 600.00m,
                Existencia = 0,
                EsCompuesto = true
            }
        );
      
    }
}