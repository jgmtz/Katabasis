using Godot;

namespace Lete.Game.Scenery;

/// <summary>Luz de fogo: a energia treme em torno de <see cref="BaseEnergy"/> (tocha, braseiro, brasa do forno).</summary>
public partial class Flicker : OmniLight3D
{
    [Export] public float BaseEnergy { get; set; } = 1.3f;

    /// <summary>Quanto treme, em fração da energia.</summary>
    [Export] public float Amount { get; set; } = 0.16f;

    /// <summary>Desencontra uma chama da outra.</summary>
    [Export] public float Phase { get; set; }

    private float _time;

    public override void _Ready() => LightEnergy = BaseEnergy;

    public override void _Process(double delta)
    {
        _time += (float)delta;
        var wave = Mathf.Sin(_time * 11f + Phase) * 0.5f + Mathf.Sin(_time * 23.7f + Phase * 2f) * 0.3f + Mathf.Sin(_time * 5.3f + Phase) * 0.2f;
        LightEnergy = BaseEnergy * (1f + Amount * wave);
    }
}
