namespace BaseDatosHermes.Entities;

/// <summary>Valoración/snapshot periódico por instrumento. Importes en céntimos.</summary>
public class Saldo
{
    public int Id { get; set; }
    public int? InstrumentoId { get; set; }
    public DateOnly Fecha { get; set; }
    public int? SaldoCents { get; set; }
    public int? Aportado { get; set; }
    public int? ValorActual { get; set; }
    public string Moneda { get; set; } = "EUR";
    public string? Notas { get; set; }
    public string Estado { get; set; } = "pendiente";

    public Instrumento? Instrumento { get; set; }
    public ICollection<ReferenciaSaldo> ReferenciasSaldos { get; set; } = new List<ReferenciaSaldo>();
}
