using Lete.Core.Conditions;
using Lete.Core.Dialogue;

namespace Lete.Core.Systems;

public enum MissionType
{
    Region,
    Offering,
    Origin,
    Companion,
}

/// <summary>Uma etapa: avança quando <see cref="When"/> vale; a frase vai para o Diário (sem lista de objetivos).</summary>
public sealed record MissionStage(Condition When, string? JournalKey);

/// <summary>
/// Uma missão (<c>content/missions/&lt;id&gt;.json</c>, GDD 5.3 e DADOS 8): aparece quando o gancho vale, avança etapa a
/// etapa pelas condições e, cumprida, aplica as consequências (os mesmos efeitos dos diálogos). Se
/// <see cref="ClosedByFight"/>, destruir a sombra <see cref="ShadeId"/> a fecha.
/// </summary>
public sealed record Mission(
    string Id,
    MissionType Type,
    string? ShadeId,
    Condition Hook,
    string? HookJournalKey,
    IReadOnlyList<MissionStage> Stages,
    IReadOnlyList<DialogueEffect> Consequences,
    bool ClosedByFight);
