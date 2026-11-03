using Lete.Core.Conditions;
using Lete.Core.Data;
using Lete.Core.Dialogue;
using Lete.Core.Events;
using Lete.Core.Recollections;
using Lete.Core.State;
using Lete.Core.Systems;

namespace Lete.Core.Tests;

/// <summary>Missões (GDD 5.3): gancho, etapas, consequências, fechar em combate, a condição quest_stage e o arquivo.</summary>
public class MissionTests
{
    private readonly WorldState _state = new();
    private readonly EventBus _bus = new();
    private readonly List<object> _events = new();
    private readonly List<DialogueEffect> _applied = new();
    private readonly Missions _missions;
    private readonly Journal _journal;

    private sealed class Effects : IDialogueEffects
    {
        private readonly MissionTests _owner;

        public Effects(MissionTests owner) => _owner = owner;

        public void Apply(DialogueEffect effect)
        {
            _owner._applied.Add(effect);
            if (effect is SetFlag flag)
            {
                _owner._state.Flags.Add(flag.Flag);
            }
        }
    }

    public MissionTests()
    {
        _bus.AnyPublished += e => _events.Add(e);
        _journal = new Journal(_state, _bus);
        var facts = new WorldFacts(_state, new MarkRules(4, 8, 4),
            new Mosaic(_state, _bus, new Dictionary<string, Recollection>(), MosaicTests.Rules), new Profile(_state, _bus, ProfileTests.Rules));
        _missions = new Missions(_state, _bus, new Dictionary<string, Mission>
        {
            ["insepultos"] = new("insepultos", MissionType.Region, null, new FlagSet("ouviu"), "diario.gancho",
                new[] { new MissionStage(new FlagSet("ajudou"), "diario.ajudou") }, new DialogueEffect[] { new SetFlag("insepultos_em_paz") },
                ClosedByFight: false),
            ["padeiro"] = new("padeiro", MissionType.Offering, "padeiro", new FlagSet("pediu"), null,
                new[] { new MissionStage(new FlagSet("levou"), null), new MissionStage(new FlagSet("voltou"), null) },
                Array.Empty<DialogueEffect>(), ClosedByFight: true),
            // Começa quando outra missão termina: um passo pode destravar o seguinte na mesma conferência.
            ["depois"] = new("depois", MissionType.Region, null, new FlagSet("insepultos_em_paz"), null,
                new[] { new MissionStage(new QuestStageMin("insepultos", 1), "diario.depois") }, Array.Empty<DialogueEffect>(), false),
        }, () => facts, new Effects(this));
    }

    [Fact]
    public void AMission_StartsByItsHook_AdvancesAndAppliesTheConsequences()
    {
        _missions.Refresh();
        Assert.Null(_missions.StageOf("insepultos"));

        _state.Flags.Add("ouviu");
        _bus.Publish(new FlagChangedForTest());
        Assert.Equal(0, _missions.StageOf("insepultos"));
        Assert.Contains("diario.gancho", _journal.Entries);

        _state.Flags.Add("ajudou");
        _missions.Refresh();
        Assert.Equal(1, _missions.StageOf("insepultos"));
        Assert.True(_missions.IsCompleted("insepultos"));
        Assert.Contains("diario.ajudou", _journal.Entries);
        Assert.Equal(new DialogueEffect[] { new SetFlag("insepultos_em_paz") }, _applied);
        Assert.Contains(new MissionCompleted("insepultos"), _events);

        // A consequência abriu o gancho da outra, que já cumpre a etapa (quest_stage) na mesma conferência.
        Assert.True(_missions.IsCompleted("depois"));
    }

    [Fact]
    public void DestroyingTheShade_ClosesTheMission_AndBeforeTheHookItNeverStarts()
    {
        _state.Flags.Add("pediu");
        _missions.Refresh();
        _state.Flags.Add("levou");
        _missions.Refresh();
        Assert.Equal(1, _missions.StageOf("padeiro"));

        _bus.Publish(new ShadeDestroyed("padeiro"));
        Assert.True(_missions.IsClosed("padeiro"));
        Assert.Contains(new MissionClosed("padeiro"), _events);
        _state.Flags.Add("voltou");
        _missions.Refresh();
        Assert.Equal(1, _missions.StageOf("padeiro"));

        var fresh = new WorldState();
        fresh.Shades["padeiro"] = ShadeState.Destroyed;
        fresh.Flags.Add("pediu");
        var facts = new WorldFacts(fresh, new MarkRules(4, 8, 4),
            new Mosaic(fresh, _bus, new Dictionary<string, Recollection>(), MosaicTests.Rules), new Profile(fresh, _bus, ProfileTests.Rules));
        using var other = new Missions(fresh, new EventBus(), _missions.Definitions, () => facts, new Effects(this));
        other.Refresh();
        Assert.Null(other.StageOf("padeiro"));
    }

    [Fact]
    public void FromJson_ReadsTheDadosFields_AndRejectsBrokenOnes()
    {
        const string json = """
            {
              "id": "insepultos_do_aqueronte", "type": "region",
              "hook": { "any": [ { "flag": "ouviu_dos_insepultos" }, { "flag": "falou_com_insepultos" } ] },
              "hook_journal_key": "g",
              "stages": [ { "when": { "flag": "ajudou_insepultos" }, "journal_key": "e" } ],
              "consequences": [ { "journal_key": "c" } ]
            }
            """;
        var mission = MissionData.FromJson(json);
        Assert.Equal(MissionType.Region, mission.Type);
        Assert.Single(mission.Stages);
        Assert.Equal(new DialogueEffect[] { new AddJournalEntry("c") }, mission.Consequences);
        Assert.False(mission.ClosedByFight);

        Assert.Throws<FormatException>(() => MissionData.FromJson(json.Replace("\"region\"", "\"quest\"")));
        Assert.Throws<FormatException>(() => MissionData.FromJson(json.Replace("\"type\": \"region\",", "\"type\": \"region\", \"closed_by_fight\": true,")));
        Assert.Throws<FormatException>(() => MissionData.FromJson("""{ "id": "a", "type": "region", "hook": { "flag": "x" }, "stages": [] }"""));
    }

    /// <summary>Qualquer evento faz as missões conferirem (como no jogo, depois de uma conversa ligar uma flag).</summary>
    private sealed record FlagChangedForTest;
}
