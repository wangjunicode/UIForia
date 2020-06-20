using ThisOtherThing.UI.ShapeUtils;
using UIForia.Graphics;
using UIForia.Graphics.ShapeKit;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Systems {

    public unsafe struct ShapeDataBuffer : IPerThreadCompatible {

        private Allocator allocator;

        public ShapeKit* shapeKit;
        public UIVertexHelper* vertexHelper;

        public void Dispose() {
            shapeKit->Dispose();
            vertexHelper->Dispose();
            TypedUnsafe.Dispose(shapeKit, allocator);
            TypedUnsafe.Dispose(vertexHelper, allocator);
            this = default;
        }

        public void InitializeForThread(Allocator inputAllocator) {
            this = default;

            // input allocator is persistent, don't want that!

            this.allocator = Allocator.TempJob;

            this.shapeKit = TypedUnsafe.Malloc<ShapeKit>(allocator);
            this.vertexHelper = TypedUnsafe.Malloc<UIVertexHelper>(allocator);

            *shapeKit = new ShapeKit(allocator);
            *vertexHelper = UIVertexHelper.Create(allocator);
        }

        public bool IsInitialized {
            get => shapeKit != null || vertexHelper != null;
        }

    }

    [BurstCompile]
    internal unsafe struct BakeShapes : IJob, IVertigoParallelDeferred {

        [NativeSetThreadIndex] public int threadIndex;

        public DataList<DrawInfo>.Shared drawList;
        public PerThread<ShapeDataBuffer> perThread_ShapeBuffer;

        public ParallelParams.Deferred defer { get; set; }

        public void Execute() {
            Run(0, drawList.size);
        }

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
        }

        private void Run(int start, int end) {

            ref ShapeDataBuffer buffer = ref perThread_ShapeBuffer.GetForThread(threadIndex);

            ref UIVertexHelper vh = ref UnsafeUtilityEx.AsRef<UIVertexHelper>(buffer.vertexHelper);
            ref ShapeKit shapeKit = ref UnsafeUtilityEx.AsRef<ShapeKit>(buffer.shapeKit);

            for (int i = start; i < end; i++) {

                ref DrawInfo drawInfo = ref drawList[i];

                if ((drawInfo.type & DrawType.Shape) == 0) {
                    continue;
                }

                switch (drawInfo.type) {

                    case DrawType.SDFText:
                        break;

                    case DrawType.Rect: {

                        vh.BeginShape();

                        RectData* rectData = (RectData*) drawInfo.shapeData;

                        shapeKit.SetDpiScale(1f);
                        shapeKit.SetAntiAliasWidth(0);//1f);
                        shapeKit.AddRect(ref vh, rectData->x, rectData->y, rectData->width, rectData->height, rectData->color);

                        drawInfo.shapeGeometrySource = buffer.vertexHelper;
                        drawInfo.shapeRange = vh.EndShape();

                        break;
                    }

                    case DrawType.RoundedRect:
                        break;

                    case DrawType.Arc:
                        break;

                    case DrawType.Polygon:
                        break;

                    case DrawType.Line:
                        break;

                    case DrawType.Ellipse:
                        break;

                }

            }

        }

    }

}