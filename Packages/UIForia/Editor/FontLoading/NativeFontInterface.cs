using System.Runtime.InteropServices;

namespace UIForia.Editor.FontLoading {

    [StructLayout(LayoutKind.Sequential)]
    public struct Atlas {

        public float fontSize;
        public float distanceRange;
        public int atlasWidth;
        public int atlasHeight;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GlyphInfo {

        public float width;
        public float height;
        public float horiBearingX;
        public float horiBearingY;
        public float horiAdvance;
        public float vertBearingX;
        public float vertBearingY;
        public float vertAdvance;

        public float atlasTop;
        public float atlasRight;
        public float atlasBottom;
        public float atlasLeft;

        public float planeTop;
        public float planeRight;
        public float planeBottom;
        public float planeLeft;

        public int codepoint;
        public float advance;

        public char character {
            get => (char) codepoint;
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KerningPair {

        public int first;
        public int second;
        public float advance;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FontMetrics {

        public float unitsPerEm;
        public float lineHeight;
        public float ascender;
        public float descender;
        public float underlineY;
        public float underlineThickness;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FontConfig {

        public int atlasWidth;
        public int atlasHeight;

        public float emSize;
        public float emRange;
        public float pixelRange;

        public float angleRadians;
        public float errorCorrection;
        public float miterLimit;

        public bool noOverlap;

        public ImageType imageType;
        public DimensionsConstraint atlasSizeConstraint;
        public ImageFormat imageFormat;

    }

    public enum ImageType {

        /// Rendered glyphs without anti-aliasing (two colors only)
        HARD_MASK,

        /// Rendered glyphs with anti-aliasing
        SOFT_MASK,

        /// Signed (true) distance field
        SDF,

        /// Signed pseudo-distance field
        PSDF,

        /// Multi-channel signed distance field
        MSDF,

        /// Multi-channel & true signed distance field
        MTSDF

    }

    public enum ImageFormat {

        UNSPECIFIED,
        PNG,
        BMP,
        TIFF,
        TEXT,
        TEXT_FLOAT,
        BINARY,
        BINARY_FLOAT,
        BINARY_FLOAT_BE

    }

    public enum DimensionsConstraint {

        POWER_OF_TWO_SQUARE,
        POWER_OF_TWO_RECTANGLE,
        MULTIPLE_OF_FOUR_SQUARE,
        EVEN_SQUARE,
        SQUARE

    }

}