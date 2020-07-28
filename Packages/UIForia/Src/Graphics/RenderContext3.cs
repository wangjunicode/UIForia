using System;
using System.Collections.Generic;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIForia.Graphics {

    public unsafe class RenderContext3 : IDisposable {

        internal ushort localDrawId;
        internal ushort renderIndex;

        internal DataList<DrawInfo2> drawList;
        private HeapAllocated<float4x4> dummyMatrix;
        private PagedByteAllocator stackAllocator;
        private float4x4* defaultMatrix;
        
        internal StructList<CommandBufferCallback> callbacks;
        internal Dictionary<int, Texture> textureMap;
        internal ResourceManager resourceManager;
        private TextMaterialInfo* textMaterialPtr;

        public RenderContext3(ResourceManager resourceManager) {
            this.resourceManager = resourceManager;
            this.textureMap = new Dictionary<int, Texture>(32);
            this.callbacks = new StructList<CommandBufferCallback>(4);
            this.dummyMatrix = new HeapAllocated<float4x4>(float4x4.identity);
            this.drawList = new DataList<DrawInfo2>(64, Allocator.Persistent);
            this.stackAllocator = new PagedByteAllocator(TypedUnsafe.Kilobytes(16), Allocator.Persistent, Allocator.Persistent);
        }
        
        public void Setup(MaterialId materialId, int renderIndex, float4x4* transform) {
            this.localDrawId = 0;
            this.renderIndex = (ushort) renderIndex;
            this.defaultMatrix = transform;
        }

        public void Callback(object context, Action<object, CommandBuffer> callback) {
            
            int callbackIdx = callbacks.size;
            
            callbacks.Add(new CommandBufferCallback() {
                context = context,
                callback = callback
            });
            
            drawList.Add(new DrawInfo2() {
                matrix = defaultMatrix,
                drawType = DrawType2.Callback,
                localBounds = default,
                materialData = null,
                shapeData = stackAllocator.Allocate(callbackIdx),
                drawSortId = new DrawSortId() {
                    localRenderIdx = localDrawId++,
                    baseRenderIdx = renderIndex
                }
            });
            
        }

        internal void AddTexture(Texture texture) {
            // GetHashCode() returns the texture's instanceId but without an IsMainThread() check
            textureMap[texture.GetHashCode()] = texture;
        }
        
        internal void DrawElementBodyInternal(in SDFMeshDesc meshDesc, in AxisAlignedBounds2D bounds, in ElementMaterialSetup materialSetup) {
            drawList.Add(new DrawInfo2() {
                matrix = defaultMatrix,
                drawType = DrawType2.UIForiaElement, // todo -- generate geometry instead if material id is not what we expect
                materialId = MaterialId.UIForiaShape,
                localBounds = bounds,
                // could consider making these different allocators to keep similar types together
                materialData = stackAllocator.Allocate(materialSetup),
                shapeData = stackAllocator.Allocate(meshDesc),
                drawSortId = new DrawSortId() {
                    localRenderIdx = localDrawId++,
                    baseRenderIdx = renderIndex
                }
            });
        }

        public struct TextMeshDesc {

            public int count;
            public TextSymbol* textRenderInfo;
            public TextMaterialInfo * materialPtr;
            public int fontAssetId;
            public int start;

        }
        
        internal void DrawTextCharacters(TextRenderRange textRenderRange) {
            
            TextMaterialSetup textMaterialSetup = new TextMaterialSetup();
            
            textMaterialSetup.faceTexture = textRenderRange.texture0;
            textMaterialSetup.outlineTexture = textRenderRange.texture1;
            // todo this feels a bit brittle
            textMaterialSetup.fontTextureId = resourceManager.fontAssetMap[textRenderRange.fontAssetId].atlasTextureId;
                
            drawList.Add(new DrawInfo2() {
                matrix = defaultMatrix,
                drawType = DrawType2.UIForiaText, 
                materialId = MaterialId.UIForiaSDFText,
                localBounds = textRenderRange.localBounds,
                // could consider making these different allocators to keep similar types together
                materialData = stackAllocator.Allocate(textMaterialSetup), 
                shapeData = stackAllocator.Allocate(new TextMeshDesc() {
                    materialPtr = textMaterialPtr,
                    fontAssetId = textRenderRange.fontAssetId,
                    textRenderInfo = textRenderRange.symbols,
                    start =  textRenderRange.characterRange.start,
                    count = textRenderRange.characterRange.length
                }),
                drawSortId = new DrawSortId() {
                    localRenderIdx = localDrawId++,
                    baseRenderIdx = renderIndex
                }
            });
            
        }
        
        public void Clear() {
            textureMap.Clear();
            drawList.size = 0;
            callbacks.Clear();
            stackAllocator.Clear();
        }
        
        public void Dispose() {
            stackAllocator.Dispose();
            drawList.Dispose();
            dummyMatrix.Dispose();
        }

        public int GetFontTextureId(int textStyleFontAssetId) {
            return resourceManager.GetFontTextureId(textStyleFontAssetId);
        }

        // todo -- if I am ABSOLUTELY sure nothing nefarious can happen to the material array while I'm prepping for render then I can avoid this copy/alloc
        internal void SetTextMaterials(in List_TextMaterialInfo textInfoMaterialBuffer) {
            if (textInfoMaterialBuffer.array != null) {
                textMaterialPtr = stackAllocator.Allocate(textInfoMaterialBuffer.array, textInfoMaterialBuffer.size);
            }
            else {
                textMaterialPtr = null;
            }
        }

    }

}