using UIForia.Text;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Graphics {

    [BurstCompile]
    internal unsafe struct BakeUIForiaText : IJob {

        public DataList<DrawInfo2>.Shared drawList;
        public DataList<UIForiaVertex>.Shared vertexList;
        public DataList<MeshInfo> meshInfoList;
        public DataList<UIForiaMaterialInfo>.Shared materialBuffer;
        public DataList<float4>.Shared float4Buffer;
        public DataList<TextEffectInfo> textEffectBuffer;

        public void Execute() {

            for (int i = 0; i < drawList.size; i++) {

                ref DrawInfo2 drawInfo = ref drawList[i];

                if (drawInfo.drawType != DrawType2.UIForiaText) {
                    continue;
                }

                // if culled, continue
                // I'm assuming culling already happened, maybe I shouldnt be

                RenderContext3.TextMeshDesc desc = *(RenderContext3.TextMeshDesc*) drawInfo.shapeData;

                // maybe assumes no underline? 
                vertexList.EnsureAdditionalCapacity(desc.count);

                ref MeshInfo meshInfo = ref meshInfoList[i];
                meshInfo.type = MeshInfoType.Text;

                UIForiaVertex* vertices = vertexList.GetArrayPointer();

                int vertexIndex = vertexList.size;
                meshInfo.vertexStart = vertexIndex;

                TextMaterialSetup* setup = (TextMaterialSetup*) drawInfo.materialData;
                TextMaterialInfo* materialPtr = desc.materialPtr;
                if (true || setup->requiresMaterialScan) {

                    int lastIdx = -1;
                    int materialIdx = -1;

                    int symbolIdx = desc.start;

                    while (symbolIdx != -1) {
                        ref TextSymbol symbol = ref desc.textRenderInfo[symbolIdx];
                        ref BurstCharInfo info = ref symbol.charInfo;

                        int idx = info.materialIndex;
                        int effectIdx = 0;

                        if (idx != lastIdx) {
                            lastIdx = idx;
                            materialIdx = materialBuffer.size;
                            // todo -- actually do hash resolution here. can profile running the hash up front or not. if done up front it'll run in managed code after effects
                            materialBuffer.Add(new UIForiaMaterialInfo() {
                                textMaterial = materialPtr[idx]
                            });
                        }

                        if (info.effectIdx != 0) {
                            effectIdx = float4Buffer.size;
                            float4Buffer.EnsureAdditionalCapacity(4);
                            ref TextEffectInfo effectInfo = ref textEffectBuffer[info.effectIdx];
                            float4Buffer.Add(effectInfo.topLeftData);
                            float4Buffer.Add(effectInfo.topRightData);
                            float4Buffer.Add(effectInfo.bottomLeftData);
                            float4Buffer.Add(effectInfo.bottomRightData);
                        }

                        ref UIForiaVertex vertex = ref vertices[vertexIndex++];

                        vertex.position.x = info.renderPosition.x;
                        vertex.position.y = -info.renderPosition.y; // todo -- move line ascender offset here or bake directly into position so I dont have to upload it

                        vertex.texCoord0.x = info.scale;
                        vertex.texCoord0.y = 0; // reserve for z position or other float. still need uv transform index somewhere
                        
                        // probably dont need more than 12 bits for font asset id
                        // can probably use font asset to store offset for glyph id, giving me more bit range
                        // matrix and clip idx set later on in the x slot
                        
                        // todo -- premultiply base textInfo opacity with opacity value before packing
                        
                        // still need:
                            // soft mask data (maybe?)
                            // clip data (float4 buffer) 
                            // uvtransform data (float4 buffer?)
                            // dont mind making vertex a little larger but needs to be aligned to float4, so im adding a full float4 or nothing
                        
                        // could also pre-multiply opacity modifier by the base element opacity and make that be per-character. could also cull when 0 or close to it
                        uint displayAndOpacity = (uint)((byte) info.displayFlags << 8) | info.opacityMultiplier;
                        vertex.indices.y = (uint) ((displayAndOpacity << 16) | (materialIdx & 0xffff)); //(uint)BitUtil.SetHighLowBits(effectIdx, outputIdx); // todo -- set effect idx
                        vertex.indices.z = (uint) ((info.glyphIndex << 16) | (desc.fontAssetId & 0xffff)); // BitUtil.SetHighLowBits(info.renderedGlyphIndex, (uint)desc.fontAssetId);
                        vertex.indices.w = (uint)effectIdx; // ushort is too small for this but 3 bytes could work if I need 1 free byte here, could move opacity here and use a 3 bytes for material id
                        symbolIdx = info.nextRenderIdx;
                    }

                }
                else {

                    // todo -- implement this later, for fast case when we dont need to check materials or effects
                    // int outputIdx;
                    // int startIdx = desc.textRenderInfo[0].materialIndex;
                    //
                    // if (UnsafeUtility.MemCmp(desc.materialPtr + startIdx, materialBuffer.GetArrayPointer() + (materialBuffer.size - 1), sizeof(TextMaterialInfo)) == 0) {
                    //     outputIdx = materialBuffer.size - 1;
                    // }
                    // else {
                    //     outputIdx = materialBuffer.size;
                    //     materialBuffer.Add(new UIForiaMaterialInfo() {
                    //         textMaterial = desc.materialPtr[startIdx]
                    //     });
                    // }
                    //
                    // // there is no effect data in this case.
                    // uint indicesY = (uint) BitUtil.SetHighLowBits(outputIdx, 0);
                    //
                    // for (int c = 0; c < desc.count; c++) {
                    //
                    //     ref RenderedCharacterInfo info = ref desc.textRenderInfo[c];
                    //
                    //     ref UIForiaVertex vertex = ref vertices[c];
                    //
                    //     vertex.position.x = info.position.x;
                    //     vertex.position.y = -info.position.y; // todo -- bake line ascender offset here
                    //
                    //     vertex.texCoord0.x = info.renderedGlyphIndex;
                    //     vertex.texCoord0.y = info.lineAscender;
                    //
                    //     // matrix and clip idx set later on
                    //     vertex.indices.y = indicesY;
                    // }
                }

                meshInfo.vertexCount = desc.count;
                vertexList.size = vertexIndex;

            }
        }

    }

}

// for (int c = 0; c < desc.count; c++) {
//     ref TextSymbol symbol = ref desc.textRenderInfo[c];
//
//     if (symbol.type != TextSymbolType.Character) {
//         continue;
//     }
//
//
//     ref BurstCharInfo info = ref symbol.charInfo;
//     
//     // might be better if i use a pointer for the offset instead
//     int idx = info.materialIndex;
//     int effectIdx = 0;
//
//     if (idx != lastIdx) {
//         lastIdx = idx;
//         outputIdx = materialBuffer.size;
//         // todo -- actually do hash resolution here. can profile running the hash up front or not. if done up front it'll run in managed code after effects
//         materialBuffer.Add(new UIForiaMaterialInfo() {
//             textMaterial = idx >= 0 ? materialPtr[idx] : default // todo -- if idx < 0 it points to a global material (for typewriting)
//         });
//     }
//
//     if (info.effectIdx != 0) {
//         effectIdx = float4Buffer.size;
//         float4Buffer.EnsureAdditionalCapacity(4);
//         ref TextEffectInfo effectInfo = ref textEffectBuffer[info.effectIdx];
//         float4Buffer.Add(effectInfo.topLeftData);
//         float4Buffer.Add(effectInfo.topRightData);
//         float4Buffer.Add(effectInfo.bottomRightData);
//         float4Buffer.Add(effectInfo.bottomLeftData);
//     }
//
//     ref UIForiaVertex vertex = ref vertices[c];
//
//     vertex.position.x = info.position.x;
//     vertex.position.y = -info.position.y; // todo -- move line ascender offset here or bake directly into position so I dont have to upload it
//
//     vertex.texCoord0.x = info.glyphIndex;  // todo -- not used anymore, can remove this 
//     vertex.texCoord0.y = info.maxAscender; // todo -- remove this 
//     // probably dont need more than 12 bits for font asset id
//     // can probably use font asset to store offset for glyph id, giving me more bit range
//     // matrix and clip idx set later on in the x slot
//     vertex.indices.y = (uint) ((effectIdx << 16) | (outputIdx & 0xffffff)); //(uint)BitUtil.SetHighLowBits(effectIdx, outputIdx); // todo -- set effect idx
//     vertex.indices.z = (uint) ((info.glyphIndex << 16) | (desc.fontAssetId & 0xffffff)); //BitUtil.SetHighLowBits(info.renderedGlyphIndex, (uint)desc.fontAssetId);
//     vertex.indices.w = (uint) (
//         ((byte) info.displayFlags << 24) |
//         (info.opacityMultiplier << 16)
//     ); //BitUtil.SetBytes((byte)info.displayFlags, info.opacityMultiplier, 0, 0);
//
// }