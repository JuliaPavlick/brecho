using System.Globalization;
using GarimpoChic.Models;

namespace GarimpoChic.Util;

public static class Fotos
{
    const int MaxFotos = 5;
    const long MaxBytes = 5 * 1024 * 1024;

    // Ignora o que não for imagem ou passar de 5 MB
    public static async Task<List<ProdutoFoto>> LerAsync(IEnumerable<IFormFile>? arquivos)
    {
        var fotos = new List<ProdutoFoto>();
        if (arquivos == null) return fotos;

        foreach (var a in arquivos.Where(a => a.ContentType.StartsWith("image/") && a.Length > 0 && a.Length <= MaxBytes).Take(MaxFotos))
        {
            using var ms = new MemoryStream();
            await a.CopyToAsync(ms);
            fotos.Add(new ProdutoFoto { ContentType = a.ContentType, Dados = ms.ToArray() });
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
