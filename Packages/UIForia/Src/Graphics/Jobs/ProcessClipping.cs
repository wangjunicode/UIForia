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

    public struct DrawLink {

        public int next;
        public int prev;

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
        
        // todo -- implement stenciled rendering
        public DataList<int>.Shared stencilDrawList;
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
                float3 p0 = math.transform(*drawInfo.matrix, new float3(aabb.xMin, aabb.yMin, 0));
                float3 p1 = math.transform(*drawInfo.matrix, new float3(aabb.xMax, aabb.yMin, 0));
                float3 p2 = math.transform(*drawInfo.matrix, new float3(aabb.xMax, aabb.yMax, 0));
                float3 p3 = math.transform(*drawInfo.matrix, new float3(aabb.xMin, aabb.yMax, 0));

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

                transformedBounds[i] = new AxisAlignedBounds2D(xMin, yMin, xMax, yMax);

            }
            
        }
        
        public void Execute() {
            
            TransformLocalBounds();
            
            Mode mode = Mode.Normal;

            DataList<Clipper> clipperStack = new DataList<Clipper>(16, Allocator.Temp);
            
            AxisAlignedBounds2D currentClipperBounds = new AxisAlignedBounds2D(0, 0, surfaceWidth, surfaceHeight);

            clipperBoundsList.Add(currentClipperBounds);

            Clipper activeClipper = new Clipper() {
                type = ClipperType.Scope,
                boundsIndex = 0,
                stencilDepth = 0,
                stencilIndex = 0,
                bounds = currentClipperBounds
            };

            stencilDataList.Add(new StencilInfo() {
                aabb = activeClipper.bounds,
                clipperDepth = 0
            });

            int currentStencilStart = 0;

            for (int i = 0; i < drawList.size; i++) {

                ref DrawInfo2 drawInfo = ref drawList[i];

                // store index of next stencil at given depth
                // lets me skip between them and possibly avoid checking for further stencils to render

                clipRectIdList.array[i] = activeClipper.boundsIndex;

                ref RenderTraversalInfo renderTraversalInfo = ref clipRenderList[i];
                renderTraversalInfo.stencilIndex = activeClipper.stencilIndex;
                renderTraversalInfo.clipRectId = activeClipper.boundsIndex;

                switch (drawInfo.drawType) {

                    case DrawType2.PushClipRect: {

                        AxisAlignedBounds2D bounds = AxisAlignedBounds2D.Intersect(*(AxisAlignedBounds2D*) drawInfo.shapeData, currentClipperBounds);

                        currentClipperBounds = bounds;
                        clipperBoundsList.Add(bounds);

                        activeClipper.bounds = bounds;
                        activeClipper.boundsIndex = clipperBoundsList.size;
                        activeClipper.type = ClipperType.AlignedRect;

                        clipperStack.Add(activeClipper);

                        break;
                    }

                    case DrawType2.BeginStencilClip: {
                        currentStencilStart = stencilDrawList.size;
                        mode = Mode.Stencil;

                        break;
                    }

                    case DrawType2.PopClipper: {

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
                        switch (mode) {

                            case Mode.Stencil: {
                                renderTraversalInfo.isStencilMember = true;
                                stencilDrawList.Add(i);
                                break;
                            }

                            default:
                            case Mode.Normal: {
                                renderTraversalInfo.isStencilMember = false;
                                break;
                            }
                        }

                        break;
                    }
                }

            }

            clipperStack.Dispose();

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

        private enum Mode {

            Normal,
            Stencil,
            Mask

        }

    }

}