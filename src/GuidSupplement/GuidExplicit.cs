using System.Runtime.InteropServices;

namespace GuidSupplement;

[StructLayout(LayoutKind.Explicit)]
internal struct GuidExplicit(Guid value)
{
    [FieldOffset(0)] public Guid Value = value;

    [FieldOffset(0)] public int a;
    [FieldOffset(4)] public short b;
    [FieldOffset(6)] public short c;
    [FieldOffset(8)] public byte d;
    [FieldOffset(9)] public byte e;
    [FieldOffset(10)] public byte f;
    [FieldOffset(11)] public byte g;
    [FieldOffset(12)] public byte h;
    [FieldOffset(13)] public byte i;
    [FieldOffset(14)] public byte j;
    [FieldOffset(15)] public byte k;
}
