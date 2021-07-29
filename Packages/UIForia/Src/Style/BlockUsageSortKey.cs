using System.Runtime.InteropServices;

namespace UIForia.Style {

    [AssertSize(8)]
    [StructLayout(LayoutKind.Explicit)]
    [AssertSize.SizeOfDependentType(typeof(BlockSourceType), sizeof(byte))]
    [AssertSize.SizeOfDependentType(typeof(StyleState), sizeof(byte))]
    internal struct BlockUsageSortKey {

        // note -- this is assuming little endian
        [FieldOffset(0)] public ulong value;
        [FieldOffset(0)] private byte padding0;
        [FieldOffset(1)] public byte indexInSource; // index of the source, either style, animation, or selector index
        [FieldOffset(2)] public byte localBlockPriority;
        [FieldOffset(3)] public byte localBlockDepth;
        [FieldOffset(4)] public byte distanceToSource;
        [FieldOffset(5)] public BlockSourceType sourceType;
        [FieldOffset(6)] public StyleStateByte stateRequirements;
        [FieldOffset(7)] public byte stateCount;

    }

}