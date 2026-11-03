using System.Text.Json;
using Lete.Core.Combat;

namespace Lete.Core.Data;

/// <summary>Carrega a escuta e a libertação de uma sombra (<c>content/shades/&lt;id&gt;.json</c>).</summary>
public static class ShadeData
{
    /// <summary>
    /// Campo <c>name_known</c>: o nome já se sabe quando a luta começa (sombras comuns: o nome é o do ofício, "a
    /// lavadeira"; decisão do João, 03/nov/2026). Sem o campo, o nome se descobre escutando.
    /// </summary>
    public static bool NameKnownFromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return NameKnown(doc.RootElement);
    }

    /// <summary>
    /// <c>kind</c> = <c>personification</c> (GDD 4.3; o Medo, a Fome do Vestíbulo): não tem nome de pessoa, então Libertar
    /// não funciona nela, e Presenciar vira Suportar, aguentar até ela passar (decisão do João, 03/nov/2026).
    /// </summary>
    public static bool PersonificationFromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("kind", out var kind) && kind.GetString() == "personification";
    }

    /// <summary>
    /// Campo <c>name_scattered</c> {<c>requires</c> (condição: os pedaços achados), <c>missing_key</c> (o que a escuta ouve
    /// antes)}: o nome dela está espalhado pela região (a Sem-Nome). Nulo se o nome se ouve normalmente.
    /// </summary>
    public static (Conditions.Condition Requires, string MissingKey)? NameScatteredFromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("name_scattered", out var scattered)
            ? (Conditions.ConditionParser.Parse(Required(scattered, "requires"), "name_scattered/requires"), String(scattered, "missing_key"))
            : null;
    }

    /// <summary>Campo <c>honor</c> {<c>forbidden</c>, <c>dishonor_key</c>, <c>blessing_flag</c>, <c>blessing_key</c>}: o duelo de honra; nulo sem ele.</summary>
    public static DuelHonor? HonorFromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("honor", out var honor)
            ? new DuelHonor(
                Required(honor, "forbidden").EnumerateArray().Select(f => f.GetString() ?? "").ToList(),
                String(honor, "dishonor_key"), String(honor, "blessing_flag"), String(honor, "blessing_key"))
            : null;
    }

    private static bool NameKnown(JsonElement root) =>
        root.TryGetProperty("name_known", out var known) && known.ValueKind == JsonValueKind.True;

    /// <summary>
    /// Campos <c>listen</c> (o que ela murmura em cada golpe do ciclo) e <c>murmur_key</c>. Uma escuta revela a fixação
    /// e uma o nome (nenhuma, se o nome já se sabe).
    /// </summary>
    public static (IReadOnlyList<ListenEntry> Entries, string MurmurKey) ListeningFromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var cycleLength = Required(root, "attacks").GetArrayLength();

        var entries = Required(root, "listen").EnumerateArray()
            .Select(e => new ListenEntry(
                (int)TbdNumber.Read(e, "cycle_step"),
                String(e, "reveals") switch
                {
                    "clue" => ListenReveal.Clue,
                    "name" => ListenReveal.Name,
                    "fixation" => ListenReveal.Fixation,
                    var other => throw new FormatException($"Escuta revela '{other}', que não existe (use clue, name ou fixation)."),
                },
                String(e, "text_key"),
                String(e, "partial_key")))
            .ToList();

        var errors = new List<string>();
        if (entries.Any(e => e.CycleStep < 0 || e.CycleStep >= cycleLength))
        {
            errors.Add($"Escuta num golpe fora do ciclo (o ciclo tem {cycleLength} golpes, contando do 0).");
        }

        if (entries.GroupBy(e => e.CycleStep).Any(g => g.Count() > 1))
        {
            errors.Add("Dois murmúrios no mesmo golpe do ciclo.");
        }

        var nameKnown = NameKnown(root);
        foreach (var reveal in new[] { ListenReveal.Name, ListenReveal.Fixation })
        {
            var expected = reveal == ListenReveal.Name && nameKnown ? 0 : 1;
            if (entries.Count(e => e.Reveals == reveal) != expected)
            {
                errors.Add(expected == 1
                    ? $"É preciso exatamente uma escuta que revele {reveal}."
                    : "Com name_known, nenhuma escuta revela o nome (ele já se sabe).");
            }
        }

        return errors.Count == 0
            ? (entries, String(root, "murmur_key"))
            : throw new FormatException(string.Join(" ", errors));
    }

    /// <summary>
    /// Campo opcional <c>heads</c> (guardião de cabeças, Cérbero): <c>count</c>, <c>calm_offering</c>, <c>calmed_key</c>,
    /// <c>last_key</c>, <c>all_calm_key</c> e <c>art_by_awake</c> (opcional). Nulo sem o campo.
    /// </summary>
    public static HeadsRules? HeadsFromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (!root.TryGetProperty("heads", out var heads))
        {
            return null;
        }

        var rules = new HeadsRules(
            (int)TbdNumber.Read(heads, "count"),
            String(heads, "calm_offering"),
            String(heads, "calmed_key"),
            String(heads, "last_key"),
            String(heads, "all_calm_key"),
            heads.TryGetProperty("art_by_awake", out var art) ? art.EnumerateArray().Select(a => a.GetString() ?? "").ToList() : new List<string>());
        var errors = rules.Validate(CombatData.ReadAttacks(Required(root, "attacks"))).ToList();
        return errors.Count == 0 ? rules : throw new FormatException(string.Join(" ", errors));
    }

    /// <summary>
    /// Campo opcional <c>companion_readings</c>: a segunda leitura da sombra por cada companheiro (GDD 4.10: ele comenta a
    /// sombra à maneira dele; pode estar certo ou errado), id do companheiro → chave da fala.
    /// </summary>
    public static IReadOnlyDictionary<string, string> ReadingsFromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("companion_readings", out var readings)
            ? readings.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetString() ?? "", StringComparer.Ordinal)
            : new Dictionary<string, string>(StringComparer.Ordinal);
    }

    /// <summary>Campos <c>fixation</c> (a verdadeira e as opções) e <c>liberation</c>.</summary>
    public static LiberationRules LiberationFromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var fixation = Required(root, "fixation");
        var liberation = Required(root, "liberation");
        var gain = Required(liberation, "peace_gain");

        var rules = new LiberationRules(
            String(fixation, "id"),
            Required(fixation, "options").EnumerateArray()
                .Select(o => new FixationOption(String(o, "id"), String(o, "label_key"), OptionalString(o, "wrong_key") ?? ""))
                .ToList(),
            TbdNumber.Read(liberation, "peace"),
            new GradeValues(TbdNumber.Read(gain, "perfect"), TbdNumber.Read(gain, "good"), TbdNumber.Read(gain, "failed")),
            TbdNumber.Read(liberation, "lucidity_cost"),
            TbdNumber.Read(liberation, "enraged_damage_multiplier"),
            String(liberation, "progress_key"),
            String(liberation, "freed_key"));

        var errors = rules.Validate().ToList();
        return errors.Count == 0 ? rules : throw new FormatException(string.Join(" ", errors));
    }

    /// <summary>Campos <c>absorb</c> (quando e quanto pesa absorver) e <c>echo</c> (o Eco que a sombra deixa).</summary>
    public static AbsorbRules AbsorbFromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var absorb = Required(root, "absorb");
        var echo = Required(root, "echo");

        var rules = new AbsorbRules(
            TbdNumber.Read(absorb, "resolve_fraction_max"),
            TbdNumber.Read(absorb, "weak_fill_factor"),
            Grades(Required(absorb, "mark_gain")),
            new EchoDefinition(
                String(echo, "id"),
                String(echo, "name_key"),
                String(echo, "description_key"),
                String(echo, "gesture"),
                Grades(Required(echo, "resolve_damage")),
                (int)TbdNumber.Read(echo, "mark_on_use")));

        var errors = rules.Validate().ToList();
        return errors.Count == 0 ? rules : throw new FormatException(string.Join(" ", errors));
    }

    /// <summary>
    /// Campos narrativos: <c>voices</c>, <c>phases</c>, <c>story_keys</c> (Presenciar), <c>flee_clue_key</c>,
    /// <c>yield</c> (falas e custo), <c>defeat_clue_keys</c> (pistas de Hécate), <c>spare_key</c> e <c>offering</c> (opcional).
    /// </summary>
    public static ShadeStory StoryFromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var yield = Required(root, "yield");

        var story = new ShadeStory(
            Required(root, "voices").EnumerateArray()
                .Select(v => new ShadeVoice(TbdNumber.Read(v, "resolve_fraction_below"), String(v, "text_key")))
                .ToList(),
            Required(root, "phases").EnumerateArray()
                .Select(p => new ShadePhase(
                    TbdNumber.Read(p, "resolve_fraction_below"),
                    CombatData.ReadAttacks(Required(p, "attacks")),
                    String(p, "caption_key"),
                    OptionalString(p, "reveals") switch
                    {
                        null => null,
                        "name" => ListenReveal.Name,
                        "fixation" => ListenReveal.Fixation,
                        var other => throw new FormatException($"Fase revela '{other}', que não existe (use name ou fixation)."),
                    },
                    OptionalString(p, "art")))
                .ToList(),
            Keys(root, "story_keys"),
            String(root, "flee_clue_key"),
            Keys(yield, "text_keys"),
            TbdNumber.Read(yield, "lucidity_cost"),
            Keys(root, "defeat_clue_keys"),
            String(root, "spare_key"),
            root.TryGetProperty("offering", out var offering)
                ? new ShadeOffering(String(offering, "accepts"), String(offering, "accept_key"), String(offering, "refuse_key"))
                : null);

        var errors = story.Validate().ToList();
        return errors.Count == 0 ? story : throw new FormatException(string.Join(" ", errors));
    }

    private static IReadOnlyList<string> Keys(JsonElement element, string property) =>
        Required(element, property).EnumerateArray().Select(k => k.GetString() ?? "").ToList();

    private static GradeValues Grades(JsonElement e) =>
        new(TbdNumber.Read(e, "perfect"), TbdNumber.Read(e, "good"), TbdNumber.Read(e, "failed"));

    private static JsonElement Required(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) ? value : throw new FormatException($"Campo '{property}' ausente.");

    private static string? OptionalString(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static string String(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()!
            : throw new FormatException($"Campo de texto '{property}' ausente.");
}
