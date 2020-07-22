using System;
using System.Collections.Generic;
using UIForia.Rendering;
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
        
        public RenderContext3(ResourceManager resourceManager) {
            this.resourceManager = resourceManager;
            textureMap = new Dictionary<int, Texture>(32);
            callbacks = new StructList<CommandBufferCallback>(4);
            dummyMatrix = new HeapAllocated<float4x4>(float4x4.identity);
            drawList = new DataList<DrawInfo2>(64, Allocator.Persistent);
            stackAllocator = new PagedByteAllocator(TypedUnsafe.Kilobytes(16), Allocator.Persistent, Allocator.Persistent);
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

        // textInfoId -> needed for bounds
        // materialInfo, get idx by pointer in list or map 
        // effectData -> need to only upload some of them. think it makes a lot of sense to use a free-list 
        // need to know if bounds are reliable, or need to compute using effect offsets/rotations/etc. best to call them unreliable I think
        // need to use skip-render 
        // foreach character
        //     if !character.isRenderable
        //        continue
        //     bounds = cmp(bounds, computeVertices());
        // or if character.hasTransformEffect -> make its own draw info. probably wont be too many


            
            // effects are either enabled for a span or they are not
            // effects can be enabled for a material probably and are then applied per character
            // or they get applied per character directly
            
            // either way I need
                // use effects or dont
                // compute bounds
                // generate/send geometry only for rendered characters
                // mapping of effect struct to character (use free-list)
                // material id on characters
                // layout position of character
                // bake geometry for characters
                // fastest way would be saving per-character geometry up front
                // will worry about sdf offsets in the shader but i guess theres no reason not to accept real geometry
                // only when effects enabled? 
                // i guess im trying to save a bunch of floats that I dont need to persist
            
            // CharSpan.SetEffectsEnabled();
            // // kind of ok with effects being thickk since most text is not effect based
            // // will need to compute bounds probably, dont see how to avoid it
            // // only store data for renderable characters
            // // might mean keeping a render index for characters 
            // CharSpan.GetCharacter(i);
            // if (renderable) {
            //     continue
            // }
            //
            // CharSpan.SetCharacterAt(i, v);
            // CharSpan.SetSymbolAt(i, xx);
            //
            // for (int i = 0; i < textRenderInfo.size; i++) {
            //     
            //     if (!CharacterInfo.isRenderable) {
            //         continue;
            //     }
            //
            //     CharSpan.renderedCharacters.add(i);
            //     CharSpan.effects[i] = new TextEffectInfo() {
            //         matrix = matrix,
            //         colorFade = 0.5f,
            //         pivot = new float2(x, y)
            //     };
            //     // 6 + 2 = 8 floats 
            //     // verse 8 floats for vertices
            //         
            // }


        public struct TextMeshDesc {

            public int count;
            public RenderedCharacterInfo* textRenderInfo;

        }
        
        internal void DrawSingleSpanUniformTextInternal(RenderedCharacterInfo * textRenderInfo, int count, in AxisAlignedBounds2D bounds, in TextMaterialSetup materialSetup) {

            drawList.Add(new DrawInfo2() {
                matrix = defaultMatrix,
                drawType = DrawType2.UIForiaText, // todo -- generate geometry instead if material id is not what we expect
                materialId = MaterialId.UIForiaSDFText,
                localBounds = bounds,
                // could consider making these different allocators to keep similar types together
                materialData = stackAllocator.Allocate(materialSetup), 
                shapeData = stackAllocator.Allocate(new TextMeshDesc() {
                    textRenderInfo = textRenderInfo,
                    count = count
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

    }

}