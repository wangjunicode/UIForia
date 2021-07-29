using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Prototype;
using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIForia {

    internal unsafe class PrototypeTextRender {

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector4> texCoords = new List<Vector4>();
        private List<int> indices = new List<int>();

        private DataList<float> fontSizeStack;
        private DataList<ushort> fontIdStack;
        private DataList<GlyphInfo> infos;
        private DataList<GlyphPosition> positions;

        public bool enableLowerCaseScale = true;
        public Camera textCamera;
        public Material material;
        public Texture2D fontTexture;
        private Color[] palette = new Color[128];

        private static readonly int s_ColorPalette = Shader.PropertyToID("_ColorPalette");
        private static readonly int s_MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int s_FontColor = Shader.PropertyToID("_FontColor");
        private static readonly int s_BgColor = Shader.PropertyToID("_BGColor");
        private static readonly int s_SDFTextureSize = Shader.PropertyToID("_SDFTextureSize");

        public PrototypeTextRender(Camera camera) {
            this.textCamera = camera;
            this.material = Resources.Load<Material>("TextPrototypeTmp");
            this.fontTexture = Resources.Load<Texture2D>("RobotoTmp");
            this.fontIdStack = new DataList<ushort>(8, Allocator.Persistent);
            this.fontSizeStack = new DataList<float>(8, Allocator.Persistent);
            this.infos = new DataList<GlyphInfo>(128, Allocator.Persistent);
            this.positions = new DataList<GlyphPosition>(128, Allocator.Persistent);
        }

        ~PrototypeTextRender() {
            fontIdStack.Dispose();
            fontSizeStack.Dispose();
            infos.Dispose();
            positions.Dispose();
        }

        public void Reset(TextLayoutInfo layoutInput) {
            fontIdStack.size = 0;
            fontIdStack.Add((ushort) layoutInput.fontAssetId.id);
            fontSizeStack.Add(layoutInput.fontSize);
            vertices.Clear();
            texCoords.Clear();
            indices.Clear();
            infos.size = 0;
            positions.size = 0;
        }

        public void DrawText(TextShapeCache shapeCache, Mesh textMesh, ref TextDataTable textDataTable, TextId textId, in float4x4 matrix, CommandBuffer commandBuffer, Color textColor) {
            int glyphOffset = 0;

            ref TextDataEntry entry = ref textDataTable.GetEntry(textId);
            TextLayoutInfo layoutInput = entry.layoutInfo;
            TextLayoutOutput textLayoutOutput = entry.layoutOutput;

            textLayoutOutput.ellipsisRunIndex = -1; // todo -- make this real via layout  

            Reset(entry.layoutInfo);

            ref DataList<SDFFontUnmanaged> fontTable = ref textDataTable.fontTable;
            SDFFontUnmanaged baseFont = fontTable[entry.layoutInfo.fontAssetId.id];

            FontMetrics fontMetrics = FontMetrics.Create(baseFont, fontSizeStack.GetLast());
            float falloff = baseFont.falloff * fontMetrics.scaleTexturePxToMetrics;
            SDFFontUnmanaged currentFont = baseFont;
            float scale = 2f * falloff * fontMetrics.pixelSize;

            int dataOffset = 0;
            int colorIndex = 0;

            palette[0] = textColor;
            int ellipsisVertexOffset = -1;
            int ellipseGlyphStart = -1;
            int ellipseGlyphEnd = -1;
            int runIndex = 0;

            float penOffsetY = 0; // -entry.layoutOutput.yOffset;

            int maxLineIndex = layoutInput.maxLineCount <= 0 ? int.MaxValue : layoutInput.maxLineCount;

            CheckedArray<TextSymbol> symbolBuffer = entry.GetSymbols();
            CheckedArray<char> dataBuffer = entry.GetDataBuffer();
            CheckedArray<TextCharacterRun> runList = entry.runList;

            for (int i = 0; i < runList.size; i++) {
                ref TextCharacterRun run = ref runList.Get(i);
                if ((run.flags & CharacterRunFlags.Characters) != 0) {
                    ShapeCacheValue shapeEntry = shapeCache.GetEntryAt(run.shapeResultIndex);

                    run.GlyphCount = (ushort) shapeEntry.glyphCount; // todo -- find this another home

                    // glyphCounts.Add(shapeEntry.glyphCount)
                    positions.AddRange(shapeEntry.Positions.array, shapeEntry.glyphCount);
                    infos.AddRange(shapeEntry.glyphInfos, shapeEntry.glyphCount);
                }
            }

            for (int symbolIndex = 0; symbolIndex < symbolBuffer.size; symbolIndex++) {

                TextSymbol symbol = symbolBuffer[symbolIndex];

                switch (symbol.symbolType) {

                    case SymbolType.InlineSpace: {
                        runIndex++;
                        break;
                    }

                    case SymbolType.HorizontalSpace: {
                        runIndex++;
                        break;
                    }

                    case SymbolType.PushCursor: {
                        palette[++colorIndex] = Color.red;
                        if (symbol.metaInfo >= 0) {
                            ApplyMidRunColor(ref runList.array[runIndex - 1], glyphOffset, colorIndex, infos,symbol.metaInfo);
                        }
                        break;
                    }
                    case SymbolType.PopCursor: {
                        palette[++colorIndex] = textColor;
                        if (symbol.metaInfo >= 0) {
                            ApplyMidRunColor(ref runList.array[runIndex - 1], glyphOffset, colorIndex, infos,symbol.metaInfo);
                        }
                        break;
                    }
                    case SymbolType.PushColor: {
                        // assume we have an index or can somehow compute it from symbols
                        // todo -- find in palette 

                        palette[++colorIndex] = *(Color*) dataBuffer.GetPointer(dataOffset);

                        if (symbol.metaInfo >= 0) {
                            ApplyMidRunColor(ref runList.array[runIndex - 1], glyphOffset, colorIndex, infos, symbol.metaInfo);
                        }

                        break;
                    }

                    case SymbolType.PopColor: {

                        if (colorIndex == 0) {
                            break;
                        }

                        colorIndex--;
                        if (symbol.metaInfo >= 0) {
                            ApplyMidRunColor(ref runList.array[runIndex - 1], glyphOffset, colorIndex, infos, symbol.metaInfo);
                        }

                        break;
                    }

                    case SymbolType.PushFontSize: {
                        float fontSize = *(float*) dataBuffer.GetPointer(dataOffset);
                        fontSizeStack.Add(fontSize);
                        fontMetrics = FontMetrics.Create(currentFont, fontSize);
                        falloff = currentFont.falloff * fontMetrics.scaleTexturePxToMetrics;
                        scale = 2f * falloff * fontMetrics.pixelSize;
                        break;
                    }

                    case SymbolType.PopFontSize: {
                        if (fontSizeStack.size > 1) {
                            fontSizeStack.Pop();
                            fontMetrics = FontMetrics.Create(currentFont, fontSizeStack.GetLast());
                            falloff = currentFont.falloff * fontMetrics.scaleTexturePxToMetrics;
                            scale = 2f * falloff * fontMetrics.pixelSize;
                        }

                        break;
                    }

                    case SymbolType.PushFont: {
                        // todo
                        currentFont = fontTable[fontIdStack.GetLast()];
                        fontMetrics = FontMetrics.Create(currentFont, fontSizeStack.GetLast());
                        falloff = currentFont.falloff * fontMetrics.scaleTexturePxToMetrics;
                        scale = 2f * falloff * fontMetrics.pixelSize;
                        break;
                    }

                    case SymbolType.PopFont: {
                        if (fontIdStack.size > 0) {
                            fontIdStack.Pop();
                            currentFont = fontTable[fontIdStack.GetLast()];
                            fontMetrics = FontMetrics.Create(currentFont, fontSizeStack.GetLast());
                            falloff = currentFont.falloff * fontMetrics.scaleTexturePxToMetrics;
                            scale = 2f * falloff * fontMetrics.pixelSize;
                        }

                        break;
                    }

                    case SymbolType.CharacterGroup: {

                        if (symbol.metaInfo == TextSymbol.k_EmptyCharacterGroup) break;

                        for (int r = runIndex; r < runList.size; r++) {

                            ref TextCharacterRun run = ref runList.Get(r);

                            if (textLayoutOutput.ellipsisRunIndex != -1 && ellipsisVertexOffset != -1) {
                                goto loop_escape;
                            }

                            if (textLayoutOutput.ellipsisRunIndex == r) {
                                ellipsisVertexOffset = vertices.Count;
                                ellipseGlyphStart = glyphOffset;
                                ellipseGlyphEnd = glyphOffset + runList[r].GlyphCount;
                            }

                            // traverse symbols
                            // hit test each run
                            // if hit passed
                            // figure out if spanning multiple hit regions
                            // if so do lookup like we handle color-swap mid word

                            if (run.IsRendered && run.lineIndex < maxLineIndex) {

                                float localX = run.x;
                                
                                int glyphStart = glyphOffset;
                                int glyphEnd = glyphOffset + run.GlyphCount;
                                glyphOffset += run.GlyphCount;

                                for (int gIdx = glyphStart; gIdx < glyphEnd; gIdx++) {

                                    // todo -- pre-fetch glyph infos 

                                    // note -- hitting this case is probably a bug!
                                    if (!currentFont.TryGetGlyph(infos[gIdx].glyphIndex, out SDFGlyphInfo sdfInfo)) {
                                        // if glyph missing we can still advance as though it were there
                                        // or we render the 'missing' glyph? maybe harfbuzz does this already?
                                        localX += positions[gIdx].advanceX;
                                        continue;
                                    }

                                    float scaleX = fontMetrics.pixelSize;
                                    float scaleY = enableLowerCaseScale && (sdfInfo.flags & 1) == 1 ? fontMetrics.lowScale : fontMetrics.capScale;

                                    // todo -- scaleXY, falloff and scaleTexturePxToMetrics should probably be shader inputs so we can use the same code for bitmap and sdf text 
                                    
                                    float top = penOffsetY + -run.y + scaleY * (sdfInfo.topBearing + falloff);
                                    float left = localX + scaleX * (sdfInfo.leftBearing - falloff);
                                    float bottom = top - scaleY * fontMetrics.scaleTexturePxToMetrics * (sdfInfo.bottom - sdfInfo.top);
                                    float right = left + scaleX * fontMetrics.scaleTexturePxToMetrics * (sdfInfo.right - sdfInfo.left);
                                    
                                    // todo -- move some of this to vertex shader for faster rendering & less data (StructuredBuffer / DrawIndirect) 
                                    vertices.Add(new Vector3(left, bottom, scale));
                                    vertices.Add(new Vector3(right, bottom, scale));
                                    vertices.Add(new Vector3(right, top, scale));
                                    vertices.Add(new Vector3(left, top, scale));

                                    texCoords.Add(new Vector4(sdfInfo.left, 1 - sdfInfo.bottom, colorIndex, 0));
                                    texCoords.Add(new Vector4(sdfInfo.right, 1 - sdfInfo.bottom, colorIndex, 0));
                                    texCoords.Add(new Vector4(sdfInfo.right, 1 - sdfInfo.top, colorIndex, 0));
                                    texCoords.Add(new Vector4(sdfInfo.left, 1 - sdfInfo.top, colorIndex, 0));

                                    localX += positions[gIdx].advanceX;

                                }

                            }

                            if (run.IsLastInGroup) {
                                runIndex = r + 1;
                                break;
                            }

                        }

                        break;
                    }

                }

                dataOffset += symbol.dataLength;
            }

            loop_escape:

            int quadCount = vertices.Count / 4;

            if (ellipsisVertexOffset != -1) {
                quadCount -= (vertices.Count - ellipsisVertexOffset) / 4;
            }

            int quadIndex = 0;
            for (int i = 0; i < quadCount; i++) {

                indices.Add(quadIndex + 0);
                indices.Add(quadIndex + 1);
                indices.Add(quadIndex + 2);
                indices.Add(quadIndex + 2);
                indices.Add(quadIndex + 3);
                indices.Add(quadIndex + 0);

                quadIndex += 4;

            }

            if (ellipsisVertexOffset != -1) {

                float maxExtents = 9999; // todo -- use maxWidth;

                ref TextCharacterRun run = ref runList.Get(textLayoutOutput.ellipsisRunIndex);

                float localX = run.x;

                if (baseFont.TryGetGlyphFromCodepoint(0x00002026, out SDFGlyphInfo sdfInfo)) {
                    maxExtents -= sdfInfo.advanceX * fontMetrics.pixelSize;
                }

                int lastQuadIndex = quadIndex;
                for (int gIdx = ellipseGlyphStart; gIdx < ellipseGlyphEnd; gIdx++) {

                    if (localX + positions[gIdx].advanceX < maxExtents) {

                        indices.Add(quadIndex + 0);
                        indices.Add(quadIndex + 1);
                        indices.Add(quadIndex + 2);
                        indices.Add(quadIndex + 2);
                        indices.Add(quadIndex + 3);
                        indices.Add(quadIndex + 0);

                        localX += positions[gIdx].advanceX;
                        lastQuadIndex = quadIndex;
                        quadIndex += 4;
                    }
                    else {
                        // bump quadIndex by remaining un-used glyph count
                        quadIndex += 4 * (ellipseGlyphEnd - gIdx);
                        break;
                    }

                }

                if (lastQuadIndex >= texCoords.Count) {
                    lastQuadIndex = texCoords.Count - 1;
                }

                int ellipsisColorIndex = (int) texCoords[lastQuadIndex].z;

                float scaleX = fontMetrics.pixelSize;
                float scaleY = fontMetrics.capScale;

                // todo -- scaleXY, falloff and scaleTexturePxToMetrics should probably be shader inputs so we can use the same code for bitmap and sdf text 
                float top = penOffsetY + -run.y + scaleY * (sdfInfo.topBearing + falloff);
                float left = localX + scaleX * (sdfInfo.leftBearing - falloff);
                float bottom = top - scaleY * fontMetrics.scaleTexturePxToMetrics * (sdfInfo.bottom - sdfInfo.top);
                float right = left + scaleX * fontMetrics.scaleTexturePxToMetrics * (sdfInfo.right - sdfInfo.left);

                vertices.Add(new Vector3(left, bottom, scale));
                vertices.Add(new Vector3(right, bottom, scale));
                vertices.Add(new Vector3(right, top, scale));
                vertices.Add(new Vector3(left, top, scale));

                texCoords.Add(new Vector4(sdfInfo.left, 1 - sdfInfo.bottom, ellipsisColorIndex, 0));
                texCoords.Add(new Vector4(sdfInfo.right, 1 - sdfInfo.bottom, ellipsisColorIndex, 0));
                texCoords.Add(new Vector4(sdfInfo.right, 1 - sdfInfo.top, ellipsisColorIndex, 0));
                texCoords.Add(new Vector4(sdfInfo.left, 1 - sdfInfo.top, ellipsisColorIndex, 0));

                indices.Add(quadIndex + 0);
                indices.Add(quadIndex + 1);
                indices.Add(quadIndex + 2);
                indices.Add(quadIndex + 2);
                indices.Add(quadIndex + 3);
                indices.Add(quadIndex + 0);

                quadIndex += 4;

            }

            material.SetColorArray(s_ColorPalette, palette);
            material.SetTexture(s_MainTex, fontTexture);
            material.SetColor(s_FontColor, Color.black);
            material.SetColor(s_BgColor, Color.white);
            material.SetVector(s_SDFTextureSize, new Vector4(fontTexture.width, fontTexture.height, 0, 0));

            textMesh.Clear(true);
            textMesh.SetVertices(vertices);
            textMesh.SetUVs(0, texCoords);
            textMesh.SetTriangles(indices, 0);

            if (commandBuffer == null) {
                material.SetPass(0);
                textCamera.orthographic = true;
                textCamera.orthographicSize = Screen.height * 0.5f;
                Vector3 position = textCamera.transform.position;
                textCamera.transform.position = new Vector3(Screen.width / 2f, -Screen.height / 2f, -2f);
                Graphics.DrawMeshNow(textMesh, matrix);
                textCamera.transform.position = position;
            }
            else {
                commandBuffer.DrawMesh(textMesh, matrix, material, 0, 0);
            }
        }

        private void ApplyMidRunColor(ref TextCharacterRun run, int glyphOffset, int colorIndex, DataList<GlyphInfo> infos, int clusterIndex) {

            const int k_PointsPerQuad = 4;

            glyphOffset -= run.GlyphCount;
            int baseOffset = vertices.Count - (run.GlyphCount * k_PointsPerQuad);

            for (int gIdx = 0, vIdx = baseOffset; gIdx < run.GlyphCount; gIdx++, vIdx += k_PointsPerQuad) {

                uint cluster = infos[glyphOffset + gIdx].cluster;

                if (cluster < clusterIndex) {
                    continue;
                }

                Vector4 v0 = texCoords[vIdx + 0];
                Vector4 v1 = texCoords[vIdx + 1];
                Vector4 v2 = texCoords[vIdx + 2];
                Vector4 v3 = texCoords[vIdx + 3];

                texCoords[vIdx + 0] = new Vector4(v0.x, v0.y, colorIndex, v0.w);
                texCoords[vIdx + 1] = new Vector4(v1.x, v1.y, colorIndex, v1.z);
                texCoords[vIdx + 2] = new Vector4(v2.x, v2.y, colorIndex, v2.z);
                texCoords[vIdx + 3] = new Vector4(v3.x, v3.y, colorIndex, v3.z);
            }
        }

    }

}