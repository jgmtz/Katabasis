namespace Lete.Core.World;

/// <summary>
/// Uma região do Além (<c>content/regions/&lt;id&gt;.json</c>): o nome, a cena do Godot que a desenha e o ponto de
/// chegada padrão (um marcador da cena), usado quando ninguém diz por onde o jogador entrou. <paramref name="Tide"/> é a
/// tabela da Maré da região (GDD 2.6): o que muda a cada degrau, em ordem.
/// </summary>
public sealed record RegionDefinition(string Id, string NameKey, string Scene, string Entry, IReadOnlyList<TideLayer>? Tide = null)
{
    public IReadOnlyList<TideLayer> TideLayers => Tide ?? Array.Empty<TideLayer>();

    /// <summary>A camada da Maré que vale no degrau <paramref name="step"/>: a do maior degrau já alcançado (nula antes da primeira).</summary>
    public TideLayer? LayerAt(int step) => TideLayers.Where(l => l.Step <= step).MaxBy(l => l.Step);

    /// <summary>Problemas da tabela da Maré (lista vazia = válida).</summary>
    public IReadOnlyList<string> ValidateTide()
    {
        var errors = new List<string>();
        for (var i = 0; i < TideLayers.Count; i++)
        {
            var layer = TideLayers[i];
            if (layer.Step < 1)
            {
                errors.Add($"Região '{Id}': a Maré começa no degrau 1 (achou {layer.Step}).");
            }

            if (layer.Stubborn < 0f)
            {
                errors.Add($"Região '{Id}', degrau {layer.Step}: 'stubborn' não pode ser negativo.");
            }

            if (i > 0 && layer.Step <= TideLayers[i - 1].Step)
            {
                errors.Add($"Região '{Id}': os degraus da Maré precisam vir em ordem, sem repetir.");
            }

            if (i > 0 && layer.Stubborn < TideLayers[i - 1].Stubborn)
            {
                errors.Add($"Região '{Id}', degrau {layer.Step}: a teimosia não diminui com a Maré.");
            }
        }

        return errors;
    }
}

/// <summary>
/// Uma camada da tabela da Maré de uma região (decisão do João, 03/nov/2026: três camadas por degrau). O
/// <b>mundo</b>: a frase que o Diário ganha ao chegar na região com a Maré nesse degrau (<paramref name="ArrivalKey"/>) e
/// os nós da cena do grupo <c>mare</c>, que aparecem e somem pelo degrau. As <b>sombras</b>: encontros novos, com a condição
/// <c>tide_min</c> (nos arquivos dos encontros). O <b>efeito mecânico</b>: as sombras da região que o jogador deixou sem
/// resolver voltam mais teimosas, com <paramref name="Stubborn"/> de Firmeza a mais (fração; GDD 2.6: há um piso, não um
/// teto infinito, por isso o valor do último degrau vale para sempre).
/// </summary>
public sealed record TideLayer(int Step, string ArrivalKey, float Stubborn);
