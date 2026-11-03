using System.Linq;
using Godot;

namespace Lete.Game.World;

/// <summary>
/// Sprite parado de mapa (uma sombra com quem se conversa, por exemplo): de frente para a câmera e iluminado como o
/// jogador (mesmos ajustes do Sprite3D dele), com os pés na origem do nó e a mesma densidade de pixel dos personagens
/// (o <c>Main</c> ajusta o grupo <see cref="Group"/>). Com <see cref="IdleFramesPrefix"/>, repete a animação parada
/// desenhada (respirar ou flutuar, as figuras em 3/4).
/// </summary>
public partial class MapSprite : Sprite3D
{
    public const string Group = "map_sprite";

    /// <summary>Caminho dos quadros sem o número (<c>..._idle_3q_esq_</c> + <c>01.png</c>); vazio = imagem fixa.</summary>
    [Export] public string IdleFramesPrefix { get; set; } = "";

    [Export] public int IdleFrameCount { get; set; } = 4;

    [Export] public float IdleFps { get; set; } = 4f;

    private Texture2D[] _idle = System.Array.Empty<Texture2D>();
    private float _time;

    public override void _Ready()
    {
        AddToGroup(Group);
        Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        Shaded = true;
        TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        AlphaCut = AlphaCutMode.Discard;
        if (IdleFramesPrefix != "")
        {
            _idle = Enumerable.Range(1, IdleFrameCount).Select(i => GD.Load<Texture2D>($"{IdleFramesPrefix}{i:00}.png")).ToArray();
            Texture = _idle[0];
        }

        SetProcess(_idle.Length > 1);
        if (Texture is { } texture)
        {
            Offset = new Vector2(0, texture.GetHeight() / 2f);
        }
    }

    public override void _Process(double delta)
    {
        _time += (float)delta;
        Texture = _idle[(int)(_time * IdleFps) % _idle.Length];
    }
}
