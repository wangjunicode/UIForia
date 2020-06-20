using UIForia.Graphics;
using UIForia.Util.Unsafe;
using Unity.Jobs;

namespace UIForia.Systems {

    public unsafe struct MergeRenderContexts_Managed : IJob {

        public HeapAllocated<int> drawListSizePtr;
        public GCHandle<RenderContextInfo> outputHandle;
        public PerThreadObjectPool<RenderContext2> contextPoolHandle;
        public DataList<ContextOffsets> contextOffsets;

        public void Execute() {

            ThreadSafePool<RenderContext2> pool = contextPoolHandle.GetPool();
            RenderContextInfo renderContextInfo = outputHandle.Get();

            int drawListSize = 0;
            int shapeInfoListSize = 0;
            int meshListSize = 0;
            int materialListSize = 0;
            int transformListSize = 0;
            int propertyOverrideSize = 0;
            int textureIdSize = 0;
            
            for (int i = 0; i < pool.perThreadData.Length; i++) {
                RenderContext2 renderContext = pool.perThreadData[i];

                if (renderContext == null) {
                    continue;
                }

                contextOffsets[i] = new ContextOffsets() {
                    drawListOffset = drawListSize,
                    materialListOffset = materialListSize,
                    meshListOffset = meshListSize,
                    propertyOverrideOffset = propertyOverrideSize,
                    textureIdOffset = textureIdSize,
                    transformListOffset = transformListSize,
                    shapeInfoListOffset = shapeInfoListSize
                };

                shapeInfoListSize += renderContext.shapeInfoList.size;
                drawListSize += renderContext.drawList.size;
                meshListSize += renderContext.meshList.size;
                materialListSize += renderContext.materialList.size;
                transformListSize += renderContext.transformList.size;
                propertyOverrideSize += renderContext.propertyOverrides.size;
                textureIdSize += renderContext.textureIds.size;

            }

            renderContextInfo.drawList.EnsureCapacity(drawListSize);
            renderContextInfo.propertyOverrides.EnsureCapacity(propertyOverrideSize);
            renderContextInfo.textureIds.EnsureCapacity(textureIdSize);
            renderContextInfo.transformList.EnsureCapacity(transformListSize);
            renderContextInfo.shapeInfoList.EnsureCapacity(shapeInfoListSize);

            renderContextInfo.materialList.EnsureCapacity(materialListSize);
            renderContextInfo.meshList.EnsureCapacity(meshListSize);

            for (int i = 0; i < pool.perThreadData.Length; i++) {
                RenderContext2 renderContext = pool.perThreadData[i];

                if (renderContext == null) {
                    continue;
                }

                renderContextInfo.drawList.AddRange(renderContext.drawList.GetArrayPointer(), renderContext.drawList.size);
                renderContextInfo.propertyOverrides.AddRange(renderContext.propertyOverrides.GetArrayPointer(), renderContext.propertyOverrides.size);
                renderContextInfo.textureIds.AddRange(renderContext.textureIds.GetArrayPointer(), renderContext.textureIds.size);
                renderContextInfo.transformList.AddRange(renderContext.transformList.GetArrayPointer(), renderContext.transformList.size);
                renderContextInfo.shapeInfoList.AddRange(renderContext.shapeInfoList.GetArrayPointer(), renderContext.shapeInfoList.size);

                renderContextInfo.materialList.AddRange(renderContext.materialList);
                renderContextInfo.meshList.AddRange(renderContext.meshList);

            }

            drawListSizePtr.Set(drawListSize);

        }

    }

}