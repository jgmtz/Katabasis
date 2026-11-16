using Godot;

namespace Lete.Game.UI;

/// <summary>
/// Uma aba do menu do jogo (<see cref="GameMenu"/>). Cada aba tem um atalho próprio no Input Map que abre o menu
/// direto nela; dentro do menu, as abas se alternam entre si. Cada aba é um objeto do mundo (decisão do João,
/// 13/nov/2026): tem o seu fundo de tela cheia e ocupa um retângulo dele.
/// </summary>
public interface IMenuTab
{
    /// <summary>Chave de localização do nome da aba.</summary>
    string TitleKey { get; }

    /// <summary>Ação do Input Map que abre o menu nesta aba (ex.: <c>journal</c>, <c>inventory</c>).</summary>
    string ShortcutAction { get; }

    /// <summary>O fundo da aba (640×360 em pixels da arte): a tabuinha de cera, o pergaminho, o piso, o pano.</summary>
    string BackgroundPath { get; }

    /// <summary>Onde o conteúdo fica sobre o fundo, em pixels da arte.</summary>
    Rect2I ContentArea { get; }

    Control View { get; }

    /// <summary>Redesenha com o estado atual (chamado ao mostrar a aba e ao trocar textos ou o estado).</summary>
    void Refresh();
}
