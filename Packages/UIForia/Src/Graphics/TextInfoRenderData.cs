using UIForia.ListTypes;
using UnityEngine;

namespace UIForia.Graphics {

    public struct TextInfoRenderData {

        internal List_TextSymbol symbolList;
        internal List_TextLayoutSymbol layoutSymbolList;
        internal List_TextLineInfo lineInfoList;
        internal float resolvedFontSize;
        internal Rect alignedTextBounds;

    }

}