using Lete.Core.Events;
using Lete.Core.State;

namespace Lete.Core.Systems;

/// <summary>
/// Atalhos físicos (GDD 3.6): portões, cordas e escadas que se abrem pelo lado de dentro e, abertos, ligam dois pontos
/// para sempre. Não custam Maré (recompensam quem explora). Quem conduz a cena leva o jogador de uma ponta à outra.
/// </summary>
public sealed class Shortcuts
{
    private readonly WorldState _state;
    private readonly EventBus _events;

    public Shortcuts(WorldState state, EventBus events)
    {
        _state = state;
        _events = events;
    }

    public bool IsOpen(string shortcutId) => _state.OpenShortcuts.Contains(shortcutId);

    /// <summary>Abre o atalho (pelo lado de dentro); falso se já estava aberto.</summary>
    public bool Open(string shortcutId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shortcutId);
        if (!_state.OpenShortcuts.Add(shortcutId))
        {
            return false;
        }

        _events.Publish(new ShortcutOpened(shortcutId));
        return true;
    }
}
