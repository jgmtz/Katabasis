namespace Lete.Core.State;

/// <summary>Onde uma missão começada está: quantas etapas já cumpriu (0 logo depois do gancho) e se foi fechada.</summary>
public sealed class MissionState
{
    public int Stage { get; set; }

    /// <summary>Fechada sem se cumprir (a sombra dela foi destruída em combate).</summary>
    public bool Closed { get; set; }
}
