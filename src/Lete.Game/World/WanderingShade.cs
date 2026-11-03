using Godot;
using Lete.Core.Conditions;
using Lete.Core.Movement;

namespace Lete.Game.World;

/// <summary>
/// Uma sombra comum que vaga pelo mapa repetindo o gesto (GDD 3.2): anda de um ponto a outro e volta, com uma pausa em
/// cada ponta. A luta começa quando ela nota o jogador: ele entra no raio dela, à frente (para onde ela anda), ou
/// encosta nela (decisão do João, 03/nov/2026). Voltando da luta com ela (fugiu, por exemplo), só nota de novo quando o
/// jogador sair do raio. Poupada, continua vagando, mas não luta mais (o encontro terminou).
/// </summary>
public partial class WanderingShade : ShadeEncounter
{
    /// <summary>Pasta dos sprites de mapa (andar e parado, como em <see cref="WalkSpriteSet"/>).</summary>
    [Export(PropertyHint.Dir)] public string SpriteFolder { get; set; } = "";
    [Export] public string SpritePrefix { get; set; } = "";

    /// <summary>Para onde ela anda a partir do lugar dela (e volta).</summary>
    [Export] public Vector3 WanderOffset { get; set; } = new(3f, 0f, 0f);

    [Export] public float WalkSpeed { get; set; } = 0.8f;

    /// <summary>Quanto tempo ela fica parada em cada ponta, repetindo o gesto.</summary>
    [Export] public float PauseSeconds { get; set; } = 1.5f;

    /// <summary>Até onde ela vê (metros) e o ângulo total da visão, à frente dela (graus).</summary>
    [Export] public float NoticeRadius { get; set; } = 4f;
    [Export] public float NoticeAngleDegrees { get; set; } = 120f;

    private AnimatedSprite3D _sprite = null!;
    private Vector3 _home;
    private bool _outward = true;
    private float _pause;
    private Vector3 _facing = Vector3.Back;
    private ScreenDirection _direction = ScreenDirection.Down;
    private bool _armed;

    /// <summary>O encontro da última luta que começou no mapa (a cena recarrega na volta).</summary>
    private static string? _lastFought;

    public override void _Ready()
    {
        _home = Position;
        _armed = _lastFought != EncounterId;
        if (!_armed)
        {
            _lastFought = null;
        }

        _sprite = new AnimatedSprite3D
        {
            Name = "Sprite",
            SpriteFrames = new WalkSpriteSet { Folder = SpriteFolder, Prefix = SpritePrefix }.BuildFrames(),
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            Shaded = true,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
            AlphaCut = SpriteBase3D.AlphaCutMode.Discard,
        };
        _sprite.AddToGroup(MapSprite.Group);
        AddChild(_sprite);
        _sprite.FrameChanged += AlignToGround;
        _sprite.AnimationChanged += AlignToGround;
        _sprite.Play(WalkSpriteSet.IdleAnimation(_direction));
        base._Ready();
    }

    public override void _Process(double delta)
    {
        var available = GameRoot.Instance.Encounters.IsAvailable(EncounterId);
        // Poupada (ou escutada até o fim) a sombra segue no mapa; libertada, absorvida ou desfeita, some.
        var wandering = available || GameRoot.Instance.Facts.ShadeIs(ShadeId, ShadeState.Spared);
        if (wandering != Visible)
        {
            Visible = wandering;
        }

        if (available != Monitoring)
        {
            Monitoring = available;
        }

        if (!wandering)
        {
            return;
        }

        Wander((float)delta);
        if (available && Player is { } player)
        {
            var toPlayer = player.GlobalPosition - GlobalPosition;
            toPlayer.Y = 0f;
            var distance = toPlayer.Length();
            if (distance > NoticeRadius)
            {
                _armed = true;
            }
            else if (_armed && player.InputEnabled && Sees(toPlayer, distance))
            {
                _armed = false;
                Engage(player);
            }
        }
    }

    protected override void OnEngaged() => _lastFought = EncounterId;

    /// <summary>Encostar também começa a luta, mas só armada (voltando da luta, o jogador fica perto dela).</summary>
    protected override bool CanEngage(Player player) => _armed && player.InputEnabled;

    private string ShadeId => GameRoot.Instance.Encounters.Definitions[EncounterId].ShadeId;

    private Player? Player => GetTree().GetFirstNodeInGroup(World.Player.Group) as Player;

    private bool Sees(Vector3 toPlayer, float distance) =>
        distance < 0.01f || _facing.Dot(toPlayer / distance) >= Mathf.Cos(Mathf.DegToRad(NoticeAngleDegrees / 2f));

    private void Wander(float delta)
    {
        if (_pause > 0f)
        {
            _pause -= delta;
            Play(moving: false);
            return;
        }

        var target = _outward ? _home + WanderOffset : _home;
        var step = target - Position;
        if (step.Length() <= WalkSpeed * delta)
        {
            Position = target;
            _outward = !_outward;
            _pause = PauseSeconds;
            Play(moving: false);
            return;
        }

        _facing = step.Normalized();
        Position += _facing * WalkSpeed * delta;
        Play(moving: true);
    }

    /// <summary>A animação da direção na tela para onde ela anda (a câmera é diagonal).</summary>
    private void Play(bool moving)
    {
        if (GetViewport().GetCamera3D() is { } camera)
        {
            var basis = camera.GlobalBasis;
            var right = new Vector3(basis.X.X, 0, basis.X.Z).Normalized();
            var forward = new Vector3(-basis.Z.X, 0, -basis.Z.Z).Normalized();
            var screen = new System.Numerics.Vector2(_facing.Dot(right), -_facing.Dot(forward));
            _direction = WalkRules.Resolve(screen, _direction);
        }

        var animation = moving ? WalkSpriteSet.WalkAnimation(_direction) : WalkSpriteSet.IdleAnimation(_direction);
        if (_sprite.Animation != animation)
        {
            _sprite.Play(animation);
        }
    }

    /// <summary>Pés na origem do nó, sem meio pixel fora da grade (como em <see cref="WalkingCharacter"/>).</summary>
    private void AlignToGround()
    {
        if (_sprite.SpriteFrames?.GetFrameTexture(_sprite.Animation, _sprite.Frame) is { } texture)
        {
            _sprite.Offset = new Vector2(texture.GetWidth() % 2 / 2f, texture.GetHeight() / 2f);
        }
    }
}
