namespace BaseDatosHermes.Entities;

/// <summary>Registro de auditoría: estado JSON previo de la fila antes de cambiarla.</summary>
public class CambioFinanciero
{
    public int Id { get; set; }
    public string Entidad { get; set; } = null!;
    public int EntidadId { get; set; }
    public string Anterior { get; set; } = null!;
    public DateTimeOffset CreadoEn { get; set; }
}
