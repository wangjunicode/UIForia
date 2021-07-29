using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia {

    // Generated from SoA Generator, so not edit by hand!
    internal unsafe partial struct PerFrameLayoutOutput {

        
        private int SoA_Capacity_PerActiveElementId;
        
        public void SoA_Initialize_PerActiveElementId(int requiredElementCount) {
            requiredElementCount = requiredElementCount < 32 ? 32 : requiredElementCount;
            requiredElementCount += (int)(requiredElementCount * 0.5f);
            int sizeForOneElement = sizeof(UIForia.Size) + sizeof(UIForia.Util.OffsetRect) + sizeof(UIForia.Util.OffsetRect) + sizeof(Unity.Mathematics.float2) + sizeof(UIForia.Layout.OrientedBounds) + sizeof(Unity.Mathematics.float4x4) + sizeof(Unity.Mathematics.float4x4) + sizeof(ushort) + sizeof(UIForia.Layout.ClipInfo);
            long totalByteSize = sizeForOneElement * requiredElementCount;
            void * __buffer__ = UnsafeUtility.Malloc(totalByteSize, UnsafeUtility.AlignOf<long>(), Allocator.Persistent);
            UnsafeUtility.MemClear(__buffer__, totalByteSize);
            sizes = new CheckedArray<UIForia.Size>((UIForia.Size*)__buffer__, requiredElementCount);
            borders = new CheckedArray<UIForia.Util.OffsetRect>((UIForia.Util.OffsetRect*)(sizes.array + requiredElementCount), requiredElementCount);
            paddings = new CheckedArray<UIForia.Util.OffsetRect>((UIForia.Util.OffsetRect*)(borders.array + requiredElementCount), requiredElementCount);
            localPositions = new CheckedArray<Unity.Mathematics.float2>((Unity.Mathematics.float2*)(paddings.array + requiredElementCount), requiredElementCount);
            bounds = new CheckedArray<UIForia.Layout.OrientedBounds>((UIForia.Layout.OrientedBounds*)(localPositions.array + requiredElementCount), requiredElementCount);
            localMatrices = new CheckedArray<Unity.Mathematics.float4x4>((Unity.Mathematics.float4x4*)(bounds.array + requiredElementCount), requiredElementCount);
            worldMatrices = new CheckedArray<Unity.Mathematics.float4x4>((Unity.Mathematics.float4x4*)(localMatrices.array + requiredElementCount), requiredElementCount);
            clipperIndex = new CheckedArray<ushort>((ushort*)(worldMatrices.array + requiredElementCount), requiredElementCount);
            clipInfos = new CheckedArray<UIForia.Layout.ClipInfo>((UIForia.Layout.ClipInfo*)(clipperIndex.array + requiredElementCount), requiredElementCount);
            SoA_Capacity_PerActiveElementId = requiredElementCount;
        }
        
        public void SoA_SetSize_PerActiveElementId(int requiredElementCount, bool copyPrevContents) {
            if (requiredElementCount > SoA_Capacity_PerActiveElementId) {
                SoA_EnsureCapacity_PerActiveElementId(requiredElementCount, copyPrevContents);
            }
            sizes = new CheckedArray<UIForia.Size>(sizes.array, requiredElementCount);
            borders = new CheckedArray<UIForia.Util.OffsetRect>(borders.array, requiredElementCount);
            paddings = new CheckedArray<UIForia.Util.OffsetRect>(paddings.array, requiredElementCount);
            localPositions = new CheckedArray<Unity.Mathematics.float2>(localPositions.array, requiredElementCount);
            bounds = new CheckedArray<UIForia.Layout.OrientedBounds>(bounds.array, requiredElementCount);
            localMatrices = new CheckedArray<Unity.Mathematics.float4x4>(localMatrices.array, requiredElementCount);
            worldMatrices = new CheckedArray<Unity.Mathematics.float4x4>(worldMatrices.array, requiredElementCount);
            clipperIndex = new CheckedArray<ushort>(clipperIndex.array, requiredElementCount);
            clipInfos = new CheckedArray<UIForia.Layout.ClipInfo>(clipInfos.array, requiredElementCount);
        }
        
        public void SoA_EnsureCapacity_PerActiveElementId(int requiredElementCount, bool copyPrevContents) {
            if (requiredElementCount < SoA_Capacity_PerActiveElementId) return;
            UIForia.Size* old_sizes = sizes.array;
            UIForia.Util.OffsetRect* old_borders = borders.array;
            UIForia.Util.OffsetRect* old_paddings = paddings.array;
            Unity.Mathematics.float2* old_localPositions = localPositions.array;
            UIForia.Layout.OrientedBounds* old_bounds = bounds.array;
            Unity.Mathematics.float4x4* old_localMatrices = localMatrices.array;
            Unity.Mathematics.float4x4* old_worldMatrices = worldMatrices.array;
            ushort* old_clipperIndex = clipperIndex.array;
            UIForia.Layout.ClipInfo* old_clipInfos = clipInfos.array;
            
            int prevCapacity = SoA_Capacity_PerActiveElementId;
            SoA_Initialize_PerActiveElementId(requiredElementCount);
            
            if (prevCapacity > 0 && copyPrevContents) {
                UnsafeUtility.MemCpy(sizes.array, old_sizes, sizeof(UIForia.Size) * prevCapacity);
                UnsafeUtility.MemCpy(borders.array, old_borders, sizeof(UIForia.Util.OffsetRect) * prevCapacity);
                UnsafeUtility.MemCpy(paddings.array, old_paddings, sizeof(UIForia.Util.OffsetRect) * prevCapacity);
                UnsafeUtility.MemCpy(localPositions.array, old_localPositions, sizeof(Unity.Mathematics.float2) * prevCapacity);
                UnsafeUtility.MemCpy(bounds.array, old_bounds, sizeof(UIForia.Layout.OrientedBounds) * prevCapacity);
                UnsafeUtility.MemCpy(localMatrices.array, old_localMatrices, sizeof(Unity.Mathematics.float4x4) * prevCapacity);
                UnsafeUtility.MemCpy(worldMatrices.array, old_worldMatrices, sizeof(Unity.Mathematics.float4x4) * prevCapacity);
                UnsafeUtility.MemCpy(clipperIndex.array, old_clipperIndex, sizeof(System.UInt16) * prevCapacity);
                UnsafeUtility.MemCpy(clipInfos.array, old_clipInfos, sizeof(UIForia.Layout.ClipInfo) * prevCapacity);
            }

            UnsafeUtility.Free(old_sizes, Allocator.Persistent);
        }
        
        public void SoA_Clear_PerActiveElementId(int requiredElementCount) {
            int sizeForOneElement = sizeof(UIForia.Size) + sizeof(UIForia.Util.OffsetRect) + sizeof(UIForia.Util.OffsetRect) + sizeof(Unity.Mathematics.float2) + sizeof(UIForia.Layout.OrientedBounds) + sizeof(Unity.Mathematics.float4x4) + sizeof(Unity.Mathematics.float4x4) + sizeof(ushort) + sizeof(UIForia.Layout.ClipInfo);
            UnsafeUtility.MemClear(sizes.array, requiredElementCount * sizeForOneElement);
        }
        
        public void SoA_Dispose_PerActiveElementId() {
            if (sizes.array == null) return;
            UnsafeUtility.Free(sizes.array, Allocator.Persistent);
            sizes = default;
            borders = default;
            paddings = default;
            localPositions = default;
            bounds = default;
            localMatrices = default;
            worldMatrices = default;
            clipperIndex = default;
            clipInfos = default;
        }

        
        public void SoA_Dispose() {
            SoA_Dispose_PerActiveElementId();
        }
    }

}
