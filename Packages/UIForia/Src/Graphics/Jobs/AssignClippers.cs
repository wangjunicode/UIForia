using System;
using UIForia.ListTypes;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Graphics {

    [BurstCompile]
    internal unsafe struct CreateDrawCalls : IJob {

        public DataList<DrawInfo>.Shared drawList;
        public DataList<StencilInfo>.Shared stencilList;
        public DataList<AxisAlignedBounds2D>.Shared clipperBoundsList;
        public DataList<Batch>.Shared batchList;
        public DataList<int>.Shared batchMemberList;
        public DataList<RenderCommand>.Shared renderCommands;

        public void Execute() {

            Runner runner = new Runner() {
                batchList = batchList,
                drawList = drawList,
                renderCommands = renderCommands,
                stencilList = stencilList,
                batchMemberList = batchMemberList,
                clipperBoundsList = clipperBoundsList
            };

            runner.CreateDrawCalls();

            runner.Dispose();

        }

        private struct Runner : IDisposable {

            public DataList<DrawInfo>.Shared drawList;
            public DataList<StencilInfo>.Shared stencilList;
            public DataList<AxisAlignedBounds2D>.Shared clipperBoundsList;
            public DataList<Batch>.Shared batchList;
            public DataList<int>.Shared batchMemberList;
            public DataList<RenderCommand>.Shared renderCommands;

            private List_Int32 memberBuffer;
            
            private List_Int32 stencilToOpen;
            private List_Int32 stencilToClose;
            private List_Int32 stencilToPop;
            
            private List_Int32 inOrderStencilIndices;
            
            private int batchStartIndex;

            private int BeginBatch(int idx) {
                DrawInfo* drawInfoArray = drawList.GetArrayPointer();

                memberBuffer.size = 0;
                
                int drawInfoCount = drawList.size;

                ref DrawInfo batchStarter = ref drawInfoArray[idx];

                int inOrderBatchEnd = idx + 1;

                // if batch start has a stencil requirement
                // if stencil is not yet open
                // open it

                stencilToOpen.size = 0;
                
                if (batchStarter.stencilIndex != -1) {
                    if (!stencilList[batchStarter.stencilIndex].isOpen) {
                        stencilToOpen.Add(batchStarter.stencilIndex);
                    }    
                }
                
                for (; inOrderBatchEnd < drawInfoCount; inOrderBatchEnd++) {
                    ref DrawInfo drawInfo = ref drawInfoArray[inOrderBatchEnd];
                    
                    // if rendered, continue (could be because it was culled)
                    if ((drawInfo.flags & DrawInfoFlags.FinalBatchSet) != 0) {
                        continue;
                    }

                    if (drawInfo.type != DrawType.Shape) {
                        continue;
                    }

                    if (drawInfo.stencilIndex != batchStarter.stencilIndex) {
                        // keep a list of stencils we require
                        // if drawInfo's stencil is open
                        if (drawInfo.stencilIndex != -1) {
                            if (stencilList[drawInfo.stencilIndex].isOpen) {
                                
                            }
                            else {
                                
                            }
                        }
                    }

                    if (!CanBatchShape(batchStarter, drawInfo)) {
                        break;
                    }

                    memberBuffer.Add(inOrderBatchEnd);
                    
                }

                return inOrderBatchEnd;

            }

            private bool CanBatchShape(in DrawInfo batchStart, in DrawInfo drawInfo) {
                if (batchStart.materialId.index != drawInfo.materialId.index) {
                    return false;
                }

                return (drawInfo.flags & DrawInfoFlags.HasNonTextureOverrides) == 0;
            }
            
            public void CreateDrawCalls() {

                memberBuffer = new List_Int32(256, Allocator.Temp);
                stencilToOpen = new List_Int32(16, Allocator.Temp);
                stencilToClose = new List_Int32(16, Allocator.Temp);
                stencilToPop = new List_Int32(16, Allocator.Temp);
                
                DrawInfo* drawInfoArray = drawList.GetArrayPointer();

                int drawInfoCount = drawList.size;

                for (int i = 0; i < drawInfoCount; i++) {
                    ref DrawInfo drawInfo = ref drawInfoArray[i];
                    if ((drawInfo.flags & DrawInfoFlags.FinalBatchSet) != 0) {
                        continue;
                    }

                    if (drawInfo.type != DrawType.Shape) {
                        continue;
                    }

                    i = BeginBatch(i);
                    
                }

            }

            public void Dispose() {
                memberBuffer.Dispose();
                stencilToOpen.Dispose();
                stencilToClose.Dispose();
                stencilToPop.Dispose();
            }

        }

    }

    [BurstCompile]
    internal unsafe struct AssignClippers : IJob {

        public DataList<DrawInfo>.Shared drawList;
        public DataList<StencilInfo>.Shared stencilList;
        public DataList<AxisAlignedBounds2D>.Shared clipperBoundsList;

        public float surfaceWidth;
        public float surfaceHeight;

        public enum ClipperType {

            Scope,
            AlignedRect,
            StencilShape,
            StencilTexture

        }

        public struct Clipper {

            public ClipperType type;
            public int boundsIndex;
            public int stencilDepth;
            public int stencilIndex;
            public AxisAlignedBounds2D bounds;

        }

        public void Execute() {

            // todo -- if bounds are unknowable because shader changes vertex positions then i cant rely on intersection checks

            DataList<Clipper> clipperStack = new DataList<Clipper>(16, Allocator.Temp);

            AxisAlignedBounds2D currentClipperBounds = new AxisAlignedBounds2D(0, 0, surfaceWidth, surfaceHeight);

            clipperBoundsList.Add(currentClipperBounds);

            Clipper activeClipper = new Clipper() {
                type = ClipperType.Scope,
                boundsIndex = 0,
                stencilDepth = -1,
                stencilIndex = -1,
                bounds = currentClipperBounds
            };

            for (int i = 0; i < drawList.size; i++) {

                ref DrawInfo drawInfo = ref drawList[i];

                drawInfo.clipRectIndex = activeClipper.boundsIndex;

                switch (drawInfo.type) {

                    case DrawType.Mesh: {
                        break;
                    }

                    case DrawType.Shape: {
                        // todo -- lift out of stencil if possible
                        drawInfo.stencilIndex = activeClipper.stencilIndex;
                        drawInfo.intersectedBounds = AxisAlignedBounds2D.Intersect(drawInfo.geometryInfo->bounds, activeClipper.bounds);
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

                    case DrawType.PushClipTexture: {
                        AxisAlignedBounds2D bounds = AxisAlignedBounds2D.Intersect(drawInfo.geometryInfo->bounds, currentClipperBounds);

                        stencilList.Add(new StencilInfo() {
                            aabb = bounds,
                            clipperDepth = clipperStack.size + 1,
                            stencilDepth = activeClipper.stencilDepth + 1,
                            parentIndex = activeClipper.stencilIndex,
                            drawInfoIndex = i
                        });

                        activeClipper.bounds = bounds;
                        activeClipper.boundsIndex = clipperBoundsList.size;
                        activeClipper.type = ClipperType.StencilTexture;
                        activeClipper.stencilDepth++;
                        activeClipper.stencilIndex = stencilList.size - 1;

                        clipperStack.Add(activeClipper);

                        break;
                    }

                    // when drawing a root level stencil for a scope, draw with stencil cmp = always pass
                    case DrawType.PushClipShape: {
                        AxisAlignedBounds2D bounds = AxisAlignedBounds2D.Intersect(drawInfo.geometryInfo->bounds, currentClipperBounds);

                        stencilList.Add(new StencilInfo() {
                            aabb = bounds,
                            // type? geometry?
                            textureId = -1,
                            clipperDepth = clipperStack.size + 1,
                            stencilDepth = activeClipper.stencilDepth + 1,
                            parentIndex = activeClipper.stencilIndex,
                            drawInfoIndex = i
                        });

                        activeClipper.bounds = bounds;
                        activeClipper.boundsIndex = clipperBoundsList.size;
                        activeClipper.type = ClipperType.StencilShape;
                        activeClipper.stencilDepth++;
                        activeClipper.stencilIndex = stencilList.size - 1;

                        clipperStack.Add(activeClipper);

                        break;
                    }

                    case DrawType.PopClipper: {

                        if (clipperStack.size == 1) {
                            continue;
                        }

                        clipperStack.size--;

                        activeClipper = clipperStack[clipperStack.size - 1];

                        break;
                    }

                    case DrawType.PushClipScope: {

                        activeClipper = new Clipper() {
                            type = ClipperType.Scope,
                            boundsIndex = 0,
                            bounds = clipperBoundsList[0],
                            stencilDepth = -1,
                            stencilIndex = -1
                        };

                        clipperStack.Add(activeClipper);

                        break;
                    }

                    case DrawType.SetRenderTarget: {
                        break;
                    }

                }

            }

        }

    }

}