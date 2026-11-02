using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GarimpoChic.Data;

public static class Banco
{
    // Com DATABASE_URL (Render) usa Postgres; sem ela, usa um arquivo SQLite local para desenvolver sem instalar nada.
    public static void Configurar(DbContextOptionsBuilder o, IConfiguration config, IHostEnvironment env)
    {
        var url = Environment.GetEnvironmentVariable("DATABASE_URL") ?? config.GetConnectionString("Postgres");

        if (!string.IsNullOrWhiteSpace(url))
            o.UseNpgsql(ConverterUrl(url));
        else
            o.UseSqlite($"Data Source={Path.Combine(env.ContentRootPath, "garimpo.db")}");
    }

    // A Render entrega "postgresql://usuario:senha@host:porta/banco"; o Npgsql espera "Host=...;Username=..."
    static string ConverterUrl(string url)
    {
        if (!url.StartsWith("postgres://") && !url.StartsWith("postgresql://"))
            return url;

        var uri = new Uri(url);
        var credenciais = uri.UserInfo.Split(':', 2);

        return new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort || uri.Port <= 0 ? 5432 : uri.Port,
            Username = Uri.UnescapeDataString(credenciais[0]),
            Password = credenciais.Length > 1 ? Uri.UnescapeDataString(credenciais[1]) : null,
            Database = uri.AbsolutePath.TrimStart('/'),
            SslMode = SslMode.Prefer,
        }.ConnectionString;
    }
}
