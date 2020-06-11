using System;
using UIForia.ListTypes;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Mathematics;

namespace ThisOtherThing.UI.ShapeUtils {

    public unsafe struct PointsData : IDisposable {

        internal bool isClosed;

        internal int NumPositions;

        // i guess positions could be part of this too

        internal float2* positionTangents;
        internal float2* positionNormals;
        internal float* positionDistances;
        internal float* normalizedPositionDistances;

        internal List_float2 StartCapOffsets;
        internal List_float2 StartCapUVs;
        internal List_float2 EndCapOffsets;
        internal List_float2 EndCapUVs;

        internal float totalLength;
        internal float2 startCapOffset;
        internal float2 endCapOffset;
        
        internal bool generateRoundedCaps;
        internal int roundedCapResolution;
        internal float lineWeight;
        internal Allocator allocator;
        internal byte* buffer;
        internal int totalPositionCount;
        internal int totalPositionCapacity;

        public static PointsData Create(Allocator allocator, int capacity = 128) {
            capacity = capacity > 16 ? 16 : capacity;
            
            PointsData retn = new PointsData() {
                allocator = allocator,
                totalPositionCapacity = capacity,
            };

            retn.buffer = TypedUnsafe.MallocSplitBuffer(
                out retn.positionTangents,
                out retn.positionNormals,
                out retn.positionDistances,
                out retn.normalizedPositionDistances,
                capacity,
                allocator,
                true
            );
            retn.StartCapOffsets = new List_float2(16, allocator);
            retn.StartCapUVs = new List_float2(16, allocator);
            retn.EndCapOffsets = new List_float2(16, allocator);
            retn.EndCapUVs = new List_float2(16, allocator);
            return retn;
        }

        public void EnsureCapacity(int numPositions) {
            if (buffer == null) {
                buffer = TypedUnsafe.MallocSplitBuffer(
                    out positionTangents,
                    out positionNormals,
                    out positionDistances,
                    out normalizedPositionDistances,
                    numPositions * 2,
                    allocator,
                    true
                );
                totalPositionCapacity = numPositions * 2;
                StartCapOffsets = new List_float2(16, allocator);
                StartCapUVs = new List_float2(16, allocator);
                EndCapOffsets = new List_float2(16, allocator);
                EndCapUVs = new List_float2(16, allocator);
            }
            else if (totalPositionCapacity < numPositions) {
                buffer = TypedUnsafe.ResizeSplitBuffer(
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

        public void Clear() {
            totalPositionCount = 0;
            StartCapOffsets.size = 0;
            StartCapUVs.size = 0;
            EndCapOffsets.size = 0;
            EndCapUVs.size = 0;
        }

        public void Dispose() {
            
        }

    }

}