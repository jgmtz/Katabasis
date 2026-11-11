using Lete.Core.State;

namespace Lete.Core.Systems;

/// <summary>
/// O que o jogador fez com a dívida, a via contra a Sombra do Esquecido (HISTORIA 3, pergunta 1). Combater rejeita o
/// passado; Libertar aceita (devolve o nome dela); Absorver carrega tudo; Trocar toma o lugar dela, como Alceste.
/// </summary>
public enum DebtAnswer
{
    Fight,
    Free,
    Absorb,
    Exchange,
}

/// <summary>Qual dos dois portões ele atravessou: o de chifre (sonhos verdadeiros) ou o de marfim (sonhos falsos).</summary>
public enum Gate
{
    Horn,
    Ivory,
}

/// <summary>Se e como ele sai (HISTORIA 3, pergunta 2).</summary>
public enum ExitKind
{
    /// <summary>Sai e carrega o peso; só ele sabe que esteve lá.</summary>
    Full,

    /// <summary>Sai sem saber o que perdeu (Combater: a Sombra levou as lembranças que carregava).</summary>
    Gaps,

    /// <summary>O portão não o reconhece: passa como sombra entre os vivos (Absorver com a Marca alta).</summary>
    Unbelonging,

    /// <summary>Preso pela ignorância: atravessou o portão de marfim e acredita que saiu.</summary>
    Trapped,

    /// <summary>Ficou nos Campos Elísios.</summary>
    StayElysium,

    /// <summary>Bebeu do Lete de propósito e ficou.</summary>
    StayLethe,

    /// <summary>Trocou de lugar com ela: ela sai, esquecida dele; ele fica.</summary>
    Exchanged,
}

/// <summary>Tudo o que decide o final, lido do estado do mundo ao chegar ao fim.</summary>
/// <param name="Stayed">Ficou: nos Elísios (<see cref="ExitKind.StayElysium"/>) ou no Lete (<see cref="ExitKind.StayLethe"/>); nulo se seguiu.</param>
/// <param name="Companion">Quem seguiu com ele (nulo: ninguém).</param>
/// <param name="OathBroken">Quebrou algum juramento (pesa como variável extra no epílogo).</param>
/// <param name="PastCards">O que ele fez do passado (o capítulo 4 das origens): cartões que entram depois do companheiro.</param>
public sealed record EndingFacts(
    DebtAnswer? Debt,
    Gate? Gate,
    ExitKind? Stayed,
    MarkBand Mark,
    FinalFace Face,
    string? Companion,
    bool OathBroken,
    IReadOnlyList<string>? PastCards = null);

/// <summary>O final composto: as duas respostas, o rosto e a sequência de cartões de texto do epílogo (chaves).</summary>
public sealed record Ending(DebtAnswer? Debt, ExitKind Exit, FinalFace Face, IReadOnlyList<string> CardKeys);

/// <summary>
/// Compõe o final (HISTORIA 3; regras em rascunho, 11/nov/2026, PENDENCIAS 98): a saída sai da dívida, do portão, da
/// escolha de ficar e da Marca; os cartões contam, em ordem, a dívida, a saída, o rosto, o companheiro, a Tabuinha, o
/// juramento quebrado (se houver) e o fecho.
/// </summary>
public static class Endings
{
    public static ExitKind ExitFor(EndingFacts facts)
    {
        if (facts.Stayed is { } stayed)
        {
            if (stayed is not (ExitKind.StayElysium or ExitKind.StayLethe))
            {
                throw new ArgumentException("Ficar é nos Elísios ou no Lete.", nameof(facts));
            }

            return stayed;
        }

        return facts.Debt switch
        {
            DebtAnswer.Exchange => ExitKind.Exchanged,
            _ when facts.Gate == Gate.Ivory => ExitKind.Trapped,
            DebtAnswer.Absorb when facts.Mark == MarkBand.High => ExitKind.Unbelonging,
            DebtAnswer.Fight => ExitKind.Gaps,
            _ => ExitKind.Full,
        };
    }

    /// <summary>O nome volta à Tabuinha só para quem o devolveu (Libertar) ou pagou por ele (Trocar).</summary>
    public static bool TabletShowsName(EndingFacts facts) => facts.Debt is DebtAnswer.Free or DebtAnswer.Exchange;

    /// <summary>O companheiro esquece o protagonista no portão; só ficar o mantém (HISTORIA 5).</summary>
    public static bool CompanionRemembers(ExitKind exit) => exit is ExitKind.StayElysium or ExitKind.StayLethe or ExitKind.Exchanged;

    public static Ending Compose(EndingFacts facts)
    {
        var exit = ExitFor(facts);
        var cards = new List<string>();
        if (facts.Debt is { } debt)
        {
            cards.Add($"final.divida.{Snake(debt)}");
        }

        cards.Add($"final.saida.{Snake(exit)}");
        if (facts.Face != FinalFace.None)
        {
            cards.Add($"final.rosto.{Snake(facts.Face)}");
        }

        cards.Add(facts.Companion is { } companion
            ? $"final.companheiro.{companion}.{(CompanionRemembers(exit) ? "lembra" : "esquece")}"
            : "final.companheiro.nenhum");
        cards.AddRange(facts.PastCards ?? Array.Empty<string>());
        cards.Add(TabletShowsName(facts) ? "final.tabuinha.nome" : "final.tabuinha.raspada");
        if (facts.OathBroken)
        {
            cards.Add("final.juramento.quebrado");
        }

        cards.Add("final.fecho");
        return new Ending(facts.Debt, exit, facts.Face, cards);
    }

    private static string Snake<T>(T value) where T : struct, Enum => Conditions.ConditionParser.SnakeCase(value);
}

/// <summary>
/// De onde o final lê o mundo (<c>content/rules/final.json</c>): a sombra da dívida e as marcas que as escolhas deixam.
/// </summary>
public sealed record EndingRules(
    string DebtShade,
    string ExchangeFlag,
    string HornFlag,
    string IvoryFlag,
    string StayElysiumFlag,
    string StayLetheFlag,
    IReadOnlyList<(string Flag, string Card)> PastCards)
{
    public static EndingRules FromJson(string json)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;
        string Text(string name) => root.TryGetProperty(name, out var v) && v.GetString() is { Length: > 0 } s
            ? s
            : throw new FormatException($"final: '{name}' é obrigatório.");
        var past = root.TryGetProperty("past_cards", out var list)
            ? list.EnumerateArray().Select(c => (c.GetProperty("flag").GetString() ?? "", c.GetProperty("card").GetString() ?? "")).ToList()
            : new List<(string, string)>();
        return new EndingRules(Text("debt_shade"), Text("exchange_flag"), Text("horn_flag"), Text("ivory_flag"),
            Text("stay_elysium_flag"), Text("stay_lethe_flag"), past);
    }

    /// <summary>A dívida pelo estado da sombra: trocou, libertou, absorveu ou destruiu (nulo: nenhuma das quatro).</summary>
    public DebtAnswer? DebtFrom(Conditions.IConditionFacts facts) =>
        facts.HasFlag(ExchangeFlag) ? DebtAnswer.Exchange
        : facts.ShadeIs(DebtShade, Conditions.ShadeState.Freed) ? DebtAnswer.Free
        : facts.ShadeIs(DebtShade, Conditions.ShadeState.Absorbed) ? DebtAnswer.Absorb
        : facts.ShadeIs(DebtShade, Conditions.ShadeState.Destroyed) ? DebtAnswer.Fight
        : null;

    /// <summary>Os cartões do passado cujas marcas estão no mundo, na ordem das regras.</summary>
    public IReadOnlyList<string> PastCardsFrom(Conditions.IConditionFacts facts) =>
        PastCards.Where(c => facts.HasFlag(c.Flag)).Select(c => c.Card).ToList();

    public Gate? GateFrom(Conditions.IConditionFacts facts) =>
        facts.HasFlag(HornFlag) ? Gate.Horn : facts.HasFlag(IvoryFlag) ? Gate.Ivory : null;

    public ExitKind? StayFrom(Conditions.IConditionFacts facts) =>
        facts.HasFlag(StayElysiumFlag) ? ExitKind.StayElysium : facts.HasFlag(StayLetheFlag) ? ExitKind.StayLethe : null;
}
