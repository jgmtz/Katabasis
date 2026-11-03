using Lete.Core.Conditions;
using Lete.Core.Dialogue;
using Lete.Core.Events;
using Lete.Core.State;

namespace Lete.Core.Systems;

/// <summary>
/// As missões (GDD 5.3): sem lista nem marcador. A cada evento, confere os ganchos das que não começaram e a etapa
/// seguinte das que estão em andamento; cada passo publica um evento com a frase do Diário. Cumprida a última etapa,
/// aplica as consequências. Destruir em combate a sombra de uma missão <c>closed_by_fight</c> a fecha (parte da
/// escolha). A etapa é o que a condição <c>quest_stage</c> lê.
/// </summary>
public sealed class Missions : IDisposable
{
    private readonly WorldState _state;
    private readonly EventBus _events;
    private readonly Func<IConditionFacts> _facts;
    private readonly IDialogueEffects _effects;
    private readonly IDisposable _destroyed;
    private bool _refreshing;

    public Missions(
        WorldState state, EventBus events, IReadOnlyDictionary<string, Mission> definitions, Func<IConditionFacts> facts,
        IDialogueEffects effects)
    {
        _state = state;
        _events = events;
        Definitions = definitions;
        _facts = facts;
        _effects = effects;
        _events.AnyPublished += OnAnyEvent;
        _destroyed = events.Subscribe<ShadeDestroyed>(e => CloseByFight(e.ShadeId));
    }

    public IReadOnlyDictionary<string, Mission> Definitions { get; }

    /// <summary>Etapas cumpridas (0 logo depois do gancho); nula se a missão não começou.</summary>
    public int? StageOf(string id) => _state.Missions.TryGetValue(id, out var mission) ? mission.Stage : null;

    public bool IsCompleted(string id) =>
        _state.Missions.TryGetValue(id, out var mission) && Definitions.TryGetValue(id, out var definition)
        && mission.Stage >= definition.Stages.Count;

    public bool IsClosed(string id) => _state.Missions.TryGetValue(id, out var mission) && mission.Closed;

    /// <summary>Confere ganchos e etapas (os eventos já chamam; chame depois de mudar o estado sem evento).</summary>
    public void Refresh()
    {
        if (_refreshing)
        {
            return;
        }

        _refreshing = true;
        try
        {
            // Um passo pode cumprir a condição do seguinte (ou o gancho de outra missão): repete até nada mudar.
            while (Definitions.Values.Any(Step))
            {
            }
        }
        finally
        {
            _refreshing = false;
        }
    }

    public void Dispose()
    {
        _events.AnyPublished -= OnAnyEvent;
        _destroyed.Dispose();
    }

    private void OnAnyEvent(object _) => Refresh();

    /// <summary>Dá um passo na missão, se puder (começar ou avançar uma etapa); verdadeiro se deu.</summary>
    private bool Step(Mission mission)
    {
        var facts = _facts();
        if (!_state.Missions.TryGetValue(mission.Id, out var state))
        {
            // Destruída antes do gancho, a sombra leva a missão junto: ela nunca começa.
            if (!ConditionEvaluator.Passes(mission.Hook, facts)
                || mission.ClosedByFight && mission.ShadeId is { } shade
                   && _state.Shades.TryGetValue(shade, out var outcome) && outcome == ShadeState.Destroyed)
            {
                return false;
            }

            _state.Missions[mission.Id] = new MissionState();
            _events.Publish(new MissionStarted(mission.Id, mission.HookJournalKey));
            CompleteIfDone(mission, 0);
            return true;
        }

        if (state.Closed || state.Stage >= mission.Stages.Count || !ConditionEvaluator.Passes(mission.Stages[state.Stage].When, facts))
        {
            return false;
        }

        state.Stage++;
        _events.Publish(new MissionAdvanced(mission.Id, state.Stage, mission.Stages[state.Stage - 1].JournalKey));
        CompleteIfDone(mission, state.Stage);
        return true;
    }

    private void CompleteIfDone(Mission mission, int stage)
    {
        if (stage < mission.Stages.Count)
        {
            return;
        }

        foreach (var effect in mission.Consequences)
        {
            _effects.Apply(effect);
        }

        _events.Publish(new MissionCompleted(mission.Id));
    }

    private void CloseByFight(string shadeId)
    {
        foreach (var mission in Definitions.Values.Where(m => m.ClosedByFight && m.ShadeId == shadeId))
        {
            if (_state.Missions.TryGetValue(mission.Id, out var state) && !state.Closed && state.Stage < mission.Stages.Count)
            {
                state.Closed = true;
                _events.Publish(new MissionClosed(mission.Id));
            }
        }
    }
}
