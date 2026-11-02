using GarimpoChic.Models;
using GarimpoChic.Util;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GarimpoChic.Data;

public class BrechoContext(DbContextOptions<BrechoContext> options) : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<ProdutoFoto> ProdutoFotos => Set<ProdutoFoto>();
    public DbSet<Favorito> Favoritos => Set<Favorito>();
    public DbSet<SacolaItem> Sacola => Set<SacolaItem>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoItem> PedidoItens => Set<PedidoItem>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Repasse> Repasses => Set<Repasse>();

    // Chaves que assinam o cookie de login; ficam no banco para o login sobreviver a um novo deploy
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // O CPF vai criptografado para o banco; a busca e a regra de "um cadastro por CPF" usam o resumo
        mb.Entity<Usuario>().Property(u => u.Cpf).HasConversion(v => Protecao.Cifrar(v), v => Protecao.Decifrar(v));
        mb.Entity<Usuario>().HasIndex(u => u.CpfResumo).IsUnique();

        mb.Entity<Produto>().Property(p => p.Preco).HasPrecision(10, 2);
        mb.Entity<Produto>()
            .HasOne(p => p.Vendedora).WithMany()
            .HasForeignKey(p => p.VendedoraId).OnDelete(DeleteBehavior.SetNull);

        mb.Entity<Favorito>().HasKey(f => new { f.UsuarioId, f.ProdutoId });

        mb.Entity<SacolaItem>().HasKey(s => new { s.UsuarioId, s.ProdutoId });

        mb.Entity<Pedido>()
            .HasOne(p => p.Cliente).WithMany(u => u.Pedidos)
            .HasForeignKey(p => p.ClienteId);

        mb.Entity<PedidoItem>().Property(i => i.Preco).HasPrecision(10, 2);
        mb.Entity<PedidoItem>()
            .HasOne(i => i.Produto).WithMany()
            .HasForeignKey(i => i.ProdutoId).OnDelete(DeleteBehavior.SetNull);

        mb.Entity<Repasse>().Property(r => r.Valor).HasPrecision(10, 2);
    }
}
