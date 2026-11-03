namespace Lete.Core.Combat;

/// <summary>Golpe direto (o jogador apara com o Anel) ou de padrão (o jogador foge no quadro da Esquiva), GDD 4.4.</summary>
public enum ShadeAttackKind
{
    Direct,
    Pattern,
}

/// <summary>
/// Um golpe do ciclo da sombra: o tipo, o gesto que o jogador faz para se defender (<c>content/gestures</c>),
/// a legenda que descreve o gesto dela e quanto Fôlego tira em cada resultado da defesa. <paramref name="Head"/>: de
/// qual cabeça é o golpe, num guardião de cabeças (<see cref="GuardianHeads"/>); nulo = de todas.
/// </summary>
public sealed record ShadeAttack(ShadeAttackKind Kind, string Gesture, string CaptionKey, GradeValues BreathDamage, int? Head = null);
