using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Johan_Reinoso_AP1_P2.Components.Models;

public class EntradasDetalles
{
    [Key]
    public int EntradasDetallesId { get; set; }

    public int ProductoId { get; set; }
    [ForeignKey("ProductoId")]
    public Productos Producto { get; set; } = null!;

    public int EntradasId { get; set; }
    [ForeignKey("EntradasId")]
    public Entradas Entrada { get; set; } = null!;

    public int Cantidad { get; set; }
}
