using Godot;

namespace Lete.Game.World;

/// <summary>
/// Onde se conhece um companheiro (HISTORIA 5.2): uma conversa (o capítulo 0) como a de <see cref="Talkable"/>. Depois
/// que ele é o escolhido, segue com o protagonista e some deste lugar; os outros continuam onde estavam.
/// </summary>
public partial class CompanionSpot : Talkable
{
    /// <summary>Id do companheiro (<c>content/companions</c>).</summary>
    [Export] public string CompanionId { get; set; } = "";

    public override void _Process(double delta)
    {
        var here = GameRoot.Instance.State.CompanionId != CompanionId;
        if (here != Visible)
        {
            Visible = here;
            Monitoring = here;
        }
    }
}
