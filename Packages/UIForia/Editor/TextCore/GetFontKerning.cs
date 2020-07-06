using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public class GetFontKerning {

    public static void Get(Font font, List<uint> codepoints) {
        FontEngine.LoadFontFace(font);
        GlyphPairAdjustmentRecord[] retn = FontEngine.GetGlyphPairAdjustmentRecords(codepoints, out int count);
    }

}