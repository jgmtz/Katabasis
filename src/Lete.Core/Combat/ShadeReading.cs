using Lete.Core.Conditions;

namespace Lete.Core.Combat;

/// <summary>
/// A leitura das Fúrias (decisão do João, 11/nov/2026: elas leem as Pesadas deixadas, os juramentos quebrados e os
/// perjúrios): cada Pesada deixada e cada juramento quebrado deixa os golpes da sombra mais fortes, até um teto.
/// </summary>
public sealed record ShadeReading(float PerHeavyLeft, float PerOathBroken, float MaxMultiplier, string? ReadKey)
{
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (PerHeavyLeft < 0f || PerOathBroken < 0f)
        {
            errors.Add("reading: os acréscimos não podem ser negativos.");
        }

        if (MaxMultiplier < 1f)
        {
            errors.Add("reading: max_multiplier precisa ser pelo menos 1.");
        }

        return errors;
    }

    /// <summary>Quanto os golpes da sombra pesam (1 é o normal).</summary>
    public float Multiplier(int heavyLeft, int oathsBroken) =>
        Math.Min(MaxMultiplier, 1f + PerHeavyLeft * Math.Max(0, heavyLeft) + PerOathBroken * Math.Max(0, oathsBroken));
}

/// <summary>
/// Trocar (HISTORIA 3: só contra a Sombra do Esquecido, e só para quem encarou A Venda e tem o nome): ele toma o lugar
/// dela, como Alceste. <see cref="Requires"/> abre a via; <see cref="Flag"/> fica no mundo; <see cref="TextKeys"/> é a
/// cena curta.
/// </summary>
public sealed record ShadeExchange(Condition Requires, string Flag, string DescriptionKey, string ClosedKey, IReadOnlyList<string> TextKeys);
