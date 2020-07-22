using UIForia.Rendering;
using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Graphics {

    internal unsafe struct TextMeshSpanInfo {

        public TextInfoRenderData* textInfo;
        public int symbolStart;
        public int symbolEnd;
        public int fontAssetId;
        public float3 scaleRatios;
        public float padding;
        public float widthScale;
        public float heightScale;
        public bool requiresBaking;
        public bool requiresCaching;
        public UIForiaVertexCache* vertexCache;

    }
    
    [BurstCompile]
    internal unsafe struct BakeUIForiaText : IJob {

        public DataList<DrawInfo2>.Shared drawList;
        public DataList<int>.Shared triangleList;
        public DataList<UIForiaVertex>.Shared vertexList;
        public DataList<MeshInfo> meshInfoList;

        // ~36 bytes of style data
        // + 4 effect id
        // + 4 glyph id
        // + 2 font id
        
        // 80-128 bytes for effect info vertices
        
        // at some point in render ill need to:
            // check if render state for text is dirty
            // this is by span I think, but might be best to re-run the whole tree
            // if dirty run through and re-assign material and font asset indices
            // output should be a list of material infos and all rendering characters have a material id assigned
            // this might just be `GenerateRenderCharacters()`
            // one step further would also build vertices for those characters
            // on the one hand its really not that bad for small text infos
            // on the other hand, can take ~0.2ms for large ones
            // threading helps, not always better, then we'd need re-merge and offset which isnt very appealing
            // could maybe store vs recompute at some threshold
            
            // this is probably a second pass, possibly can cache for larger inputs
            
            // when render spans are dirty OR text was laid out (implicitly laid out on change so we dont need to detect change, unless text is immediate mode?)
            // i need to:
            //    recompute draw infos
            //    recompute underlines & strikethrough
            //    break lines into draw infos if non uniform
            //    compute effect bounds, possibly as their own draw info
            //    no culling happens here. large texts will be per-word culled later maybe
            //    can be done in parallel, need enough number space for this to run and still have sort work properly
            // when text has effects associated i may be able to re-use the material buffer for some data
            
            // charInfo.TransformDefault(matrix);
            
        public void Execute2() {
            for (int i = 0; i < drawList.size; i++) {

                ref DrawInfo2 drawInfo = ref drawList[i];

                if (drawInfo.drawType != DrawType2.UIForiaText) {
                    continue;
                }
                
                RenderContext3.TextMeshDesc desc = *(RenderContext3.TextMeshDesc*) drawInfo.shapeData;
                
                // maybe assumes no underline?
                vertexList.EnsureAdditionalCapacity(desc.count * 4);
                triangleList.EnsureAdditionalCapacity(desc.count * 6);
                
                ref MeshInfo meshInfo = ref meshInfoList[i];
                meshInfo.type = MeshInfoType.Text;
                
                int effectOffset = 0;
                int materialOffset = 0;
                int* triangles = triangleList.GetArrayPointer();
                UIForiaVertex* vertices = vertexList.GetArrayPointer();
                
                int triangleIndex = triangleList.size;
                int vertexIndex = vertexList.size;
                meshInfo.triangleStart = triangleIndex;
                meshInfo.vertexStart = vertexIndex;
                
                for (int c = 0; c < desc.count; c++) {
                    
                    ref RenderedCharacterInfo info = ref desc.textRenderInfo[c];

                    if (info.effectIndex != 0) {
                                
                    }
                                        
                    ref UIForiaVertex vertex = ref vertices[c];
                    
                    vertex.position.x = info.position.x;
                    vertex.position.y = -info.position.y;

                    vertex.texCoord0.x = info.renderedGlyphIndex;
                    vertex.texCoord0.y = info.effectIndex;
                    
                    // except for underline I know width and height implicitly
                    vertex.texCoord1.x = info.width;
                    vertex.texCoord1.y = info.height;

                    // todo -- i dont need this, can do it when building the index buffer in a single pass since all quads are submitted with a single vertex now
                    triangles[triangleIndex + 0] = c;
                    triangles[triangleIndex + 1] = BitUtil.SetHigh1Low3(1, c);
                    triangles[triangleIndex + 2] = BitUtil.SetHigh1Low3(2, c);
                    triangles[triangleIndex + 3] = BitUtil.SetHigh1Low3(2, c);
                    triangles[triangleIndex + 4] = BitUtil.SetHigh1Low3(3, c);
                    triangles[triangleIndex + 5] = c;
               
                    vertexIndex += 4;
                    triangleIndex += 6;
                }

                meshInfo.vertexCount = desc.count; //vertexIndex - meshInfo.vertexStart;
                meshInfo.triangleCount = triangleIndex - meshInfo.triangleStart;

                vertexList.size += desc.count;
                triangleList.size = triangleIndex;

            }
        }
        
        public void Execute() {
        
            Execute2();
            return;
            
            for (int i = 0; i < drawList.size; i++) {

                ref DrawInfo2 drawInfo = ref drawList[i];

                if (drawInfo.drawType != DrawType2.UIForiaText) {
                    continue;
                }

                ref MeshInfo meshInfo = ref meshInfoList[i];

                TextMeshSpanInfo* desc = (TextMeshSpanInfo*) drawInfo.shapeData;

                meshInfo.type = MeshInfoType.Text;

                meshInfo.triangleStart = triangleList.size;
                meshInfo.vertexStart = vertexList.size;

                if (desc->requiresBaking || desc->vertexCache == null) {
                    Bake(*desc);
                    meshInfo.vertexCount = vertexList.size - meshInfo.vertexStart;
                    meshInfo.triangleCount = triangleList.size - meshInfo.triangleStart;
                    if (desc->requiresCaching && desc->vertexCache != null) {
                        // write back to cache
                        desc->vertexCache->vertices.AddRange(vertexList.GetArrayPointer() + meshInfo.vertexStart, meshInfo.vertexCount);
                        desc->vertexCache->triangles.AddRange(triangleList.GetArrayPointer() + meshInfo.triangleStart, meshInfo.triangleCount);
                    }
                }
                else {
                    vertexList.AddRange(desc->vertexCache->vertices.array, desc->vertexCache->vertices.size);
                    triangleList.AddRange(desc->vertexCache->triangles.array, desc->vertexCache->triangles.size);
                    meshInfo.vertexCount = vertexList.size - meshInfo.vertexStart;
                    meshInfo.triangleCount = triangleList.size - meshInfo.triangleStart;
                }

            }
        }
        
        // the more i can do in layout the better since it runs less frequently and i want to keep graphics as streamlined as possible
        // with layout utilities i can borrow the functionality for immediate mode
        // worst case I can push extra material infos for underline segments
        // positions will already be correct, widths wont be.
        // can use rect buffer for this, probably the easiest but still have to know to push one in it
        // somehow need to encode the width. could use word buffer? if i have one that may be best, still need to know width in addition to all text data
        
        // need to know which index in underline we are in the shader
        // could use real vertices but then i need to know the type of char which i dont want to do
        // alternative might be to store all vertices
        // on the one hand then I dont have to build them 
        // on the other hand i store a lot more data
        // could use different shader. feels bad
        
        // layout could handle underlines pretty easily by injecting a render character info with a type
        // would i need 3 characters to stretch it? probably, also need a flag for vs to know how to behave :/
        
        // layout sets wordOffset, line index. i think thats it
        // the rest should be setup when updating text / span
        // if we update a span do we have a material list per span or just re-process the text block? 
        // list per span is faster but requires full reprocess. probably fine to re-process tbh, dont expect huge text blocks

        public void BakeUniformText(in TextMeshSpanInfo spanInfo) {
                 
        }

        public void Bake(in TextMeshSpanInfo spanInfo) {
            
            ref TextMeshSpanInfo textRenderInfo = ref UnsafeUtilityEx.AsRef<TextMeshSpanInfo>(spanInfo.textInfo);

            TextSymbol* symbolArray = textRenderInfo.textInfo->symbolList.array;
            TextLayoutSymbol* layoutSymbolArray = textRenderInfo.textInfo->layoutSymbolList.array;

            int start = textRenderInfo.symbolStart;
            int end = textRenderInfo.symbolEnd;
            
            vertexList.EnsureAdditionalCapacity(4 * (end - start));
            
            float padding = textRenderInfo.padding;
            
            int vertexIndex = vertexList.size;
            int vertexStart = vertexIndex;
            
            int triangleIndex = triangleList.size;

            UIForiaVertex* vertices = vertexList.GetArrayPointer();
            
            for (int i = start; i < end; i++) {
                ref TextSymbol symbol = ref symbolArray[i];

                if (symbol.type != TextSymbolType.Character) {
                    continue;
                }

                // todo -- skip other non printing characters
                if (symbol.charInfo.character == 32 || symbol.charInfo.character == 10) {
                    continue;
                }
                //
                // ref BurstCharInfo charInfo = ref symbol.charInfo;
                // ref WordInfo wordInfo = ref layoutSymbolArray[charInfo.wordIndex].wordInfo;
                //
                // ref UIForiaVertex v0 = ref vertices[vertexIndex + 0];
                // ref UIForiaVertex v1 = ref vertices[vertexIndex + 1];
                // ref UIForiaVertex v2 = ref vertices[vertexIndex + 2];
                // ref UIForiaVertex v3 = ref vertices[vertexIndex + 3];
                //
                // float charX = wordInfo.x + (charInfo.topLeft.x - padding);
                // float charY = wordInfo.y + (charInfo.topLeft.y - padding);
                //
                // float charWidth = (charInfo.bottomRight.x - charInfo.topLeft.x) + (padding * 2);
                // float charHeight = (charInfo.bottomRight.y - charInfo.topLeft.y) + (padding * 2);
                //
                // v0.position.x = charX + (charInfo.shearTop);
                // v0.position.y = -charY;
                // v0.texCoord0.x = (charInfo.topLeftUv.x - padding) * spanInfo.widthScale;
                // v0.texCoord0.y = (charInfo.bottomRightUv.y + padding) *  spanInfo.heightScale;
                // v0.texCoord1.x = 0;
                // v0.texCoord1.y = 1;
                //
                // v1.position.x = charX + charWidth + (charInfo.shearTop);
                // v1.position.y = -charY;
                // v1.texCoord0.x = (charInfo.bottomRightUv.x + padding) *  spanInfo.widthScale;
                // v1.texCoord0.y = v0.texCoord0.y;
                // v1.texCoord1.x = 1;
                // v1.texCoord1.y = 1;
                //
                // v2.position.x = charX + charWidth + (charInfo.shearBottom);
                // v2.position.y = -(charY + charHeight);
                // v2.texCoord0.x = v1.texCoord0.x;
                // v2.texCoord0.y = (charInfo.topLeftUv.y - padding) * spanInfo.heightScale;
                // v2.texCoord1.x = 1;
                // v2.texCoord1.y = 0;
                //
                // v3.position.x = charX + (charInfo.shearBottom);
                // v3.position.y = -(charY + charHeight);
                // v3.texCoord0.x = v0.texCoord0.x;
                // v3.texCoord0.y = v2.texCoord0.y;
                // v3.texCoord1.x = 0;
                // v3.texCoord1.y = 0;
                //
                vertexIndex += 4;

            }

            int vertexCount = vertexIndex - vertexStart;
            triangleList.EnsureAdditionalCapacity((vertexCount % 4) * 6);
            int* triangles = triangleList.GetArrayPointer();
            
            for (int i = 0; i < vertexCount; i += 4) {
                triangles[triangleIndex + 0] = i + 0;
                triangles[triangleIndex + 1] = i + 1;
                triangles[triangleIndex + 2] = i + 2;
                triangles[triangleIndex + 3] = i + 2;
                triangles[triangleIndex + 4] = i + 3;
                triangles[triangleIndex + 5] = i + 0;
                triangleIndex += 6;     
            }

            triangleList.size = triangleIndex;
            vertexList.size = vertexIndex;

            ComputeCharacterUVMapping();
            
        }

        private void ComputeCharacterUVMapping() {
            // todo -- map char uvs across lines, text block, words, etc
        }

    }

}