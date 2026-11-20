using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace BaseDatosHermes;

/// <summary>
/// Fábrica de diseño para las herramientas de EF Core (`dotnet ef ...`).
/// La cadena de conexión NUNCA vive en el repositorio: se lee de la variable de
/// entorno <c>DATABASE_URL</c> o <c>CONNECTION_STRING_POSTGRES_HERMES</c> y, si no
/// existen, de esas mismas claves en <c>/opt/data/.env</c>. Así `dotnet ef` funciona
/// sin secretos en git.
///
/// Acepta tanto el formato URI (<c>postgresql://usuario:clave@host:5432/base</c>) como
/// el de palabras clave de ADO.NET (<c>Host=...;Database=...</c>). La URI se convierte
/// aquí porque ni Npgsql ni psycopg2 la parsean bien cuando la clave lleva caracteres
/// como <c>@</c>: el primer <c>@</c> de la cadena no es el separador.
/// </summary>
public sealed class FinanzasDbContextFactory : IDesignTimeDbContextFactory<FinanzasDbContext>
{
    private const string FicheroEnvPorDefecto = "/opt/data/.env";

    private static readonly string[] ClavesDeConexion =
    [
        "DATABASE_URL",
        "CONNECTION_STRING_POSTGRES_HERMES",
    ];

    public FinanzasDbContext CreateDbContext(string[] args)
    {
        var connectionString = ResolverCadenaDeConexion();
        var options = new DbContextOptionsBuilder<FinanzasDbContext>()
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public"))
            .Options;
        return new FinanzasDbContext(options);
    }

    private static string ResolverCadenaDeConexion()
    {
        foreach (var clave in ClavesDeConexion)
        {
            var delEntorno = Environment.GetEnvironmentVariable(clave);
            if (!string.IsNullOrWhiteSpace(delEntorno))
                return ADatosDeConexion(delEntorno.Trim());
        }

        var fichero = Environment.GetEnvironmentVariable("FINANZAS_ENV_FILE") ?? FicheroEnvPorDefecto;
        if (File.Exists(fichero))
        {
            var valores = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var linea in File.ReadLines(fichero))
            {
                var corte = linea.IndexOf('=');
                if (corte <= 0) continue;
                var nombre = linea[..corte].Trim();
                if (!ClavesDeConexion.Contains(nombre)) continue;
                var valor = linea[(corte + 1)..].Trim().Trim('"').Trim('\'');
                if (!string.IsNullOrWhiteSpace(valor)) valores[nombre] = valor;
            }

            foreach (var clave in ClavesDeConexion)
                if (valores.TryGetValue(clave, out var valor))
                    return ADatosDeConexion(valor);
        }

        throw new InvalidOperationException(
            "Falta la cadena de conexión: define DATABASE_URL o CONNECTION_STRING_POSTGRES_HERMES " +
            $"como variable de entorno, o añade una de las dos a {fichero}.");
    }

    /// <summary>
    /// Convierte una URI de Postgres en cadena de palabras clave de ADO.NET. Si el valor
    /// ya viene en ese formato se devuelve tal cual.
    /// </summary>
    public static string ADatosDeConexion(string valor)
    {
        var esUri = valor.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
                    || valor.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);
        if (!esUri) return valor;

        var resto = valor[(valor.IndexOf("://", StringComparison.Ordinal) + 3)..];

        // Último '@': la clave puede contener '@' sin escapar.
        var arroba = resto.LastIndexOf('@');
        if (arroba < 0) throw new FormatException("La URI de conexión no tiene separador '@'.");

        var credenciales = resto[..arroba];
        var hostYBase = resto[(arroba + 1)..];

        var barra = hostYBase.IndexOf('/');
        var hostPuerto = barra >= 0 ? hostYBase[..barra] : hostYBase;
        var baseDatos = barra >= 0 ? hostYBase[(barra + 1)..] : string.Empty;

        var interrogante = baseDatos.IndexOf('?');
        if (interrogante >= 0) baseDatos = baseDatos[..interrogante];

        var dosPuntosCred = credenciales.IndexOf(':');
        var usuario = dosPuntosCred >= 0 ? credenciales[..dosPuntosCred] : credenciales;
        var clave = dosPuntosCred >= 0 ? credenciales[(dosPuntosCred + 1)..] : string.Empty;

        // Último ':' del host:port, ignorando IPv6 entre corchetes.
        var dosPuntosHost = hostPuerto.LastIndexOf(':');
        var host = dosPuntosHost >= 0 ? hostPuerto[..dosPuntosHost] : hostPuerto;
        var puerto = dosPuntosHost >= 0 ? hostPuerto[(dosPuntosHost + 1)..] : "5432";

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = Uri.UnescapeDataString(host.Trim('[', ']')),
            Port = int.TryParse(puerto, out var p) ? p : 5432,
            Database = Uri.UnescapeDataString(baseDatos),
            Username = Uri.UnescapeDataString(usuario),
            Password = Uri.UnescapeDataString(clave),
        };

        return builder.ConnectionString;
    }
}
