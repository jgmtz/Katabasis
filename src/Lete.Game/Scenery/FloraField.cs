using System;
using Godot;

namespace Lete.Game.Scenery;

/// <summary>
/// Um campo de flora em cartões (asfódelos, capim), espalhado numa área retangular centrada no nó: milhares de
/// cartões numa malha só (MultiMesh), em moitas (não em grade), fora dos retângulos de <see cref="Avoid"/> (caminhos,
/// casas). O shader vira cada cartão para a câmera e o balança ao vento.
/// </summary>
public partial class FloraField : Node3D
{
    /// <summary>Pixels de arte por metro (o mesmo dos personagens: 56 px para 1,7 m).</summary>
    public const float TexelDensity = 56f / 1.7f;

    /// <summary>O nome da folha de variantes em <see cref="SceneryMaterials.Folder"/> (asfodelo, capim).</summary>
    [Export] public string Kind { get; set; } = "asfodelo";

    /// <summary>Largura (X) e profundidade (Z) da área, em metros.</summary>
    [Export] public Vector2 Area { get; set; } = new(10f, 10f);

    [Export] public int Count { get; set; } = 400;

    [Export] public int Seed { get; set; }

    /// <summary>Quantas variantes a folha tem, lado a lado.</summary>
    [Export] public int Variants { get; set; } = 4;

    /// <summary>0 = espalhado por igual; 1 = só em moitas, com clareiras entre elas.</summary>
    [Export] public float Clump { get; set; } = 0.6f;

    /// <summary>Retângulos sem flora, no plano do mundo (x, z, largura, profundidade).</summary>
    [Export] public Godot.Collections.Array<Rect2> Avoid { get; set; } = new();

    public override void _Ready()
    {
        var texture = SceneryMaterials.Texture(Kind);
        var size = new Vector2(texture.GetWidth() / (float)Variants, texture.GetHeight()) / TexelDensity;
        var material = SceneryMaterials.Own("flora");
        material.SetShaderParameter("folha", texture);
        material.SetShaderParameter("variantes", (float)Variants);
        material.SetShaderParameter("tamanho", size);

        var rng = new Random(Seed);
        var origin = GlobalPosition;
        var spots = new System.Collections.Generic.List<(Vector3 At, float Variant, float Scale, float Phase)>();
        for (var tries = 0; tries < Count * 6 && spots.Count < Count; tries++)
        {
            var x = ((float)rng.NextDouble() - 0.5f) * Area.X;
            var z = ((float)rng.NextDouble() - 0.5f) * Area.Y;
            var world = new Vector2(origin.X + x, origin.Z + z);
            if (Patch(world) < Clump * (float)rng.NextDouble() || Blocked(world))
            {
                continue;
            }

            spots.Add((new Vector3(x, 0, z), rng.Next(Variants), 0.8f + (float)rng.NextDouble() * 0.4f, (float)rng.NextDouble()));
        }

        var multi = new MultiMesh
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            UseCustomData = true,
            Mesh = new QuadMesh { Size = Vector2.One },
            InstanceCount = spots.Count,
        };
        for (var i = 0; i < spots.Count; i++)
        {
            var (at, variant, scale, phase) = spots[i];
            multi.SetInstanceTransform(i, new Transform3D(Basis.Identity, at));
            multi.SetInstanceCustomData(i, new Color(variant, scale, phase, 0f));
        }

        AddChild(new MultiMeshInstance3D
        {
            Name = "Flora",
            Multimesh = multi,
            MaterialOverride = material,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            // O shader move os vértices: a caixa de corte cobre a área inteira, até a altura da flora.
            CustomAabb = new Aabb(new Vector3(-Area.X / 2f - 1f, -0.5f, -Area.Y / 2f - 1f), new Vector3(Area.X + 2f, 3f, Area.Y + 2f)),
        });
    }

    /// <summary>Um desenho suave de manchas (0 a 1) no plano do mundo: onde é alto, há moita.</summary>
    private static float Patch(Vector2 p) =>
        0.5f + 0.25f * Mathf.Sin(p.X * 0.31f + 1.7f) * Mathf.Sin(p.Y * 0.27f + 0.4f)
        + 0.25f * Mathf.Sin(p.X * 0.83f + p.Y * 0.61f + 2.1f);

    private bool Blocked(Vector2 world)
    {
        foreach (var rect in Avoid)
        {
            if (rect.HasPoint(world))
            {
                return true;
            }
        }

        return false;
    }
}
