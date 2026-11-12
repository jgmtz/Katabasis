namespace Lete.Core.State;

/// <summary>
/// O rosto no fim do jogo pela regra C (<c>docs/PROTAGONISTA_SOMBRA.md</c> 7): devolvido (os traços verdadeiros) ou
/// coberto pela romã; meio coberto quando as Pesadas encaradas e deixadas empatam (decisão do João, 11/nov/2026).
/// <see cref="None"/> até ser calculado.
/// </summary>
public enum FinalFace
{
    None,
    Returned,
    Covered,
    Half,
}
