using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Lete.Game.World;

/// <summary>
/// O acabamento de um personagem no mapa (protagonista, companheiro, sombra): põe no sprite o shader
/// <c>personagem</c> (luz de lado, respiração, sombra dos mortos) e deita no chão a sombra projetada, para longe da
/// luz mais forte por perto. Vive como filho do sprite; quem cria é <see cref="Attach"/>.
/// </summary>
public partial class CharacterLook : Node3D
{
    private static readonly Shader BodyShader = GD.Load<Shader>("res://src/Lete.Game/World/personagem.gdshader");
    private static readonly Texture2D ContactTexture = GD.Load<Texture2D>("res://art/sprites/comum/sombra_chao.png");
    private static readonly Shader ShadowShader = GD.Load<Shader>("res://src/Lete.Game/World/sombra_projetada.gdshader");

    /// <summary>Quem não é sombra dos mortos, mesmo na pasta das sombras: deuses, monstros, personificações, castigados.</summary>
    private static readonly HashSet<string> SolidFigures = new()
    {
        "cerbero", "caronte", "campeao", "tanato", "sonho", "furias_alecto", "furias_megera", "furias_tisifone",
        "ascalafo", "guerra", "fome", "doenca", "medo", "guardioes_mnemosine", "sisifo", "tantalo", "danaides",
        "esquecido_m", "esquecido_f",
    };

    /// <summary>Figuras em que o encolher do peito ficaria errado (pedra, grupo deitado, corpo dobrado).</summary>
    private static readonly HashSet<string> NoBreath = new()
    {
        "sisifo", "tantalo", "danaides", "doenca", "insepultos", "afogado", "cerbero", "guardioes_mnemosine",
    };

    /// <summary>Quanto de sombra dos mortos (0 = corpo sólido).</summary>
    private const float ShadeAmount = 0.7f;

    /// <summary>Força da sombra no chão: um corpo sólido e uma sombra dos mortos (que quase não a tem).</summary>
    private const float SolidShadow = 0.9f;
    private const float ShadeShadow = 0.5f;

    /// <summary>A mancha escura bem debaixo dos pés: largura em relação ao sprite, proporção e força.</summary>
    private const float ContactWidth = 1.15f;
    private const float ContactDepth = 0.5f;
    private const float ContactMaxWidth = 1.6f;
    private const float SolidContact = 1f;
    private const float ShadeContact = 0.55f;

    /// <summary>Altura das sombras sobre o chão: acima das lâminas de chão do cenário, que ficam empilhadas.</summary>
    private const float GroundLift = 0.07f;

    /// <summary>Peso da lua contra as chamas ao escolher para onde a sombra cai.</summary>
    private const float MoonWeight = 0.3f;

    /// <summary>Comprimento da sombra em relação à altura do sprite: sob a lua e perto de uma chama.</summary>
    private const float MoonReach = 0.7f;
    private const float FireReach = 1.2f;

    /// <summary>
    /// Para onde cai a sombra da lua, na tela: para a esquerda e um pouco para a frente (em direção à câmera), onde
    /// ela aparece inteira ao lado do corpo em vez de se esconder atrás dele.
    /// </summary>
    private static readonly Vector2 MoonFallOnScreen = new(-1f, 0.45f);

    /// <summary>Altura (em pixels do sprite) do flutuar das sombras dos mortos.</summary>
    private const float FloatPixels = 1f;

    private SpriteBase3D _sprite = null!;
    private ShaderMaterial _body = null!;
    private ShaderMaterial _shadowMaterial = null!;
    private MeshInstance3D _shadow = null!;
    private MeshInstance3D _contact = null!;
    private StandardMaterial3D _contactMaterial = null!;
    private Texture2D? _texture;
    private float _shade;
    private bool _breathes;
    private bool _moving;
    private bool _castsShadow = true;
    private float _phase;
    private float _time;
    private Vector3 _reach = new(-0.6f, 0f, -0.6f);
    private float _strength;
    private float _lightTimer;
    private Vector3 _targetReach;
    private float _targetStrength;
    /// <summary>As luzes da cena, relidas de tempos em tempos (a região troca com o jogo aberto) e divididas por todos.</summary>
    private static List<OmniLight3D> _lights = new();
    private static ulong _lightsReadAt;
    private const ulong LightsRereadMs = 1500;

    /// <summary>Põe o acabamento num sprite de mapa (uma vez só) e o devolve.</summary>
    public static CharacterLook Attach(SpriteBase3D sprite)
    {
        if (sprite.GetNodeOrNull<CharacterLook>("Look") is { } existing)
        {
            return existing;
        }

        var look = new CharacterLook { Name = "Look", _sprite = sprite };
        sprite.AddChild(look);
        return look;
    }

    /// <summary>Andando: sem respiração (o ciclo de andar já mexe o corpo).</summary>
    public bool Moving
    {
        get => _moving;
        set => _moving = value;
    }

    /// <summary>A Marca: perda de cor e, nas faixas altas, nenhuma sombra no chão.</summary>
    public void SetMark(float desaturation, bool castsShadow)
    {
        _body.SetShaderParameter("desaturation", desaturation);
        _castsShadow = castsShadow;
    }

    public override void _Ready()
    {
        _phase = (float)(GD.Randi() % 628) / 100f;
        _body = new ShaderMaterial { Shader = BodyShader };
        _body.SetShaderParameter("phase", _phase);
        _sprite.MaterialOverride = _body;
        _sprite.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;

        _shadowMaterial = new ShaderMaterial { Shader = ShadowShader, RenderPriority = -1 };
        _shadow = new MeshInstance3D
        {
            Name = "Shadow",
            Mesh = new QuadMesh { Size = Vector2.One },
            MaterialOverride = _shadowMaterial,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            // O vértice é posto no mundo pelo shader: a caixa de corte precisa cobrir a sombra inteira.
            CustomAabb = new Aabb(new Vector3(-8, -1, -8), new Vector3(16, 2, 16)),
        };
        AddChild(_shadow);

        _contactMaterial = new StandardMaterial3D
        {
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoTexture = ContactTexture,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
            DisableFog = true,
            RenderPriority = -1,
        };
        _contact = new MeshInstance3D
        {
            Name = "Contact",
            Mesh = new PlaneMesh { Size = Vector2.One },
            MaterialOverride = _contactMaterial,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            TopLevel = true,
        };
        AddChild(_contact);
        Refresh(force: true);
        FindLight();
        _reach = _targetReach;
        _strength = _targetStrength;
    }

    public override void _Process(double delta)
    {
        _time += (float)delta;
        Refresh(force: false);

        _lightTimer -= (float)delta;
        if (_lightTimer <= 0f)
        {
            _lightTimer = 0.2f;
            FindLight();
        }

        var t = 1f - Mathf.Exp(-6f * (float)delta);
        _reach = _reach.Lerp(_targetReach, t);
        _strength = Mathf.Lerp(_strength, _targetStrength, t);
        UpdateShadow();

        _body.SetShaderParameter("breath", _breathes && !_moving ? 1f : 0f);
        if (_shade > 0f)
        {
            // Flutua em passos de um pixel: sobe e desce devagar, a sombra no chão fica.
            var lift = Mathf.Round((Mathf.Sin(_time * 1.1f + _phase) * 0.5f + 0.5f) * FloatPixels);
            _body.SetShaderParameter("lift", lift * _sprite.PixelSize * _sprite.GlobalBasis.Scale.Y);
        }
    }

    /// <summary>Quadro novo do sprite: passa a textura aos dois shaders e, se mudou a figura, o perfil dela.</summary>
    private void Refresh(bool force)
    {
        var texture = CurrentTexture();
        if (texture is null || (!force && texture == _texture))
        {
            return;
        }

        var first = _texture is null;
        _texture = texture;
        _body.SetShaderParameter("sprite_texture", texture);
        _shadowMaterial.SetShaderParameter("sprite_texture", texture);
        if (first || force)
        {
            ApplyProfile(texture.ResourcePath);
        }
    }

    private Texture2D? CurrentTexture() => _sprite switch
    {
        AnimatedSprite3D animated => animated.SpriteFrames?.GetFrameTexture(animated.Animation, animated.Frame),
        Sprite3D still => still.Texture,
        _ => null,
    };

    /// <summary>O perfil sai da pasta da arte: <c>art/sprites/sombras/&lt;figura&gt;/...</c> é sombra dos mortos, salvo as exceções.</summary>
    private void ApplyProfile(string path)
    {
        var parts = path.Split('/');
        var index = System.Array.IndexOf(parts, "sombras");
        var figure = index >= 0 && index + 1 < parts.Length ? parts[index + 1] : "";
        _shade = figure != "" && !SolidFigures.Contains(figure) ? ShadeAmount : 0f;

        // Quem já tem o parado desenhado (respirar ou flutuar em quadros) não ganha o de código por cima.
        var drawnIdle = _sprite is MapSprite { IdleFramesPrefix: not "" };
        _breathes = !drawnIdle && !NoBreath.Contains(figure);
        _body.SetShaderParameter("shade", _shade);
        if (drawnIdle)
        {
            _body.SetShaderParameter("lift", 0f);
        }
    }

    /// <summary>Para onde a sombra cai: para longe das chamas ao alcance (pesadas pela força) e da lua.</summary>
    private void FindLight()
    {
        if (!IsInsideTree())
        {
            return;
        }

        var now = Time.GetTicksMsec();
        if (_lightsReadAt == 0 || now - _lightsReadAt > LightsRereadMs)
        {
            _lightsReadAt = now;
            var scene = GetTree().CurrentScene ?? GetTree().Root;
            _lights = scene.FindChildren("*", nameof(OmniLight3D), true, false).OfType<OmniLight3D>().ToList();
        }

        var feet = GlobalPosition;
        var away = Vector3.Zero;
        var fire = 0f;
        foreach (var light in _lights)
        {
            if (!IsInstanceValid(light) || !light.IsVisibleInTree())
            {
                continue;
            }

            var from = feet - light.GlobalPosition;
            from.Y = 0f;
            var distance = from.Length();
            if (distance >= light.OmniRange || distance < 0.05f)
            {
                continue;
            }

            var falloff = 1f - distance / light.OmniRange;
            var weight = light.LightEnergy * falloff * falloff;
            away += from / distance * weight;
            fire += weight;
        }

        var moon = new Vector3(-0.3f, 0f, 1f).Normalized();
        if (GetViewport().GetCamera3D() is { } camera)
        {
            var basis = camera.GlobalBasis;
            var screenRight = new Vector3(basis.X.X, 0, basis.X.Z).Normalized();
            var towardCamera = new Vector3(basis.Z.X, 0, basis.Z.Z).Normalized();
            moon = (screenRight * MoonFallOnScreen.X + towardCamera * MoonFallOnScreen.Y).Normalized();
        }

        away += moon * MoonWeight;

        var direction = away.LengthSquared() > 0.0001f ? away.Normalized() : moon;
        var near = Mathf.Clamp(fire, 0f, 1f);
        _targetReach = direction * Mathf.Lerp(MoonReach, FireReach, near);
        _targetStrength = (_shade > 0f ? ShadeShadow : SolidShadow) * Mathf.Lerp(0.75f, 1f, near);
    }

    private void UpdateShadow()
    {
        var visible = _castsShadow && _texture is not null && _sprite.IsVisibleInTree();
        _shadow.Visible = visible;
        _contact.Visible = visible;
        if (!visible || _texture is null || GetViewport().GetCamera3D() is not { } camera)
        {
            return;
        }

        var scale = _sprite.GlobalBasis.Scale;
        var width = _texture.GetWidth() * _sprite.PixelSize * scale.X;
        var height = _texture.GetHeight() * _sprite.PixelSize * scale.Y;
        // A silhueta deita no chão ao longo da direção da sombra; a base fica atravessada a ela.
        var along = _reach.LengthSquared() > 0.0001f ? _reach.Normalized() : Vector3.Back;
        var right = new Vector3(-along.Z, 0f, along.X);
        if (right.Dot(camera.GlobalBasis.X) < 0f)
        {
            right = -right;
        }

        var reach = _reach;
        var blob = Mathf.Min(width * ContactWidth, ContactMaxWidth);
        _contact.GlobalTransform = new Transform3D(
            Basis.Identity.Scaled(new Vector3(blob, 1f, blob * ContactDepth)).Rotated(Vector3.Up, camera.GlobalRotation.Y),
            GlobalPosition + Vector3.Up * GroundLift);
        _contactMaterial.AlbedoColor = new Color(1f, 1f, 1f, _shade > 0f ? ShadeContact : SolidContact);

        _shadowMaterial.SetShaderParameter("reach", reach * height);
        _shadowMaterial.SetShaderParameter("width", right * width);
        _shadowMaterial.SetShaderParameter("strength", _strength);
        _shadowMaterial.SetShaderParameter("flip", _sprite.FlipH ? 1f : 0f);
    }
}
