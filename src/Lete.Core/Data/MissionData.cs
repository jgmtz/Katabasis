using System.Text.Json;
using Lete.Core.Conditions;
using Lete.Core.Dialogue;
using Lete.Core.Systems;

namespace Lete.Core.Data;

/// <summary>
/// Lê uma missão (<c>content/missions/&lt;id&gt;.json</c>, DADOS 8): <c>id</c>, <c>type</c> (region | offering | origin |
/// companion), <c>shade</c>, <c>hook</c> (condição de aparecer) com <c>hook_journal_key</c>, <c>stages</c>
/// [{<c>when</c>, <c>journal_key</c>}], <c>consequences</c> (efeitos como os dos diálogos) e <c>closed_by_fight</c>
/// (pede <c>shade</c>). Sem campo de marcador de mapa.
/// </summary>
public static class MissionData
{
    private static readonly Dictionary<string, MissionType> Types = new(StringComparer.Ordinal)
    {
        ["region"] = MissionType.Region,
        ["offering"] = MissionType.Offering,
        ["origin"] = MissionType.Origin,
        ["companion"] = MissionType.Companion,
    };

    public static Mission FromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var id = String(root, "id");
        var type = Types.TryGetValue(String(root, "type"), out var t)
            ? t
            : throw new FormatException($"Missão '{id}': 'type' precisa ser {string.Join(", ", Types.Keys)}.");
        var shade = Optional(root, "shade");
        var stages = root.TryGetProperty("stages", out var list)
            ? list.EnumerateArray().Select((s, i) => new MissionStage(
                s.TryGetProperty("when", out var when)
                    ? ConditionParser.Parse(when, $"{id}.stages[{i}].when")
                    : throw new FormatException($"Missão '{id}': a etapa {i} não tem 'when'."),
                Optional(s, "journal_key"))).ToList()
            : new List<MissionStage>();
        var consequences = root.TryGetProperty("consequences", out var effects)
            ? effects.EnumerateArray().Select(e => DialogueData.EffectFromJson(e, $"{id}.consequences", shade)).ToList()
            : new List<DialogueEffect>();
        var closedByFight = root.TryGetProperty("closed_by_fight", out var closed) && closed.GetBoolean();

        if (stages.Count == 0)
        {
            throw new FormatException($"Missão '{id}': precisa de pelo menos uma etapa.");
        }

        if (closedByFight && shade is null)
        {
            throw new FormatException($"Missão '{id}': 'closed_by_fight' precisa de 'shade'.");
        }

        return new Mission(
            id,
            type,
            shade,
            root.TryGetProperty("hook", out var hook)
                ? ConditionParser.Parse(hook, $"{id}.hook")
                : throw new FormatException($"Missão '{id}': falta 'hook'."),
            Optional(root, "hook_journal_key"),
            stages,
            consequences,
            closedByFight);
    }

    private static string? Optional(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static string String(JsonElement element, string property) =>
        Optional(element, property) ?? throw new FormatException($"Campo de texto '{property}' ausente.");
}
