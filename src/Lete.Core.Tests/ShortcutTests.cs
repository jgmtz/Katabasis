using Lete.Core.Events;
using Lete.Core.State;
using Lete.Core.Systems;

namespace Lete.Core.Tests;

public class ShortcutTests
{
    [Fact]
    public void Open_OnceForGood_AndPublishes()
    {
        var state = new WorldState();
        var bus = new EventBus();
        var published = new List<ShortcutOpened>();
        using var _ = bus.Subscribe<ShortcutOpened>(published.Add);
        var shortcuts = new Shortcuts(state, bus);

        Assert.False(shortcuts.IsOpen("corda_tenaro"));
        Assert.True(shortcuts.Open("corda_tenaro"));
        Assert.False(shortcuts.Open("corda_tenaro"));
        Assert.True(shortcuts.IsOpen("corda_tenaro"));
        Assert.Equal(new[] { new ShortcutOpened("corda_tenaro") }, published);

        // Aberto, fica no save.
        Assert.True(new Shortcuts(SaveGame.Read(SaveGame.Write(state)), bus).IsOpen("corda_tenaro"));
    }
}
