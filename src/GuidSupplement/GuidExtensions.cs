
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GuidSupplement;

public static class GuidExtensions
{
#if NETSTANDARD2_0

    public static bool TryWriteBytes(this Guid guid, Span<byte> destination)
    {
        // =============================================================================
        // The following source code is based on the implementation of Guid.TryWriteBytes(Span<byte> destination) in dotnet/runtime.
        // https://github.com/prozolic/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Guid.cs#L1024
        // =============================================================================

        if (destination.Length < 16)
        {
            return false;
        }

        if (BitConverter.IsLittleEndian)
        {
            MemoryMarshal.Write(destination, ref guid);
        }
        else
        {
            var guidExplicit = new GuidExplicit(guid);
            if (!BitConverter.IsLittleEndian)
            {
                guidExplicit.a = BinaryPrimitives.ReverseEndianness(guidExplicit.a);
                guidExplicit.b = BinaryPrimitives.ReverseEndianness(guidExplicit.b);
                guidExplicit.c = BinaryPrimitives.ReverseEndianness(guidExplicit.c);
            }

            var value = guidExplicit.Value;
            MemoryMarshal.Write(destination, ref value);
        }
        return true;
    }

#endif

#if !NET8_0_OR_GREATER

    public static bool TryWriteBytes(this Guid guid, Span<byte> destination, bool bigEndian, out int bytesWritten)
    {
        // =============================================================================
        // The following source code is based on the implementation of Guid.TryWriteBytes(Span<byte> destination, bool bigEndian, out int bytesWritten) in dotnet/runtime.
        // https://github.com/prozolic/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Guid.cs#L1043
        // =============================================================================

        if (destination.Length < 16)
        {
            Unsafe.SkipInit(out bytesWritten);
            return false;
        }

        if (BitConverter.IsLittleEndian != bigEndian)
        {
            MemoryMarshal.Write(destination, ref guid);
        }
        else
        {
            var guidExplicit = new GuidExplicit(guid);
            if (BitConverter.IsLittleEndian == bigEndian)
            {
                guidExplicit.a = BinaryPrimitives.ReverseEndianness(guidExplicit.a);
                guidExplicit.b = BinaryPrimitives.ReverseEndianness(guidExplicit.b);
                guidExplicit.c = BinaryPrimitives.ReverseEndianness(guidExplicit.c);
            }

            var value = guidExplicit.Value;
            MemoryMarshal.Write(destination, ref value);
        }
        bytesWritten = 16;
        return true;
    }

#endif

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