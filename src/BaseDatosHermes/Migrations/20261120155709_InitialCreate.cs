using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseDatosHermes.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "finanzas");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "cambios_financieros",
                schema: "finanzas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    entidad = table.Column<string>(type: "text", nullable: false),
                    entidad_id = table.Column<int>(type: "integer", nullable: false),
                    anterior = table.Column<string>(type: "text", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cambios_financieros", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "instrumentos",
                schema: "finanzas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    moneda = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instrumentos", x => x.id);
                    table.CheckConstraint("instrumentos_moneda_check", "moneda = 'EUR'");
                });

            migrationBuilder.CreateTable(
                name: "schema_migrations",
                schema: "public",
                columns: table => new
                {
                    version = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    checksum = table.Column<string>(type: "text", nullable: false),
                    applied_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schema_migrations", x => x.version);
                });

            migrationBuilder.CreateTable(
                name: "tipos_movimiento",
                schema: "finanzas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_movimiento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "saldos",
                schema: "finanzas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    instrumento_id = table.Column<int>(type: "integer", nullable: true),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    saldo = table.Column<int>(type: "integer", nullable: true),
                    aportado = table.Column<int>(type: "integer", nullable: true),
                    valor_actual = table.Column<int>(type: "integer", nullable: true),
                    moneda = table.Column<string>(type: "text", nullable: false),
                    notas = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<string>(type: "text", nullable: false, defaultValue: "pendiente")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saldos", x => x.id);
                    table.CheckConstraint("saldos_estado_check", "estado IN ('pendiente', 'confirmado')");
                    table.CheckConstraint("saldos_moneda_check", "moneda = 'EUR'");
                    table.ForeignKey(
                        name: "FK_saldos_instrumentos_instrumento_id",
                        column: x => x.instrumento_id,
                        principalSchema: "finanzas",
                        principalTable: "instrumentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "categorias_gasto",
                schema: "finanzas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    tipo_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias_gasto", x => x.id);
                    table.ForeignKey(
                        name: "FK_categorias_gasto_tipos_movimiento_tipo_id",
                        column: x => x.tipo_id,
                        principalSchema: "finanzas",
                        principalTable: "tipos_movimiento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "referencias_saldos",
                schema: "finanzas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    saldo_id = table.Column<int>(type: "integer", nullable: false),
                    origen = table.Column<string>(type: "text", nullable: false),
                    identificador = table.Column<string>(type: "text", nullable: false),
                    registro_original = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referencias_saldos", x => x.id);
                    table.ForeignKey(
                        name: "FK_referencias_saldos_saldos_saldo_id",
                        column: x => x.saldo_id,
                        principalSchema: "finanzas",
                        principalTable: "saldos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movimientos",
                schema: "finanzas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    id_origen = table.Column<string>(type: "text", nullable: true),
                    numero_cuenta = table.Column<string>(type: "text", nullable: true),
                    cuenta_origen = table.Column<string>(type: "text", nullable: true),
                    cuenta_destino = table.Column<string>(type: "text", nullable: true),
                    transferencia_interna = table.Column<int>(type: "integer", nullable: true),
                    numero_orden = table.Column<int>(type: "integer", nullable: true),
                    fecha_operacion = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_valor = table.Column<DateOnly>(type: "date", nullable: true),
                    concepto_banco = table.Column<string>(type: "text", nullable: true),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    referencia = table.Column<string>(type: "text", nullable: true),
                    importe = table.Column<int>(type: "integer", nullable: false),
                    moneda = table.Column<string>(type: "text", nullable: false, defaultValue: "EUR"),
                    saldo_posterior = table.Column<int>(type: "integer", nullable: true),
                    categoria_id = table.Column<int>(type: "integer", nullable: true),
                    notas = table.Column<string>(type: "text", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    liquidacion_tarjeta_id = table.Column<int>(type: "integer", nullable: true),
                    instrumento_id = table.Column<int>(type: "integer", nullable: true),
                    instrumento_destino_id = table.Column<int>(type: "integer", nullable: true),
                    estado = table.Column<string>(type: "text", nullable: false, defaultValue: "pendiente"),
                    tipo_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos", x => x.id);
                    table.CheckConstraint("movimientos_check", "liquidacion_tarjeta_id IS NULL OR liquidacion_tarjeta_id <> id");
                    table.CheckConstraint("movimientos_estado_check", "estado IN ('pendiente', 'confirmado')");
                    table.CheckConstraint("movimientos_transferencia_interna_check", "transferencia_interna IN (0, 1)");
                    table.ForeignKey(
                        name: "FK_movimientos_categorias_gasto_categoria_id",
                        column: x => x.categoria_id,
                        principalSchema: "finanzas",
                        principalTable: "categorias_gasto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimientos_instrumentos_instrumento_destino_id",
                        column: x => x.instrumento_destino_id,
                        principalSchema: "finanzas",
                        principalTable: "instrumentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimientos_instrumentos_instrumento_id",
                        column: x => x.instrumento_id,
                        principalSchema: "finanzas",
                        principalTable: "instrumentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimientos_movimientos_liquidacion_tarjeta_id",
                        column: x => x.liquidacion_tarjeta_id,
                        principalSchema: "finanzas",
                        principalTable: "movimientos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimientos_tipos_movimiento_tipo_id",
                        column: x => x.tipo_id,
                        principalSchema: "finanzas",
                        principalTable: "tipos_movimiento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "referencias_origen",
                schema: "finanzas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    movimiento_id = table.Column<int>(type: "integer", nullable: true),
                    origen = table.Column<string>(type: "text", nullable: false),
                    identificador = table.Column<string>(type: "text", nullable: false),
                    registro_original = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referencias_origen", x => x.id);
                    table.ForeignKey(
                        name: "FK_referencias_origen_movimientos_movimiento_id",
                        column: x => x.movimiento_id,
                        principalSchema: "finanzas",
                        principalTable: "movimientos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categorias_gasto_tipo_id",
                schema: "finanzas",
                table: "categorias_gasto",
                column: "tipo_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_id_origen",
                schema: "finanzas",
                table: "movimientos",
                column: "id_origen",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_liquidacion_tarjeta_id",
                schema: "finanzas",
                table: "movimientos",
                column: "liquidacion_tarjeta_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_tipo_id",
                schema: "finanzas",
                table: "movimientos",
                column: "tipo_id");

            migrationBuilder.CreateIndex(
                name: "idx_movimientos_categoria",
                schema: "finanzas",
                table: "movimientos",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "idx_movimientos_fecha",
                schema: "finanzas",
                table: "movimientos",
                column: "fecha_operacion");

            migrationBuilder.CreateIndex(
                name: "idx_movimientos_instrumento",
                schema: "finanzas",
                table: "movimientos",
                column: "instrumento_id");

            migrationBuilder.CreateIndex(
                name: "idx_movimientos_instrumento_destino",
                schema: "finanzas",
                table: "movimientos",
                column: "instrumento_destino_id");

            migrationBuilder.CreateIndex(
                name: "IX_referencias_origen_origen_identificador",
                schema: "finanzas",
                table: "referencias_origen",
                columns: new[] { "origen", "identificador" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_referencias_origen_mov",
                schema: "finanzas",
                table: "referencias_origen",
                column: "movimiento_id");

            migrationBuilder.CreateIndex(
                name: "IX_referencias_saldos_origen_identificador",
                schema: "finanzas",
                table: "referencias_saldos",
                columns: new[] { "origen", "identificador" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_referencias_saldos_saldo",
                schema: "finanzas",
                table: "referencias_saldos",
                column: "saldo_id");

            migrationBuilder.CreateIndex(
                name: "idx_saldos_instrumento",
                schema: "finanzas",
                table: "saldos",
                column: "instrumento_id");

            migrationBuilder.CreateIndex(
                name: "IX_tipos_movimiento_codigo",
                schema: "finanzas",
                table: "tipos_movimiento",
                column: "codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cambios_financieros",
                schema: "finanzas");

            migrationBuilder.DropTable(
                name: "referencias_origen",
                schema: "finanzas");

            migrationBuilder.DropTable(
                name: "referencias_saldos",
                schema: "finanzas");

            migrationBuilder.DropTable(
                name: "schema_migrations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "movimientos",
                schema: "finanzas");

            migrationBuilder.DropTable(
                name: "saldos",
                schema: "finanzas");

            migrationBuilder.DropTable(
                name: "categorias_gasto",
                schema: "finanzas");

            migrationBuilder.DropTable(
                name: "instrumentos",
                schema: "finanzas");

            migrationBuilder.DropTable(
                name: "tipos_movimiento",
                schema: "finanzas");
        }
    }
}
