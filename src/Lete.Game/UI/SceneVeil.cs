using System;
using System.Threading.Tasks;
using Godot;

namespace Lete.Game.UI;

/// <summary>
/// O véu de névoa das trocas de cena (do mapa para a luta e de volta): fecha antes de trocar e abre quando a cena nova
/// está pronta. Vive na raiz da árvore, fora das cenas, para atravessar a troca. Sem véu fechado, abrir não faz nada.
/// </summary>
public static class SceneVeil
{
    private const float CloseSeconds = 0.45f;
    private const float OpenSeconds = 0.6f;

    /// <summary>Quadros a esperar antes de abrir (a cena nova monta peças, luzes e o primeiro quadro da câmera).</summary>
    private const int SettleFrames = 4;

    private static readonly Shader VeilShader = GD.Load<Shader>("res://src/Lete.Game/UI/veu_transicao.gdshader");

    private static CanvasLayer? _layer;
    private static ShaderMaterial? _material;
    private static bool _closed;

    /// <summary>Fecha o véu e, fechado, chama <paramref name="then"/> (a troca de cena).</summary>
    public static void CloseThen(SceneTree tree, Action then) => _ = CloseThenAsync(tree, then);

    /// <summary>Abre o véu, se ele estiver fechado, depois que a cena nova assentar.</summary>
    public static void Lift(SceneTree tree) => _ = LiftAsync(tree);

    private static async Task CloseThenAsync(SceneTree tree, Action then)
    {
        var material = Ensure(tree);
        _closed = true;
        var tween = tree.CreateTween();
        tween.TweenMethod(Callable.From<float>(v => material.SetShaderParameter("amount", v)), 0f, 1f, CloseSeconds)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
        await tween.ToSignal(tween, Tween.SignalName.Finished);
        then();
    }

    private static async Task LiftAsync(SceneTree tree)
    {
        if (!_closed || _material is not { } material)
        {
            return;
        }

        _closed = false;
        for (var i = 0; i < SettleFrames; i++)
        {
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        }

        var tween = tree.CreateTween();
        tween.TweenMethod(Callable.From<float>(v => material.SetShaderParameter("amount", v)), 1f, 0f, OpenSeconds)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
    }

    private static ShaderMaterial Ensure(SceneTree tree)
    {
        if (_layer is not null && GodotObject.IsInstanceValid(_layer) && _material is not null)
        {
            return _material;
        }

        _material = new ShaderMaterial { Shader = VeilShader };
        _material.SetShaderParameter("amount", 0f);
        _layer = new CanvasLayer { Name = "SceneVeil", Layer = 120 };
        var rect = new ColorRect { Material = _material, MouseFilter = Control.MouseFilterEnum.Ignore };
        rect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _layer.AddChild(rect);
        tree.Root.AddChild(_layer);
        return _material;
    }
}
