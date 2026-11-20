using BaseDatosHermes.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseDatosHermes;

/// <summary>
/// Contexto EF Core de la base de datos de finanzas personales (Postgres/Neon).
/// El mapeo reproduce fielmente el esquema real: nombres snake_case, PK integer
/// sin identity, constraints CHECK y FKs con ON DELETE RESTRICT.
/// </summary>
public class FinanzasDbContext : DbContext
{
    public FinanzasDbContext(DbContextOptions<FinanzasDbContext> options) : base(options)
    {
    }

    public DbSet<TipoMovimiento> TiposMovimiento => Set<TipoMovimiento>();
    public DbSet<CategoriaGasto> CategoriasGasto => Set<CategoriaGasto>();
    public DbSet<Instrumento> Instrumentos => Set<Instrumento>();
    public DbSet<Movimiento> Movimientos => Set<Movimiento>();
    public DbSet<Saldo> Saldos => Set<Saldo>();
    public DbSet<ReferenciaOrigen> ReferenciasOrigen => Set<ReferenciaOrigen>();
    public DbSet<ReferenciaSaldo> ReferenciasSaldos => Set<ReferenciaSaldo>();
    public DbSet<CambioFinanciero> CambiosFinancieros => Set<CambioFinanciero>();
    public DbSet<SchemaMigration> SchemaMigrations => Set<SchemaMigration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Esquema propio para la contabilidad doméstica: nada de mezclarse con
        // `public`. Todas las tablas y el historial de migraciones viven aquí.
        modelBuilder.HasDefaultSchema("finanzas");

        modelBuilder.Entity<TipoMovimiento>(e =>
        {
            e.ToTable("tipos_movimiento");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(x => x.Codigo).HasColumnName("codigo").IsRequired();
            e.HasIndex(x => x.Codigo).IsUnique();
        });

        modelBuilder.Entity<CategoriaGasto>(e =>
        {
            e.ToTable("categorias_gasto");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(x => x.Nombre).HasColumnName("nombre").IsRequired();
            e.Property(x => x.Descripcion).HasColumnName("descripcion");
            e.Property(x => x.TipoId).HasColumnName("tipo_id").IsRequired();
            e.HasOne(x => x.Tipo)
                .WithMany(x => x.Categorias)
                .HasForeignKey(x => x.TipoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Instrumento>(e =>
        {
            e.ToTable("instrumentos", t =>
                t.HasCheckConstraint("instrumentos_moneda_check", "moneda = 'EUR'"));
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(x => x.Nombre).HasColumnName("nombre").IsRequired();
            e.Property(x => x.Tipo).HasColumnName("tipo").IsRequired();
            e.Property(x => x.Moneda).HasColumnName("moneda").IsRequired();
        });

        modelBuilder.Entity<Movimiento>(e =>
        {
            e.ToTable("movimientos", t =>
            {
                t.HasCheckConstraint("movimientos_estado_check", "estado IN ('pendiente', 'confirmado')");
                t.HasCheckConstraint("movimientos_transferencia_interna_check", "transferencia_interna IN (0, 1)");
                t.HasCheckConstraint("movimientos_check", "liquidacion_tarjeta_id IS NULL OR liquidacion_tarjeta_id <> id");
            });
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(x => x.IdOrigen).HasColumnName("id_origen");
            e.Property(x => x.NumeroCuenta).HasColumnName("numero_cuenta");
            e.Property(x => x.CuentaOrigen).HasColumnName("cuenta_origen");
            e.Property(x => x.CuentaDestino).HasColumnName("cuenta_destino");
            e.Property(x => x.TransferenciaInterna).HasColumnName("transferencia_interna");
            e.Property(x => x.NumeroOrden).HasColumnName("numero_orden");
            e.Property(x => x.FechaOperacion).HasColumnName("fecha_operacion").IsRequired();
            e.Property(x => x.FechaValor).HasColumnName("fecha_valor");
            e.Property(x => x.ConceptoBanco).HasColumnName("concepto_banco");
            e.Property(x => x.Descripcion).HasColumnName("descripcion");
            e.Property(x => x.Referencia).HasColumnName("referencia");
            e.Property(x => x.Importe).HasColumnName("importe").IsRequired();
            e.Property(x => x.Moneda).HasColumnName("moneda").IsRequired().HasDefaultValue("EUR");
            e.Property(x => x.SaldoPosterior).HasColumnName("saldo_posterior");
            e.Property(x => x.CategoriaId).HasColumnName("categoria_id");
            e.Property(x => x.Notas).HasColumnName("notas");
            e.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired().HasDefaultValueSql("now()");
            e.Property(x => x.LiquidacionTarjetaId).HasColumnName("liquidacion_tarjeta_id");
            e.Property(x => x.InstrumentoId).HasColumnName("instrumento_id");
            e.Property(x => x.InstrumentoDestinoId).HasColumnName("instrumento_destino_id");
            e.Property(x => x.Estado).HasColumnName("estado").IsRequired().HasDefaultValue("pendiente");
            e.Property(x => x.TipoId).HasColumnName("tipo_id").IsRequired();

            e.HasIndex(x => x.IdOrigen).IsUnique();
            e.HasIndex(x => x.FechaOperacion).HasDatabaseName("idx_movimientos_fecha");
            e.HasIndex(x => x.CategoriaId).HasDatabaseName("idx_movimientos_categoria");
            e.HasIndex(x => x.InstrumentoId).HasDatabaseName("idx_movimientos_instrumento");
            e.HasIndex(x => x.InstrumentoDestinoId).HasDatabaseName("idx_movimientos_instrumento_destino");

            e.HasOne(x => x.Categoria)
                .WithMany(x => x.Movimientos)
                .HasForeignKey(x => x.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Tipo)
                .WithMany(x => x.Movimientos)
                .HasForeignKey(x => x.TipoId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Instrumento)
                .WithMany(x => x.Movimientos)
                .HasForeignKey(x => x.InstrumentoId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.InstrumentoDestino)
                .WithMany(x => x.MovimientosComoDestino)
                .HasForeignKey(x => x.InstrumentoDestinoId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.LiquidacionTarjeta)
                .WithMany(x => x.CargosLiquidados)
                .HasForeignKey(x => x.LiquidacionTarjetaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Saldo>(e =>
        {
            e.ToTable("saldos", t =>
            {
                t.HasCheckConstraint("saldos_estado_check", "estado IN ('pendiente', 'confirmado')");
                t.HasCheckConstraint("saldos_moneda_check", "moneda = 'EUR'");
            });
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(x => x.InstrumentoId).HasColumnName("instrumento_id");
            e.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
            e.Property(x => x.SaldoCents).HasColumnName("saldo");
            e.Property(x => x.Aportado).HasColumnName("aportado");
            e.Property(x => x.ValorActual).HasColumnName("valor_actual");
            e.Property(x => x.Moneda).HasColumnName("moneda").IsRequired();
            e.Property(x => x.Notas).HasColumnName("notas");
            e.Property(x => x.Estado).HasColumnName("estado").IsRequired().HasDefaultValue("pendiente");

            e.HasIndex(x => x.InstrumentoId).HasDatabaseName("idx_saldos_instrumento");
            e.HasOne(x => x.Instrumento)
                .WithMany(x => x.Saldos)
                .HasForeignKey(x => x.InstrumentoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReferenciaOrigen>(e =>
        {
            e.ToTable("referencias_origen");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(x => x.MovimientoId).HasColumnName("movimiento_id");
            e.Property(x => x.Origen).HasColumnName("origen").IsRequired();
            e.Property(x => x.Identificador).HasColumnName("identificador").IsRequired();
            e.Property(x => x.RegistroOriginal).HasColumnName("registro_original").IsRequired();
            e.HasIndex(x => new { x.Origen, x.Identificador }).IsUnique();
            e.HasIndex(x => x.MovimientoId).HasDatabaseName("idx_referencias_origen_mov");
            e.HasOne(x => x.Movimiento)
                .WithMany(x => x.ReferenciasOrigen)
                .HasForeignKey(x => x.MovimientoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReferenciaSaldo>(e =>
        {
            e.ToTable("referencias_saldos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(x => x.SaldoId).HasColumnName("saldo_id").IsRequired();
            e.Property(x => x.Origen).HasColumnName("origen").IsRequired();
            e.Property(x => x.Identificador).HasColumnName("identificador").IsRequired();
            e.Property(x => x.RegistroOriginal).HasColumnName("registro_original").IsRequired();
            e.HasIndex(x => new { x.Origen, x.Identificador }).IsUnique();
            e.HasIndex(x => x.SaldoId).HasDatabaseName("idx_referencias_saldos_saldo");
            e.HasOne(x => x.Saldo)
                .WithMany(x => x.ReferenciasSaldos)
                .HasForeignKey(x => x.SaldoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CambioFinanciero>(e =>
        {
            e.ToTable("cambios_financieros");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(x => x.Entidad).HasColumnName("entidad").IsRequired();
            e.Property(x => x.EntidadId).HasColumnName("entidad_id").IsRequired();
            e.Property(x => x.Anterior).HasColumnName("anterior").IsRequired();
            e.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired().HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<SchemaMigration>(e =>
        {
            e.ToTable("schema_migrations");
            e.HasKey(x => x.Version);
            e.Property(x => x.Version).HasColumnName("version").ValueGeneratedNever();
            e.Property(x => x.Name).HasColumnName("name").IsRequired();
            e.Property(x => x.Checksum).HasColumnName("checksum").IsRequired();
            e.Property(x => x.AppliedAt).HasColumnName("applied_at").IsRequired();
        });
    }
}
