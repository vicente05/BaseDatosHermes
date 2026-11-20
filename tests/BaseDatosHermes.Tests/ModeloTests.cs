using BaseDatosHermes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BaseDatosHermes.Tests;

/// <summary>
/// Comprobaciones del modelo EF contra el esquema real de la base de datos.
/// No necesitan conexión: construyen los metadatos en memoria.
/// </summary>
public class ModeloTests
{
    private static FinanzasDbContext NuevoContexto() =>
        new(new DbContextOptionsBuilder<FinanzasDbContext>()
            .UseNpgsql("Host=localhost;Database=inexistente;Username=ninguno;Password=ninguno")
            .Options);

    private static readonly string[] TablasEsperadas =
    [
        "cambios_financieros",
        "categorias_gasto",
        "instrumentos",
        "movimientos",
        "referencias_origen",
        "referencias_saldos",
        "saldos",
        "schema_migrations",
        "tipos_movimiento",
    ];

    [Fact]
    public void Modelo_expone_las_nueve_tablas_del_esquema()
    {
        using var ctx = NuevoContexto();

        var tablas = ctx.Model.GetEntityTypes()
            .Select(e => e.GetTableName())
            .Where(t => t is not null)
            .OrderBy(t => t)
            .ToArray();

        Assert.Equal(TablasEsperadas, tablas);
    }

    [Fact]
    public void Las_tablas_de_negocio_viven_en_finanzas_y_la_infraestructura_en_public()
    {
        using var ctx = NuevoContexto();

        Assert.Equal("finanzas", ctx.Model.GetDefaultSchema());

        var porTabla = ctx.Model.GetEntityTypes()
            .ToDictionary(e => e.GetTableName()!, e => e.GetSchema());

        // El historial legado del migrador no es contabilidad: se queda en public.
        Assert.Equal("public", porTabla["schema_migrations"]);
        Assert.Equal(9, porTabla.Count);

        var negocio = porTabla.Where(p => p.Key != "schema_migrations").ToArray();
        Assert.Equal(8, negocio.Length);
        Assert.All(negocio, p => Assert.Equal("finanzas", p.Value));
    }

    [Fact]
    public void Ninguna_clave_primaria_es_autogenerada()
    {
        using var ctx = NuevoContexto();

        var autogeneradas = ctx.Model.GetEntityTypes()
            .SelectMany(e => e.FindPrimaryKey()!.Properties)
            .Where(p => p.ValueGenerated != ValueGenerated.Never)
            .Select(p => $"{p.DeclaringType.DisplayName()}.{p.Name}")
            .ToArray();

        Assert.Empty(autogeneradas);
    }

    [Fact]
    public void Movimientos_tiene_indice_unico_sobre_id_origen()
    {
        using var ctx = NuevoContexto();

        var indice = ctx.Model.FindEntityType("BaseDatosHermes.Entities.Movimiento")!
            .GetIndexes()
            .SingleOrDefault(i => i.Properties.Count == 1 && i.Properties[0].Name == "IdOrigen");

        Assert.NotNull(indice);
        Assert.True(indice!.IsUnique);
        Assert.Equal("id_origen", indice.Properties[0].GetColumnName());
    }

    [Fact]
    public void Importes_son_enteros_en_centimos()
    {
        using var ctx = NuevoContexto();

        var movimiento = ctx.Model.FindEntityType("BaseDatosHermes.Entities.Movimiento")!;
        var importe = movimiento.FindProperty("Importe")!;
        var saldo = ctx.Model.FindEntityType("BaseDatosHermes.Entities.Saldo")!;
        var valor = saldo.FindProperty("ValorActual")!;

        Assert.Equal(typeof(int), importe.ClrType);
        Assert.Equal(typeof(int?), valor.ClrType);
        Assert.Equal("importe", importe.GetColumnName());
        Assert.Equal("saldo", saldo.FindProperty("SaldoCents")!.GetColumnName());
    }

    [Fact]
    public void Restricciones_check_del_esquema_estan_mapeadas()
    {
        using var ctx = NuevoContexto();
        // Los CHECK no viajan al modelo "read-optimized" de ejecución: hay que pedir el de diseño.
        var modelo = ctx.GetService<IDesignTimeModel>().Model;

        string[] checks(string entidad) =>
            modelo.FindEntityType(entidad)!
                .GetCheckConstraints()
                .Select(c => c.Name!)
                .OrderBy(n => n)
                .ToArray();

        Assert.Equal(
            new[]
            {
                "movimientos_check",
                "movimientos_estado_check",
                "movimientos_transferencia_interna_check",
            },
            checks("BaseDatosHermes.Entities.Movimiento"));

        Assert.Equal(
            new[] { "saldos_estado_check", "saldos_moneda_check" },
            checks("BaseDatosHermes.Entities.Saldo"));

        Assert.Equal(
            new[] { "instrumentos_moneda_check" },
            checks("BaseDatosHermes.Entities.Instrumento"));
    }

    [Fact]
    public void Hay_nueve_claves_foraneas_y_todas_restringen_el_borrado()
    {
        using var ctx = NuevoContexto();

        var fks = ctx.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()).ToArray();

        Assert.Equal(9, fks.Length);
        Assert.All(fks, fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
    }

    [Fact]
    public void Schema_migrations_se_mapea_por_version_y_sigue_siendo_consultable()
    {
        using var ctx = NuevoContexto();

        var entidad = ctx.Model.FindEntityType("BaseDatosHermes.Entities.SchemaMigration")!;

        Assert.Equal("schema_migrations", entidad.GetTableName());
        Assert.Equal("version", entidad.FindPrimaryKey()!.Properties.Single().GetColumnName());
    }
}
