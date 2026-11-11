using Lete.Core.Combat;
using Lete.Core.Data;

namespace Lete.Core.Tests;

public class DuelHonorTests
{
    [Fact]
    public void Honor_ParsesAndKnowsWhatIsDishonorable()
    {
        var honor = ShadeData.HonorFromJson("""
            { "honor": { "forbidden": [ "memoria", "eco", "oferenda", "provisao" ], "dishonor_key": "d",
                         "blessing_flag": "bencao", "blessing_key": "b" } }
            """)!;

        Assert.True(honor.IsDishonorable("memoria:colo_da_avo"));
        Assert.True(honor.IsDishonorable("oferenda:mel"));
        // Abrir a página não é usar; atacar e aproximar são as armas do duelo.
        Assert.False(honor.IsDishonorable("memoria"));
        Assert.False(honor.IsDishonorable("atacar"));
        Assert.Equal("bencao", honor.BlessingFlag);
        Assert.Null(honor.BlessingItem);
        Assert.Null(ShadeData.HonorFromJson("{}"));
    }

    [Fact]
    public void Honor_ReadsTheBlessingItem()
    {
        var honor = ShadeData.HonorFromJson("""
            { "honor": { "forbidden": [ "eco" ], "dishonor_key": "d", "blessing_flag": "f", "blessing_key": "b",
                         "blessing_item": "exvoto" } }
            """)!;

        Assert.Equal("exvoto", honor.BlessingItem);
    }
}
