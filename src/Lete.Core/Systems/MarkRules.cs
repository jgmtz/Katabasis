using Lete.Core.State;

namespace Lete.Core.Systems;

/// <summary>Faixas da Marca (GDD 2.3): baixa (nada acontece), média e alta.</summary>
public enum MarkBand
{
    Low,
    Mid,
    High,
}

/// <summary>
/// Números da Marca (<c>content/rules/marca.json</c>, todos provisórios): onde começam as faixas média e alta e
/// quantas Memórias Pesadas encaradas devolvem o rosto pela regra C (PENDÊNCIAS 44) e quantas deixam o rosto meio
/// coberto (<see cref="FacedForHalfFace"/>; 0 = sem meio-termo); quanto sobe por perjúrio e por oferenda (o sangue, GDD 5.1).
/// </summary>
public sealed record MarkRules(int MidFrom, int HighFrom, int FacedToReturnFace, int PerjuryMark = 0,
    IReadOnlyDictionary<string, int>? OfferingMark = null, int FacedForHalfFace = 0)
{
    /// <summary>A regra C: o rosto pelo número de Pesadas encaradas.</summary>
    public FinalFace FaceFor(int faced) =>
        faced >= FacedToReturnFace ? FinalFace.Returned
        : FacedForHalfFace > 0 && faced >= FacedForHalfFace ? FinalFace.Half
        : FinalFace.Covered;

    public int MarkOfOffering(string kind) => OfferingMark?.GetValueOrDefault(kind) ?? 0;

    public MarkBand BandOf(int mark) => mark >= HighFrom ? MarkBand.High : mark >= MidFrom ? MarkBand.Mid : MarkBand.Low;

    public IEnumerable<string> Validate()
    {
        if (MidFrom <= 0 || HighFrom <= MidFrom)
        {
            yield return "As faixas da Marca precisam ser 0 < média < alta.";
        }

        if (FacedToReturnFace <= 0)
        {
            yield return "O mínimo de Pesadas encaradas para devolver o rosto precisa ser positivo.";
        }

        if (FacedForHalfFace < 0 || (FacedForHalfFace > 0 && FacedForHalfFace >= FacedToReturnFace))
        {
            yield return "O meio rosto precisa de menos Pesadas encaradas que o rosto devolvido.";
        }

        if (PerjuryMark < 0)
        {
            yield return "A Marca do perjúrio não pode ser negativa.";
        }
    }
}
