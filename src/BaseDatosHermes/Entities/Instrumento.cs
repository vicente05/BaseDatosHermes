namespace BaseDatosHermes.Entities;

/// <summary>Cuenta, efectivo, plan de ahorro, fondo o broker. Moneda fija: EUR.</summary>
public class Instrumento
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Tipo { get; set; } = null!;
    public string Moneda { get; set; } = null!;

    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    public ICollection<Movimiento> MovimientosComoDestino { get; set; } = new List<Movimiento>();
    public ICollection<Saldo> Saldos { get; set; } = new List<Saldo>();
}
