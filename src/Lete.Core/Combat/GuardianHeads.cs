namespace Lete.Core.Combat;

/// <summary>
/// Regra de um guardião de várias cabeças (Cérbero, HISTORIA 6; campo <c>heads</c> da sombra): cada cabeça tem os
/// golpes dela no ciclo (<see cref="ShadeAttack.Head"/>, de 1 a <see cref="Count"/>); os golpes sem cabeça são de todas
/// (o padrão das três de uma vez, com tantos lados quantas estão acordadas). A oferenda <see cref="CalmOffering"/> acalma
/// uma cabeça por vez, da primeira à última: a luta muda de fase a cada uma e, a última acalmada, termina (a fera se
/// acalma: conta como libertar). Com uma só acordada, ela só aceita oferenda ou o nome (Libertar).
/// </summary>
/// <param name="CalmedKey">A legenda quando uma cabeça se acalma (e ainda restam outras).</param>
/// <param name="LastKey">A legenda quando só resta uma (a última só aceita oferenda ou o nome).</param>
/// <param name="AllCalmKey">A legenda quando a última se acalma.</param>
/// <param name="ArtByAwake">Arte de batalha para cada número de cabeças acordadas (do primeiro ao último: todas, uma a
/// menos...); vazio = a mesma arte sempre.</param>
public sealed record HeadsRules(
    int Count, string CalmOffering, string CalmedKey, string LastKey, string AllCalmKey, IReadOnlyList<string> ArtByAwake)
{
    public IEnumerable<string> Validate(IReadOnlyList<ShadeAttack> attacks)
    {
        if (Count < 2)
        {
            yield return "Um guardião de cabeças precisa de pelo menos 2.";
        }

        if (attacks.Any(a => a.Head is { } h && (h < 1 || h > Count)))
        {
            yield return $"Golpe de uma cabeça que não existe (as cabeças vão de 1 a {Count}).";
        }

        for (var awake = 1; awake <= Count; awake++)
        {
            if (!GuardianHeads.CycleFor(attacks, Count, awake).Any())
            {
                yield return $"Com {awake} cabeça(s) acordada(s), o ciclo fica sem golpes.";
            }
        }

        if (ArtByAwake.Count is not 0 && ArtByAwake.Count != Count)
        {
            yield return "A arte por cabeças acordadas precisa de uma imagem para cada número de cabeças (ou nenhuma).";
        }
    }
}

/// <summary>As cabeças acordadas de um guardião numa luta (começa com todas).</summary>
public sealed class GuardianHeads
{
    private readonly IReadOnlyList<ShadeAttack> _attacks;

    public GuardianHeads(HeadsRules rules, IReadOnlyList<ShadeAttack> attacks)
    {
        Rules = rules;
        _attacks = attacks;
        Awake = rules.Count;
    }

    public HeadsRules Rules { get; }

    /// <summary>Quantas cabeças ainda estão acordadas (as últimas da lista: a primeira é a primeira a se acalmar).</summary>
    public int Awake { get; private set; }

    /// <summary>Só uma acordada: ela só aceita oferenda ou o nome.</summary>
    public bool IsLast => Awake == 1;

    public bool AllCalm => Awake == 0;

    /// <summary>A fase da luta pelas cabeças: 1 com todas acordadas, +1 a cada uma acalmada.</summary>
    public int Phase => Rules.Count - Awake + 1;

    /// <summary>Os golpes das cabeças acordadas, na ordem do ciclo.</summary>
    public IReadOnlyList<ShadeAttack> Cycle => CycleFor(_attacks, Rules.Count, Awake).ToList();

    /// <summary>A arte de batalha agora (nula se o guardião não troca de arte).</summary>
    public string? Art => Rules.ArtByAwake.Count == 0 || AllCalm ? null : Rules.ArtByAwake[Rules.Count - Awake];

    /// <summary>Acalma a próxima cabeça; falso se já estavam todas calmas.</summary>
    public bool Calm()
    {
        if (AllCalm)
        {
            return false;
        }

        Awake--;
        return true;
    }

    internal static IEnumerable<ShadeAttack> CycleFor(IReadOnlyList<ShadeAttack> attacks, int count, int awake) =>
        awake <= 0 ? Enumerable.Empty<ShadeAttack>() : attacks.Where(a => a.Head is not { } h || h > count - awake);
}
