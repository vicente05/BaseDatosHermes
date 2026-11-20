namespace BaseDatosHermes.Entities;

/// <summary>
/// Historial de migraciones heredado del SQLite original (3 filas).
/// Se conserva tal cual por trazabilidad; EF usa su propia tabla
/// <c>__EFMigrationsHistory</c> a partir de ahora.
/// </summary>
public class SchemaMigration
{
    public int Version { get; set; }
    public string Name { get; set; } = null!;
    public string Checksum { get; set; } = null!;
    public DateTimeOffset AppliedAt { get; set; }
}
