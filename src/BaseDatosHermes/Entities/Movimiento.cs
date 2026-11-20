namespace BaseDatosHermes.Entities;

/// <summary>
/// Libro mayor. <c>Importe</c> va en céntimos (entero), nunca decimal.
/// La PK no tiene identity: cada inserción debe asignar el id.
/// </summary>
public class Movimiento
{
    public int Id { get; set; }
    public string? IdOrigen { get; set; }
    public string? NumeroCuenta { get; set; }
    public string? CuentaOrigen { get; set; }
    public string? CuentaDestino { get; set; }
    public int? TransferenciaInterna { get; set; }
    public int? NumeroOrden { get; set; }
    public DateOnly FechaOperacion { get; set; }
    public DateOnly? FechaValor { get; set; }
    public string? ConceptoBanco { get; set; }
    public string? Descripcion { get; set; }
    public string? Referencia { get; set; }
    public int Importe { get; set; }
    public string Moneda { get; set; } = "EUR";
    public int? SaldoPosterior { get; set; }
    public int? CategoriaId { get; set; }
    public string? Notas { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
    public int? LiquidacionTarjetaId { get; set; }
    public int? InstrumentoId { get; set; }
    public int? InstrumentoDestinoId { get; set; }
    public string Estado { get; set; } = "pendiente";
    public int TipoId { get; set; }

    public CategoriaGasto? Categoria { get; set; }
    public TipoMovimiento Tipo { get; set; } = null!;
    public Instrumento? Instrumento { get; set; }
    public Instrumento? InstrumentoDestino { get; set; }
    public Movimiento? LiquidacionTarjeta { get; set; }
    public ICollection<Movimiento> CargosLiquidados { get; set; } = new List<Movimiento>();
    public ICollection<ReferenciaOrigen> ReferenciasOrigen { get; set; } = new List<ReferenciaOrigen>();
}
