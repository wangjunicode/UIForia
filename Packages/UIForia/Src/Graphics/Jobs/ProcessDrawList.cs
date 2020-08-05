using System.Diagnostics;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Graphics {

    [BurstCompile]
    internal unsafe struct ProcessDrawList : IJob {

        public DataList<DrawInfo>.Shared drawList;
        public DataList<DrawInfo>.Shared stencilDrawList;
        public DataList<StencilInfo>.Shared stencilDataList;
        public DataList<AxisAlignedBounds2D>.Shared clipperBoundsList;
        public DataList<ProcessedDrawInfo>.Shared processedDrawList;

        public float surfaceWidth;
        public float surfaceHeight;

        private enum Mode {

            Normal,
            Stencil,
            Mask

        }

        public void Execute() {

            // todo -- if bounds are unknowable because shader changes vertex positions then i cant rely on intersection checks
            Mode mode = Mode.Normal;

            DataList<Clipper> clipperStack = new DataList<Clipper>(16, Allocator.Temp);

            processedDrawList.EnsureCapacity(drawList.size);

            AxisAlignedBounds2D currentClipperBounds = new AxisAlignedBounds2D(0, 0, surfaceWidth, surfaceHeight);

            DataList<MaterialOverrideSet> overrides = new DataList<MaterialOverrideSet>(32, Allocator.Temp);

            clipperBoundsList.Add(currentClipperBounds);

            Clipper activeClipper = new Clipper() {
                type = ClipperType.Scope,
                boundsIndex = 0,
                stencilDepth = -1,
                stencilIndex = -1,
                bounds = currentClipperBounds
            };

            int outputSize = 0;

            stencilDataList.Add(new StencilInfo() {
                aabb = activeClipper.bounds,
                clipperDepth = 0,
            });

            ProcessedDrawInfo* output = processedDrawList.GetArrayPointer();
            int currentStencilStart = 0;

            for (int i = 0; i < drawList.size; i++) {

                ref DrawInfo drawInfo = ref drawList[i];

                drawInfo.clipRectIndex = activeClipper.boundsIndex;

                switch (drawInfo.type) {

                    case DrawType.Mesh: {
                        break;
                    }

                    case DrawType.Shape: {

                        switch (mode) {

                            case Mode.Stencil: {
                                int propertyOverrideSetId = GetPropertyOverrideId(ref overrides, drawInfo.materialOverrideCount, drawInfo.materialOverrideValues);
                                
                                drawInfo.propertySetId = propertyOverrideSetId;
                                drawInfo.clipRectIndex = activeClipper.boundsIndex;
                                
                                stencilDrawList.Add(drawInfo);
                                break;
                            }

                            default:
                            case Mode.Normal: {
                                drawInfo.stencilIndex = activeClipper.stencilIndex;
                                drawInfo.intersectedBounds = AxisAlignedBounds2D.Intersect(drawInfo.geometryInfo->bounds, activeClipper.bounds);

                                int propertyOverrideSetId = GetPropertyOverrideId(ref overrides, drawInfo.materialOverrideCount, drawInfo.materialOverrideValues);

                                output[outputSize++] = new ProcessedDrawInfo() {
                                    drawInfoIndex = i,
                                    stencilIndex = activeClipper.stencilIndex,
                                    intersectedBounds = drawInfo.intersectedBounds,
                                    clipRectIndex = activeClipper.boundsIndex,
                                    type = drawInfo.type,
                                    materialId = drawInfo.materialId,
                                    vertexLayout = drawInfo.vertexLayout,
                                    materialPropertySetId = propertyOverrideSetId,
                                    prevIndex = outputSize - 1,
                                    nextIndex = outputSize
                                };
                                break;
                            }
                        }

                        break;
                    }

                    case DrawType.BeginStencilClip: {
                        currentStencilStart = stencilDrawList.size;
                        mode = Mode.Stencil;
                        break;
                    }

                    case DrawType.PushStencilClip: {
                        RangeInt stencilDrawRange = new RangeInt(currentStencilStart, stencilDataList.size - currentStencilStart);
                        AxisAlignedBounds2D bounds = ComputeCompositeBounds(stencilDrawRange, ref stencilDrawList);
                        bounds = AxisAlignedBounds2D.Intersect(bounds, activeClipper.bounds);
                        stencilDataList.Add(new StencilInfo() {
                            aabb = bounds,
                            clipperDepth = clipperStack.size + 1,
                            stencilDepth = activeClipper.stencilDepth + 1,
                            parentIndex = activeClipper.stencilIndex,
                            pushIndex = outputSize,
                            popIndex = -1, // pop index will be set later
                            //drawInfoRange = stencilDrawRange
                        });

                        clipperBoundsList.Add(bounds);

                        activeClipper.bounds = bounds;
                        activeClipper.boundsIndex = clipperBoundsList.size;
                        activeClipper.type = ClipperType.Stencil;
                        activeClipper.stencilDepth++;
                        activeClipper.stencilIndex = stencilDataList.size - 1;
                        clipperStack.Add(activeClipper);
                        
                        mode = Mode.Normal;
                        
                        break;
                    }

                    case DrawType.PushClipRect: {

                        AxisAlignedBounds2D bounds = AxisAlignedBounds2D.Intersect(*(AxisAlignedBounds2D*) drawInfo.shapeData, currentClipperBounds);

                        currentClipperBounds = bounds;
                        clipperBoundsList.Add(bounds);

                        activeClipper.bounds = bounds;
                        activeClipper.boundsIndex = clipperBoundsList.size;
                        activeClipper.type = ClipperType.AlignedRect;

                        clipperStack.Add(activeClipper);

                        break;
                    }

                    // when drawing a root level stencil for a scope, draw with stencil cmp = always pass
                    // case DrawType.PushClipShape: {
                    //     AxisAlignedBounds2D bounds = AxisAlignedBounds2D.Intersect(drawInfo.geometryInfo->bounds, currentClipperBounds);
                    //
                    //     stencilDataList.Add(new StencilInfo() {
                    //         aabb = bounds,
                    //         clipperDepth = clipperStack.size + 1,
                    //         stencilDepth = activeClipper.stencilDepth + 1,
                    //         parentIndex = activeClipper.stencilIndex,
                    //         pushIndex = outputSize,
                    //     });
                    //
                    //     activeClipper.bounds = bounds;
                    //     activeClipper.boundsIndex = clipperBoundsList.size;
                    //     activeClipper.type = ClipperType.Stencil;
                    //     activeClipper.stencilDepth++;
                    //     activeClipper.stencilIndex = stencilDataList.size - 1;
                    //
                    //     clipperStack.Add(activeClipper);
                    //
                    //     break;
                    // }

                    case DrawType.PopClipper: {

                        if (clipperStack.size == 1) {
                            continue;
                        }

                        ref Clipper current = ref clipperStack[clipperStack.size - 1];
                        
                        if (current.type == ClipperType.Stencil) {
                            stencilDataList[current.stencilIndex].popIndex = outputSize;
                        }

                        clipperStack.size--;

                        activeClipper = clipperStack[clipperStack.size - 1];

                        // might need to track ending indices

                        break;
                    }

                    case DrawType.PushClipScope: {

                        activeClipper = new Clipper() {
                            type = ClipperType.Scope,
                            boundsIndex = 0,
                            bounds = clipperBoundsList[0],
                            stencilDepth = activeClipper.stencilDepth, // think this is wrong, might need to be depth + 1, but only where it ames 
                            stencilIndex = -1
                        };

                        clipperStack.Add(activeClipper);
                        drawInfo.flags |= DrawInfoFlags.BatchSet; // mark rendered so we can skip it

                        break;
                    }

                }

            }

            processedDrawList[0].prevIndex = -1;
            processedDrawList[outputSize - 1].nextIndex = -1;

            processedDrawList.size = outputSize;
            overrides.Dispose();
            clipperStack.Dispose();
        }

        private static AxisAlignedBounds2D ComputeCompositeBounds(RangeInt stencilDrawRange, ref DataList<DrawInfo>.Shared list) {
            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMin = float.MinValue;
            float yMax = float.MaxValue;

            if (stencilDrawRange.length == 1) {
                return list[stencilDrawRange.start].geometryInfo->bounds;
            }

            for (int i = stencilDrawRange.start; i < stencilDrawRange.end; i++) {
                AxisAlignedBounds2D bounds = list[i].geometryInfo->bounds;
                if (bounds.xMin < xMin) xMin = bounds.xMin;
                if (bounds.xMin > xMax) xMax = bounds.xMin;

                if (bounds.xMax < xMin) xMin = bounds.xMax;
                if (bounds.xMax > xMax) xMax = bounds.xMax;

                if (bounds.yMin < yMin) yMin = bounds.yMin;
                if (bounds.yMin > yMax) yMax = bounds.yMin;

                if (bounds.yMax < yMin) yMin = bounds.yMax;
                if (bounds.yMax > yMax) yMax = bounds.yMax;

            }

            return new AxisAlignedBounds2D(xMin, yMin, xMax, yMax);
        }

        private static int GetPropertyOverrideId(ref DataList<MaterialOverrideSet> propertyList, int cnt, MaterialPropertyOverride* propertyOverrides) {

            if (propertyOverrides == null) {
                return -1;
            }

            MaterialOverrideSet* array = propertyList.GetArrayPointer();
            int size = propertyList.size;

            // maybe a map is better for this for large inputs
            for (int i = 0; i < size; i++) {
                if (array[i].propertyOverrides == propertyOverrides) {
                    return i;
                }
            }

            NativeSortExtension.Sort(propertyOverrides, cnt);

            for (int i = 0; i < size; i++) {
                if (array[i].propertyCount == cnt && UnsafeUtility.MemCmp(propertyOverrides, array[i].propertyOverrides, sizeof(MaterialPropertyOverride) * cnt) == 0) {
                    return i;
                }
            }

            propertyList.Add(new MaterialOverrideSet() {
                propertyCount = cnt,
                propertyOverrides = propertyOverrides
            });

            return size;

        }

        private struct MaterialOverrideSet {

            public int propertyCount;
            public MaterialPropertyOverride* propertyOverrides;

        }

        public enum ClipperType {

            Scope,
            AlignedRect,
            Stencil,
            StencilTexture

        }

        public struct Clipper {

            public ClipperType type;
            public int boundsIndex;
            public int stencilDepth;
            public int stencilIndex;
            public AxisAlignedBounds2D bounds;

        }

    }

}