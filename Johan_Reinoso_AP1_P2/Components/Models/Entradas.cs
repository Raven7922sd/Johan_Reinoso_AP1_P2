using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Johan_Reinoso_AP1_P2.Components.Models;

public class Entradas
{
    [Key]
    public int EntradasId { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    public string Concepto { get; set; } = string.Empty;


    public decimal PesoTotal { get; set; }

    public int IdProducido { get; set; }

    public decimal CantidadProducida { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    [InverseProperty("Entrada")]
    public virtual ICollection<EntradasDetalles> EntradasDetalles { get; set; } = new List<EntradasDetalles>();
}
