using System;
using UIForia.Text;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia.Layout {

    internal enum TextStyleType {

        Invalid,
        FaceDilate,
        OutlineSoftness,
        OutlineWidth,
        GlowOffset,
        GlowOuter,
        UnderlayX,
        UnderlayY,
        UnderlayDilate,
        UnderlaySoftness,

        FontSize

    }

    internal struct TextMeasureState : IDisposable {

        private struct FloatPair {

            public TextStyleType type;
            public float value;

            public FloatPair(TextStyleType textStyleType, float value) {
                this.type = textStyleType;
                this.value = value;
            }

        }

        private DataList<FontStyle> fontStyleStack;
        private DataList<FontAssetInfo> fontStack;
        private DataList<FloatPair> floatList;
        public TextScript scriptStyle;

        public TextMeasureState(Allocator allocator) : this() {
            fontStack = new DataList<FontAssetInfo>(8, allocator);
            fontStyleStack = new DataList<FontStyle>(8, Allocator.Temp);
            floatList = new DataList<FloatPair>(16, Allocator.Temp);
        }

        public void Initialize(float baseFontSize, in TextStyle textStyle, in FontAssetInfo fontAssetInfo) {
            fontStyleStack.size = 0;
            floatList.size = 0;
            fontStack.size = 0;
            fontStack.Add(fontAssetInfo);

            floatList.Add(new FloatPair(TextStyleType.FontSize, baseFontSize));

            if (textStyle.faceDilate != 0) {
                floatList.Add(new FloatPair(TextStyleType.FaceDilate, textStyle.faceDilate));
            }

            if (textStyle.outlineWidth != 0) {
                floatList.Add(new FloatPair(TextStyleType.OutlineWidth, textStyle.outlineWidth));
            }

            if (textStyle.outlineSoftness != 0) {
                floatList.Add(new FloatPair(TextStyleType.OutlineSoftness, textStyle.outlineSoftness));
            }

            if (textStyle.glowOffset != 0) {
                floatList.Add(new FloatPair(TextStyleType.GlowOffset, textStyle.glowOffset));
            }

            if (textStyle.glowOuter != 0) {
                floatList.Add(new FloatPair(TextStyleType.GlowOuter, textStyle.glowOuter));
            }

            if (textStyle.underlayDilate != 0) {
                floatList.Add(new FloatPair(TextStyleType.UnderlayDilate, textStyle.underlayDilate));
            }

            if (textStyle.underlaySoftness != 0) {
                floatList.Add(new FloatPair(TextStyleType.UnderlaySoftness, textStyle.underlaySoftness));
            }

            if (textStyle.underlayX != 0) {
                floatList.Add(new FloatPair(TextStyleType.UnderlayX, textStyle.underlayX));
            }

            if (textStyle.underlayY != 0) {
                floatList.Add(new FloatPair(TextStyleType.UnderlayY, textStyle.underlayX));
            }
        }

        public TextTransform textTransform {
            get => TextTransform.None; // todo
        }

        public float faceDilate {
            get => FindFloat(TextStyleType.FaceDilate, 0);
        }

        public float outlineWidth {
            get => FindFloat(TextStyleType.OutlineWidth, 0);
        }

        public float outlineSoftness {
            get => FindFloat(TextStyleType.OutlineSoftness, 0);
        }

        public float glowOffset {
            get => FindFloat(TextStyleType.GlowOffset, 0);
        }

        public float glowOuter {
            get => FindFloat(TextStyleType.GlowOuter, 0);
        }

        public float underlayX {
            get => FindFloat(TextStyleType.UnderlayX, 0);
        }

        public float underlayY {
            get => FindFloat(TextStyleType.UnderlayY, 0);
        }

        public float underlayDilate {
            get => FindFloat(TextStyleType.UnderlayDilate, 0);
        }

        public float underlaySoftness {
            get => FindFloat(TextStyleType.UnderlaySoftness, 0);
        }

        public void PushFloat(TextStyleType type, float value) {
            floatList.Add(new FloatPair() {type = type, value = value});
        }

        public void PopFloat(TextStyleType type) {
            if (floatList.size >= 1 && floatList[floatList.size - 1].type == type) {
                floatList.size--;
                return;
            }

            // instead of removing from the list, just mark the last one as invalid
            for (int i = floatList.size - 2; i >= 0; i--) {
                if (floatList[i].type == type) {
                    floatList[i].type = TextStyleType.Invalid;
                    break;
                }
            }
        }

        private float FindFloat(TextStyleType type, float defaultValue) {
            for (int i = floatList.size - 1; i >= 0; i--) {
                if (floatList[i].type == type) {
                    return floatList[i].value;
                }
            }

            return defaultValue;
        }

        public float fontSize {
            get => FindFloat(TextStyleType.FontSize, 18);
        }

        public FontStyle fontStyle {
            get => fontStyleStack.GetLast();
        }

        public ref FontAssetInfo fontAssetInfo {
            get => ref fontStack.GetLast();
        }

        public void PushFont(FontAssetInfo assetInfo) {
            fontStack.Add(assetInfo);
        }

        public void PopFont() {
            if (fontStack.size > 0) {
                fontStack.size--;
            }
        }

        public void PushFontSize(UIFixedLength newFontSize, float viewportWidth, float viewportHeight) {
            switch (newFontSize.unit) {

                default:
                case UIFixedUnit.Unset:
                case UIFixedUnit.Pixel:
                    PushFloat(TextStyleType.FontSize, newFontSize.value);
                    break;

                case UIFixedUnit.Percent:
                    PushFloat(TextStyleType.FontSize, newFontSize.value * fontSize * 100);
                    break;

                case UIFixedUnit.Em:
                    PushFloat(TextStyleType.FontSize, newFontSize.value * fontSize);
                    break;

                case UIFixedUnit.ViewportWidth:
                    PushFloat(TextStyleType.FontSize, viewportWidth * fontSize);
                    break;

                case UIFixedUnit.ViewportHeight:
                    PushFloat(TextStyleType.FontSize, viewportHeight * fontSize);
                    break;
            }
        }

        public void PopFontSize() {
            PopFloat(TextStyleType.FontSize);
        }

        public void PushFontStyle(FontStyle fontStyle) {
            fontStyleStack.Add(fontStyle);
        }

        public void PopFontStyle() {
            fontStyleStack.size--;
        }

        public void Dispose() {
            fontStack.Dispose();
            fontStyleStack.Dispose();
            floatList.Dispose();
        }

    }

}