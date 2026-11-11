using System;
using Godot;
using Lete.Core.Conditions;

namespace Lete.Game.World;

/// <summary>
/// Passagem para outra região: ao entrar na área, o jogador vai para <see cref="TargetRegion"/> e chega no marcador
/// <see cref="TargetEntry"/> de lá. A cena (Main) troca a região e salva (salvar ao trocar de região). Uma passagem
/// pode pedir uma chave do Caminho (<see cref="RequiredPathKey"/>: a barca de Caronte pede a do Aqueronte) ou uma
/// condição (<see cref="RequiredCondition"/>, em JSON, como nos dados: o caminho do Estige pede os três caminhos do
/// Ato II visitados); sem ela, mostra <see cref="BlockedKey"/>. <see cref="Milestone"/> é o marco da Maré de cruzá-la
/// (uma vez só).
/// </summary>
public partial class RegionExit : Area3D
{
    /// <summary>Id da região de destino (<c>content/regions</c>).</summary>
    [Export] public string TargetRegion { get; set; } = "";

    /// <summary>Nome do marcador de chegada na cena de destino (no grupo <c>teleport_points</c>).</summary>
    [Export] public string TargetEntry { get; set; } = "";

    [Export] public string RequiredPathKey { get; set; } = "";

    /// <summary>Condição em JSON (<c>docs/DADOS.md</c>); vazia, não pede nada.</summary>
    [Export] public string RequiredCondition { get; set; } = "";

    [Export] public string BlockedKey { get; set; } = "";
    [Export] public string Milestone { get; set; } = "";

    /// <summary>Região, marcador de chegada e marco da Maré (ou nulo).</summary>
    public static event Action<string, string, string?>? Crossed;

    private bool _crossed;
    private Condition? _condition;

    public override void _Ready()
    {
        _condition = RequiredCondition == "" ? null : ConditionParser.Parse(RequiredCondition, $"{Name}.RequiredCondition");
        BodyEntered += body =>
        {
            if (body is not Player || _crossed)
            {
                return;
            }

            GameRoot.Instance.PathKeys.Refresh();
            if ((RequiredPathKey != "" && !GameRoot.Instance.PathKeys.Has(RequiredPathKey))
                || (_condition is not null && !ConditionEvaluator.Passes(_condition, GameRoot.Instance.Facts)))
            {
                Interactable.Say(BlockedKey);
                return;
            }

            _crossed = true;
            Crossed?.Invoke(TargetRegion, TargetEntry, Milestone == "" ? null : Milestone);
        };
    }
}
