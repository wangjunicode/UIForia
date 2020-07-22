using UIForia.Elements;
using UIForia.Graphics;
using UIForia.Layout;
using UIForia.Text;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine.Profiling;

namespace UIForia.Rendering {

    public unsafe class TextRenderBox2 : RenderBox, IUnityInspector {

        internal TextStyleBuffer styleBuffer;
        internal DataList<TextMeshSpanInfo> textRenderSpans; // only used if text has multiple render spans

        private DataList<RenderedCharacterInfo>.Shared renderCharList;

        // remove?
        private float width;
        private float height;

        public override void OnInitialize() {
            base.OnInitialize();
            renderCharList = new DataList<RenderedCharacterInfo>.Shared(0, Allocator.Persistent);
        }

        // todo -- might be an issue if in-use by render system
        ~TextRenderBox2() {
            renderCharList.Dispose();
        }

        public override void OnStylePropertyChanged(StyleProperty[] propertyList, int propertyCount) {
            base.OnStylePropertyChanged(propertyList, propertyCount);

            for (int i = 0; i < propertyCount; i++) {
                ref StyleProperty property = ref propertyList[i];
                switch (property.propertyId) {
                    case StylePropertyId.TextFontAsset:
                    case StylePropertyId.Opacity:
                    case StylePropertyId.TextColor:
                        styleBuffer.faceColor = property.AsColor32;
                        break;

                    case StylePropertyId.TextFaceDilate:
                        styleBuffer.faceDilate = property.AsFloat;
                        break;

                    case StylePropertyId.TextUnderlayColor:
                        styleBuffer.underlayColor = property.AsColor32;
                        break;

                    case StylePropertyId.TextUnderlayX:
                    case StylePropertyId.TextUnderlayY:
                    case StylePropertyId.TextUnderlaySoftness:
                    case StylePropertyId.TextUnderlayDilate:
                    case StylePropertyId.TextFontStyle:
                    case StylePropertyId.TextGlowColor:
                    case StylePropertyId.TextGlowInner:
                    case StylePropertyId.TextGlowOuter:
                    case StylePropertyId.TextGlowOffset:
                        break;
                }
            }

        }

        public float underlayX = 0;
        public float underlayY = 0;
        public float underlaySoftness = 0;
        public float underlayDilate = 0;
        public float faceDilate = 0;
        public float glowOffset;
        public float glowOuter;
        public float glowInner;
        public float glowPower;
        public float outlineWidth;
        public float outlineSoftness;

        public float SliderGUI(string label, float value, float min, float max) {
#if UNITY_EDITOR
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            value = EditorGUILayout.Slider(value, min, max);
            EditorGUILayout.EndHorizontal();
            return value;
#endif
            return 0;
        }

        public override void OnSizeChanged(Size size) {
            base.OnSizeChanged(size);
            width = size.width;
            height = size.height;
        }

        [BurstCompile]
        public struct GatherJob : IJob {

            public DataList<RenderedCharacterInfo>.Shared renderList;
            [NativeDisableUnsafePtrRestriction] public TextInfo* textInfo;

            public void Execute() {
                int s = textInfo->symbolList.size;
                TextSymbol* symbolList = textInfo->symbolList.array;
                TextLayoutSymbol* layoutList = textInfo->layoutSymbolList.array;
                RenderedCharacterInfo* v = renderList.GetArrayPointer();
                int idx = 0;

                for (int i = 0; i < s; i++) {

                    ref BurstCharInfo charInfo = ref symbolList[i].charInfo;

                    if (charInfo.renderBufferIndex <= 0) {
                        continue;
                    }

                    float x = charInfo.position.x + layoutList[charInfo.wordIndex].wordInfo.x;
                    float y = charInfo.position.y + layoutList[charInfo.wordIndex].wordInfo.y;

                    v[idx++] = (new RenderedCharacterInfo() {
                        position = new float2(x, y),
                        // width = charInfo.bottomRight.x - charInfo.topLeft.x,
                        // height = charInfo.bottomRight.y - charInfo.topLeft.y,
                        materialIndex = 0,
                        textureIndex = 0,
                        renderedGlyphIndex = charInfo.renderBufferIndex
                    });

                }

                renderList.size = idx;
            }

        }

        public override void PaintBackground3(RenderContext3 ctx) {

            // need to know if text changed
            // or was laid out differently
            // or style changed
            // or span style changed
            // or or or
            // maybe regenerating isnt so awful now 
            // can still do high level culling of lines, maybe even characters if line is overlapping its clip bounds. can be parallel and is fast
            // profile, maybe its not a problem
            if (!((UITextElement) element).TryGetTextInfo(out TextInfo textInfo)) {
                return;
            }

            Graphics.TextMaterialInfo materialInfo = default;
            materialInfo.opacity = 1;
            materialInfo.scale = 1;
            materialInfo.weight = 0; // todo -- use font style instead
            materialInfo.glowColor = textInfo.textStyle.glowColor;
            materialInfo.faceColor = textInfo.textStyle.faceColor;
            materialInfo.outlineColor = textInfo.textStyle.outlineColor;
            materialInfo.zPosition = 0;
            materialInfo.outlineWidth = 0; //textInfo.textStyle.outlineWidth;
            Profiler.BeginSample("Build Render List");

            if (renderCharList.size == 0) {

                renderCharList.size = 0;
                renderCharList.EnsureCapacity(textInfo.symbolList.size); // todo -- buffer this so we dont over alloc, also dont update every frame

                new GatherJob() {
                    renderList = renderCharList,
                    textInfo = &textInfo,
                }.Run();

            }

            Profiler.EndSample();

            ctx.DrawSingleSpanUniformTextInternal(renderCharList.GetArrayPointer(), renderCharList.size, new AxisAlignedBounds2D(0, 0, width, height), new TextMaterialSetup() {
                materialInfo = materialInfo,
                faceTexture = new TextureUsage(),
                outlineTexture = new TextureUsage(),
                fontTextureId = ctx.GetFontTextureId(textInfo.textStyle.fontAssetId)
            });

        }

        public void OnGUI() {
            underlayX = SliderGUI("Underlay X", underlayX, -1, 1);
            underlayY = SliderGUI("Underlay Y", underlayY, -1, 1);
            underlaySoftness = SliderGUI("Underlay Softness", underlaySoftness, 0, 1);
            underlayDilate = SliderGUI("Underlay Dilate", underlayDilate, 0, 1);
            outlineSoftness = SliderGUI("Outline Softness", outlineSoftness, 0, 1);
            outlineWidth = SliderGUI("Outline Width", outlineWidth, 0, 1);
            glowInner = SliderGUI("Glow Inner", glowInner, 0, 1);
            glowOuter = SliderGUI("Glow Outer", glowOuter, 0, 1);
            glowPower = SliderGUI("Glow Power", glowPower, 0, 1);
            glowOffset = SliderGUI("Glow Offset", glowOffset, -1, 1);
        }

    }

    public struct RenderedCharacterInfo {

        public float2 position;
        public uint renderedGlyphIndex;
        public uint materialIndex;
        public uint textureIndex;
        public float width;
        public float height;
        public int effectIndex;

    }

}