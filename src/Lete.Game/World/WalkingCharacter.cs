using System.Collections.Generic;
using System.Linq;
using Godot;
using Lete.Core.Movement;

namespace Lete.Game.World;

/// <summary>
/// Personagem que anda nas quatro direções da tela (relativas à câmera diagonal) com as animações
/// de andar/parado, e troca entre os conjuntos de sprites com <c>switch_character</c>.
/// </summary>
public partial class WalkingCharacter : CharacterBody3D
{
    /// <summary>Velocidade de caminhada em metros por segundo (sensação de controle, não balanceamento).</summary>
    [Export] public float MoveSpeed { get; set; } = 1.6f;

    /// <summary>
    /// Multiplica a velocidade enquanto <c>run</c> está pressionado. A animação acelera na mesma
    /// proporção, para os pés continuarem acompanhando o chão.
    /// </summary>
    [Export] public float RunMultiplier { get; set; } = 1.75f;

    /// <summary>Quanto tempo leva para chegar à velocidade cheia e para parar (segundos): dá peso ao andar.</summary>
    [Export] public float StartSeconds { get; set; } = 0.14f;
    [Export] public float StopSeconds { get; set; } = 0.11f;

    [Export] public WalkSpriteSet[] Characters { get; set; } = System.Array.Empty<WalkSpriteSet>();

    private List<SpriteFrames> _frames = new();
    private AnimatedSprite3D _sprite = null!;
    private int _current;
    private ScreenDirection _direction = ScreenDirection.Down;
    private CharacterLook _look = null!;
    private Node3D? _groundShadow;

    /// <summary>Prefixo do conjunto de sprites mostrado agora (ex.: "sombra_f").</summary>
    public string? CurrentPrefix => Characters.Length > 0 ? Characters[_current].Prefix : null;

    /// <summary>Trocou o conjunto de sprites mostrado (<c>switch_character</c> ou <see cref="SelectCharacter"/>).</summary>
    public event System.Action? CharacterChanged;

    /// <summary>Quando falso, ignora a entrada (por exemplo, com um painel aberto).</summary>
    public bool InputEnabled { get; set; } = true;

    public float PixelSize
    {
        get => _sprite.PixelSize;
        set => _sprite.PixelSize = value;
    }

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite3D>("Sprite");
        _groundShadow = GetNodeOrNull<Node3D>("GroundShadow");
        _frames = Characters.Select(c => c.BuildFrames()).ToList();
        _sprite.FrameChanged += AlignToPixelGrid;
        _sprite.AnimationChanged += AlignToPixelGrid;
        _look = CharacterLook.Attach(_sprite);
        ShowCharacter(0);
    }

    public override void _PhysicsProcess(double delta)
    {
        var raw = InputEnabled
            ? Input.GetVector("move_left", "move_right", "move_up", "move_down")
            : Vector2.Zero;
        var input = WalkRules.LimitLength(new System.Numerics.Vector2(raw.X, raw.Y));

        var speedScale = InputEnabled && Input.IsActionPressed("run") ? RunMultiplier : 1f;
        var full = MoveSpeed * speedScale;
        var wanted = ScreenToWorld(new Vector2(input.X, input.Y)) * full;
        var moving = input != System.Numerics.Vector2.Zero;

        // Sai do lugar e para em rampa curta, em vez de num estalo.
        var ramp = full / Mathf.Max(moving ? StartSeconds : StopSeconds, 0.001f);
        Velocity = Velocity.MoveToward(wanted, ramp * (float)delta);
        MoveAndSlide();

        _direction = WalkRules.Resolve(input, _direction);
        var animation = moving
            ? WalkSpriteSet.WalkAnimation(_direction)
            : WalkSpriteSet.IdleAnimation(_direction);
        _look.Moving = moving;

        // Os pés acompanham o chão: o ciclo anda na proporção da velocidade de verdade.
        var pace = new Vector2(Velocity.X, Velocity.Z).Length() / Mathf.Max(MoveSpeed, 0.001f);
        _sprite.SpeedScale = moving ? Mathf.Clamp(pace, 0.55f, speedScale) : 1f;
        if (_sprite.Animation != animation)
        {
            _sprite.Play(animation);
        }
    }

    public override void _Process(double delta)
    {
        if (InputEnabled && _frames.Count > 1 && Input.IsActionJustPressed("switch_character"))
        {
            ShowCharacter((_current + 1) % _frames.Count);
        }
    }

    /// <summary>Mostra o personagem cujo conjunto de sprites tem este prefixo (ex.: "lia"); sem efeito se não houver.</summary>
    public void SelectCharacter(string prefix)
    {
        var index = System.Array.FindIndex(Characters, c => c.Prefix == prefix);
        if (index >= 0)
        {
            ShowCharacter(index);
        }
    }

    /// <summary>
    /// A Marca no sprite (<c>Lete.Core.Presentation.MarkLook</c>): com perda de cor, o sprite passa para o shader
    /// <c>personagem</c> tira a cor na medida da faixa. Sem sombra, somem o disco e a sombra projetada no chão.
    /// </summary>
    public void SetMarkLook(float desaturation, bool castsShadow)
    {
        _look.SetMark(desaturation, castsShadow);
        // O disco antigo da cena deu lugar à sombra de contato do acabamento (a mesma para todos os personagens).
        if (_groundShadow is not null)
        {
            _groundShadow.Visible = false;
        }

        AlignToPixelGrid();
    }

    public void TeleportTo(Vector3 position)
    {
        GlobalPosition = position;
        Velocity = Vector3.Zero;
    }

    private void ShowCharacter(int index)
    {
        if (_frames.Count == 0)
        {
            return;
        }

        _current = index;
        var animation = _sprite.Animation;
        var frame = _sprite.Frame;
        _sprite.SpriteFrames = _frames[index];

        if (_frames[index].HasAnimation(animation))
        {
            _sprite.Play(animation);
            _sprite.Frame = frame;
        }
        else
        {
            _sprite.Play(WalkSpriteSet.IdleAnimation(_direction));
        }

        AlignToPixelGrid();
        CharacterChanged?.Invoke();
    }

    /// <summary>
    /// Pés na origem do nó: todos os quadros têm a mesma altura e a base é o chão. A largura muda entre
    /// quadros (e pode ser ímpar); centralizar um quadro ímpar deixaria a arte meio pixel fora da grade.
    /// </summary>
    private void AlignToPixelGrid()
    {
        var texture = _sprite.SpriteFrames?.GetFrameTexture(_sprite.Animation, _sprite.Frame);
        if (texture is null)
        {
            return;
        }

        _sprite.Offset = new Vector2(texture.GetWidth() % 2 / 2f, texture.GetHeight() / 2f);
    }

    /// <summary>Converte a entrada da tela (y para baixo) numa direção no chão, segundo a câmera atual.</summary>
    private Vector3 ScreenToWorld(Vector2 input)
    {
        var camera = GetViewport().GetCamera3D();
        if (camera is null || input == Vector2.Zero)
        {
            return Vector3.Zero;
        }

        var basis = camera.GlobalBasis;
        var right = new Vector3(basis.X.X, 0, basis.X.Z).Normalized();
        var forward = new Vector3(-basis.Z.X, 0, -basis.Z.Z).Normalized();

        // A câmera inclinada encurta na tela o que se anda "para dentro" (1 m vira ~sen(inclinação)).
        // Dividir por esse encurtamento deixa a velocidade na tela igual em todas as direções.
        var screenShortening = Mathf.Max(Mathf.Abs(forward.Dot(basis.Y)), 0.1f);
        return right * input.X + forward * (-input.Y / screenShortening);
    }
}
