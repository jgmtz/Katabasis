using Godot;

namespace Lete.Game.World;

/// <summary>
/// Câmera ortográfica diagonal fixa (estilo Hades) que segue um alvo.
/// O rig gira no eixo Y (yaw), o nó "Pitch" inclina, e a câmera fica afastada no eixo Z local.
/// Segue com uma folga suave e olha um pouco à frente de quem anda. No render em baixa resolução, a câmera só para em
/// posições inteiras da grade de pixels (o cenário não treme ao andar) e a sobra, menor que um pixel, é devolvida em
/// <see cref="SubPixelMoved"/> para quem mostra a imagem deslocá-la: a rolagem continua lisa.
/// </summary>
public partial class CameraRig : Node3D
{
    [Export] public float YawDegrees { get; set; } = 45f;
    [Export] public float PitchDegrees { get; set; } = -35f;
    [Export] public float Distance { get; set; } = 30f;
    [Export] public float FollowSharpness { get; set; } = 7f;
    [Export] public Node3D? Target { get; set; }

    /// <summary>Quanto a câmera se adianta na direção em que o alvo anda (segundos de caminhada à frente).</summary>
    [Export] public float LeadSeconds { get; set; } = 0.32f;

    /// <summary>Com que rapidez a antecipação acompanha a mudança de direção (menor = mais preguiçosa).</summary>
    [Export] public float LeadSharpness { get; set; } = 2.2f;

    /// <summary>Prender a câmera à grade de pixels (só faz sentido no render em baixa resolução).</summary>
    public bool SnapToPixels { get; set; }

    /// <summary>A sobra da posição da câmera, em pixels do render (x para a direita, y para cima), entre -0,5 e 0,5.</summary>
    public event System.Action<Vector2>? SubPixelMoved;

    public Camera3D Camera { get; private set; } = null!;

    /// <summary>Onde a câmera estaria sem a grade (a posição suave).</summary>
    private Vector3 _smooth;
    private Vector3 _lead;

    public override void _Ready()
    {
        var pitch = GetNode<Node3D>("Pitch");
        Camera = pitch.GetNode<Camera3D>("Camera3D");

        RotationDegrees = new Vector3(0, YawDegrees, 0);
        pitch.RotationDegrees = new Vector3(PitchDegrees, 0, 0);
        Camera.Position = new Vector3(0, 0, Distance);
        Camera.Projection = Camera3D.ProjectionType.Orthogonal;
        SnapToTarget();
    }

    public override void _Process(double delta)
    {
        if (Target is null)
        {
            return;
        }

        var velocity = Target is CharacterBody3D body ? body.Velocity : Vector3.Zero;
        velocity.Y = 0f;
        _lead = _lead.Lerp(velocity * LeadSeconds, 1f - Mathf.Exp(-LeadSharpness * (float)delta));

        var t = 1f - Mathf.Exp(-FollowSharpness * (float)delta);
        _smooth = _smooth.Lerp(Target.GlobalPosition + _lead, t);
        Place();
    }

    /// <summary>Pula direto para o alvo (usado no início e no teleporte).</summary>
    public void SnapToTarget()
    {
        if (Target is not null)
        {
            _lead = Vector3.Zero;
            _smooth = Target.GlobalPosition;
            Place();
        }
    }

    /// <summary>Põe o rig na posição suave ou, com a grade, no pixel inteiro mais próximo (e avisa a sobra).</summary>
    private void Place()
    {
        var pixel = SnapToPixels && Camera.GetViewport() is { } viewport
            ? Camera.Size / viewport.GetVisibleRect().Size.Y
            : 0f;
        if (pixel <= 0f)
        {
            GlobalPosition = _smooth;
            SubPixelMoved?.Invoke(Vector2.Zero);
            return;
        }

        var basis = Camera.GlobalBasis;
        var across = _smooth.Dot(basis.X) / pixel;
        var up = _smooth.Dot(basis.Y) / pixel;
        var rest = new Vector2(across - Mathf.Round(across), up - Mathf.Round(up));
        GlobalPosition = _smooth - basis.X * (rest.X * pixel) - basis.Y * (rest.Y * pixel);
        SubPixelMoved?.Invoke(rest);
    }
}
