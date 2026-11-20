using GarimpoChic.Data;
using GarimpoChic.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GarimpoChic.Tests.Infra;

// Sobe o site inteiro em memória, com um banco SQLite novo (em arquivo temporário) e os dados de demonstração.
// Cada teste cria o seu, então um teste nunca enxerga o que o outro gravou.
public class SiteDeTeste : WebApplicationFactory<Program>
{
    // Contas de demonstração (Data/DadosIniciais.cs)
    public const string Senha = "garimpo123";
    public const string CpfJulia = "529.982.247-25";   // cliente e funcionária (só no desenvolvimento)
    public const string CpfAna = "111.444.777-35";     // cliente e vendedora
    public const string CpfMariana = "123.456.789-09"; // só cliente

    // A chave dos CPFs é lida antes de o site aplicar as configurações do teste, então vai por variável de ambiente.
    // O valor é o mesmo para todos os testes, então não importa que eles rodem ao mesmo tempo.
    static SiteDeTeste() =>
        Environment.SetEnvironmentVariable("CHAVE_DADOS", "chave-dos-testes-automaticos-com-mais-de-32-caracteres");

    readonly string arquivo = Path.Combine(Path.GetTempPath(), $"garimpo-teste-{Guid.NewGuid():N}.db");
    readonly string ambiente;
    readonly Dictionary<string, string?> extras;

    public SiteDeTeste(string ambiente = "Development", Dictionary<string, string?>? extras = null)
    {
        this.ambiente = ambiente;
        this.extras = extras ?? [];
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(ambiente);
        builder.UseSetting("SQLITE_ARQUIVO", arquivo);
        builder.UseSetting("DadosDemo", "true");
        builder.UseSetting("SenhaDemo", Senha);
        foreach (var (chave, valor) in extras) builder.UseSetting(chave, valor);
    }

    public Navegador Navegador() => new(CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true,
    }));

    // Acesso direto ao banco, para preparar a situação do teste e conferir o resultado
    public async Task<T> Banco<T>(Func<BrechoContext, Task<T>> consulta)
    {
        using var escopo = Services.CreateScope();
        return await consulta(escopo.ServiceProvider.GetRequiredService<BrechoContext>());
    }

    public Task<Produto> Produto(string nome) =>
        Banco(db => db.Produtos.AsNoTracking().FirstAsync(p => p.Nome == nome));

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        try { File.Delete(arquivo); } catch (IOException) { }
    }
}
