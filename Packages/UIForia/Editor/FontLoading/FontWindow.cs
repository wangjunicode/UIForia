using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ThisOtherThing.UI.ShapeUtils;
using TMPro;
using UIForia.Graphics.ShapeKit;
using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore.LowLevel;
using Object = System.Object;

namespace UIForia.Editor.FontLoading {

    public class FontWindow : EditorWindow {

        [MenuItem("Window/UIForia/Font Asset Creator", false, 2025)]
        public static void ShowWindow() {
            FontWindow window = GetWindow<FontWindow>();
            window.titleContent = new GUIContent("Font Asset Creator");
            window.Focus();
        }

        private CommandBuffer commandBuffer;

        private delegate void DebugCallback(string message);

        [DllImport("UIForiaFontGenerator")]
        private static extern void RegisterDebugCallback(DebugCallback callback);

        [DllImport("UIForiaFontGenerator")]
        private static extern Atlas GetAtlas();

        [DllImport("UIForiaFontGenerator")]
        private static extern FontMetrics GetFontMetrics();

        [DllImport("UIForiaFontGenerator")]
        private static extern int GetGlyphInfo(out IntPtr ptr);

        [DllImport("UIForiaFontGenerator")]
        private static extern int GetKerningPairs(out IntPtr ptr);

        [DllImport("UIForiaFontGenerator")]
        private static extern int LoadFont(string fontPath, string charSet, string texturePath, FontConfig config);

        [DllImport("UIForiaFontGenerator")]
        private static extern void UnloadFont();

        private Font font;
        [Range(0, 64)] public float pixelRange;

        [Range(0, 1000)] public float emSize;

        public DimensionsConstraint constraint;

        [Range(0, 2048)] public int atlasWidth;
        [Range(0, 2048)] public int atlasHeight;

        public ImageType imageType;
        private Mesh mesh;
        private Material material;

        public void OnEnable() {
            mesh = new Mesh();

            material = Resources.Load<Material>("UIForiaShape");

            commandBuffer = new CommandBuffer();
            renderTexture = new RenderTexture(512, 256, 24);
            atlasWidth = 256;
            atlasHeight = 256;
            emSize = 64;
            pixelRange = 2;
        }

        private RenderTexture renderTexture;

        public void OnDisable() {
            if (renderTexture != null) {
                DestroyImmediate(renderTexture);
                renderTexture = null;
            }

            if (mesh != null) {
                //     DestroyImmediate(mesh);
            }
        }

        private bool useKerning = true;
        private float fontSize = 18;

        public void OnGUI() {

            Font lastFont = font;
            EditorGUI.BeginChangeCheck();
            font = (Font) EditorGUILayout.ObjectField("Font", font, typeof(Font), false);
            imageType = (ImageType) EditorGUILayout.EnumPopup("Image Type", imageType);
            pixelRange = EditorGUILayout.Slider("pixelRange", pixelRange, 0, 64);
            emSize = EditorGUILayout.FloatField("Em size", emSize);

            atlasWidth = EditorGUILayout.IntField("Atlas Width", atlasWidth);
            atlasHeight = EditorGUILayout.IntField("Atlas Height", atlasHeight);
            
            constraint = (DimensionsConstraint) EditorGUILayout.EnumPopup("Constraint", constraint);
            bool oldKerning = useKerning;
            useKerning = EditorGUILayout.Toggle("Kerning", oldKerning);
            int newSize = EditorGUILayout.IntSlider((int) fontSize, 10, 72);
            
            EditorGUI.EndChangeCheck();

            if (GUILayout.Button("Generate") || newSize != fontSize || useKerning != oldKerning || font != lastFont) {
                fontSize = newSize;
                UpdateFont();
            }

            Rect rt = GUILayoutUtility.GetRect(0, 0, 512, 256);
            GUI.DrawTexture(new Rect(rt.x, rt.y, 512, 256), renderTexture);

        }

        private static void DebugMethod(string message) {
            Debug.Log("dll error: " + message);
        }

        private unsafe void UpdateFont() {
            string fontPath = AssetDatabase.GetAssetPath(font);
            string outputpath = Path.GetDirectoryName(fontPath) + "/output.png";
            RegisterDebugCallback(DebugMethod);

            LoadFont(fontPath, null, outputpath, new FontConfig() {
                imageFormat = ImageFormat.PNG,
                imageType = imageType,
                pixelRange = pixelRange,
                emSize = emSize,
                atlasWidth = atlasWidth,
                atlasHeight = atlasHeight,
                atlasSizeConstraint = constraint,
                errorCorrection = 2,
            });

            Atlas atlas = GetAtlas();
            FontMetrics metrics = GetFontMetrics();

            int glyphCount = GetGlyphInfo(out IntPtr glyphs);
            GlyphInfo[] glyphInfos = new GlyphInfo[glyphCount];
            fixed (GlyphInfo* ptr = glyphInfos) {
                UnsafeUtility.MemCpy(ptr, (GlyphInfo*) glyphs.ToPointer(), sizeof(GlyphInfo) * glyphCount);
            }

            List<uint> codepoints = new List<uint>();
            for (int i = 0; i < glyphInfos.Length; i++) {
                codepoints.Add((uint)glyphInfos[i].codepoint);
            }
         //   GetFontKerning.Get(font, codepoints);
            
            IntMap<GlyphInfo> glyphMap = new IntMap<GlyphInfo>(256, Allocator.TempJob);
            IntMap<float> kerningDictionary = new IntMap<float>(256, Allocator.TempJob);

            for (int i = 0; i < glyphInfos.Length; i++) {
                ref GlyphInfo glyph = ref glyphInfos[i];
                glyphMap.Add(glyph.codepoint, glyph);
            }

            int kerningCount = GetKerningPairs(out IntPtr kerningPtr);
            KerningPair[] kerningPairs = new KerningPair[kerningCount];

            fixed (KerningPair* ptr = kerningPairs) {
                UnsafeUtility.MemCpy(ptr, (KerningPair*) kerningPtr.ToPointer(), sizeof(KerningPair) * kerningCount);
            }

            for (int i = 0; i < kerningPairs.Length; i++) {
                kerningDictionary.Add(BitUtil.SetHighLowBits(kerningPairs[i].first, kerningPairs[i].second), kerningPairs[i].advance);
            }

            UnloadFont();

            AssetDatabase.Refresh();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(outputpath);
            TextureImporter importer = new TextureImporter();
            material.mainTexture = texture;

            FontAssetInfo fontAssetInfo = new FontAssetInfo() {
                atlasTextureId = texture.GetHashCode(),
                glyphMapState = glyphMap.GetState(),
                kerningMapState = kerningDictionary.GetState(),
                atlasWidth = texture.width,
                atlasHeight = texture.height,
                // boldSpacing = boldSpacing,
                // boldStyle = boldStyle,
                faceInfo = new FaceInfo() {
                    ascender = metrics.ascender,
                    descender = metrics.descender,
                    lineHeight = metrics.lineHeight,
                    atlasWidth = texture.width,
                    atlasHeight = texture.height,
                    scale = 1,
                    pointSize = 64
                },
                // gradientScale = gradientScale,
                // italicStyle = italicStyle,
                // normalStyle = normalStyle,
                // weightBold = weightBold,
                // weightNormal = weightNormal,
                // normalSpacingOffset = normalSpacingOffset,
            };

            DataList<FontAssetInfo>.Shared fontAssetMap = new DataList<FontAssetInfo>.Shared(1, Allocator.Temp);

            fontAssetMap.Add(fontAssetInfo);

            // text can contain sprites and other fonts
            // both of these need to be accessed somehow in layout

            UIVertexHelper vh = UIVertexHelper.Create(Allocator.TempJob);
            using (ShapeKit shapeKit = new ShapeKit(Allocator.TempJob)) {

                TextStyle textStyle = new TextStyle() {
                    fontAssetId = 0,
                    fontSize = new UIFixedLength(32),
                };

                // textLayout.SetFontMap(fontMap);
                // textLayout.SetSpriteMap(spriteMap);
                
                mesh.Clear();
                float x = 0;
                float y = 0;
                string s = "UIForia is the fucking greatest!";
                int previous = 0;

                for (int i = 0; i < s.Length; i++) {

                    char c = s[i];
                    int codepoint = (int) c;

                    float fsScale = 1f / metrics.unitsPerEm;

                    if (glyphMap.TryGetValue(codepoint, out GlyphInfo glyphInfo)) {

                        if (previous != 0) {
                            kerningDictionary.TryGetValue(BitUtil.SetHighLowBits(previous, glyphInfo.codepoint), out float kerning);
                            x += kerning;
                        }

                        float width = (fsScale * glyphInfo.width) * fontSize;
                        float height = (fsScale * glyphInfo.height) * fontSize;
                        float xAdvance = (fsScale * glyphInfo.advance) * fontSize;
                        float yBearing = (fsScale * glyphInfo.horiBearingY) * fontSize;

                        shapeKit.AddQuad(ref vh, x + (glyphInfo.horiBearingX * fsScale * fontSize), 25 + (metrics.ascender - metrics.descender) - yBearing, width, height, Color.white);
                        float4* uvs = vh.texCoord + (vh.currentVertCount - 4);
                        float left = glyphInfo.atlasLeft / texture.width;
                        float right = glyphInfo.atlasRight / texture.width;
                        float top = glyphInfo.atlasTop / texture.height;
                        float bottom = glyphInfo.atlasBottom / texture.height;
                        uvs[0].x = left;
                        uvs[0].y = top; //1; //top;
                        uvs[1].x = right; //right;
                        uvs[1].y = top; //top;
                        uvs[2].x = right; //right;
                        uvs[2].y = bottom; //bottom;
                        uvs[3].x = left; //left;//
                        uvs[3].y = bottom; //bottom;
                        x += xAdvance;
                        previous = glyphInfo.codepoint;
                    }

                }

                // TextInfo textInfo = TextInfo.Create("UIForia", textStyle, fontAssetMap);

                // textInfo.Layout(fontAssetMap);
                // shapeKit.AddText(ref vh, textInfo, new float2(0, 0));

                // textInfo.Dispose();
            }

            vh.FillMesh(mesh);

            vh.Dispose();

            fontAssetMap.Dispose();
            glyphMap.Dispose();
            kerningDictionary.Dispose();
        }

        public void Update() {

            commandBuffer.Clear();
            commandBuffer.SetRenderTarget(renderTexture);
            commandBuffer.ClearRenderTarget(true, true, Color.black, 1f);

            float height = 256f;
            float halfHeight = height * 0.5f;
            float halfWidth = 256;
            //material.mainTexture = Resources.Load<Texture>("icon_1");
            commandBuffer.SetViewMatrix(Matrix4x4.identity);
            commandBuffer.SetProjectionMatrix(Matrix4x4.Ortho(-halfWidth, halfWidth, -halfHeight, halfHeight, -100, 100));
            Matrix4x4 origin = Matrix4x4.TRS(new Vector3(-halfWidth, halfHeight, 0), Quaternion.identity, Vector3.one);
            commandBuffer.DrawMesh(mesh, origin, material);
            UnityEngine.Graphics.ExecuteCommandBuffer(commandBuffer);
            Repaint();
        }

    }

}