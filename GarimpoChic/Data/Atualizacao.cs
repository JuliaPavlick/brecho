using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Data;

public static class Atualizacao
{
    // Primeira migration: corresponde ao banco que já existia antes das migrations
    const string MigrationInicial = "20261102153252_Inicial";

    public static async Task PrepararBancoAsync(BrechoContext db)
    {
        // SQLite (desenvolvimento): cria direto pelo modelo. Se o modelo mudar, apague o garimpo.db.
        if (!db.Database.IsNpgsql())
        {
            await db.Database.EnsureCreatedAsync();
            return;
        }

        // O banco da Render foi criado antes das migrations existirem. Se as tabelas já estão lá
        // mas não há histórico, registra a migration inicial como aplicada para o EF não tentar recriá-las.
        await db.Database.ExecuteSqlRawAsync($"""
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" character varying(150) NOT NULL PRIMARY KEY,
                "ProductVersion" character varying(32) NOT NULL
            );
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            SELECT '{MigrationInicial}', '10.0.14'
            WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Usuarios')
              AND NOT EXISTS (SELECT 1 FROM "__EFMigrationsHistory");
            """);

        await db.Database.MigrateAsync();
    }
}
