using System;
using System.Runtime.InteropServices;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia.Text {

    ///<summary>
    /// matches TextShapeRequestInfo in cpp
    /// </summary> 
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct TextShapeRequestInfo {

        public char* contentBuffer;
        public ShapingCharacterRun* shapeRequests;
        public int shapeRequestCount;

    }

    ///<summary>
    /// matches TextShapeBuffers in cpp
    /// </summary> 
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct TextShapeBuffers : IDisposable {

        public ShapeResult* shapeResults;
        public GlyphInfo* glyphInfos;
        public GlyphPosition* glyphPositions;

        public Allocator allocator; // todo -- make sure this is ok for marshalling 
        
        public TextShapeBuffers(int shapeQueryCount, int estimatedGlyphCount, Allocator allocator) {
            this.allocator = allocator;
            this.glyphInfos = TypedUnsafe.Malloc<GlyphInfo>(2 * estimatedGlyphCount, allocator);
            this.glyphPositions = TypedUnsafe.Malloc<GlyphPosition>(2 * estimatedGlyphCount, allocator);
            this.shapeResults = TypedUnsafe.Malloc<ShapeResult>(shapeQueryCount, allocator);
        }

        public void Dispose() {
            TypedUnsafe.Dispose(glyphInfos, allocator);
            TypedUnsafe.Dispose(glyphPositions, allocator);
            TypedUnsafe.Dispose(shapeResults, allocator);
        }

    }

    ///<summary>
    /// matches GlyphInfo in cpp
    /// </summary> 
    [StructLayout(LayoutKind.Sequential)]
    public struct GlyphInfo {

        public uint glyphIndex;
        public uint cluster;

    }

    public enum HarfbuzzLanguageIdByte : byte { }

    internal enum HarfbuzzDirectionByte : byte {

        Invalid = 0,
        LeftToRight = 4,
        RightToLeft = 5,
        TopToBottom = 6,
        BottomToTop = 7

    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct GlyphPosition {

        public float advanceX;
        public float advanceY;
        public float offsetX;
        public float offsetY;

    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ShapeResult {

        public uint glyphCount;
        public float advanceWidth;

    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HarfbuzzFeature {

        public uint tag;
        public uint value;
        public uint start;
        public uint end;

    }

    public enum HarfbuzzScriptIdByte : byte {

        Common = 0,
        Inherited,
        Unknown,
        Arabic,
        Armenian,
        Bengali,
        Cyrillic,
        Devanagari,
        Georgian,
        Greek,
        Gujarati,
        Gurmukhi,
        Hangul,
        Han,
        Hebrew,
        Hiragana,
        Kannada,
        Katakana,
        Lao,
        Latin,
        Malayalam,
        Oriya,
        Tamil,
        Telugu,
        Thai,
        Tibetan,
        Bopomofo,
        Braille,
        Canadian_syllabics,
        Cherokee,
        Ethiopic,
        Khmer,
        Mongolian,
        Myanmar,
        Ogham,
        Runic,
        Sinhala,
        Syriac,
        Thaana,
        Yi,
        Deseret,
        Gothic,
        Old_italic,
        Buhid,
        Hanunoo,
        Tagalog,
        Tagbanwa,
        Cypriot,
        Limbu,
        Linear_b,
        Osmanya,
        Shavian,
        Tai_le,
        Ugaritic,
        Buginese,
        Coptic,
        Glagolitic,
        Kharoshthi,
        New_tai_lue,
        Old_persian,
        Syloti_nagri,
        Tifinagh,
        Balinese,
        Cuneiform,
        Nko,
        Phags_pa,
        Phoenician,
        Carian,
        Cham,
        Kayah_li,
        Lepcha,
        Lycian,
        Lydian,
        Ol_chiki,
        Rejang,
        Saurashtra,
        Sundanese,
        Vai,
        Avestan,
        Bamum,
        EgyptianHieroglyphs,
        ImperialAramaic,
        InscriptionalPahlavi,
        InscriptionalParthian,
        Javanese,
        Kaithi,
        Lisu,
        MeeteiMayek,
        OldSouthArabian,
        OldTurkic,
        Samaritan,
        TaiTham,
        TaiViet,
        Batak,
        Brahmi,
        Mandaic,
        Chakma,
        MeroiticCursive,
        MeroiticHieroglyphs,
        Miao,
        Sharada,
        SoraSompeng,
        Takri,
        BassaVah,
        CaucasianAlbanian,
        Duployan,
        Elbasan,
        Grantha,
        Khojki,
        Khudawadi,
        Linear_a,
        Mahajani,
        Manichaean,
        Mende_kikakui,
        Modi,
        Mro,
        Nabataean,
        Old_north_arabian,
        Old_permic,
        Pahawh_hmong,
        Palmyrene,
        Pau_cin_hau,
        Psalter_pahlavi,
        Siddham,
        Tirhuta,
        Warang_citi,
        Ahom,
        Anatolian_hieroglyphs,
        Hatran,
        Multani,
        Old_hungarian,
        Signwriting,
        Adlam,
        Bhaiksuki,
        Marchen,
        Osage,
        Tangut,
        Newa,
        Masaram_gondi,
        Nushu,
        Soyombo,
        Zanabazar_square,
        Dogra,
        Gunjala_gondi,
        Hanifi_rohingya,
        Makasar,
        Medefaidrin,
        Old_sogdian,
        Sogdian,
        Elymaic,
        Nandinagari,
        Nyiakeng_puachue_hmong,
        Wancho,
        Chorasmian,
        Dives_akuru,
        Khitan_small_script,
        Yezidi,

    }

}