using System.Globalization;
using GarimpoChic.Models;

namespace GarimpoChic.Util;

public static class Fotos
{
    public const int MaxFotos = 5;
    public const long MaxBytes = 5 * 1024 * 1024;

    public const string Regra = "Envie até 5 fotos JPG, PNG ou WebP de até 5 MB cada.";

    // Quantos arquivos escolhidos ficaram de fora (formato errado, grandes demais ou além do limite)
    public static int Recusadas(IEnumerable<IFormFile>? arquivos, List<ProdutoFoto> aceitas) =>
        (arquivos?.Count(a => a.Length > 0) ?? 0) - aceitas.Count;

    public static string? AvisoRecusadas(int recusadas) => recusadas switch
    {
        0 => null,
        1 => "1 foto não foi aceita. " + Regra,
        _ => $"{recusadas} fotos não foram aceitas. " + Regra,
    };

    // Só estes formatos são aceitos. SVG fica de fora de propósito: ele pode conter JavaScript.
    public static readonly string[] TiposPermitidos = ["image/jpeg", "image/png", "image/webp"];

    // Descobre o formato pelos primeiros bytes do arquivo, sem confiar no tipo informado pelo navegador
    public static string? DetectarTipo(byte[] dados)
    {
        if (dados.Length >= 3 && dados[0] == 0xFF && dados[1] == 0xD8 && dados[2] == 0xFF)
            return "image/jpeg";

        if (dados.Length >= 8 && dados.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return "image/png";

        if (dados.Length >= 12 && dados.AsSpan(0, 4).SequenceEqual("RIFF"u8) && dados.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            return "image/webp";

        return null;
    }

    // Ignora o que não for JPEG, PNG ou WebP de verdade, ou passar de 5 MB
    public static async Task<List<ProdutoFoto>> LerAsync(IEnumerable<IFormFile>? arquivos)
    {
        var fotos = new List<ProdutoFoto>();
        if (arquivos == null) return fotos;

        foreach (var a in arquivos.Where(a => a.Length > 0 && a.Length <= MaxBytes))
        {
            using var ms = new MemoryStream();
            await a.CopyToAsync(ms);
            var dados = ms.ToArray();

            var tipo = DetectarTipo(dados);
            if (tipo == null) continue;

            fotos.Add(new ProdutoFoto { ContentType = tipo, Dados = dados });
            if (fotos.Count == MaxFotos) break;
        }

        return fotos;
    }
}

public static class Precos
{
    // Aceita "79.90" (campo number do navegador) e "79,90" (digitado à mão)
    public static decimal Ler(string? texto)
    {
        var t = (texto ?? "").Trim().Replace("R$", "").Trim();
        if (t.Contains(',')) t = t.Replace(".", "").Replace(',', '.');
        return decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? Math.Round(v, 2) : 0;
    }
}
