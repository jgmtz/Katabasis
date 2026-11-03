using System.Text.RegularExpressions;

namespace Lete.Core.Localization;

/// <summary>
/// Marcação de origem <c>{origem:chave}</c> (05/nov/2026): vira o texto de <c>chave.&lt;origem&gt;</c> (ex.:
/// <c>pesada.o_convite.pessoa.pescador</c>) ou, se a origem não tiver um próprio (ou não houver origem), o de
/// <c>chave</c>. Assim um texto muda só o que a origem pede (um substantivo, uma frase), sem quatro cópias.
/// </summary>
public static partial class OriginMarkup
{
    public const string Prefix = "origem:";

    [GeneratedRegex(@"\{origem:([a-z0-9_.]+)\}")]
    private static partial Regex Pattern();

    /// <summary>Troca as marcações; <paramref name="lookup"/> devolve o texto de uma chave (nulo se não existir).</summary>
    public static string Resolve(string text, string? originId, Func<string, string?> lookup) =>
        Pattern().Replace(text, m =>
        {
            var key = m.Groups[1].Value;
            return (originId is null ? null : lookup($"{key}.{originId}")) ?? lookup(key) ?? $"[{key}]";
        });

    /// <summary>As chaves citadas pelas marcações de um texto (a forma padrão, sem a origem).</summary>
    public static IEnumerable<string> Keys(string text) => Pattern().Matches(text).Select(m => m.Groups[1].Value);
}
