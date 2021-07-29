using UIForia.Style;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UIForia.Layout {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct ComputeClipping : IJob {

        public float screenWidth;
        public float screenHeight;
  
        [NativeDisableUnsafePtrRestriction] public StyleTables* styleTables;
        [NativeDisableUnsafePtrRestriction] public PerFrameLayoutOutput* perFrameLayoutOutput;
        [NativeDisableUnsafePtrRestriction] public LockedBumpAllocator* lockedBumpAllocator;
        [NativeDisableUnsafePtrRestriction] public LayoutTree* layoutTree;

        public void Execute() {
            
            BuildAndAssignClippers(out CheckedArray<Clipper> clippers, out CheckedArray<ushort> clipIndices);

            ComputeOrientedBoundingBoxes(); // actually no dependencies, can be parallel w/ BuildAndAssignClippers technically

            for (int i = 2; i < clippers.size; i++) {
                clippers.array[i].orientedBounds = perFrameLayoutOutput->clipInfos[clippers[i].flatElementIndex].orientedBounds;
            }
            
            ComputeClipperBounds(clippers, clipIndices);
            
            ComputeAxisAlignedBounds(clippers, clipIndices);

        }

        private void BuildAndAssignClippers(out CheckedArray<Clipper> clippers, out CheckedArray<ushort> clipIndices) {
            clipIndices = perFrameLayoutOutput->clipperIndex;
            ClipBehavior* clipBehaviorTable = styleTables->ClipBehavior;
            Overflow* overflowX = styleTables->OverflowX;
            Overflow* overflowY = styleTables->OverflowY;

            int clipperCount = 2;
            
            for (int i = 0; i < layoutTree->elementCount; i++) {

                ElementId elementId = layoutTree->nodeList.array[i].elementId;

                if (overflowX[elementId.index] == Overflow.Hidden || overflowY[elementId.index] == Overflow.Hidden) {
                    clipperCount++;
                }

            }

            perFrameLayoutOutput->clippers = new CheckedArray<Clipper>(lockedBumpAllocator->Allocate<Clipper>(clipperCount), clipperCount);
            clippers = perFrameLayoutOutput->clippers;
            
            AxisAlignedBounds2D neverAABB = new AxisAlignedBounds2D(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);
            AxisAlignedBounds2D screenAABB = new AxisAlignedBounds2D(0, 0, screenWidth, screenHeight);  
            clippers[0] = new Clipper() {
                aabb = neverAABB,
                orientedBounds = new OrientedBounds(neverAABB),
                elementId = default,
                flatElementIndex = -1,
                isCulled = false,
                parentIndex = -1
            };

            clippers[1] = new Clipper() {
                aabb = screenAABB,
                orientedBounds = new OrientedBounds(screenAABB),
                elementId = default,
                flatElementIndex = -1,
                isCulled = false,
                parentIndex = -1
            };

            int mapSize = LongBoolMap.GetMapSize(layoutTree->elementCount);
            ulong* mapStorage = stackalloc ulong[mapSize];
            LongBoolMap clipperMap = new LongBoolMap(mapStorage, mapSize);

            int clipperIdx = 2;

            for (int i = 0; i < layoutTree->elementCount; i++) {

                // todo -- view roots are implicitly clippers but only some things clip against them
                
                ElementId elementId = layoutTree->nodeList.array[i].elementId;

                int elementIdIndex = elementId.index;
                
                if (overflowX[elementIdIndex] == Overflow.Hidden || overflowY[elementIdIndex] == Overflow.Hidden) {
                    clipperMap.Set(i);
                    clippers[clipperIdx++] = new Clipper() {
                        flatElementIndex = i,
                        elementId = elementId,
                    };
                }

            }

            for (int i = 0; i < layoutTree->elementCount; i++) {

                int elementIdIndex = layoutTree->nodeList[i].elementId.index;

                switch (clipBehaviorTable[elementIdIndex]) {
                    default:
                    case ClipBehavior.Normal: {
                        int parentIndex = layoutTree->nodeList[i].parentIndex;
                        // if parent is a clipper, point at the parents index
                        
                        if (clipperMap.Get(parentIndex)) {
                            ElementId parentId = layoutTree->nodeList[parentIndex].elementId;
                            int clipIdx = 2;

                            while (clippers[clipIdx].elementId != parentId) {
                                clipIdx++;
                            }

                            clipIndices[i] = (ushort) clipIdx;
                        }
                        else {
                            clipIndices[i] = (ushort) (parentIndex == -1 ? 1 : clipIndices[parentIndex]); // roots clip to screen by default 
                        }

                        break;
                    }

                    case ClipBehavior.Never:
                        clipIndices[i] = 0;
                        break;

                    case ClipBehavior.Screen:
                        clipIndices[i] = 1;
                        break;

                    case ClipBehavior.View:
                        clipIndices[i] = 1; // todo -- lookup view index somehow. elementId to view root element Id (not view Id!), find view clipper 
                        break;

                }
            }

        }

        private void ComputeAxisAlignedBounds(CheckedArray<Clipper> clippers, CheckedArray<ushort> clipIndices) {
            for (int i = 0; i < layoutTree->elementCount; i++) {

                ref ClipInfo clipInfo = ref perFrameLayoutOutput->clipInfos.Get(i);
                ref Clipper clipper = ref clippers.Get(clipIndices[i]);
                Size size = perFrameLayoutOutput->sizes[i];
                ref OrientedBounds orientedBounds = ref clipInfo.orientedBounds;

                if (clipper.isCulled || size.width * size.height == 0) {
                    clipInfo.isCulled = true; // flag somewhere instead?
                    orientedBounds = default;
                }
                else {

                    float xMin = float.MaxValue;
                    float xMax = float.MinValue;
                    float yMin = float.MaxValue;
                    float yMax = float.MinValue;

                    if (orientedBounds.p0.x < xMin) xMin = orientedBounds.p0.x;
                    if (orientedBounds.p1.x < xMin) xMin = orientedBounds.p1.x;
                    if (orientedBounds.p2.x < xMin) xMin = orientedBounds.p2.x;
                    if (orientedBounds.p3.x < xMin) xMin = orientedBounds.p3.x;

                    if (orientedBounds.p0.x > xMax) xMax = orientedBounds.p0.x;
                    if (orientedBounds.p1.x > xMax) xMax = orientedBounds.p1.x;
                    if (orientedBounds.p2.x > xMax) xMax = orientedBounds.p2.x;
                    if (orientedBounds.p3.x > xMax) xMax = orientedBounds.p3.x;

                    if (orientedBounds.p0.y < yMin) yMin = orientedBounds.p0.y;
                    if (orientedBounds.p1.y < yMin) yMin = orientedBounds.p1.y;
                    if (orientedBounds.p2.y < yMin) yMin = orientedBounds.p2.y;
                    if (orientedBounds.p3.y < yMin) yMin = orientedBounds.p3.y;

                    if (orientedBounds.p0.y > yMax) yMax = orientedBounds.p0.y;
                    if (orientedBounds.p1.y > yMax) yMax = orientedBounds.p1.y;
                    if (orientedBounds.p2.y > yMax) yMax = orientedBounds.p2.y;
                    if (orientedBounds.p3.y > yMax) yMax = orientedBounds.p3.y;

                    bool overlappingOrContains = xMax >= clipper.aabb.xMin && xMin <= clipper.aabb.xMax && yMax >= clipper.aabb.yMin && yMin <= clipper.aabb.yMax;

                    clipInfo.isCulled = !overlappingOrContains;
                    clipInfo.aabb = new AxisAlignedBounds2D(xMin, yMin, xMax, yMax);

                }

            }
        }

        private void ComputeClipperBounds(CheckedArray<Clipper> clippers, CheckedArray<ushort> clipIndices) {

            // skip never & screen clippers
            for (int i = 2; i < clippers.size; i++) {

                ref Clipper clipper = ref clippers.array[i];
                int parentClipperIndex = clipIndices[clipper.flatElementIndex];
                ref Clipper parent = ref clippers.array[parentClipperIndex];

                if (parent.isCulled) {
                    clipper.isCulled = true;
                    continue;
                }

                Size size = perFrameLayoutOutput->sizes[clipper.flatElementIndex];

                if (size.width * size.height == 0) {
                    clipper.isCulled = true;
                    continue;
                }

                OrientedBounds bounds = perFrameLayoutOutput->bounds[clipper.flatElementIndex]; // todo -- this is never assigned!

                float xMin = float.MaxValue;
                float xMax = float.MinValue;
                float yMin = float.MaxValue;
                float yMax = float.MinValue;

                if (bounds.p0.x < xMin) xMin = bounds.p0.x;
                if (bounds.p1.x < xMin) xMin = bounds.p1.x;
                if (bounds.p2.x < xMin) xMin = bounds.p2.x;
                if (bounds.p3.x < xMin) xMin = bounds.p3.x;

                if (bounds.p0.x > xMax) xMax = bounds.p0.x;
                if (bounds.p1.x > xMax) xMax = bounds.p1.x;
                if (bounds.p2.x > xMax) xMax = bounds.p2.x;
                if (bounds.p3.x > xMax) xMax = bounds.p3.x;

                if (bounds.p0.y < yMin) yMin = bounds.p0.y;
                if (bounds.p1.y < yMin) yMin = bounds.p1.y;
                if (bounds.p2.y < yMin) yMin = bounds.p2.y;
                if (bounds.p3.y < yMin) yMin = bounds.p3.y;

                if (bounds.p0.y > yMax) yMax = bounds.p0.y;
                if (bounds.p1.y > yMax) yMax = bounds.p1.y;
                if (bounds.p2.y > yMax) yMax = bounds.p2.y;
                if (bounds.p3.y > yMax) yMax = bounds.p3.y;

                clipper.aabb = AxisAlignedBounds2D.Intersect(clippers[clipper.parentIndex].aabb, new AxisAlignedBounds2D(xMin, yMin, xMax, yMax));

                if (clipper.aabb.Width <= 0 || clipper.aabb.Height <= 0) {
                    clipper.isCulled = true;
                    clipper.aabb = default;
                }

            }
        }

        private void ComputeOrientedBoundingBoxes() {
            
            for (int i = 0; i < layoutTree->elementCount; i++) {
                
                Size size = perFrameLayoutOutput->sizes[i];
                ref float4x4 worldMatrix = ref perFrameLayoutOutput->worldMatrices.array[i];
                ref OrientedBounds orientedBounds = ref perFrameLayoutOutput->clipInfos.array[i].orientedBounds;
                
                const float x = 0;
                const float y = 0;

                // todo can probably find a way to simd this but thats a problem for smarter future me
                
                orientedBounds.p0.x = worldMatrix.c0.x * x + worldMatrix.c1.x * y + worldMatrix.c3.x;
                orientedBounds.p0.y = (worldMatrix.c0.y * x + worldMatrix.c1.y * y + -worldMatrix.c3.y);

                orientedBounds.p1.x = worldMatrix.c0.x * size.width + worldMatrix.c1.x * y + worldMatrix.c3.x;
                orientedBounds.p1.y = (worldMatrix.c0.y * size.width + worldMatrix.c1.x * y + -worldMatrix.c3.y);

                orientedBounds.p2.x = worldMatrix.c0.x * size.width + worldMatrix.c1.x * size.height + worldMatrix.c3.x;
                orientedBounds.p2.y = (worldMatrix.c0.y * size.width + worldMatrix.c1.y * size.height + -worldMatrix.c3.y);
                
                orientedBounds.p3.x = worldMatrix.c0.x * x + worldMatrix.c1.x * size.height + worldMatrix.c3.x;
                orientedBounds.p3.y = (worldMatrix.c0.y * x + worldMatrix.c1.y * size.height + -worldMatrix.c3.y);
            }

        }

    }

}