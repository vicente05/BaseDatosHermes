namespace BaseDatosHermes.Entities;

/// <summary>Categoría de gasto/ingreso. Taxonomía cerrada y curada.</summary>
public class CategoriaGasto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public int TipoId { get; set; }

    public TipoMovimiento Tipo { get; set; } = null!;
    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}
