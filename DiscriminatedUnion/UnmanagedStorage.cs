using System.Runtime.InteropServices;

namespace NickStrupat;

[StructLayout(LayoutKind.Sequential, Size = Size)]
internal struct UnmanagedStorage
{
    private const Int32 Size = 16;
    public Span<Byte> Span => MemoryMarshal.CreateSpan(ref _0, Size);

    public Byte _0;
    // public Byte _1;
    // public Byte _2;
    // public Byte _3;
    // public Byte _4;
    // public Byte _5;
    // public Byte _6;
    // public Byte _7;
    // public Byte _8;
    // public Byte _9;
    // public Byte _10;
    // public Byte _11;
    // public Byte _12;
    // public Byte _13;
    // public Byte _14;
    // public Byte _15;
}