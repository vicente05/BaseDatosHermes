# BaseDatosHermes

Acceso a la base de datos de finanzas personales (Postgres/Neon, proyecto
`PersonalDatabase`) desde C# con Entity Framework Core.

- **Runtime/servidor:** .NET 10 (LTS, soporte hasta 2028-11-14).
- **Proveedor:** `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3.
- **Modelo:** generado por *scaffold* (DB-first) desde el esquema real, no escrito a mano.
- **Historial de migraciones:** EF Core (`__EFMigrationsHistory`) a partir de ahora.
  La tabla `schema_migrations` (3 filas heredadas del SQLite original) se queda en la
  base como histórico legado y **no se toca**.

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

## Reglas del dominio

- Los importes son **enteros en céntimos**, nunca decimales.
- Las PK son `integer` **sin** identity/sequence: al insertar hay que asignar el `id`.
- Los cambios de esquema van **siempre** por migración, nunca `ALTER TABLE` a mano.
- Nada de triggers, funciones ni procedimientos almacenados: las reglas de integridad
  viven en constraints o en la aplicación.
