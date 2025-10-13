
using System.Runtime.CompilerServices;

namespace GuidSupplement;

public static class GuidExtensions
{
    public static int GetVersion(this Guid guid)
    {
#if NET9_0_OR_GREATER
        return guid.Version;
#else
        // Guid._c include the version field.
        // XXXXXXXX-XXXX-VXXX-XX-XX-XX-XX-XX-XX-XX-XX
        // int-short-short-byte-byte-byte-byte-byte-byte-byte-byte

        var c = Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref Unsafe.As<Guid, byte>(ref Unsafe.AsRef(in guid)), 6));
        return c >>> 12;
#endif
    }

    public static int GetVariant(this Guid guid)
    {
#if NET9_0_OR_GREATER
        return guid.Variant;
#else
        // Guid._d include the variant field.
        // XXXXXXXX-XXXX-XXXX-XX-XX-XX-XX-XX-XX-XX-XX
        // int-short-short-byte-byte-byte-byte-byte-byte-byte-byte

        var d = Unsafe.ReadUnaligned<byte>(ref Unsafe.Add(ref Unsafe.As<Guid, byte>(ref Unsafe.AsRef(in guid)), 8));
        return d >> 4;
#endif
    }

}