
using Xunit;
using Shouldly;

namespace GuidSupplement.Tests;


public class GuidSupplementTest
{
    [Fact]
    public void CreateTest()
    {
        var id = GuidVersion7.Create();
        var timestamp = GuidVersion7.GetTimestamp(id);
    }

    [Fact]
    public async Task TimestampComparerTest()
    {
        var ids = new List<Guid>(100);
        for (int i = 0; i < 100; i++)
        {
            ids.Add(GuidVersion7.Create(new DateTimeOffset(DateTime.Now)));
            await Task.Delay(1, TestContext.Current.CancellationToken);
        }

        var shuffled = ids.OrderBy(_ => Guid.NewGuid()).ToList();
        ids.SequenceEqual(shuffled).ShouldBeFalse();

        shuffled.Sort(GuidVersion7.TimestampComparer);
        ids.SequenceEqual(shuffled).ShouldBeTrue();
    }

}