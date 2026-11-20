using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseDatosHermes.Migrations
{
    /// <summary>
    /// Baseline del esquema ya existente en Neon (9 tablas: cambios_financieros,
    /// categorias_gasto, instrumentos, movimientos, referencias_origen,
    /// referencias_saldos, saldos, schema_migrations y tipos_movimiento).
    ///
    /// Up/Down van VACÍOS a propósito: la base de datos ya tiene esas tablas y
    /// crearlas aquí volvería a ejecutar el DDL. Esta migración solo escribe su
    /// fila en __EFMigrationsHistory; a partir de aquí EF calcula los cambios
    /// contra FinanzasDbContextModelSnapshot.cs, que sí refleja el esquema actual.
    /// No editar el snapshot a mano.
    ///
    /// La tabla schema_migrations (3 filas heredadas del SQLite original) se
    /// conserva como histórico legado y no se gestiona desde EF.
    /// </summary>
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intencionadamente vacío: el esquema ya existe (ver comentario de la clase).
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intencionadamente vacío: bajar el baseline no debe borrar tablas con datos.
        }
    }
}
