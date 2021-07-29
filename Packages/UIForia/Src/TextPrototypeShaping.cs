using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Prototype {

    public unsafe class TextPrototypeShaping : MonoBehaviour {

        public string content;
        public float penOffset = 20;
        public float penOffsetY = 20;
        public Camera textCamera;
        public Material material;
        public Texture fontTexture;
        public TextAsset robotoJson;
        public TextAsset pacificoJson;
        public bool enableLowerCaseScale;

        private SDFFont baseFont;

        private Mesh textMesh;

        private ushort robotoId;
        private ushort pacificoId;

        private StringTagger tagger;
        private ShapeContext shapeContext;
        private TextShapeCache shapeCache;

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector4> texCoords = new List<Vector4>();
        private List<int> indices = new List<int>();

        private RichText richText = new RichText();

        public TextLayoutInfo layoutInput;
        public float maxWidth = 280;

        private TextLayoutOutput textLayoutOutput;

        private DataList<SDFFontUnmanaged> fontTable;

        public void Start() {
            fontTable = new DataList<SDFFontUnmanaged>(8, Allocator.Persistent);
            shapeContext = new ShapeContext(ShapeLogSettings.None, message => Debug.Log(message));
            shapeCache = TextShapeCache.Create(Time.frameCount, 16);
            tagger = StringTagger.Create(Time.frameCount, 100);
            textMesh = new Mesh();
            textMesh.MarkDynamic();

            baseFont = JsonUtility.FromJson<SDFFont>(robotoJson.text);

            string dataPath = UnityEngine.Application.dataPath;
            string roboto = Path.Combine(dataPath, @"Fonts\Roboto-Regular.ttf");
            string pacifico = Path.Combine(dataPath, @"Fonts\Pacifico.ttf");

            robotoId = shapeContext.CreateFontFromFile(roboto);
            pacificoId = shapeContext.CreateFontFromFile(pacifico);

            // textLayoutOutput.results = new DataList<TextLayoutResult>.Shared(32, Allocator.Persistent);
            // textLayoutOutput.runList = new DataList<CharacterRun>.Shared(32, Allocator.Persistent);

            fontTable.Add(default);
            fontTable.Add(SDFFontUnmanaged.Create(baseFont));
            // fontTable.Add(SDFFontUnmanaged.Create(JsonUtility.FromJson<SDFFont>(pacificoJson.text)));
        }

        ~TextPrototypeShaping() {

            for (int i = 1; i < fontTable.size; i++) {
                fontTable[i].Dispose();
            }

            fontTable.Dispose();
            // textLayoutOutput.results.Dispose();
            // textLayoutOutput.runList.Dispose();

            shapeCache.Dispose();
            tagger.Dispose();
            richText.Dispose();
        }

        // em size 
        // space to layout into 
        // basic style info 
        // fonts
        // ellipsis handling 
        // content

        public struct InlineSpaceHeight {

            public float value;
            public InlineSpaceHeightType unit;

        }

        public enum InlineSpaceHeightType {

            Baseline = 0,
            Fixed,
            LineHeight,
            Descent,
            KeepAspect

        }

        // width = fixed, stretch, keep aspect 
        public void Update() {

            content = Regex.Unescape(content);

            richText.Clear();

            richText.AppendSpaced("This example demonstrates");
            richText.PushFontSize(20);
            richText.PushVerticalAlignment(VerticalAlignment.Top);
            richText.AppendSpaced("Christian");
            richText.PushVerticalAlignment(VerticalAlignment.Bottom);
            richText.AppendSpaced("Matt");
            richText.PopVerticalAlignment();
            richText.PopVerticalAlignment();
            richText.AppendSpaced("working ");

            // set width and compute aspect height 

            // default height = pixelSize * currentFontAscent
            // fixed size
            // match width
            // width match height
            // richText.PushHitRegion(out hitId);
            // bool richText.HitTest(point, out hitId);

            // do what you want with hit id

            // richText.PushClickHandler(evt); -> pushHitRegion();

            // richText.InlineSpace(new UISpaceSize(100, 1), height, out int spaceId);

            // reserve space
            // draw 
            // image/sprite -> implemented as Draw()

            // richText.Draw(Size, (callback) => {}); // reserve space (spriteSize) + draw without user telling us to

            // richText.Append("Contrary to popular belief, Lorem Ipsum is not simply random text. It has roots in a piece of classical Latin literature from 45 BC, making it over 2000 years old. Richard McClintock, a Latin professor at Hampden-Sydney College in Virginia, looked up one of the more obscure Latin words, consectetur, from a Lorem Ipsum passage, and going through the cites of the word in classical literature, discovered the undoubtable source. Lorem Ipsum comes from sections 1.10.32 and 1.10.33 of (The Extremes of Good and Evil) by Cicero, written in 45 BC. This book is a treatise on the theory of ethics, very popular during the Renaissance. The first line of Lorem Ipsum, , comes from a line in section 1.10.32.");

            // TextLayout layout = new TextLayout(tagger, shapeContext, shapeCache);

            // layout.Layout(fontTable, richText.dataBuffer, richText.symbolBuffer, layoutInput, ref textLayoutOutput);

            // richText.GetSpaceById(spaceId); => rect 

            // layout.HitTest(new Vector2(100, 200));
            // how do hit test?
            // per glyph? per run? line ranges? 
            // hit test needs a start and maybe a char / cluster id like color 

        }

        public void HitTest(float2 point) {

            // hit test all runs
            // return list hits? or single hit? maybe just he last hit run?
            // hit test from end of text then?

            for (int symbolIndex = 0; symbolIndex < richText.symbolBuffer.size; symbolIndex++) {
                TextSymbol symbol = richText.symbolBuffer[symbolIndex];
            }

        }

        public void OnPostRender() {
            vertices.Clear();
            texCoords.Clear();
            indices.Clear();

            int runIndex = 0;
            int glyphOffset = 0;

            DataList<GlyphPosition> positions = new DataList<GlyphPosition>(richText.dataBuffer.size, Allocator.Temp);
            DataList<GlyphInfo> infos = new DataList<GlyphInfo>(richText.dataBuffer.size, Allocator.Temp);

            // for (int i = 0; i < textLayoutOutput.runList.size; i++) {
            //     ref CharacterRun run = ref textLayoutOutput.runList.Get(i);
            //     ref TextLayoutResult result = ref textLayoutOutput.results.Get(i);
            //
            //     if ((run.flags & CharacterRunFlags.Characters) != 0) {
            //         ShapeCacheValue entry = shapeCache.GetEntryAt(result.shapeResultIndex);
            //
            //         result.GlyphCount = (ushort) entry.glyphCount;
            //
            //         positions.AddRange(entry.glyphPositions, entry.glyphCount);
            //         infos.AddRange(entry.glyphInfos, entry.glyphCount);
            //     }
            // }

            // DrawText(runIndex, infos, glyphOffset, positions);

            positions.Dispose();
            infos.Dispose();
        }

        // private void DrawText(int runIndex, DataList<GlyphInfo> infos, int glyphOffset, DataList<GlyphPosition> positions) {
        //     DataList<float> fontSizeStack = new DataList<float>(8, Allocator.Temp);
        //
        //     DataList<ushort> fontIdStack = new DataList<ushort>(8, Allocator.Temp);
        //
        //     StructList<SDFFont> fontList = new StructList<SDFFont>(); // todo -- make unmanaged
        //
        //     // todo -- make these real 
        //     fontList.Add(baseFont);
        //     fontIdStack.Add(robotoId); // (ushort) layoutInfo.fontAssetId.id);
        //     fontSizeStack.Add(layoutInput.fontSize);
        //
        //     FontMetrics fontMetrics = FontMetrics.Create(baseFont, fontSizeStack.GetLast());
        //     float falloff = baseFont.falloff * fontMetrics.scaleTexturePxToMetrics;
        //     SDFFont currentFont = baseFont;
        //     float scale = 2f * falloff * fontMetrics.pixelSize;
        //
        //     int dataOffset = 0;
        //
        //     Color[] palette = new Color[128];
        //
        //     int colorIndex = 0;
        //
        //     palette[0] = Color.black;
        //     int ellipsisVertexOffset = -1;
        //     int ellipseGlyphStart = -1;
        //     int ellipseGlyphEnd = -1;
        //
        //     int maxLineIndex = layoutInput.maxLineCount <= 0 ? int.MaxValue : layoutInput.maxLineCount;
        //
        //     for (int symbolIndex = 0; symbolIndex < richText.symbolBuffer.size; symbolIndex++) {
        //
        //         TextSymbol symbol = richText.symbolBuffer[symbolIndex];
        //
        //         switch (symbol.symbolType) {
        //
        //             case SymbolType.InlineSpace: {
        //                 runIndex++;
        //                 break;
        //             }
        //
        //             case SymbolType.HorizontalSpace: {
        //                 runIndex++;
        //                 break;
        //             }
        //
        //             case SymbolType.PushColor: {
        //                 // assume we have an index or can somehow compute it from symbols
        //                 // todo -- find in palette 
        //
        //                 palette[++colorIndex] = *(Color*) richText.dataBuffer.GetPointer(dataOffset);
        //
        //                 if (symbol.metaInfo >= 0) {
        //                     ApplyMidRunColor(runIndex, colorIndex, infos, symbol.metaInfo);
        //                 }
        //
        //                 break;
        //             }
        //
        //             case SymbolType.PopColor: {
        //
        //                 if (colorIndex == 0) {
        //                     break;
        //                 }
        //
        //                 colorIndex--;
        //                 if (symbol.metaInfo >= 0) {
        //                     ApplyMidRunColor(runIndex, colorIndex, infos, symbol.metaInfo);
        //                 }
        //
        //                 break;
        //             }
        //
        //             case SymbolType.PushFontSize: {
        //                 float fontSize = *(float*) richText.dataBuffer.GetPointer(dataOffset);
        //                 fontSizeStack.Add(fontSize);
        //                 fontMetrics = FontMetrics.Create(currentFont, fontSize);
        //                 falloff = currentFont.falloff * fontMetrics.scaleTexturePxToMetrics;
        //                 scale = 2f * falloff * fontMetrics.pixelSize;
        //                 break;
        //             }
        //
        //             case SymbolType.PopFontSize: {
        //                 if (fontSizeStack.size > 1) {
        //                     fontSizeStack.Pop();
        //                     fontMetrics = FontMetrics.Create(currentFont, fontSizeStack.GetLast());
        //                     falloff = currentFont.falloff * fontMetrics.scaleTexturePxToMetrics;
        //                     scale = 2f * falloff * fontMetrics.pixelSize;
        //                 }
        //
        //                 break;
        //             }
        //
        //             case SymbolType.PushFont: {
        //                 // todo
        //                 currentFont = fontList[fontIdStack.GetLast()];
        //                 fontMetrics = FontMetrics.Create(currentFont, fontSizeStack.GetLast());
        //                 falloff = currentFont.falloff * fontMetrics.scaleTexturePxToMetrics;
        //                 scale = 2f * falloff * fontMetrics.pixelSize;
        //                 break;
        //             }
        //
        //             case SymbolType.PopFont: {
        //                 if (fontIdStack.size > 0) {
        //                     fontIdStack.Pop();
        //                     currentFont = fontList[fontIdStack.GetLast()];
        //                     fontMetrics = FontMetrics.Create(currentFont, fontSizeStack.GetLast());
        //                     falloff = currentFont.falloff * fontMetrics.scaleTexturePxToMetrics;
        //                     scale = 2f * falloff * fontMetrics.pixelSize;
        //                 }
        //
        //                 break;
        //             }
        //
        //             case SymbolType.CharacterGroup: {
        //
        //                 if (symbol.metaInfo == TextSymbol.k_EmptyCharacterGroup) break;
        //
        //                 for (int r = runIndex; r < textLayoutOutput.runList.size; r++) {
        //
        //                     ref CharacterRun run = ref textLayoutOutput.runList.Get(r);
        //                     ref TextLayoutResult result = ref textLayoutOutput.results.Get(r);
        //
        //                     if (textLayoutOutput.ellipsisRunIndex != -1 && ellipsisVertexOffset != -1) {
        //                         goto loop_escape;
        //                     }
        //
        //                     if (textLayoutOutput.ellipsisRunIndex == r) {
        //                         ellipsisVertexOffset = vertices.Count;
        //                         ellipseGlyphStart = glyphOffset;
        //                         ellipseGlyphEnd = glyphOffset + textLayoutOutput.results.Get(r).utilValue;
        //                     }
        //
        //                     // traverse symbols
        //                     // hit test each run
        //                     // if hit passed
        //                     // figure out if spanning multiple hit regions
        //                     // if so do lookup like we handle color-swap mid word
        //
        //                     if (run.IsRendered && result.lineIndex < maxLineIndex) {
        //
        //                         float localX = result.x;
        //
        //                         int glyphStart = glyphOffset;
        //                         int glyphEnd = glyphOffset + result.utilValue;
        //
        //                         for (int gIdx = glyphStart; gIdx < glyphEnd; gIdx++) {
        //
        //                             // todo -- pre-fetch glyph infos 
        //
        //                             if (!currentFont.TryGetGlyph(infos[gIdx].glyphIndex, out SDFGlyphInfo sdfInfo)) {
        //                                 // if glyph missing we can still advance as though it were there
        //                                 // or we render the 'missing' glyph? maybe harfbuzz does this already?
        //                                 localX += positions[gIdx].advanceX;
        //                                 continue;
        //                             }
        //
        //                             float scaleX = fontMetrics.pixelSize;
        //                             float scaleY = enableLowerCaseScale && (sdfInfo.flags & 1) == 1 ? fontMetrics.lowScale : fontMetrics.capScale;
        //
        //                             // todo -- scaleXY, falloff and scaleTexturePxToMetrics should probably be shader inputs so we can use the same code for bitmap and sdf text 
        //                             float top = penOffsetY + -result.y + scaleY * (sdfInfo.topBearing + falloff);
        //                             float left = localX + scaleX * (sdfInfo.leftBearing - falloff);
        //                             float bottom = top - scaleY * fontMetrics.scaleTexturePxToMetrics * (sdfInfo.bottom - sdfInfo.top);
        //                             float right = left + scaleX * fontMetrics.scaleTexturePxToMetrics * (sdfInfo.right - sdfInfo.left);
        //                             // todo -- move some of this to vertex shader for faster rendering & less data (StructuredBuffer / DrawIndirect) 
        //                             vertices.Add(new Vector3(left, bottom, scale));
        //                             vertices.Add(new Vector3(right, bottom, scale));
        //                             vertices.Add(new Vector3(right, top, scale));
        //                             vertices.Add(new Vector3(left, top, scale));
        //
        //                             texCoords.Add(new Vector4(sdfInfo.left, 1 - sdfInfo.bottom, colorIndex, 0));
        //                             texCoords.Add(new Vector4(sdfInfo.right, 1 - sdfInfo.bottom, colorIndex, 0));
        //                             texCoords.Add(new Vector4(sdfInfo.right, 1 - sdfInfo.top, colorIndex, 0));
        //                             texCoords.Add(new Vector4(sdfInfo.left, 1 - sdfInfo.top, colorIndex, 0));
        //
        //                             localX += positions[gIdx].advanceX;
        //
        //                         }
        //
        //                         glyphOffset += result.utilValue;
        //                     }
        //
        //                     if (run.IsLastInGroup) {
        //                         runIndex = r + 1;
        //                         break;
        //                     }
        //
        //                 }
        //
        //                 break;
        //             }
        //
        //         }
        //
        //         dataOffset += symbol.dataLength;
        //     }
        //
        //     loop_escape:
        //
        //     int quadCount = vertices.Count / 4;
        //
        //     if (ellipsisVertexOffset != -1) {
        //         quadCount -= (vertices.Count - ellipsisVertexOffset) / 4;
        //     }
        //
        //     int quadIndex = 0;
        //     for (int i = 0; i < quadCount; i++) {
        //
        //         indices.Add(quadIndex + 0);
        //         indices.Add(quadIndex + 1);
        //         indices.Add(quadIndex + 2);
        //         indices.Add(quadIndex + 2);
        //         indices.Add(quadIndex + 3);
        //         indices.Add(quadIndex + 0);
        //
        //         quadIndex += 4;
        //
        //     }
        //
        //     if (ellipsisVertexOffset != -1) {
        //
        //         float maxExtents = maxWidth;
        //
        //         ref TextLayoutResult result = ref textLayoutOutput.results.Get(textLayoutOutput.ellipsisRunIndex);
        //         ref CharacterRun run = ref textLayoutOutput.runList.Get(textLayoutOutput.ellipsisRunIndex);
        //
        //         float localX = result.x;
        //
        //         if (baseFont.TryGetGlyphFromCodepoint(0x00002026, out SDFGlyphInfo sdfInfo)) {
        //             maxExtents -= sdfInfo.advanceX * fontMetrics.pixelSize;
        //         }
        //
        //         int lastQuadIndex = quadIndex;
        //         for (int gIdx = ellipseGlyphStart; gIdx < ellipseGlyphEnd; gIdx++) {
        //
        //             if (localX + positions[gIdx].advanceX < maxExtents) {
        //
        //                 indices.Add(quadIndex + 0);
        //                 indices.Add(quadIndex + 1);
        //                 indices.Add(quadIndex + 2);
        //                 indices.Add(quadIndex + 2);
        //                 indices.Add(quadIndex + 3);
        //                 indices.Add(quadIndex + 0);
        //
        //                 localX += positions[gIdx].advanceX;
        //                 lastQuadIndex = quadIndex;
        //                 quadIndex += 4;
        //             }
        //             else {
        //                 // bump quadIndex by remaining un-used glyph count
        //                 quadIndex += 4 * (ellipseGlyphEnd - gIdx);
        //                 break;
        //             }
        //
        //         }
        //
        //         if (lastQuadIndex >= texCoords.Count) {
        //             lastQuadIndex = texCoords.Count - 1;
        //         }
        //
        //         int ellipsisColorIndex = (int) texCoords[lastQuadIndex].z;
        //
        //         float scaleX = fontMetrics.pixelSize;
        //         float scaleY = fontMetrics.capScale;
        //
        //         // todo -- scaleXY, falloff and scaleTexturePxToMetrics should probably be shader inputs so we can use the same code for bitmap and sdf text 
        //         float top = penOffsetY + -result.y + scaleY * (sdfInfo.topBearing + falloff);
        //         float left = localX + scaleX * (sdfInfo.leftBearing - falloff);
        //         float bottom = top - scaleY * fontMetrics.scaleTexturePxToMetrics * (sdfInfo.bottom - sdfInfo.top);
        //         float right = left + scaleX * fontMetrics.scaleTexturePxToMetrics * (sdfInfo.right - sdfInfo.left);
        //
        //         vertices.Add(new Vector3(left, bottom, scale));
        //         vertices.Add(new Vector3(right, bottom, scale));
        //         vertices.Add(new Vector3(right, top, scale));
        //         vertices.Add(new Vector3(left, top, scale));
        //
        //         texCoords.Add(new Vector4(sdfInfo.left, 1 - sdfInfo.bottom, ellipsisColorIndex, 0));
        //         texCoords.Add(new Vector4(sdfInfo.right, 1 - sdfInfo.bottom, ellipsisColorIndex, 0));
        //         texCoords.Add(new Vector4(sdfInfo.right, 1 - sdfInfo.top, ellipsisColorIndex, 0));
        //         texCoords.Add(new Vector4(sdfInfo.left, 1 - sdfInfo.top, ellipsisColorIndex, 0));
        //
        //         indices.Add(quadIndex + 0);
        //         indices.Add(quadIndex + 1);
        //         indices.Add(quadIndex + 2);
        //         indices.Add(quadIndex + 2);
        //         indices.Add(quadIndex + 3);
        //         indices.Add(quadIndex + 0);
        //
        //         quadIndex += 4;
        //
        //     }
        //
        //     textCamera.orthographic = true;
        //     textCamera.orthographicSize = Screen.height * 0.5f;
        //
        //     Vector3 position = textCamera.transform.position;
        //     textCamera.transform.position = new Vector3(Screen.width / 2f, -Screen.height / 2f, -2f);
        //
        //     material.SetColorArray("_ColorPalette", palette);
        //     material.SetTexture("_MainTex", fontTexture);
        //     material.SetColor("_FontColor", Color.black);
        //     material.SetColor("_BGColor", Color.white);
        //     material.SetVector("_SDFTextureSize", new Vector4(fontTexture.width, fontTexture.height, 0, 0));
        //     material.SetPass(0);
        //
        //     textMesh.Clear(true);
        //     textMesh.SetVertices(vertices);
        //     textMesh.SetUVs(0, texCoords);
        //     textMesh.SetTriangles(indices, 0);
        //
        //     Graphics.DrawMeshNow(textMesh, Matrix4x4.identity);
        //     textCamera.transform.position = position;
        //     fontIdStack.Dispose();
        //     fontSizeStack.Dispose();
        // }
        //
        // private void ApplyMidRunColor(int runIndex, int colorIndex, DataList<GlyphInfo> infos, int clusterIndex) {
        //     ref TextLayoutResult result = ref textLayoutOutput.results.Get(runIndex - 1);
        //
        //     int baseOffset = vertices.Count - (result.utilValue * 4);
        //
        //     for (int gIdx = 0, vIdx = baseOffset; gIdx < result.utilValue; gIdx++, vIdx += 4) {
        //
        //         uint cluster = infos[gIdx].cluster;
        //
        //         if (cluster < clusterIndex) {
        //             continue;
        //         }
        //
        //         Vector4 v0 = texCoords[vIdx + 0];
        //         Vector4 v1 = texCoords[vIdx + 1];
        //         Vector4 v2 = texCoords[vIdx + 2];
        //         Vector4 v3 = texCoords[vIdx + 3];
        //
        //         texCoords[vIdx + 0] = new Vector4(v0.x, v0.y, colorIndex, v0.w);
        //         texCoords[vIdx + 1] = new Vector4(v1.x, v1.y, colorIndex, v1.z);
        //         texCoords[vIdx + 2] = new Vector4(v2.x, v2.y, colorIndex, v2.z);
        //         texCoords[vIdx + 3] = new Vector4(v3.x, v3.y, colorIndex, v3.z);
        //     }
        // }

    }

}