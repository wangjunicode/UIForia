using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace UIForia.Text {

    [StructLayout(LayoutKind.Sequential)]
    public struct BurstCharInfo {

        public int character;
        public float shearTop;
        public float shearBottom;
        public float2 topLeft;
        public float2 bottomRight;
        public float2 topLeftUv;
        public float2 bottomRightUv;
        public float advanceWidth;

    }

}