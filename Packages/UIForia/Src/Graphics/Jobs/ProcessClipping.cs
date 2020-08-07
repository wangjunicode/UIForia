using UIForia.ListTypes;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Graphics {

    public struct RenderTraversalInfo {

        // maybe separate list
        public AxisAlignedBounds2D intersectedBounds;

        public int clipRectId;
        public int stencilIndex; // stencil that applies to this draw

        // can i just unlink from the draw list instead?
        public bool requiresRendering;
        public bool isStencilMember;

    }

    // assign clipper ids
    // do bounds intersections
    // cull where intersections are 0 height or width

    [BurstCompile]
    internal unsafe struct ProcessClipping : IJob {

        public DataList<AxisAlignedBounds2D> transformedBounds;
        public DataList<DrawInfo2>.Shared drawList;
        public DataList<RenderTraversalInfo> clipRenderList;
        public DataList<AxisAlignedBounds2D>.Shared clipperBoundsList;
        public DataList<StencilInfo>.Shared stencilDataList;

        public List_Int32 clipRectIdList;

        public float surfaceWidth;
        public float surfaceHeight;

        public void IntersectDrawnBounds() {

            for (int i = 0; i < drawList.size; i++) {

                ref DrawInfo2 drawInfo = ref drawList[i];

                if (drawInfo.IsNonRendering()) {
                    continue;
                }

                ref AxisAlignedBounds2D clipper = ref clipperBoundsList[clipRectIdList.array[i]];
                ref RenderTraversalInfo renderTraversalInfo = ref clipRenderList[i];
                renderTraversalInfo.intersectedBounds = AxisAlignedBounds2D.Intersect(clipper, transformedBounds[i]);

                renderTraversalInfo.requiresRendering = (
                    renderTraversalInfo.intersectedBounds.xMax - renderTraversalInfo.intersectedBounds.xMin > 0 &&
                    renderTraversalInfo.intersectedBounds.yMax - renderTraversalInfo.intersectedBounds.yMin > 0
                );

                // todo -- remove link from draw list in this case
                // if a stencil is culled do I remove all its shit? 

            }

        }

        private void TransformLocalBounds() {
            for (int i = 0; i < drawList.size; i++) {

                // note: assumes all drawInfos have a valid matrix pointer, even if its bogus

                ref DrawInfo2 drawInfo = ref drawList[i];
                AxisAlignedBounds2D aabb = drawInfo.localBounds;

                // todo -- without burst this is stupid slow
                float3 p0 = math.transform(*drawInfo.matrix, new float3(aabb.xMin, -aabb.yMin, 0));
                float3 p1 = math.transform(*drawInfo.matrix, new float3(aabb.xMax, -aabb.yMin, 0));
                float3 p2 = math.transform(*drawInfo.matrix, new float3(aabb.xMax, -aabb.yMax, 0));
                float3 p3 = math.transform(*drawInfo.matrix, new float3(aabb.xMin, -aabb.yMax, 0));
                
                float xMin = float.MaxValue;
                float xMax = float.MinValue;
                float yMin = float.MaxValue;
                float yMax = float.MinValue;

                if (p0.x < xMin) xMin = p0.x;
                if (p1.x < xMin) xMin = p1.x;
                if (p2.x < xMin) xMin = p2.x;
                if (p3.x < xMin) xMin = p3.x;

                if (p0.x > xMax) xMax = p0.x;
                if (p1.x > xMax) xMax = p1.x;
                if (p2.x > xMax) xMax = p2.x;
                if (p3.x > xMax) xMax = p3.x;

                if (p0.y < yMin) yMin = p0.y;
                if (p1.y < yMin) yMin = p1.y;
                if (p2.y < yMin) yMin = p2.y;
                if (p3.y < yMin) yMin = p3.y;

                if (p0.y > yMax) yMax = p0.y;
                if (p1.y > yMax) yMax = p1.y;
                if (p2.y > yMax) yMax = p2.y;
                if (p3.y > yMax) yMax = p3.y;

                transformedBounds[i] = new AxisAlignedBounds2D(xMin, -yMax, xMax, -yMin); // min max y flipped

            }

        }

        public void Execute() {

            TransformLocalBounds();
            
            DataList<Clipper> clipperStack = new DataList<Clipper>(16, Allocator.Temp);

            Clipper activeClipper = new Clipper() {
                type = ClipperType.Scope,
                boundsIndex = 0,
                bounds = new AxisAlignedBounds2D(0, 0, surfaceWidth, surfaceHeight)
            };
            
            clipperBoundsList.Add(activeClipper.bounds);

            stencilDataList.Add(new StencilInfo() {
                aabb = activeClipper.bounds,
                clipperDepth = 0,
                stencilDepth = 0,
                parentIndex = -1,
                drawState = StencilSetupState.Pushed // base level stencil is no stencil, always valid
            });

            clipperStack.Add(activeClipper);
            
            int stencilIdx = 0;
            int stencilDepth = 0;
            bool isStencilMember = false;
            
            for (int i = 0; i < drawList.size; i++) {

                ref DrawInfo2 drawInfo = ref drawList[i];

                // store index of next stencil at given depth
                // lets me skip between them and possibly avoid checking for further stencils to render

                clipRectIdList.array[i] = activeClipper.boundsIndex;

                ref RenderTraversalInfo renderTraversalInfo = ref clipRenderList[i];
                renderTraversalInfo.stencilIndex = stencilIdx;
                renderTraversalInfo.clipRectId = activeClipper.boundsIndex;
                renderTraversalInfo.requiresRendering = false;
                renderTraversalInfo.isStencilMember = isStencilMember;

                switch (drawInfo.drawType) {
                    
                    case DrawType2.Callback: {
                        break;
                    }

                    case DrawType2.PushClipRect: {

                        float3 topLeft = new float3(drawInfo.localBounds.xMin, -drawInfo.localBounds.yMin, 0);

                        float2 basePoint = topLeft.xy;
                        
                        // todo -- this method is sooo slow without burst
                        topLeft = math.transform(*drawInfo.matrix, topLeft);
                        // note -- cheating with width/height storage, its is NOT a max point in this case
                        float width = drawInfo.localBounds.xMax;
                        float height = drawInfo.localBounds.yMax; 
                        // need to invert the y position
                        AxisAlignedBounds2D bounds = AxisAlignedBounds2D.Intersect(new AxisAlignedBounds2D(topLeft.x, -topLeft.y, topLeft.x + width, basePoint.y + (-topLeft.y + height)), activeClipper.bounds);

                        activeClipper.bounds = bounds;
                        activeClipper.boundsIndex = clipperBoundsList.size;
                        activeClipper.type = ClipperType.AlignedRect;
                        clipperBoundsList.Add(bounds);
                        clipperStack.Add(activeClipper);
                        break;
                    }

                    case DrawType2.BeginStencilClip: {
                        isStencilMember = true;
                        stencilDepth++;
                        stencilIdx = stencilDataList.size;
                        stencilDataList.Add(new StencilInfo() {
                            aabb = default,
                            drawState = StencilSetupState.Uninitialized,
                            stencilDepth = stencilDepth,
                            beginIndex = i + 1
                        });
                        break;
                    }

                    case DrawType2.PushStencilClip: {
                        isStencilMember = false;
                        ref StencilInfo stencilInfo = ref stencilDataList[stencilIdx];
                        stencilInfo.pushIndex = i;
                        ComputeStencilBounds(ref stencilInfo);
                        
                        AxisAlignedBounds2D bounds = AxisAlignedBounds2D.Intersect(stencilInfo.aabb, activeClipper.bounds);

                        activeClipper.bounds = bounds;
                        activeClipper.boundsIndex = clipperBoundsList.size;
                        activeClipper.type = ClipperType.Stencil;
                        clipperBoundsList.Add(bounds);
                        clipperStack.Add(activeClipper);
                        break;
                    }
                        
                    case DrawType2.PopStencilClip: {
                        stencilDepth--;
                        // todo -- save pop index somehow
                        if (clipperStack.size == 1) {
                            continue;
                        }
                        ref Clipper current = ref clipperStack[clipperStack.size - 1];
                        clipperStack.size--;
                        activeClipper = clipperStack[clipperStack.size - 1];
                        break;
                    }

                    case DrawType2.PopClipRect: {

                        if (clipperStack.size == 1) {
                            continue;
                        }

                        ref Clipper current = ref clipperStack[clipperStack.size - 1];

                        if (current.type == ClipperType.Stencil) {
                            // stencilDataList[current.stencilIndex].popIndex = outputSize;
                        }

                        clipperStack.size--;
                        activeClipper = clipperStack[clipperStack.size - 1];
                        // might need to track ending indices
                        break;
                    }

                    case DrawType2.UIForiaElement:
                    case DrawType2.UIForiaText: {
                        renderTraversalInfo.requiresRendering = true;
                        break;
                    }
                }

            }

            clipperStack.Dispose();

        }

        private void ComputeStencilBounds(ref StencilInfo stencilData) {
            float xMin = float.MaxValue;
            float yMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMax = float.MinValue;
            
            for (int i = stencilData.beginIndex; i < stencilData.pushIndex; i++) {
                ref DrawInfo2 drawInfo = ref drawList[i];
                if (drawInfo.drawType == DrawType2.UIForiaElement || drawInfo.drawType == DrawType2.UIForiaText) {

                    ref AxisAlignedBounds2D bounds = ref transformedBounds[i];

                    if (bounds.xMin < xMin) xMin = bounds.xMin;
                    if (bounds.xMax > xMax) xMax = bounds.xMax;
                    
                    if (bounds.yMin < yMin) yMin = bounds.yMin;
                    if (bounds.yMax > yMax) yMax = bounds.yMax;

                }
                
            }

            stencilData.aabb = new AxisAlignedBounds2D(xMin, yMin, xMax, yMax);
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
            public AxisAlignedBounds2D bounds;

        }

        private enum Mode {

            Normal,
            Stencil,
            Mask

        }

    }

}