namespace BaseDatosHermes.Entities;

/// <summary>Catálogo de tipos de movimiento (sustituye a los dos vocabularios CHECK antiguos).</summary>
public class TipoMovimiento
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;

    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    public ICollection<CategoriaGasto> Categorias { get; set; } = new List<CategoriaGasto>();
}
