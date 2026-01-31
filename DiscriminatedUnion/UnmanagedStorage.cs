using System.Runtime.InteropServices;

namespace NickStrupat;

[StructLayout(LayoutKind.Sequential, Size = Size)]
internal struct UnmanagedStorage(Byte index)
{
    private const Int32 Size = 16;
    public Byte _0 = index;

#if DEBUG
    public Span<Byte> Span => MemoryMarshal.CreateSpan(ref _0, Size);
#endif
}