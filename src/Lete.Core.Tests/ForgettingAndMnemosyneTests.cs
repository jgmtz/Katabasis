using Lete.Core.Combat;
using Lete.Core.Combat.Gestures;
using Lete.Core.Dialogue;
using Lete.Core.Events;
using Lete.Core.Recollections;
using Lete.Core.State;
using System.Text.Json;

namespace Lete.Core.Tests;

/// <summary>Esquecer até a Trívia, a fonte de Mnemósine e os efeitos de conversa do Ato III (decisões do João, 10/nov/2026).</summary>
public class ForgettingAndMnemosyneTests
{
    private static MemorySkill Skill(string id) => MemoryTests.Skill(id);

    [Fact]
    public void MemoryBook_StartsWithWhatWasForgottenBefore_UntilRest()
    {
        var book = new MemoryBook(new[] { Skill("colo"), Skill("mesa") }, forgotten: new[] { "mesa", "nao_existe" });

        Assert.True(book.IsForgotten("mesa"));
        Assert.False(book.CanUse("mesa", new Gauge(50f)));
        Assert.Equal(new[] { "mesa" }, book.Forgotten);

        book.Rest();
        Assert.False(book.IsForgotten("mesa"));
    }

    [Fact]
    public void ForgetNow_ForgetsWithoutTheLightness_OnlyKnownSkills()
    {
        var book = new MemoryBook(new[] { Skill("colo"), Skill("mesa") }, protections: 2);

        Assert.True(book.ForgetNow("mesa"));
        Assert.False(book.ForgetNow("mesa"));
        Assert.False(book.ForgetNow("nao_existe"));
        Assert.True(book.IsForgotten("mesa"));
        Assert.Equal(2, book.ProtectionsLeft);
    }

    [Fact]
    public void ForgettingOutsideBattle_TakesTheHighestStage_NeverTheGuilt()
    {
        var memories = new[]
        {
            new CombatMemory(Skill("colo"), "colo_da_avo", RecollectionStage.Fragment),
            new CombatMemory(Skill("mesa"), "mesa_cheia", RecollectionStage.Full),
            new CombatMemory(Skill("barro"), "maos_no_barro", RecollectionStage.Full),
            new CombatMemory(Skill("culpa"), "o_convite", RecollectionStage.Full, Guilt: true),
        };

        Assert.Equal("mesa", MemoryForgetting.PickOutsideBattle(memories, new HashSet<string>()));
        Assert.Equal("barro", MemoryForgetting.PickOutsideBattle(memories, new HashSet<string> { "mesa" }));
        Assert.Null(MemoryForgetting.PickOutsideBattle(memories, new HashSet<string> { "mesa", "barro", "colo" }));
    }

    [Fact]
    public void Mnemosyne_ConfirmsTheUncertain_AndGivesTheNew()
    {
        var state = new WorldState();
        var bus = new EventBus();
        RecollectionSource Source(string id, SourceType type, bool reliable = true) => new(id, type, null, null, reliable, false);
        Recollection Make(string id, params RecollectionSource[] sources) =>
            new(id, RecollectionKind.Mourning, RecollectionScope.Common, null, $"rec.{id}", new Dictionary<RecollectionStage, string>(),
                $"mem_{id}", sources, new[] { "ato" }, 0, 0);
        var mosaic = new Mosaic(state, bus, new[]
        {
            Make("incerta", Source("sonho", SourceType.Dream, reliable: false), Source("mnemosine", SourceType.Mnemosyne)),
            Make("nova", Source("mnemosine", SourceType.Mnemosyne), Source("outra", SourceType.Dialogue)),
            Make("alheia", Source("a", SourceType.Anchor), Source("b", SourceType.Dialogue)),
            Make("nao_tem", Source("a", SourceType.Anchor), Source("mnemosine", SourceType.Mnemosyne)),
        }.ToDictionary(r => r.Id), MosaicTests.Rules);
        mosaic.Hear("incerta", "sonho");
        Assert.Equal(RecollectionCertainty.Uncertain, mosaic.CertaintyOf("incerta"));

        Assert.True(mosaic.HearMnemosyne());

        Assert.Equal(RecollectionCertainty.Certain, mosaic.CertaintyOf("incerta"));
        Assert.Equal(RecollectionStage.Recalled, mosaic.StageOf("incerta"));
        Assert.Equal(RecollectionStage.Fragment, mosaic.StageOf("nova"));
        Assert.Null(mosaic.StageOf("alheia"));
        // Mnemósine confirma o que se tem; não dá uma Lembrança que nasce em outro lugar.
        Assert.Null(mosaic.StageOf("nao_tem"));
        Assert.False(mosaic.HearMnemosyne());
    }

    [Theory]
    [InlineData("""{ "raise_mark": 3 }""", typeof(RaiseMark))]
    [InlineData("""{ "acquire_item": "roma" }""", typeof(AcquireItem))]
    [InlineData("""{ "drink_mnemosyne": true }""", typeof(DrinkMnemosyne))]
    [InlineData("""{ "forget_memory": true }""", typeof(ForgetMemory))]
    [InlineData("""{ "end_game": true }""", typeof(EndGame))]
    public void DialogueEffects_OfActThree_Parse(string json, Type expected)
    {
        using var doc = JsonDocument.Parse(json);
        Assert.IsType(expected, DialogueData.EffectFromJson(doc.RootElement, "teste"));
    }

    [Theory]
    [InlineData("""{ "raise_mark": 0 }""")]
    [InlineData("""{ "drink_mnemosyne": false }""")]
    [InlineData("""{ "end_game": false }""")]
    public void DialogueEffects_OfActThree_RejectBadValues(string json)
    {
        using var doc = JsonDocument.Parse(json);
        Assert.Throws<FormatException>(() => DialogueData.EffectFromJson(doc.RootElement, "teste"));
    }
}
