
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace GuidSupplement;

public static class GuidVersion7
{
    public static IComparer<Guid> TimestampComparer => TimestampGuidComparer.Instance;

    public static Guid Create() => Create(DateTimeOffset.UtcNow);

#if !NET9_0_OR_GREATER

    // =============================================================================
    // The following source code is based on the implementation of Guid.CreateVersion7() in dotnet/runtime.
    // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Guid.cs#L297
    // =============================================================================

    private const byte Variant10xxMask = 0xC0;
    private const byte Variant10xxValue = 0x80;
    private const ushort VersionMask = 0xF000;
    private const ushort Version7Value = 0x7000;

    public static Guid Create(DateTimeOffset timestamp)
    {
        // NewGuid uses CoCreateGuid on Windows and Interop.GetCryptographicallySecureRandomBytes on Unix to get
        // cryptographically-secure random bytes. We could use Interop.BCrypt.BCryptGenRandom to generate the random
        // bytes on Windows, as is done in RandomNumberGenerator, but that's measurably slower than using CoCreateGuid.
        // And while CoCreateGuid only generates 122 bits of randomness, the other 6 bits being for the version / variant
        // fields, this method also needs those bits to be non-random, so we can just use NewGuid for efficiency.
        Guid result = Guid.NewGuid();

        // 2^48 is roughly 8925.5 years, which from the Unix Epoch means we won't
        // overflow until around July of 10,895. So there isn't any need to handle
        // it given that DateTimeOffset.MaxValue is December 31, 9999. However, we
        // can't represent timestamps prior to the Unix Epoch since UUIDv7 explicitly
        // stores a 48-bit unsigned value, so we do need to throw if one is passed in.

        long unix_ts_ms = timestamp.ToUnixTimeMilliseconds();

#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNegative(unix_ts_ms, nameof(timestamp));
#else
        if (unix_ts_ms < 0)
        {
            ThrowIfNegative(unix_ts_ms, nameof(timestamp));
        }
#endif

        // Guid._a, Guid._b, Guid._c, Guid._d are accessed via Unsafe to avoid the overhead.
        // The fields are defined with the following layout.
        // =============================================================================
        // private readonly int _a;   // Do not rename (binary serialization)
        // private readonly short _b; // Do not rename (binary serialization)
        // private readonly short _c; // Do not rename (binary serialization)
        // private readonly byte _d;  // Do not rename (binary serialization)
        // =============================================================================

        ref var resultRef = ref Unsafe.AsRef(in result);

        // Guid._a short field.
        Unsafe.As<Guid, int>(ref resultRef) = (int)(unix_ts_ms >> 16);

        // Guid._b short field.
        Unsafe.Add(ref Unsafe.As<Guid, short>(ref resultRef), 2) = (short)unix_ts_ms;

        // Guid._c short field.
        ref var c = ref Unsafe.Add(ref Unsafe.As<Guid, short>(ref resultRef), 3);
        c = (short)((c & ~VersionMask) | Version7Value);

        // Guid._d byte field.
        ref var d = ref Unsafe.Add(ref Unsafe.As<Guid, byte>(ref resultRef), 8);
        d = (byte)((d & ~Variant10xxMask) | Variant10xxValue);

        return result;
    }
#else
    public static Guid Create(DateTimeOffset timestamp) => Guid.CreateVersion7(timestamp);
#endif

    public static bool IsVersion7(Guid guid)
    {
#if NET9_0_OR_GREATER
        return guid.Version
#else
        return guid.GetVersion()
#endif
        == 7;
    }

    public static DateTimeOffset GetTimestamp(Guid guid)
    {
        if (!IsVersion7(guid))
        {
            ThrowIfNotVersion7Guid(guid);
        }

        return DateTimeOffset.FromUnixTimeMilliseconds(GetUnixTimeSecondsCore(guid));
    }

    public static long GetUnixTimeSeconds(Guid guid)
    {
        if (!IsVersion7(guid))
        {
            ThrowIfNotVersion7Guid(guid);
        }

        return GetUnixTimeSecondsCore(guid);
    }

    private static long GetUnixTimeSecondsCore(Guid guid)
    {
        // Timestamp is 48-bit big-endian unsigned number.
        // But .NET GUID is little-endian.
        // Guid._a(uint) + Guid._b(ushort)

        ref var ptr = ref Unsafe.As<Guid, byte>(ref Unsafe.AsRef(in guid));
        var lower = ((long)Unsafe.ReadUnaligned<uint>(ref ptr)) << 16;
        var upper = (long)Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref ptr, 4));
        return upper | lower;
    }

    [DoesNotReturn]
    private static void ThrowIfNotVersion7Guid(Guid guid) => throw new ArgumentException("Value is not a version 7 Guid.");

    [DoesNotReturn]
    private static void ThrowIfNegative(long unixTimeMilliseconds, string paramName) => throw new ArgumentOutOfRangeException(paramName, unixTimeMilliseconds, $"{paramName} ('{unixTimeMilliseconds}') must be a non-negative value.");

    private sealed class TimestampGuidComparer : IComparer<Guid>
    {
        public static readonly TimestampGuidComparer Instance = new();

        public int Compare(Guid x, Guid y)
        {
            if (IsVersion7(x) && IsVersion7(y))
            {
                var xTimestamp = GetUnixTimeSecondsCore(x);
                var yTimestamp = GetUnixTimeSecondsCore(y);
                return xTimestamp.CompareTo(yTimestamp);
            }

            return 0;
        }
    }
}