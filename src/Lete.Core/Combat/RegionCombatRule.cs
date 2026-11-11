namespace Lete.Core.Combat;

/// <summary>O que a regra de combate de uma região faz (HISTORIA 7.1; decisão do João de 11/nov/2026, PENDÊNCIA 101).</summary>
public enum RegionRuleKind
{
    /// <summary>Campos do Luto, as lágrimas: tudo fica mais lento (os golpes dela e os gestos de quem luta).</summary>
    SlowAll,

    /// <summary>Tártaro, o fogo do Flegetonte: ao fim de cada rodada, fere todos (você e a sombra).</summary>
    BurnAll,

    /// <summary>Campos Elísios: ao fim de cada rodada, qualquer dano cura devagar (de você e da sombra).</summary>
    MendAll,
}

/// <summary>Quanto a regra mudou ao fim de uma rodada (positivo: perdeu; negativo: curou).</summary>
public readonly record struct RegionRuleTick(float Breath, float Resolve)
{
    public bool Changed => Breath != 0f || Resolve != 0f;
}

/// <summary>
/// A regra de combate de uma região. <see cref="Power"/>: em <see cref="RegionRuleKind.SlowAll"/>, quantas vezes mais
/// lento (≥ 1); nas outras, a fração do máximo de cada barra por rodada (0 a 1). O fogo nunca derruba sozinho: deixa
/// pelo menos <see cref="Floor"/> em cada barra (quem decide a luta são as escolhas).
/// </summary>
public sealed record RegionCombatRule(string Id, RegionRuleKind Kind, float Power, string TextKey, float Floor = 1f)
{
    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(TextKey))
        {
            yield return "Regra de combate sem id ou texto.";
        }

        if (Kind == RegionRuleKind.SlowAll ? Power < 1f : Power is <= 0f or > 1f)
        {
            yield return $"Regra de combate {Id}: força {Power} fora da faixa de {Kind}.";
        }

        if (Floor < 0f)
        {
            yield return $"Regra de combate {Id}: piso {Floor} negativo.";
        }
    }

    /// <summary>Quantas vezes mais lentos ficam os gestos e golpes (1 = normal).</summary>
    public float SlowFactor => Kind == RegionRuleKind.SlowAll ? Power : 1f;

    /// <summary>Aplica a regra ao fim de uma rodada e diz quanto cada barra mudou.</summary>
    public RegionRuleTick EndRound(Gauge breath, ShadeResolve resolve)
    {
        switch (Kind)
        {
            case RegionRuleKind.BurnAll:
            {
                var lostBreath = Math.Clamp(breath.Max * Power, 0f, Math.Max(0f, breath.Current - Floor));
                var lostResolve = Math.Clamp(resolve.Max * Power, 0f, Math.Max(0f, resolve.Current - Floor));
                breath.Reduce(lostBreath);
                resolve.Reduce(lostResolve);
                return new RegionRuleTick(lostBreath, lostResolve);
            }
            case RegionRuleKind.MendAll:
            {
                var healBreath = Math.Min(breath.Max * Power, breath.Max - breath.Current);
                var healResolve = resolve.IsBroken ? 0f : Math.Min(resolve.Max * Power, resolve.Max - resolve.Current);
                breath.Restore(healBreath);
                resolve.Restore(healResolve);
                return new RegionRuleTick(-healBreath, -healResolve);
            }
            default:
                return default;
        }
    }
}
