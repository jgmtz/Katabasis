namespace Lete.Core.Combat;

/// <summary>Papel de alguém do elenco.</summary>
public enum CastKind
{
    Protagonist,
    Companion,
    Shade,
}

/// <summary>
/// Uma pessoa do elenco, num lugar só (<c>content/characters/elenco.json</c>): nome, papel, gênero, altura no mundo,
/// arte de batalha de frente e de costas (nome do arquivo, sem pasta nem extensão), retrato, pasta dos sprites de
/// mapa, tinta (sombras) e a animação parada desenhada. Batalhas e telas citam só o <see cref="Id"/>.
/// <paramref name="Selectable"/>: aparece na escolha de personagem (provisório até os 6 protagonistas existirem).
/// <paramref name="FrontFacesRight"/>: a arte de frente olha para a direita (o normal do 3/4 é olhar para a esquerda);
/// a escolha pelo reflexo espelha quem precisa para que todos olhem para o mesmo lado. <paramref name="BackFacesRight"/>:
/// a arte de costas olha para a direita (o normal; falso quando o rosto vira para a esquerda). Na luta, o grupo olha para
/// a direita e as sombras para a esquerda, e a arte que olha para o outro lado é espelhada (<see cref="AsActor(bool, bool)"/>).
/// <paramref name="DialoguePortrait"/>: o busto da caixa de diálogo (128 px, decisão do João de 06/nov/2026); o
/// <paramref name="Portrait"/> é o retratinho do turno na luta.
/// <paramref name="Reflection"/>: caminho da imagem de frente mostrada no reflexo do rio na escolha (os dois corpos do
/// protagonista-sombra, que não têm arte de batalha de frente); sem ela, o reflexo usa a arte de batalha de frente.
/// <paramref name="InventoryFolder"/>: pasta das imagens do corpo no inventário (protagonista-sombra): <c>etapa0..3.png</c>
/// pela etapa da Marca e <c>rosto_devolvido.png</c>/<c>rosto_coberto.png</c> pela regra C.
/// </summary>
public sealed record CastMember(
    string Id,
    string NameKey,
    CastKind Kind,
    string? Gender,
    float Height,
    string? FrontArt,
    string? BackArt,
    string? Portrait,
    string? MapFolder,
    string? Tint,
    IdleAnimation? Idle,
    bool Selectable,
    bool FrontFacesRight = false,
    string? Reflection = null,
    string? InventoryFolder = null,
    bool BackFacesRight = true,
    string? DialoguePortrait = null)
{
    /// <summary>A arte de batalha para a pose pedida.</summary>
    public string ArtFor(bool back) =>
        (back ? BackArt : FrontArt) ?? throw new InvalidOperationException($"'{Id}' não tem arte de batalha de {(back ? "costas" : "frente")}.");

    /// <summary>Como participante de batalha, na pose pedida: de costas no grupo, de frente no lugar das sombras.</summary>
    public BattleActorArt AsActor(bool back) => AsActor(back, partySide: back);

    /// <summary>
    /// Como participante de batalha, na pose e no lado pedidos. O grupo (embaixo à esquerda) olha para a direita e as
    /// sombras (em cima à direita) olham para a esquerda; a arte que olha para o outro lado sai espelhada.
    /// </summary>
    public BattleActorArt AsActor(bool back, bool partySide) =>
        new(Id, ArtFor(back), Height, FlipH: (back ? BackFacesRight : FrontFacesRight) != partySide, Tint, Portrait,
            Kind == CastKind.Shade ? NameKey : null, Idle);
}
