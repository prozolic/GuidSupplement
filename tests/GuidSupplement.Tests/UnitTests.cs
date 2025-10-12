
using Xunit;
using Shouldly;

namespace GuidSupplement.Tests;


public class GuidSupplementTest
{
    [Fact]
    public void CreateTest()
    {
        var id = GuidVersion7.Create();

        var timeStamp = DateTimeOffset.UtcNow;
        var id2 = GuidVersion7.Create(timeStamp);
    }

    [Fact]
    public void GetTimestampTest()
    {
        var expected = new DateTimeOffset(2025, 10, 12, 19, 56, 28, TimeSpan.Zero);
        var id = GuidVersion7.Create(expected);
        var timestamp = GuidVersion7.GetTimestamp(id);

        timestamp.ToUnixTimeMilliseconds().ShouldBe(expected.ToUnixTimeMilliseconds());
    }

    [Fact]
    public void GetTimestampFailedTest()
    {
        Should.Throw<ArgumentException>(() => GuidVersion7.GetTimestamp(Guid.NewGuid()));
    }

    [Fact]
    public void GetUnixTimeSecondsTest()
    {
        var expected = new DateTimeOffset(2025, 10, 12, 19, 56, 28, TimeSpan.Zero);
        var id = GuidVersion7.Create(expected);
        var timestamp = GuidVersion7.GetUnixTimeSeconds(id);

        timestamp.ShouldBe(expected.ToUnixTimeMilliseconds());
    }

    [Fact]
    public void GetUnixTimeSecondsFailedTest()
    {
        Should.Throw<ArgumentException>(() => GuidVersion7.GetUnixTimeSeconds(Guid.NewGuid()));
    }

    [Fact]
    public void GetVersionTest()
    {
        var expected = 7;
        var id = GuidVersion7.Create();
        var version = GuidVersion7.GetVersion(id);
        version.ShouldBe(expected);
    }


    [Fact]
    public void IsVersion7Test()
    {
        var id = Guid.NewGuid();
        GuidVersion7.IsVersion7(id).ShouldBeFalse();

        var id2 = GuidVersion7.Create();
        GuidVersion7.IsVersion7(id2).ShouldBeTrue();
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