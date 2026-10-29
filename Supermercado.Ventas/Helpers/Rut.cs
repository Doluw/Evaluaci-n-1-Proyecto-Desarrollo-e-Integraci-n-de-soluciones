using System.Text.RegularExpressions;

namespace Supermercado.Ventas.Helpers;

public static class Rut
{
    // Quita puntos y espacios, pasa la K a mayúscula y deja el guion.
    // "12.345.678-k" -> "12345678-K"
    public static string Normalizar(string? rut)
    {
        if (string.IsNullOrWhiteSpace(rut)) return "";
        var limpio = Regex.Replace(rut, @"[\.\s-]", "").ToUpperInvariant();
        if (limpio.Length < 2) return limpio;
        return $"{limpio[..^1]}-{limpio[^1]}";
    }

    // Verifica el formato y el dígito verificador (módulo 11).
    public static bool EsValido(string? rut)
    {
        var r = Normalizar(rut);
        var m = Regex.Match(r, @"^(\d{1,8})-([\dK])$");
        if (!m.Success) return false;

        int suma = 0, factor = 2;
        foreach (var c in m.Groups[1].Value.Reverse())
        {
            suma += (c - '0') * factor;
            factor = factor == 7 ? 2 : factor + 1;
        }

        int resto = 11 - (suma % 11);
        char dv = resto == 11 ? '0' : resto == 10 ? 'K' : (char)('0' + resto);
        return dv == m.Groups[2].Value[0];
    }
}