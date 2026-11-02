using System.Globalization;

namespace GarimpoChic.Util;

public static class Formato
{
    public static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    static readonly TimeZoneInfo Fuso = CarregarFuso();

    static TimeZoneInfo CarregarFuso()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"); }
        catch { return TimeZoneInfo.CreateCustomTimeZone("BRT", TimeSpan.FromHours(-3), "Brasília", "Brasília"); }
    }

    // O banco guarda tudo em UTC; a tela mostra no horário de Brasília
    public static DateTime Local(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Fuso);

    public static DateTime Agora => Local(DateTime.UtcNow);

    public static DateOnly Hoje => DateOnly.FromDateTime(Agora);

    // Converte um horário de Brasília para UTC (para filtrar o banco)
    public static DateTime ParaUtc(DateTime local) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), Fuso);

    public static string Moeda(decimal valor) => valor.ToString("C", PtBr);

    public static string Data(DateTime utc) => Local(utc).ToString("dd/MM/yyyy", PtBr);

    public static string DataExtenso(DateTime utc) => Local(utc).ToString("d 'de' MMMM 'de' yyyy", PtBr);

    public static string Cpf(string digitos) =>
        digitos.Length == 11 ? $"{digitos[..3]}.{digitos[3..6]}.{digitos[6..9]}-{digitos[9..]}" : digitos;

    // Mostra só o meio do CPF, como na tela de clientes
    public static string CpfMascarado(string digitos) =>
        digitos.Length == 11 ? $"•••.{digitos[3..6]}.{digitos[6..9]}-••" : "";

    public static string SoDigitos(string? texto) => new((texto ?? "").Where(char.IsDigit).ToArray());

    public static bool CpfValido(string? valor)
    {
        var n = SoDigitos(valor);
        if (n.Length != 11 || n.Distinct().Count() == 1) return false;

        for (var t = 9; t < 11; t++)
        {
            var soma = 0;
            for (var i = 0; i < t; i++) soma += (n[i] - '0') * (t + 1 - i);
            if ((soma * 10 % 11) % 10 != n[t] - '0') return false;
        }

        return true;
    }

    public static bool EmailValido(string? email) =>
        !string.IsNullOrWhiteSpace(email) &&
        System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$");
}
