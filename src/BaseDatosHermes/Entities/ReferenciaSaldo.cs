namespace BaseDatosHermes.Entities;

/// <summary>Procedencia de un saldo importado (siempre ligado a su saldo).</summary>
public class ReferenciaSaldo
{
    public int Id { get; set; }
    public int SaldoId { get; set; }
    public string Origen { get; set; } = null!;
    public string Identificador { get; set; } = null!;
    public string RegistroOriginal { get; set; } = null!;

    public Saldo Saldo { get; set; } = null!;
}
