using System;
using Godot;

namespace Lete.Game.World;

/// <summary>
/// Uma ponta de um atalho físico (GDD 3.6; <c>Systems.Shortcuts</c>): uma corda, um portão, uma escada. A ponta de dentro
/// (<see cref="Inside"/>) o abre; aberto, qualquer ponta leva à outra (o marcador <see cref="Destination"/>), sem custar
/// Maré. Fechado, a ponta de fora só mostra a frase <see cref="ClosedKey"/>.
/// </summary>
public partial class PhysicalShortcut : Interactable
{
    [Export] public string ShortcutId { get; set; } = "";

    /// <summary>Marcador (grupo de teleporte da região) onde o jogador chega pela outra ponta.</summary>
    [Export] public string Destination { get; set; } = "";

    /// <summary>Esta é a ponta de dentro: daqui se abre o atalho.</summary>
    [Export] public bool Inside { get; set; }

    [Export] public string ClosedKey { get; set; } = "";
    [Export] public string OpenedKey { get; set; } = "";
    [Export] public string UseKey { get; set; } = "";

    /// <summary>O jogador usou o atalho: o mapa o leva ao marcador.</summary>
    public static event Action<string>? Used;

    public override string PromptKey => GameRoot.Instance.Shortcuts.IsOpen(ShortcutId) || Inside
        ? "ui.prompt.atalho"
        : "ui.prompt.examinar";

    public override void Examine()
    {
        var shortcuts = GameRoot.Instance.Shortcuts;
        if (shortcuts.IsOpen(ShortcutId))
        {
            Announce(UseKey);
            Used?.Invoke(Destination);
        }
        else if (Inside)
        {
            shortcuts.Open(ShortcutId);
            Announce(OpenedKey);
        }
        else
        {
            Announce(ClosedKey);
        }
    }
}
