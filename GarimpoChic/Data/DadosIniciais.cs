using GarimpoChic.Models;
using GarimpoChic.Util;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Data;

public static class DadosIniciais
{
    static readonly PasswordHasher<Usuario> Hasher = new();

    public static string HashSenha(Usuario u, string senha) => Hasher.HashPassword(u, senha);

    public static bool SenhaConfere(Usuario u, string senha) =>
        !string.IsNullOrEmpty(u.SenhaHash) &&
        Hasher.VerifyHashedPassword(u, u.SenhaHash, senha) != PasswordVerificationResult.Failed;

    public static async Task PopularAsync(BrechoContext db, IConfiguration config)
    {
        await CriarFuncionariaAsync(db, config);

        if (config.GetValue<bool>("DadosDemo") && !await db.Produtos.AnyAsync())
            await CriarDemoAsync(db, config["SenhaDemo"]);
    }

    // Primeira funcionária, definida pelas variáveis ADMIN_CPF / ADMIN_SENHA (ex.: na Render)
    static async Task CriarFuncionariaAsync(BrechoContext db, IConfiguration config)
    {
        var cpf = Formato.SoDigitos(config["ADMIN_CPF"]);
        var senha = config["ADMIN_SENHA"];
        if (cpf.Length != 11 || string.IsNullOrEmpty(senha)) return;

        var u = await db.Usuarios.FirstOrDefaultAsync(x => x.Cpf == cpf);
        if (u == null)
        {
            u = new Usuario
            {
                Cpf = cpf,
                Nome = config["ADMIN_NOME"] ?? "Funcionária",
                Email = config["ADMIN_EMAIL"] ?? "",
            };
            u.SenhaHash = HashSenha(u, senha);
            db.Usuarios.Add(u);
        }

        u.EhFuncionaria = true;
        await db.SaveChangesAsync();
    }

    // Mesmo conteúdo que as páginas antigas mostravam fixo no HTML
    static async Task CriarDemoAsync(BrechoContext db, string? senha)
    {
        // Se um CPF de demonstração já tem conta (ex.: a funcionária criada por ADMIN_CPF), reaproveita essa conta
        // em vez de criar outra: o CPF é único e a duplicata impediria o site de iniciar.
        var existentes = await db.Usuarios.ToDictionaryAsync(u => u.Cpf);

        // Sem SenhaDemo definida, as contas de demonstração ficam sem senha (não dá para entrar com elas)
        Usuario Pessoa(string nome, string cpf, string email, string tel, string cidade, bool vendedora, bool funcionaria, int diasAtras)
        {
            if (existentes.TryGetValue(cpf, out var jaExiste))
            {
                jaExiste.EhVendedora |= vendedora;
                jaExiste.EhFuncionaria |= funcionaria;
                return jaExiste;
            }

            var u = new Usuario
            {
                Nome = nome, Cpf = cpf, Email = email, Telefone = tel, Cidade = cidade, Estado = "SC",
                EhVendedora = vendedora, EhFuncionaria = funcionaria,
                CriadoEm = DateTime.UtcNow.AddDays(-diasAtras),
            };
            if (!string.IsNullOrEmpty(senha)) u.SenhaHash = HashSenha(u, senha);
            return u;
        }

        var julia = Pessoa("Julia", "52998224725", "julia@email.com", "(47) 99999-9999", "Joinville", false, true, 90);
        var ana = Pessoa("Ana Clara", "11144477735", "anaclara@email.com", "(47) 98888-1234", "Joinville", true, false, 80);
        var mariana = Pessoa("Mariana Souza", "12345678909", "mariana.souza@email.com", "(47) 97777-4321", "Jaraguá do Sul", false, false, 75);
        var beatriz = Pessoa("Beatriz Lima", "98765432100", "bia.lima@email.com", "(47) 96666-8765", "Blumenau", true, false, 120);
        var camila = Pessoa("Camila Rocha", "39053344705", "camila.rocha@email.com", "(47) 95555-2468", "São Francisco do Sul", true, false, 5);

        // Só as contas novas entram como inserção; as reaproveitadas já estão no banco
        db.Usuarios.AddRange(new[] { julia, ana, mariana, beatriz, camila }.Where(u => u.Id == 0));

        Produto Peca(string nome, string desc, string cat, decimal preco, string status = StatusProduto.Disponivel, Usuario? vendedora = null) =>
            new() { Nome = nome, Descricao = desc, Categoria = cat, Preco = preco, Status = status, Vendedora = vendedora };

        var blusa = Peca("Blusa Vintage", "Peça exclusiva", "Roupas", 79.90m);
        var vestidoMidi = Peca("Vestido Midi Floral", "Tamanho M", "Vestidos", 139.90m);
        var bolsa = Peca("Bolsa Retrô", "Nova coleção", "Bolsas", 129.90m);
        var salto = Peca("Salto Clássico", "Última unidade", "Calçados", 159.90m);
        var casaco = Peca("Casaco de Lã", "Tamanho G", "Roupas", 189.90m, vendedora: ana);
        var calca = Peca("Calça Jeans Reta", "Tamanho 38", "Roupas", 99.90m);
        var chapeu = Peca("Chapéu Palha", "Peça exclusiva", "Acessórios", 49.90m);
        var oculos = Peca("Óculos Vintage", "Anos 80", "Acessórios", 59.90m);
        var vestidoPreto = Peca("Vestido Preto Clássico", "Tamanho P", "Vestidos", 119.90m);
        var jaqueta = Peca("Jaqueta Jeans Oversized", "Enviada para avaliação", "Roupas", 149.90m, StatusProduto.Avaliacao, camila);
        var lenco = Peca("Lenço de Seda", "Anos 70", "Acessórios", 39.90m, StatusProduto.Reservado, beatriz);

        var catalogo = new[] { blusa, vestidoMidi, bolsa, salto, casaco, calca, chapeu, oculos, vestidoPreto, jaqueta, lenco };
        db.Produtos.AddRange(catalogo);

        foreach (var p in new[] { blusa, vestidoMidi, bolsa, oculos })
            db.Favoritos.Add(new Favorito { Usuario = julia, Produto = p });

        PedidoItem Item(Produto p) => new() { Nome = p.Nome, Descricao = p.Descricao, Categoria = p.Categoria, Preco = p.Preco };

        var pedidos = new List<Pedido>
        {
            new() { Cliente = julia, Data = DateTime.UtcNow.AddDays(-2), Status = StatusPedido.Preparo, Itens = [Item(casaco)] },
            new() { Cliente = ana, Data = DateTime.UtcNow.AddDays(-6), Status = StatusPedido.Caminho, Itens = [Item(oculos), Item(chapeu)] },
            new() { Cliente = julia, Data = DateTime.UtcNow.AddDays(-12), Status = StatusPedido.Caminho, Itens = [Item(oculos), Item(chapeu)] },
            new() { Cliente = julia, Data = DateTime.UtcNow.AddDays(-30), Status = StatusPedido.Entregue, Itens = [Item(vestidoMidi)] },
            new() { Cliente = julia, Data = DateTime.UtcNow.AddDays(-47), Status = StatusPedido.Entregue, Itens = [Item(blusa), Item(bolsa)] },
        };

        // Histórico para os gráficos do painel não ficarem vazios
        var aleatorio = new Random(42);
        var clientes = new[] { mariana, beatriz, ana };
        var vendaveis = catalogo.Where(p => p.Status == StatusProduto.Disponivel).ToArray();
        for (var dia = 1; dia < 270; dia += aleatorio.Next(1, 4))
        {
            var itens = Enumerable.Range(0, aleatorio.Next(1, 3))
                .Select(_ => Item(vendaveis[aleatorio.Next(vendaveis.Length)])).ToList();
            pedidos.Add(new Pedido
            {
                Cliente = clientes[aleatorio.Next(clientes.Length)],
                Data = DateTime.UtcNow.AddDays(-dia).AddHours(-aleatorio.Next(0, 10)),
                Status = dia < 8 ? StatusPedido.Caminho : StatusPedido.Entregue,
                Itens = itens,
            });
        }

        // Ids crescem com a data, como numa loja de verdade
        foreach (var p in pedidos.OrderBy(p => p.Data))
        {
            db.Pedidos.Add(p);
            await db.SaveChangesAsync();
        }

        db.Repasses.AddRange(
            new Repasse { Vendedora = ana, Valor = 84.00m, Data = DateTime.UtcNow.AddDays(-3) },
            new Repasse { Vendedora = beatriz, Valor = 120.00m, Data = DateTime.UtcNow.AddDays(-7) },
            new Repasse { Vendedora = ana, Valor = 96.50m, Data = DateTime.UtcNow.AddDays(-35) },
            new Repasse { Vendedora = beatriz, Valor = 210.00m, Data = DateTime.UtcNow.AddDays(-64) });

        var hoje = Formato.Hoje;
        db.Eventos.AddRange(
            new Evento { Data = hoje, Hora = new TimeOnly(14, 0), Titulo = "Recebimento de peças da Ana Clara", Tipo = TipoEvento.Recebimento },
            new Evento { Data = hoje.AddDays(1), Hora = new TimeOnly(10, 0), Titulo = "Avaliação de roupas vintage", Tipo = TipoEvento.Avaliacao },
            new Evento { Data = hoje.AddDays(4), Hora = new TimeOnly(10, 0), Titulo = "Nova coleção em destaque", Tipo = TipoEvento.Colecao },
            new Evento { Data = hoje.AddDays(9), Hora = new TimeOnly(9, 0), Titulo = "Funcionamento reduzido da loja", Tipo = TipoEvento.Outro });

        await db.SaveChangesAsync();
    }
}
