# BaseDatosHermes

Acceso a la base de datos de finanzas personales (Postgres/Neon, proyecto
`PersonalDatabase`) desde C# con Entity Framework Core.

- **Runtime/servidor:** .NET 10 (LTS, soporte hasta 2028-11-14).
- **Proveedor:** `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3.
- **Modelo:** generado por *scaffold* (DB-first) desde el esquema real, no escrito a mano.
- **Esquema:** las 8 tablas de negocio viven en el esquema Postgres `finanzas`
  (`HasDefaultSchema`), no en `public`. Las dos tablas de infraestructura se quedan en
  `public`: `__EFMigrationsHistory` (historial de EF) y `schema_migrations`.
- **Historial de migraciones:** EF Core (`__EFMigrationsHistory`, en `public`). La tabla
  `schema_migrations` (3 filas heredadas del SQLite original) se replica en `public` como
  histórico legado y **no se toca** desde EF.


## Estructura

```
BaseDatosHermes.slnx
src/BaseDatosHermes/            # DbContext + entidades (scaffold) + fábrica de diseño
tests/BaseDatosHermes.Tests/    # xUnit
.config/dotnet-tools.json       # dotnet-ef 10.0.15 (herramienta local)
```

## Credenciales

La cadena de conexión **nunca** va al repositorio. Orden de resolución:

1. Variable de entorno `DATABASE_URL`.
2. Línea `DATABASE_URL=...` en `/opt/data/.env` (o en el fichero que indique `FINANZAS_ENV_FILE`).

`FinanzasDbContextFactory` implementa esa lectura para las herramientas de EF.

```bash
export DOTNET_ROOT=/opt/data/.dotnet
export PATH=$DOTNET_ROOT:$PATH
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
```

## Comandos habituales

```bash
# 1) Refrescar el modelo desde la base (DDL -> clases). Sobrescribe Entities/.
set -a; . /opt/data/.env; set +a
dotnet ef dbcontext scaffold "$DATABASE_URL" Npgsql.EntityFrameworkCore.PostgreSQL \
  --project src/BaseDatosHermes --startup-project src/BaseDatosHermes \
  --context FinanzasDbContext --context-namespace BaseDatosHermes \
  --context-dir . --output-dir Entities --namespace BaseDatosHermes.Entities \
  --no-onconfiguring

# 2) Nueva migración (solo código, no toca la base)
dotnet ef migrations add <Nombre> --project src/BaseDatosHermes --startup-project src/BaseDatosHermes

# 3) Revisar el SQL antes de aplicar
dotnet ef migrations script --project src/BaseDatosHermes --startup-project src/BaseDatosHermes

# 4) Aplicar (ESCRITURA en la base de datos: pedir confirmación antes)
dotnet ef database update --project src/BaseDatosHermes --startup-project src/BaseDatosHermes

# Tests
dotnet test
```

## Base local desde cero

El repo genera el esquema completo; no hace falta dump. `InitialCreate` crea el esquema
`finanzas` con las 8 tablas de negocio, los 6 CHECKs, las 9 FKs (`ON DELETE RESTRICT`) y
los índices (incluidos los de FK que la base de Neon no tenía), más `schema_migrations` en
`public`.

La cadena de conexión se resuelve en este orden: `DATABASE_URL`,
`CONNECTION_STRING_POSTGRES_HERMES`, y si no, esas mismas claves en `/opt/data/.env`.
Acepta URI (`postgresql://...`) o palabras clave de ADO.NET; la URI se convierte en la
factoría porque la clave puede llevar `@` sin escapar.


```bash
# 1) El esquema lo crea la migración; la base de datos tiene que existir antes.
#    En este entorno es la base `hermes` del Postgres del compose (usuario `hermes`,
#    sin privilegio CREATEDB, pero sí puede crear esquemas en su propia base).

# 2) Cadena de conexión, fuera de git (DATABASE_URL o CONNECTION_STRING_POSTGRES_HERMES)
export DATABASE_URL="Host=postgres;Port=5432;Database=hermes;Username=<usuario>;Password=<clave>"

# 3) Aplicar
dotnet ef database update --project src/BaseDatosHermes --startup-project src/BaseDatosHermes
```

Notas de fidelidad con respecto a Neon:

- Los *nombres* de PK, índices únicos y FKs los pone EF (`PK_movimientos`,
  `IX_movimientos_id_origen`, `FK_movimientos_tipos_movimiento_tipo_id`) y no los de Neon
  (`movimientos_pkey`, `movimientos_id_origen_key`, `movimientos_tipo_id_fkey`). Misma
  semántica, distinto nombre.
- `tipos_movimiento.id` es `IDENTITY` en Neon pero el modelo lo mapea como
  `ValueGeneratedNever`, así que en local es `integer` normal. Los ids se asignan a mano,
  como en el resto de tablas.
- EF añade índices a `categorias_gasto.tipo_id`, `movimientos.tipo_id` y
  `movimientos.liquidacion_tarjeta_id`; Neon no los tenía.

## Reglas del dominio


- Los importes son **enteros en céntimos**, nunca decimales.
- Las PK son `integer` **sin** identity/sequence: al insertar hay que asignar el `id`.
- Los cambios de esquema van **siempre** por migración, nunca `ALTER TABLE` a mano.
- Nada de triggers, funciones ni procedimientos almacenados: las reglas de integridad
  viven en constraints o en la aplicación.
