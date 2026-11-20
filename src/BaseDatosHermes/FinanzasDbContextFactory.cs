using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BaseDatosHermes;

/// <summary>
/// Fábrica de diseño para las herramientas de EF Core (`dotnet ef ...`).
/// La cadena de conexión NUNCA vive en el repositorio: se lee de la variable
/// de entorno DATABASE_URL y, si no existe, de la línea DATABASE_URL=... de
/// /opt/data/.env. Así `dotnet ef migrations add` funciona sin secretos en git.
/// </summary>
public sealed class FinanzasDbContextFactory : IDesignTimeDbContextFactory<FinanzasDbContext>
{
    private const string VariableDeEntorno = "DATABASE_URL";
    private const string FicheroEnvPorDefecto = "/opt/data/.env";

    public FinanzasDbContext CreateDbContext(string[] args)
    {
        var connectionString = ResolverCadenaDeConexion();
        var options = new DbContextOptionsBuilder<FinanzasDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new FinanzasDbContext(options);
    }

    private static string ResolverCadenaDeConexion()
    {
        var delEntorno = Environment.GetEnvironmentVariable(VariableDeEntorno);
        if (!string.IsNullOrWhiteSpace(delEntorno))
            return delEntorno.Trim();

        var fichero = Environment.GetEnvironmentVariable("FINANZAS_ENV_FILE") ?? FicheroEnvPorDefecto;
        if (File.Exists(fichero))
        {
            foreach (var linea in File.ReadLines(fichero))
            {
                if (!linea.StartsWith(VariableDeEntorno + "=", StringComparison.Ordinal)) continue;
                var valor = linea[(VariableDeEntorno.Length + 1)..].Trim().Trim('"');
                if (!string.IsNullOrWhiteSpace(valor)) return valor;
            }
        }

        throw new InvalidOperationException(
            $"Falta la cadena de conexión: define {VariableDeEntorno} como variable de entorno " +
            $"o añade '{VariableDeEntorno}=...' a {fichero}.");
    }
}
