using Lete.Core.Combat;
using Lete.Core.Combat.Gestures;
using Lete.Core.Data;

namespace Lete.Core.Tests;

public class GuardianHeadsTests
{
    private static readonly GradeValues Damage = new(0f, 5f, 10f);
    private static readonly string Root = RepoRoot();

    private static readonly IReadOnlyList<ShadeAttack> Attacks = new[]
    {
        new ShadeAttack(ShadeAttackKind.Pattern, "tres", "todas", Damage),
        new ShadeAttack(ShadeAttackKind.Direct, "a", "esquerda", Damage, Head: 1),
        new ShadeAttack(ShadeAttackKind.Direct, "a", "direita", Damage, Head: 2),
        new ShadeAttack(ShadeAttackKind.Direct, "b", "meio", Damage, Head: 3),
    };

    private static HeadsRules Rules(params string[] art) => new(3, "mel", "calma", "ultima", "todas", art);

    [Fact]
    public void Calming_RemovesOneHeadAtATime_FromTheFirst()
    {
        var heads = new GuardianHeads(Rules(), Attacks);
        Assert.Equal(new[] { "todas", "esquerda", "direita", "meio" }, heads.Cycle.Select(a => a.CaptionKey));
        Assert.Equal(1, heads.Phase);

        Assert.True(heads.Calm());
        Assert.Equal(new[] { "todas", "direita", "meio" }, heads.Cycle.Select(a => a.CaptionKey));
        Assert.Equal(2, heads.Phase);
        Assert.False(heads.IsLast);

        Assert.True(heads.Calm());
        Assert.True(heads.IsLast);
        Assert.Equal(new[] { "todas", "meio" }, heads.Cycle.Select(a => a.CaptionKey));

        Assert.True(heads.Calm());
        Assert.True(heads.AllCalm);
        Assert.Empty(heads.Cycle);
        Assert.False(heads.Calm());
    }

    [Fact]
    public void Art_FollowsTheHeadsAwake()
    {
        var heads = new GuardianHeads(Rules("f1", "f2", "f3"), Attacks);
        Assert.Equal("f1", heads.Art);
        heads.Calm();
        heads.Calm();
        Assert.Equal("f3", heads.Art);
        Assert.Null(new GuardianHeads(Rules(), Attacks).Art);
    }

    [Fact]
    public void Validate_NeedsAnAttackForEveryNumberOfHeadsAwake()
    {
        Assert.Empty(Rules().Validate(Attacks));
        // Sem o padrão de todas, com só a última acordada sobra a mordida dela: ainda vale.
        Assert.Empty(Rules().Validate(Attacks.Skip(1).ToList()));
        // Sem golpe da última cabeça nem padrão de todas, a última fica sem golpes.
        Assert.NotEmpty(Rules().Validate(Attacks.Skip(1).Take(2).ToList()));
        Assert.NotEmpty(Rules().Validate(new[] { Attacks[0] with { Head = 4 } }));
        Assert.NotEmpty(Rules("f1").Validate(Attacks));
    }

    [Fact]
    public void CalmGuardian_EndsTheFightAsFreed()
    {
        var flow = new BattleFlow(new Gauge(10f), new Gauge(10f), new ShadeResolve(100f), Attacks, support: null);
        flow.CalmGuardian();
        flow.EndPlayerAction();
        Assert.Equal(BattlePhase.Victory, flow.Phase);
        Assert.Equal(BattleOutcome.Freed, flow.Outcome);
    }

    [Fact]
    public void SidesPattern_BitesFromEachHeadAwake_InTurn()
    {
        var box = new DodgeSettings(5f, 104f, 64f, 60f, 2f, 3f, 0.6f, 4);
        var three = new SidesPattern(box, SpawnInterval: 1f, Speed: 40f, Spacing: 12f, Sides: 3);
        Assert.Equal(new[] { DodgeSide.Left, DodgeSide.Right, DodgeSide.Top, DodgeSide.Left }, Enumerable.Range(0, 4).Select(three.SideOf));

        // A da esquerda se acalma primeiro e a do meio (de cima) fica por último.
        Assert.Equal(new[] { DodgeSide.Right, DodgeSide.Top }, (three with { Sides = 2 }).ActiveSides);
        Assert.Equal(new[] { DodgeSide.Top }, (three with { Sides = 1 }).ActiveSides);

        // A primeira mordida (esquerda) entra pela esquerda, com um vão.
        var first = three.BulletsAt(0.5f).ToList();
        Assert.All(first, b => Assert.Equal(17f, b.X, 3));
        Assert.Equal((int)(64f / 12f) - 1, first.Count);

        // Só a de cima: entra por cima.
        Assert.All((three with { Sides = 1 }).BulletsAt(0.5f), b => Assert.Equal(17f, b.Y, 3));
    }

    [Fact]
    public void Data_LoadsCerberus()
    {
        var json = File.ReadAllText(Path.Combine(Root, "content", "shades", "cerbero.json"));
        var rules = ShadeData.HeadsFromJson(json);
        Assert.NotNull(rules);
        Assert.Equal(3, rules!.Count);
        Assert.Equal("mel", rules.CalmOffering);
        Assert.True(ShadeData.NameKnownFromJson(json));
        Assert.Null(ShadeData.HeadsFromJson(File.ReadAllText(Path.Combine(Root, "content", "shades", "remador.json"))));

        var (_, pattern) = GestureData.DodgeFromJson(
            File.ReadAllText(Path.Combine(Root, "content", "gestures", "esquiva_tres_cabecas.json")));
        Assert.Equal(3, Assert.IsType<SidesPattern>(pattern).Sides);
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "project.godot")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new DirectoryNotFoundException("project.godot não encontrado.");
    }
}
