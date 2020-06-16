using System;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Mathematics;

namespace ThisOtherThing.UI.ShapeUtils {

    public unsafe struct PointsData : IDisposable {

        internal bool isClosed;
        internal bool generateRoundedCaps;

        internal float2* positionTangents;
        internal float2* positionNormals;
        internal float* positionDistances;
        internal float* normalizedPositionDistances;
        internal float2* StartCapOffsets;
        internal float2* StartCapUVs;
        internal float2* EndCapOffsets;
        internal float2* EndCapUVs;

        internal float2 startCapOffset;
        internal float2 endCapOffset;

        internal float lineWeight;
        internal Allocator allocator;

        internal float totalLength;
        internal int totalCapCount;
        internal int totalPositionCount;
        internal int totalPositionCapacity;
        internal int totalCapCapacity;
        
        public static PointsData Create(Allocator allocator, int capacity = 128) {
            capacity = capacity > 16 ? 16 : capacity;

            PointsData retn = new PointsData() {
                allocator = allocator,
                totalPositionCapacity = capacity,
            };

            TypedUnsafe.MallocSplitBuffer(
                out retn.positionTangents,
                out retn.positionNormals,
                out retn.positionDistances,
                out retn.normalizedPositionDistances,
                capacity,
                allocator,
                true
            );
            
            return retn;
        }

        internal byte* positionBuffer {
            get => (byte*)positionTangents;
        }

        private byte* capBuffer {
            get => (byte*) StartCapOffsets;
        }
        
        public void EnsurePositionCapacity(int numPositions) {
            if (positionBuffer == null) {
                TypedUnsafe.MallocSplitBuffer(
                    out positionTangents,
                    out positionNormals,
                    out positionDistances,
                    out normalizedPositionDistances,
                    numPositions * 2,
                    allocator,
                    true
                );
                totalPositionCapacity = numPositions * 2;
            }
            else if (totalPositionCapacity < numPositions) {
                TypedUnsafe.ResizeSplitBuffer(
                    ref positionTangents,
                    ref positionNormals,
                    ref positionDistances,
                    ref normalizedPositionDistances,
                    totalPositionCapacity,
                    numPositions * 2,
                    allocator,
                    true
                );
                totalPositionCapacity = numPositions * 2;
            }
        }

        public void EnsureCapCapacity(int capacity) {
            if (capBuffer == null) {
                TypedUnsafe.MallocSplitBuffer(
                    out StartCapOffsets,
                    out StartCapUVs,
                    out EndCapOffsets,
                    out EndCapUVs,
                    capacity,
                    allocator
                );
                totalCapCapacity = capacity;
            }
            else if (totalCapCapacity < capacity) {
                TypedUnsafe.ResizeSplitBuffer(
                    ref positionTangents,
                    ref positionNormals,
                    ref positionDistances,
                    ref normalizedPositionDistances,
                    totalCapCapacity,
                    capacity * 2,
                    allocator
                );
                totalCapCapacity = capacity * 2;
            }
        }
        
        public void Clear() {
            totalPositionCount = 0;
            totalCapCount = 0;
            startCapOffset = 0;
            endCapOffset = 0;
            lineWeight = 1f;
        }

        public void Dispose() {
            TypedUnsafe.Dispose(positionBuffer, allocator);
            TypedUnsafe.Dispose(capBuffer, allocator);
            this = default;
        }


    }

}