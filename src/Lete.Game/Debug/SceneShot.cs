using System.Globalization;
using System.Linq;
using Godot;

namespace Lete.Game.Debug;

/// <summary>
/// Conferência da tela de título e da abertura sem jogar à mão: <c>-- --foto=arquivo.png --foto-em=segundos</c> salva
/// a janela nesse instante e fecha o jogo (as fotos do mapa e da luta são o <c>--shot</c> das cenas delas).
/// </summary>
public static class SceneShot
{
    private const string PathArg = "--foto=";
    private const string TimeArg = "--foto-em=";

    /// <summary>Segundos de espera quando <c>--foto-em</c> não vem.</summary>
    private const float DefaultSeconds = 2f;

    /// <summary>Agenda a foto, se a linha de comando pediu; a cena chama no <c>_Ready</c>.</summary>
    public static async void Schedule(Node scene)
    {
        var args = OS.GetCmdlineUserArgs();
        if (args.FirstOrDefault(a => a.StartsWith(PathArg)) is not { } pathArg)
        {
            return;
        }

        var seconds = args.FirstOrDefault(a => a.StartsWith(TimeArg)) is { } timeArg
            ? float.Parse(timeArg[TimeArg.Length..], CultureInfo.InvariantCulture)
            : DefaultSeconds;
        await scene.ToSignal(scene.GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
        await scene.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        var path = pathArg[PathArg.Length..];
        var error = scene.GetViewport().GetTexture().GetImage().SavePng(path);
        GD.Print($"foto: {path} ({error})");
        scene.GetTree().Quit();
    }
}
