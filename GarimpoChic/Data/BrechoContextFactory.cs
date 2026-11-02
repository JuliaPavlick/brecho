using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GarimpoChic.Data;

// Usada só pelo "dotnet ef migrations add": as migrations são geradas para o Postgres (produção).
// Não conecta em nada; a string abaixo só diz ao EF qual banco ele deve imitar.
public class BrechoContextFactory : IDesignTimeDbContextFactory<BrechoContext>
{
    public BrechoContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<BrechoContext>()
            .UseNpgsql("Host=localhost;Database=garimpo")
            .Options);
}
