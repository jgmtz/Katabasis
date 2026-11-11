using Lete.Core.Combat;
using Lete.Core.World;

namespace Lete.Core.Tests;

public class RegionCombatRuleTests
{
    private static BattleFlow Flow(RegionCombatRule? rule, float breath = 100f, float resolve = 100f)
    {
        var flow = new BattleFlow(new Gauge(100f), new Gauge(50f), new ShadeResolve(100f),
            new[] { new ShadeAttack(ShadeAttackKind.Direct, "aparo", "golpe", new GradeValues(0f, 10f, 20f)) }, null) { RegionRule = rule };
        flow.Breath.Reduce(100f - breath);
        flow.Resolve.Reduce(100f - resolve);
        return flow;
    }

    private static void EndRound(BattleFlow flow)
    {
        flow.EndPlayerAction();
        flow.EndShadeTurn();
    }

    [Fact]
    public void Fire_HurtsBoth_AtTheEndOfEachRound()
    {
        var flow = Flow(new RegionCombatRule("fogo", RegionRuleKind.BurnAll, 0.05f, "regra.fogo"));
        EndRound(flow);
        Assert.Equal(95f, flow.Breath.Current);
        Assert.Equal(95f, flow.Resolve.Current);
        Assert.Equal(new RegionRuleTick(5f, 5f), flow.LastRegionTick);
    }

    [Fact]
    public void Fire_NeverDecidesTheFightAlone()
    {
        var flow = Flow(new RegionCombatRule("fogo", RegionRuleKind.BurnAll, 0.5f, "regra.fogo"), breath: 3f, resolve: 1f);
        EndRound(flow);
        Assert.Equal(1f, flow.Breath.Current);
        Assert.Equal(1f, flow.Resolve.Current);
        Assert.False(flow.IsOver);
    }

    [Fact]
    public void Peace_MendsAnyDamage_Slowly_ButNeverAboveTheMax()
    {
        var flow = Flow(new RegionCombatRule("paz", RegionRuleKind.MendAll, 0.04f, "regra.paz"), breath: 50f, resolve: 98f);
        EndRound(flow);
        Assert.Equal(54f, flow.Breath.Current);
        Assert.Equal(100f, flow.Resolve.Current);
    }

    [Fact]
    public void Tears_SlowEverything_AndAreAFloorForTheMourningMemory()
    {
        var flow = Flow(new RegionCombatRule("lagrimas", RegionRuleKind.SlowAll, 1.3f, "regra.lagrimas"));
        Assert.Equal(1.3f, flow.ShadeSlowFactor);
        flow.ApplyUntilShadeTurnEnds(MemoryEffectKind.Slow, 1.8f);
        Assert.Equal(1.8f, flow.ShadeSlowFactor);
        EndRound(flow);
        Assert.Equal(1.3f, flow.ShadeSlowFactor);
        Assert.False(flow.LastRegionTick.Changed);
    }

    [Fact]
    public void WithoutRule_NothingChanges()
    {
        var flow = Flow(null, breath: 50f);
        EndRound(flow);
        Assert.Equal(50f, flow.Breath.Current);
        Assert.Equal(1f, flow.ShadeSlowFactor);
    }

    [Fact]
    public void Region_ReadsTheRule_AndRejectsBadOnes()
    {
        var region = WorldData.RegionFromJson("""
            { "id": "t", "name_key": "n", "scene": "res://a.tscn", "entry": "E",
              "combat_rule": { "id": "fogo", "kind": "burn_all", "power": { "_tbd": true, "value": 0.04 }, "text_key": "regra.fogo" } }
            """);
        Assert.Equal(new RegionCombatRule("fogo", RegionRuleKind.BurnAll, 0.04f, "regra.fogo"), region.CombatRule);
        Assert.ThrowsAny<FormatException>(() => WorldData.RegionFromJson("""
            { "id": "t", "name_key": "n", "scene": "res://a.tscn", "entry": "E",
              "combat_rule": { "id": "x", "kind": "slow_all", "power": 0.5, "text_key": "k" } }
            """));
        Assert.ThrowsAny<FormatException>(() => WorldData.RegionFromJson("""
            { "id": "t", "name_key": "n", "scene": "res://a.tscn", "entry": "E",
              "combat_rule": { "id": "x", "kind": "chuva", "power": 0.5, "text_key": "k" } }
            """));
    }
}
