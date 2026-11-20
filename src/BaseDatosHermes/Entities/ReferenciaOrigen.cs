namespace BaseDatosHermes.Entities;

/// <summary>Procedencia de un movimiento importado (evita duplicados al reimportar).</summary>
public class ReferenciaOrigen
{
    public int Id { get; set; }
    public int? MovimientoId { get; set; }
    public string Origen { get; set; } = null!;
    public string Identificador { get; set; } = null!;
    public string RegistroOriginal { get; set; } = null!;

    public Movimiento? Movimiento { get; set; }
}
