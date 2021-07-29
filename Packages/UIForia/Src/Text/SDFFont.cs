using System;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEngine;

namespace UIForia.Prototype {

    public struct SDFFont {

        public float textureWidth;
        public float textureHeight;
        public float falloff;
        public float glyphHeight;
        public float descent;
        public float lineGap;
        public float capHeight;
        public float xHeight;
        public float advanceXSpace;
        public SDFGlyphInfo[] chars;

        public bool TryGetGlyph(uint glyphIndex, out SDFGlyphInfo retn) {

            // todo -- this is really dumb 
            for (int i = 0; i < chars.Length; i++) {
                if (chars[i].glyphIndex == glyphIndex) {
                    retn = chars[i];
                    return true;
                }
            }

            retn = default;
            return false;

        }

        public bool TryGetGlyphFromCodepoint(int codepoint, out SDFGlyphInfo retn) {
            // todo -- this is really dumb 
            for (int i = 0; i < chars.Length; i++) {
                if (chars[i].codepoint == codepoint) {
                    retn = chars[i];
                    return true;
                }
            }

            retn = default;
            return false;
        }

    }

    public unsafe struct SDFFontUnmanaged : IDisposable {

        public float textureWidth;
        public float textureHeight;
        public float falloff;
        public float glyphHeight;
        public float descent;
        public float lineGap;
        public float capHeight;
        public float xHeight;
        public float advanceXSpace;
        public SDFGlyphInfo* chars;
        public int charCount;

        public static SDFFontUnmanaged Create(SDFFont font) {

            SDFFontUnmanaged retn = new SDFFontUnmanaged {
                textureWidth = font.textureWidth,
                textureHeight = font.textureHeight,
                falloff = font.falloff,
                glyphHeight = font.glyphHeight,
                descent = font.descent,
                lineGap = font.lineGap,
                capHeight = font.capHeight,
                xHeight = font.xHeight,
                advanceXSpace = font.advanceXSpace,
                chars = TypedUnsafe.Malloc<SDFGlyphInfo>(font.chars.Length, Allocator.Persistent),
                charCount = font.chars.Length
            };

            fixed (SDFGlyphInfo* glyphs = font.chars) {
                TypedUnsafe.MemCpy(retn.chars, glyphs, font.chars.Length);
            }

            return retn;
        }

        public void Dispose() {
            TypedUnsafe.Dispose(chars, Allocator.Persistent);
        }

        public float ScaledLineHeight(float pixelSize) {
            return pixelSize * (1 - descent + lineGap);
        }

        public bool TryGetGlyph(uint glyphIndex, out SDFGlyphInfo retn) {

            // todo -- this is really dumb 
            for (int i = 0; i < charCount; i++) {
                if (chars[i].glyphIndex == glyphIndex) {
                    retn = chars[i];
                    return true;
                }
            }

            retn = default;
            return false;

        }

        public bool TryGetGlyphFromCodepoint(int codepoint, out SDFGlyphInfo retn) {
            // todo -- this is really dumb 
            for (int i = 0; i < charCount; i++) {
                if (chars[i].codepoint == codepoint) {
                    retn = chars[i];
                    return true;
                }
            }

            retn = default;
            return false;
        }

    }

    [Serializable]
    public struct SDFGlyphInfo {

        public uint codepoint;
        public uint glyphIndex;

        public float tcTop;
        public float tcRight;
        public float tcBottom;
        public float tcLeft;
        public uint flags;
        public float topBearing;
        public float leftBearing;
        public float advanceX;

        public float left => tcLeft;
        public float top => tcTop;
        public float right => tcRight;
        public float bottom => tcBottom;

    }

    public struct FontMetrics {

        public float capScale;
        public float lowScale;
        public float pixelSize;
        public float lineHeight;
        public float scaleTexturePxToMetrics;

        public static FontMetrics Create(in SDFFont font, float pixelSize, float extraLineGap = 0f) {

            const float kAscent = 1f; // fonts are normalized to ascent = 1
            float capScale = Mathf.Round(pixelSize);
            float lowScale = Mathf.Round(font.xHeight * capScale) / font.xHeight;
            float lineHeight = pixelSize * (kAscent - font.descent + font.lineGap + extraLineGap);
            float scaleTexturePxToMetrics = (1 - font.descent) / font.glyphHeight;

            return new FontMetrics() {
                capScale = capScale,
                lineHeight = lineHeight,
                lowScale = lowScale,
                pixelSize = pixelSize,
                scaleTexturePxToMetrics = scaleTexturePxToMetrics
            };
        }
        
        public static FontMetrics Create(in SDFFontUnmanaged font, float pixelSize, float extraLineGap = 0f) {

            const float kAscent = 1f; // fonts are normalized to ascent = 1
            float capScale = Mathf.Round(pixelSize);
            float lowScale = Mathf.Round(font.xHeight * capScale) / font.xHeight;
            float lineHeight = pixelSize * (kAscent - font.descent + font.lineGap + extraLineGap);
            float scaleTexturePxToMetrics = (1 - font.descent) / font.glyphHeight;

            return new FontMetrics() {
                capScale = capScale,
                lineHeight = lineHeight,
                lowScale = lowScale,
                pixelSize = pixelSize,
                scaleTexturePxToMetrics = scaleTexturePxToMetrics
            };
        }

    }

}