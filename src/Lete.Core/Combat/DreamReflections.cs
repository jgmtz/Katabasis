namespace Lete.Core.Combat;

/// <summary>
/// Números e textos da regra do Sonho (campo <c>reflections</c> da sombra): quantos reflexos, quanto o Sonho se refaz e
/// quanta Lucidez custa acertar um falso, os títulos das lembranças inventadas, a frase de cada reflexo
/// (<see cref="DetailKey"/>, com o título no lugar de <c>{0}</c>) e a do reflexo vazio (sem Lembrança plena).
/// </summary>
public sealed record DreamRules(
    int Count, float FalseHealFraction, float FalseLucidityCost, IReadOnlyList<string> FalseTitleKeys, string DetailKey,
    string EmptyKey)
{
    public IEnumerable<string> Validate()
    {
        if (Count < 2)
        {
            yield return "reflections.count: pelo menos 2 reflexos.";
        }

        if (FalseTitleKeys.Count < Count - 1)
        {
            yield return "reflections.false_titles: precisa de um título inventado para cada reflexo falso.";
        }

        if (FalseHealFraction is < 0f or > 1f || FalseLucidityCost < 0f)
        {
            yield return "reflections: false_heal entre 0 e 1 e false_lucidity positivo.";
        }
    }
}

/// <summary>Um reflexo: o título que ele segura (nulo no reflexo vazio) e se é o verdadeiro.</summary>
public sealed record DreamReflection(string? TitleKey, bool IsTrue);

/// <summary>O que aconteceu ao golpear um reflexo: o verdadeiro leva o Sonho à fase seguinte; o falso o refaz e custa Lucidez.</summary>
public sealed record DreamStrike(bool HitTrue, float Healed, float LucidityCost);

/// <summary>
/// O Sonho, chefe da morada de Hipno e Tânato: distinguir o reflexo verdadeiro (decisão do João, 10/nov/2026). A cada
/// arranjo surgem reflexos do protagonista; só o verdadeiro segura uma Lembrança plena do Mosaico. Os falsos seguram
/// lembranças inventadas e, se houver, uma Lembrança incerta (que parece verdadeira e confunde a pista). Golpear o
/// verdadeiro avança a fase (na última, desfaz o Sonho); golpear um falso refaz o Sonho e gasta Lucidez.
/// </summary>
public sealed class DreamReflections
{
    private readonly DreamRules _rules;
    private List<DreamReflection> _current = new();

    public DreamReflections(DreamRules rules) => _rules = rules;

    public IReadOnlyList<DreamReflection> Current => _current;

    /// <summary>
    /// Novo arranjo. <paramref name="fullTitleKeys"/>: os títulos das Lembranças plenas (o verdadeiro segura uma delas;
    /// sem nenhuma, ele não segura nada). <paramref name="uncertainTitleKeys"/>: as incertas (um falso segura uma delas).
    /// </summary>
    public void Arrange(IReadOnlyList<string> fullTitleKeys, IReadOnlyList<string> uncertainTitleKeys, Random random)
    {
        var reflections = new List<DreamReflection>
        {
            new(fullTitleKeys.Count == 0 ? null : fullTitleKeys[random.Next(fullTitleKeys.Count)], IsTrue: true),
        };
        if (uncertainTitleKeys.Count > 0)
        {
            reflections.Add(new DreamReflection(uncertainTitleKeys[random.Next(uncertainTitleKeys.Count)], IsTrue: false));
        }

        foreach (var title in _rules.FalseTitleKeys.OrderBy(_ => random.Next()))
        {
            if (reflections.Count >= _rules.Count)
            {
                break;
            }

            reflections.Add(new DreamReflection(title, IsTrue: false));
        }

        _current = reflections.Take(_rules.Count).OrderBy(_ => random.Next()).ToList();
    }

    /// <summary>
    /// Golpeia o reflexo <paramref name="index"/>. O verdadeiro leva a Firmeza para logo abaixo do próximo limiar de
    /// fase (<paramref name="phaseThresholds"/>, as frações das fases) ou a zero, se não houver outro; o falso devolve
    /// <see cref="DreamRules.FalseHealFraction"/> da Firmeza máxima e gasta Lucidez.
    /// </summary>
    public DreamStrike Strike(int index, ShadeResolve resolve, Gauge lucidity, IEnumerable<float> phaseThresholds)
    {
        if (index < 0 || index >= _current.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Esse reflexo não existe.");
        }

        if (_current[index].IsTrue)
        {
            var next = phaseThresholds.Where(t => t < resolve.Fraction).DefaultIfEmpty(0f).Max();
            // A fase muda quando a fração fica abaixo do limiar (ShadeNarration.NewPhase).
            var target = next <= 0f ? 0f : next * resolve.Max * (1f - NudgeBelow);
            resolve.Reduce(Math.Max(0f, resolve.Current - target));
            return new DreamStrike(HitTrue: true, Healed: 0f, LucidityCost: 0f);
        }

        var before = resolve.Current;
        resolve.Restore(_rules.FalseHealFraction * resolve.Max);
        var cost = Math.Min(lucidity.Current, _rules.FalseLucidityCost);
        lucidity.Reduce(cost);
        return new DreamStrike(HitTrue: false, Healed: resolve.Current - before, LucidityCost: cost);
    }

    /// <summary>Quanto abaixo do limiar o golpe verdadeiro deixa a Firmeza (fração do próprio limiar).</summary>
    private const float NudgeBelow = 0.001f;
}
