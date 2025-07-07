using Johan_Reinoso_AP1_P2.Components.Models;
using Microsoft.EntityFrameworkCore;

namespace Johan_Reinoso_AP1_P2.Components.DAL;

public class Contexto: DbContext
{
    public Contexto(DbContextOptions<Contexto> options) : base(options){}
    public DbSet<ModeloParcial> Modelos { get; set; }
}