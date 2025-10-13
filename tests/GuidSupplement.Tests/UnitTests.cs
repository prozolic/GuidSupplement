
using Shouldly;
using Xunit;

namespace GuidSupplement.Tests;

public class GuidSupplementTest
{
    [Fact]
    public void CreateTest()
    {
        var id = GuidVersion7.Create();
        var id2 = GuidVersion7.Create(DateTimeOffset.UtcNow);
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
        {
            var id = Guid.NewGuid();
            id.GetVersion().ShouldBe(4);
        }

        {
            var id = GuidVersion7.Create();
            id.GetVersion().ShouldBe(7);
        }
    }

    [Fact]
    public void GetVariantTest()
    {
        {
            var id = Guid.NewGuid();
            var variant = id.GetVariant();
        }

        {
            var id = GuidVersion7.Create();
            var variant = id.GetVariant();
        }
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

    [Fact]
    public void TryWriteBytesTest()
    {
        var id = new Guid("00112233-4455-6677-8899-aabbccddeeff");

        Span<byte> value = stackalloc byte[16];
        id.TryWriteBytes(value).ShouldBeTrue();
        value.ToArray().ShouldBe([51, 34, 17, 0, 85, 68, 119, 102, 136, 153, 170, 187, 204, 221, 238, 255]);

        Span<byte> value2 = stackalloc byte[16];
        id.TryWriteBytes(value2, false, out _).ShouldBeTrue();
        value2.ToArray().ShouldBe([51, 34, 17, 0, 85, 68, 119, 102, 136, 153, 170, 187, 204, 221, 238, 255]);

        Span<byte> value3 = stackalloc byte[16];
        id.TryWriteBytes(value3, true, out _).ShouldBeTrue();
        value3.ToArray().ShouldBe([0, 17, 34, 51, 68, 85, 102, 119, 136, 153, 170, 187, 204, 221, 238, 255]);

    }
}