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

        FontSize,
        FontStyle,

        CharSpacing

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

        private DataList<FontAssetInfo> fontStack;
        private DataList<FloatPair> floatList;
        public TextScript scriptStyle;
        public float viewportWidth;
        public float viewportHeight;

        public TextMeasureState(Allocator allocator) : this() {
            fontStack = new DataList<FontAssetInfo>(8, allocator);
            floatList = new DataList<FloatPair>(16, allocator);
        }

        public void Initialize(float baseFontSize, in TextInfo textInfo, in FontAssetInfo fontAssetInfo) {
            floatList.size = 0;
            fontStack.size = 0;
            fontStack.Add(fontAssetInfo);

            floatList.Add(new FloatPair(TextStyleType.FontSize, baseFontSize));

            if (textInfo.fontStyle != 0) {
                floatList.Add(new FloatPair(TextStyleType.FontStyle, (int) textInfo.fontStyle));
            }

        }

        public TextTransform textTransform {
            get => TextTransform.None; // todo
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
            get => (FontStyle) (int) FindFloat(TextStyleType.FontStyle, 0);
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

        public float PushFontSize(UIFixedLength newFontSize) {
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

            return floatList[floatList.size - 1].value;
        }

        public float PushCharSpacing(UIFixedLength length) {
            float size = 0;
            switch (length.unit) {

                default:
                case UIFixedUnit.Unset:
                case UIFixedUnit.Pixel:
                    size = length.value;
                    break;

                case UIFixedUnit.Percent:
                    size = length.value * fontSize * 100;
                    break;

                case UIFixedUnit.Em:
                    size = length.value * fontSize;
                    break;

                case UIFixedUnit.ViewportWidth:
                    size = viewportWidth * fontSize;
                    break;

                case UIFixedUnit.ViewportHeight:
                    size = viewportHeight * fontSize;
                    break;
            }

            PushFloat(TextStyleType.CharSpacing, size);
            return size;

        }

        private bool TryPopFloat(TextStyleType type, out float currentValue, float defaultValue) {
            if (floatList.size >= 1 && floatList[floatList.size - 1].type == type) {
                floatList.size--;
                currentValue = FindFloat(type, defaultValue);
                return true;
            }

            // instead of removing from the list, just mark the last one as invalid
            for (int i = floatList.size - 2; i >= 0; i--) {
                if (floatList[i].type == type) {
                    floatList[i].type = TextStyleType.Invalid;
                    currentValue = FindFloat(type, defaultValue);
                    return true;
                }
            }

            currentValue = defaultValue;
            return false;
        }

        public bool TryPopCharSpacing(out float currentValue) {
            return TryPopFloat(TextStyleType.CharSpacing, out currentValue, 0);
        }

        public void PopFontSize() {
            PopFloat(TextStyleType.FontSize);
        }

        public void PushFontStyle(FontStyle fontStyle) {
            PushFloat(TextStyleType.FontStyle, (int) fontStyle);
        }

        public void PopFontStyle() {
            PopFloat(TextStyleType.FontStyle);
        }

        public void Dispose() {
            fontStack.Dispose();
            floatList.Dispose();
        }

    }

}