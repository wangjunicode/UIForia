using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace UIForia.Text {

    [StructLayout(LayoutKind.Sequential)]
    public struct BurstCharInfo {

        public int character;
        public int wordIndex;
        
        // maybe dont need to store this
        public float shearTop;
        public float shearBottom;
        
        // can just store a glyph index here
        public float2 topLeft;
        public float2 bottomRight;
        public float2 topLeftUv;
        public float2 bottomRightUv;

    }

}