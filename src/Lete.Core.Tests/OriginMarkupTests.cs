using Lete.Core.Localization;
using Lete.Core.State;

namespace Lete.Core.Tests;

public class OriginMarkupTests
{
    private static readonly Localizer Text = new(new Dictionary<string, string>
    {
        ["convite"] = "Você chamou {origem:pessoa}. {origem:frase}",
        ["pessoa"] = "a outra pessoa",
        ["pessoa.pescador"] = "o parceiro de barco",
        ["frase"] = "Ninguém viu.",
        ["frase.pescador"] = "A rede ficou no porto, molhad{o|a}.",
    });

    [Fact]
    public void Resolve_UsesTheOriginVariant_ThenGender()
    {
        Assert.Equal("Você chamou o parceiro de barco. A rede ficou no porto, molhada.",
            Text.Get("convite", Gender.Feminine, "pescador"));
    }

    [Fact]
    public void Resolve_FallsBackToTheStandardForm_WithoutVariantOrOrigin()
    {
        Assert.Equal("Você chamou a outra pessoa. Ninguém viu.", Text.Get("convite", Gender.Masculine, "artesao"));
        Assert.Equal("Você chamou a outra pessoa. Ninguém viu.", Text.Get("convite", Gender.Masculine));
    }

    [Fact]
    public void Validator_AcceptsTheMarkup_AndFlagsAMissingStandardForm()
    {
        Assert.Empty(LocaleValidator.Validate(Text.Strings));
        var problems = LocaleValidator.Validate(new Dictionary<string, string> { ["x"] = "Olá, {origem:nome}." }).ToList();
        Assert.Single(problems);
        Assert.Contains("nome", problems[0]);
    }
}
