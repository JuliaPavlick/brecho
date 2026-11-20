using GarimpoChic.Models;
using GarimpoChic.Util;

namespace GarimpoChic.Tests;

// Regras pequenas, testadas direto, sem subir o site
public class RegrasTests
{
    [Theory]
    [InlineData("529.982.247-25", true)]
    [InlineData("52998224725", true)]
    [InlineData("529.982.247-26", false)] // dígito verificador errado
    [InlineData("111.111.111-11", false)] // todos iguais
    [InlineData("1234", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Cpf_valido(string? cpf, bool esperado) => Assert.Equal(esperado, Formato.CpfValido(cpf));

    [Fact]
    public void Cpf_mascarado_esconde_o_comeco_e_o_fim() =>
        Assert.Equal("•••.982.247-••", Formato.CpfMascarado("52998224725"));

    [Theory]
    [InlineData("79,90", 79.90)]
    [InlineData("79.90", 79.90)]
    [InlineData("R$ 1.234,56", 1234.56)]
    [InlineData("abc", 0)]
    [InlineData("", 0)]
    [InlineData(null, 0)]
    public void Preco_aceita_virgula_ponto_e_simbolo(string? texto, double esperado) =>
        Assert.Equal((decimal)esperado, Precos.Ler(texto));

    [Theory]
    [InlineData("ana@exemplo.com", true)]
    [InlineData("ana@exemplo", false)]
    [InlineData("ana exemplo.com", false)]
    [InlineData("", false)]
    public void Email_valido(string email, bool esperado) => Assert.Equal(esperado, Formato.EmailValido(email));

    [Theory]
    [InlineData(StatusPedido.Preparo, StatusPedido.Caminho, true)]
    [InlineData(StatusPedido.Caminho, StatusPedido.Entregue, true)]
    [InlineData(StatusPedido.Preparo, StatusPedido.Cancelado, true)]
    [InlineData(StatusPedido.Entregue, StatusPedido.Cancelado, false)]
    [InlineData(StatusPedido.Cancelado, StatusPedido.Preparo, false)]
    [InlineData(StatusPedido.Preparo, "inventado", false)]
    public void Pedido_entregue_ou_cancelado_nao_muda_de_status(string de, string para, bool esperado) =>
        Assert.Equal(esperado, StatusPedido.PodeMudar(de, para));

    [Fact]
    public void Cpf_criptografado_volta_igual_e_nunca_se_repete()
    {
        Protecao.Configurar("chave-dos-testes-automaticos-com-mais-de-32-caracteres");

        var a = Protecao.Cifrar("52998224725");
        var b = Protecao.Cifrar("52998224725");

        Assert.NotEqual(a, b); // cada cifragem usa um nonce novo
        Assert.Equal("52998224725", Protecao.Decifrar(a));
        Assert.Equal(Protecao.ResumoCpf("52998224725"), Protecao.ResumoCpf("52998224725"));
    }

    [Fact]
    public void Chave_curta_demais_impede_o_site_de_subir() =>
        Assert.Throws<InvalidOperationException>(() => Protecao.Configurar("curta"));
}
