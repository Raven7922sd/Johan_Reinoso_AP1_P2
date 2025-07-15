using System.ComponentModel.DataAnnotations;

namespace Johan_Reinoso_AP1_P2.Components.Models;

public class Productos
{
    [Key]   
    public int ProductoId { get; set; }
    
    [Required(ErrorMessage = "Campo obligatorio")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    public decimal Peso { get; set; }

    public int Existencia { get; set; }

    public bool EsCompuesto { get; set; } = false;
}