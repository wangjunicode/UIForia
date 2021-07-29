using UIForia.Layout;
using UIForia.Util;
using Unity.Mathematics;

namespace UIForia {

    internal partial struct PerFrameLayoutOutput {

        [SoAGeneratorGroup("PerActiveElementId")]
        public CheckedArray<Size> sizes;

        [SoAGeneratorGroup("PerActiveElementId")]
        public CheckedArray<OffsetRect> borders; // maybe merge w/ paddings

        [SoAGeneratorGroup("PerActiveElementId")]
        public CheckedArray<OffsetRect> paddings;

        [SoAGeneratorGroup("PerActiveElementId")]
        public CheckedArray<float2> localPositions;

        [SoAGeneratorGroup("PerActiveElementId")]
        public CheckedArray<OrientedBounds> bounds;

        [SoAGeneratorGroup("PerActiveElementId")]
        public CheckedArray<float4x4> localMatrices;

        [SoAGeneratorGroup("PerActiveElementId")]
        public CheckedArray<float4x4> worldMatrices;

        [SoAGeneratorGroup("PerActiveElementId")]
        public CheckedArray<ushort> clipperIndex;

        [SoAGeneratorGroup("PerActiveElementId")]
        public CheckedArray<ClipInfo> clipInfos;

        // bump allocated!
        public CheckedArray<Clipper> clippers;

        // bump allocated!
        public CheckedArray<InputQueryResult> mouseQueryResults;

    }

}