using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using GarimpoChic.Util;

namespace GarimpoChic.Models;

// Cada pessoa tem um único cadastro e pode acumular papéis:
// toda pessoa é cliente; vendedora externa e funcionária são marcados à parte.
public class Usuario
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string Nome { get; set; } = "";

    // Só os 11 dígitos, sem pontuação. No banco fica criptografado (veja Protecao e BrechoContext);
    // ao mudar o CPF, o resumo usado para achar a conta é recalculado junto.
    string cpf = "";

    [MaxLength(100)]
    public string Cpf
    {
        get => cpf;
        set
        {
            cpf = value;
            CpfResumo = Protecao.ResumoCpf(value);
        }
    }

    // Fica fora do backup: depende da CHAVE_DADOS, e é recalculado a partir do CPF ao restaurar
    [MaxLength(64), JsonIgnore]
    public string? CpfResumo { get; set; }

    [MaxLength(160)]
    public string Email { get; set; } = "";

    [MaxLength(20)]
    public string Telefone { get; set; } = "";

    [MaxLength(80)]
    public string? Cidade { get; set; }

    [MaxLength(2)]
    public string? Estado { get; set; }

    public string SenhaHash { get; set; } = "";

    public bool EhVendedora { get; set; }

    public bool EhFuncionaria { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // Senhas erradas seguidas; ao chegar no limite, a conta fica bloqueada até BloqueadoAte
    public int TentativasFalhas { get; set; }

    public DateTime? BloqueadoAte { get; set; }

    // Quando a pessoa aceitou a política de privacidade no cadastro (contas antigas ficam vazias)
    public DateTime? AceitouPrivacidadeEm { get; set; }

    // Conta excluída a pedido da pessoa: os dados pessoais são apagados e só sobram os pedidos, sem identificação
    public DateTime? ExcluidaEm { get; set; }

    public List<Pedido> Pedidos { get; set; } = [];

    public List<Favorito> Favoritos { get; set; } = [];

    public string PrimeiroNome =>
        Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? Nome;

    public string Inicial => string.IsNullOrEmpty(Nome) ? "?" : Nome[..1].ToUpper();
}
