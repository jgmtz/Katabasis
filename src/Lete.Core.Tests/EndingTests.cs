using Lete.Core.State;
using Lete.Core.Systems;

namespace Lete.Core.Tests;

public class EndingTests
{
    private static EndingFacts Facts(DebtAnswer? debt = DebtAnswer.Free, Gate? gate = Gate.Horn, ExitKind? stayed = null,
        MarkBand mark = MarkBand.Low, FinalFace face = FinalFace.Returned, string? companion = "aulo", bool oathBroken = false) =>
        new(debt, gate, stayed, mark, face, companion, oathBroken);

    [Theory]
    [InlineData(DebtAnswer.Free, ExitKind.Full)]
    [InlineData(DebtAnswer.Fight, ExitKind.Gaps)]
    [InlineData(DebtAnswer.Absorb, ExitKind.Full)]
    [InlineData(DebtAnswer.Exchange, ExitKind.Exchanged)]
    public void Exit_FollowsTheDebt_ThroughTheHornGate(DebtAnswer debt, ExitKind exit) =>
        Assert.Equal(exit, Endings.ExitFor(Facts(debt)));

    [Fact]
    public void Absorb_WithHighMark_PassesUnrecognized() =>
        Assert.Equal(ExitKind.Unbelonging, Endings.ExitFor(Facts(DebtAnswer.Absorb, mark: MarkBand.High)));

    [Fact]
    public void IvoryGate_Traps_UnlessHeExchanged()
    {
        Assert.Equal(ExitKind.Trapped, Endings.ExitFor(Facts(DebtAnswer.Free, Gate.Ivory)));
        Assert.Equal(ExitKind.Exchanged, Endings.ExitFor(Facts(DebtAnswer.Exchange, Gate.Ivory)));
    }

    [Fact]
    public void Staying_WinsOverEverything()
    {
        Assert.Equal(ExitKind.StayElysium, Endings.ExitFor(Facts(null, null, ExitKind.StayElysium)));
        Assert.Equal(ExitKind.StayLethe, Endings.ExitFor(Facts(DebtAnswer.Fight, Gate.Ivory, ExitKind.StayLethe)));
        Assert.Throws<ArgumentException>(() => Endings.ExitFor(Facts(stayed: ExitKind.Full)));
    }

    [Fact]
    public void Cards_TellDebtExitFaceCompanionTabletAndClose_InOrder()
    {
        var ending = Endings.Compose(Facts());
        Assert.Equal(new[]
        {
            "final.divida.free", "final.saida.full", "final.rosto.returned", "final.companheiro.aulo.esquece",
            "final.tabuinha.nome", "final.diario.full", "final.fecho",
        }, ending.CardKeys);
    }

    [Fact]
    public void Cards_SkipTheDebtWhenHeStayed_AndTheCompanionRemembers()
    {
        var ending = Endings.Compose(Facts(null, null, ExitKind.StayElysium, face: FinalFace.Covered, oathBroken: true));
        Assert.Equal(new[]
        {
            "final.saida.stay_elysium", "final.rosto.covered", "final.companheiro.aulo.lembra", "final.tabuinha.raspada",
            "final.diario.stay_elysium", "final.juramento.quebrado", "final.fecho",
        }, ending.CardKeys);
    }

    [Fact]
    public void Cards_ThePastComesAfterTheCompanion()
    {
        var ending = Endings.Compose(Facts() with { PastCards = new[] { "final.acerto_pescador_rito" } });
        Assert.Equal("final.acerto_pescador_rito", ending.CardKeys[4]);
        Assert.Equal("final.tabuinha.nome", ending.CardKeys[5]);
    }

    [Fact]
    public void Rules_ReadThePastCards_InOrder()
    {
        var rules = EndingRules.FromJson("""
            { "debt_shade": "s", "exchange_flag": "t", "horn_flag": "h", "ivory_flag": "i", "stay_elysium_flag": "e",
              "stay_lethe_flag": "l", "past_cards": [ { "flag": "a", "card": "final.a" }, { "flag": "b", "card": "final.b" } ] }
            """);
        Assert.Equal(new[] { ("a", "final.a"), ("b", "final.b") }, rules.PastCards);
        Assert.Throws<FormatException>(() => EndingRules.FromJson("""{ "debt_shade": "s" }"""));
    }

    [Fact]
    public void Cards_WithoutCompanionOrFace()
    {
        var ending = Endings.Compose(Facts(DebtAnswer.Fight, face: FinalFace.None, companion: null));
        Assert.Equal(new[] { "final.divida.fight", "final.saida.gaps", "final.companheiro.nenhum", "final.tabuinha.raspada", "final.diario.gaps", "final.fecho" },
            ending.CardKeys);
    }
}
