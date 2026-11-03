namespace Lete.Core.Combat;

/// <summary>
/// Enquadramento de uma luta (<c>content/rules/enquadramento_batalha.json</c>): a altura na tela (1080p) de quem tem
/// altura 1 no elenco (<paramref name="ScreenHeightPx"/>; menor = câmera mais aberta), as faixas de cinema em cima e
/// embaixo (<paramref name="Letterbox"/>, fração da tela, cada uma) e a folga mínima entre quem luta e a borda do quadro
/// (<paramref name="FitMargin"/>). As lutas de chefe usam um enquadramento próprio (decisão do João, 05/nov/2026).
/// </summary>
public sealed record BattleFraming(string Id, float ScreenHeightPx, float Letterbox, float FitMargin)
{
    public const string Normal = "normal";
    public const float MaxLetterbox = 0.2f;

    /// <summary>Problemas do enquadramento (lista vazia = válido).</summary>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (ScreenHeightPx <= 0f)
        {
            errors.Add($"{Id}: screen_height_px precisa ser positivo.");
        }

        if (Letterbox is < 0f or > MaxLetterbox)
        {
            errors.Add($"{Id}: letterbox precisa ficar entre 0 e {MaxLetterbox} (fração da tela, cada faixa).");
        }

        if (FitMargin < 0f || Letterbox + FitMargin >= 0.5f)
        {
            errors.Add($"{Id}: fit_margin precisa ser positivo e caber na tela com as faixas.");
        }

        return errors;
    }

    /// <summary>
    /// Quanto a câmera (ortográfica, centrada na tela) precisa se afastar para que todos os <paramref name="rects"/>
    /// fiquem dentro do quadro: entre as faixas de cinema, com a folga. 1 = já cabem; 1,25 = abrir 25%.
    /// Afastar encolhe tudo na direção do centro, então cada borda que passa do limite pede a razão entre as duas.
    /// </summary>
    public float ZoomToFit(IEnumerable<ScreenRect> rects)
    {
        var halfWidth = 0.5f - FitMargin;
        var halfHeight = 0.5f - Letterbox - FitMargin;
        var zoom = 1f;
        foreach (var rect in rects)
        {
            zoom = Math.Max(zoom, Math.Max(
                Math.Max(0.5f - rect.X, rect.Right - 0.5f) / halfWidth,
                Math.Max(0.5f - rect.Y, rect.Bottom - 0.5f) / halfHeight));
        }

        return zoom;
    }
}
