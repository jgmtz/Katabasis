using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Lete.Game.World;

/// <summary>
/// Protagonista no mapa: anda como qualquer <see cref="WalkingCharacter"/> e examina objetos próximos.
/// </summary>
public partial class Player : WalkingCharacter
{
    /// <summary>Grupo do jogador no mapa (quem precisa achá-lo, como as sombras que vagam).</summary>
    public const string Group = "player";

    private readonly List<Interactable> _nearby = new();

    public event Action? NearbyChanged;

    public Interactable? CurrentInteractable =>
        _nearby.OrderBy(i => i.GlobalPosition.DistanceSquaredTo(GlobalPosition)).FirstOrDefault();

    public void AddNearby(Interactable interactable)
    {
        if (!_nearby.Contains(interactable))
        {
            _nearby.Add(interactable);
            NearbyChanged?.Invoke();
        }
    }

    public void RemoveNearby(Interactable interactable)
    {
        if (_nearby.Remove(interactable))
        {
            NearbyChanged?.Invoke();
        }
    }

    /// <summary>
    /// A luz própria do protagonista, o único vivo no Além: fraca e morna, acompanha o corpo e acende o chão em volta
    /// para ele não sumir no escuro. Fica um pouco à frente (do lado da câmera) e acima, para pegar o rosto do sprite.
    /// </summary>
    [Export] public Color AuraColor { get; set; } = new(1f, 0.86f, 0.7f);
    [Export] public float AuraEnergy { get; set; } = 1.6f;
    [Export] public float AuraRange { get; set; } = 3.6f;
    [Export] public Vector3 AuraOffset { get; set; } = new(0.4f, 1.1f, 0.4f);

    /// <summary>A cor para onde a luz esfria conforme a Marca a apaga, e quanto tempo a mudança leva.</summary>
    [Export] public Color AuraFadedColor { get; set; } = new(0.55f, 0.66f, 0.72f);
    [Export] public float AuraChangeSeconds { get; set; } = 1.5f;

    private OmniLight3D _aura = null!;
    private Tween? _auraTween;
    private bool _auraApplied;

    /// <summary>
    /// Quanto resta da luz própria (a Marca, <c>MarkLook.AuraOf</c>): 1 inteira e morna; menos, mais fraca e fria; 0
    /// apagada. Muda devagar, sem estalo.
    /// </summary>
    public void SetAura(float amount)
    {
        amount = Mathf.Clamp(amount, 0f, 1f);
        _auraTween?.Kill();
        if (!_auraApplied)
        {
            // A primeira vez (a cena abrindo) já começa no ponto certo; só as mudanças depois são devagar.
            _auraApplied = true;
            _aura.LightEnergy = AuraEnergy * amount;
            _aura.LightColor = AuraFadedColor.Lerp(AuraColor, amount);
            return;
        }

        _auraTween = CreateTween().SetParallel().SetTrans(Tween.TransitionType.Sine);
        _auraTween.TweenProperty(_aura, "light_energy", AuraEnergy * amount, AuraChangeSeconds);
        _auraTween.TweenProperty(_aura, "light_color", AuraFadedColor.Lerp(AuraColor, amount), AuraChangeSeconds);
    }

    public override void _Ready()
    {
        base._Ready();
        AddToGroup(Group);
        AddChild(_aura = new OmniLight3D
        {
            Name = "Aura",
            Position = AuraOffset,
            LightColor = AuraColor,
            LightEnergy = AuraEnergy,
            OmniRange = AuraRange,
            OmniAttenuation = 1.2f,
            ShadowEnabled = false,
        });
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        // Input global: o jogador vive dentro do SubViewport, mas a ação vale para o jogo todo.
        if (InputEnabled && Input.IsActionJustPressed("interact") && CurrentInteractable is { } target)
        {
            target.Examine();
        }
    }
}
