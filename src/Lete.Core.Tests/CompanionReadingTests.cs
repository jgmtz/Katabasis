using System.Text.Json;
using Lete.Core.Data;

namespace Lete.Core.Tests;

public class CompanionReadingTests
{
    [Fact]
    public void Readings_AreFromKnownCompanions_AndEveryLineExists()
    {
        var root = RepoRoot();
        var companions = Directory.GetFiles(Path.Combine(root, "content", "companions"), "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .ToHashSet();
        using var locale = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "content", "locale", "pt_BR.json")));
        var read = 0;
        foreach (var file in Directory.GetFiles(Path.Combine(root, "content", "shades"), "*.json"))
        {
            foreach (var (companion, key) in ShadeData.ReadingsFromJson(File.ReadAllText(file)))
            {
                Assert.Contains(companion, companions);
                Assert.True(locale.RootElement.TryGetProperty(key, out _), $"{Path.GetFileName(file)}: '{key}' não existe.");
                read++;
            }
        }

        // As sombras de Asfódelos têm a leitura dos seis companheiros (03/nov/2026).
        Assert.True(read >= 24);
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "project.godot")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new DirectoryNotFoundException("project.godot não encontrado.");
    }
}
