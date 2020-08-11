using System;
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
        

        // remove?
        private float width;
        private float height;

        public override void OnInitialize() {
            base.OnInitialize();
            isBuiltIn = true;
            isTextBox = true;
        }

        // todo -- might be an issue if in-use by render system
        ~TextRenderBox2() {
        }

        // todo -- move to the text system let that handle this except for non text specific stuff
        public override void OnStylePropertyChanged(StyleProperty[] propertyList, int propertyCount) {
            base.OnStylePropertyChanged(propertyList, propertyCount);

            for (int i = 0; i < propertyCount; i++) {
                ref StyleProperty property = ref propertyList[i];
                switch (property.propertyId) {
                    case StylePropertyId.TextFontAsset:
                    case StylePropertyId.Opacity:
                    case StylePropertyId.TextColor:
                      //  styleBuffer.faceColor = property.AsColor32;
                        break;

                    case StylePropertyId.TextFaceDilate:
                      //  styleBuffer.faceDilate = property.AsFloat;
                        break;

                    case StylePropertyId.TextUnderlayColor:
                      //  styleBuffer.underlayColor = property.AsColor32;
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

            ctx.SetTextMaterials(textInfo.materialBuffer);
            for (int i = 0; i < textInfo.renderRangeList.size; i++) {
                ref TextRenderRange render = ref textInfo.renderRangeList[i];
                
                // todo -- should definitely do a broadphase cull here 
                // bool overlappingOrContains = xMax >= clipper.aabb.xMin && xMin <= clipper.aabb.xMax && yMax >= clipper.aabb.yMin && yMin <= clipper.aabb.yMax;

                switch (render.type) {

                    case TextRenderType.Characters:
                        ctx.DrawTextCharacters(render);
                        break;

                    case TextRenderType.Underline:
                        break;

                    case TextRenderType.Sprite:
                        break;

                    case TextRenderType.Image:
                        break;

                    case TextRenderType.Element:
                        break;
                }

            }


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



}