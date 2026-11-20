using BaseDatosHermes;
using Npgsql;

namespace BaseDatosHermes.Tests;

/// <summary>
/// La cadena de conexión puede llegar como URI (`postgresql://...`) o como palabras
/// clave de ADO.NET. La URI se convierte en la factoría porque el parser estándar se
/// rompe cuando la clave lleva `@` sin escapar: toma el primer `@` como separador.
/// </summary>
public class CadenaDeConexionTests
{
    [Fact]
    public void Uri_con_arroba_en_la_clave_se_convierte_bien()
    {
        var original = "postgresql://hermes:cla@ve#con>raros@postgres:5432/hermes";
        var convertida = FinanzasDbContextFactory.ADatosDeConexion(original);
        var builder = new NpgsqlConnectionStringBuilder(convertida);

        Assert.Equal("postgres", builder.Host);
        Assert.Equal(5432, builder.Port);
        Assert.Equal("hermes", builder.Database);
        Assert.Equal("hermes", builder.Username);
        Assert.Equal("cla@ve#con>raros", builder.Password);
    }

    [Fact]
    public void Uri_sin_puerto_usa_5432()
    {
        var convertida = FinanzasDbContextFactory.ADatosDeConexion("postgres://u:p@basedatos/base");
        var builder = new NpgsqlConnectionStringBuilder(convertida);

        Assert.Equal("basedatos", builder.Host);
        Assert.Equal(5432, builder.Port);
        Assert.Equal("base", builder.Database);
    }

    [Fact]
    public void Formato_palabras_clave_pasa_tal_cual()
    {
        const string yaValida = "Host=postgres;Port=5432;Database=hermes;Username=hermes;Password=x";

        Assert.Equal(yaValida, FinanzasDbContextFactory.ADatosDeConexion(yaValida));
    }
}
