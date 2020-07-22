using UIForia.ListTypes;

namespace UIForia.Rendering {

    public unsafe struct UnmanagedSpanInfo {

        // maniuplation is always from manged code, 
        // only need enough data here for layout & rendering to unmanaged

        public int firstChild;
        public int lastChild;
        public int nextSibling;
        public int prevSibling;
        public int parent;
        public bool isEnabled;
        // public UnmanagedTextInfo* textInfo;

        internal float resolvedFontSize;
        internal bool isRichText;
        internal bool isRenderDecorated;

        internal int lineStart;
        internal int wordStart;
        internal int charStart;

        internal int lineEnd;
        internal int wordEnd;
        internal int charEnd;

        // maybe want a list type that resizes more exactly
        internal List_Char characters; // nessessary? maybe not, can just rebuild it

        internal List_TextSymbol symbolList;
        // internal List_TextLayoutSymbol layoutList; // maybe also belongs on text info
        // internal List_TextLineInfo lineInfoList; // belongs on text info

        // bindings will check text == but against the managed version which can just hold a string

        public void SetText() {
            // textInfo->spanDirty = true;

        }

        public void AppendCharacter() { }

        public void AppendSymbol() { }

        public void InsertSymbol() { }

        public void InsertCharacter() { }
        //
        // public bool GetTextEffect(int charIndex, ref TextRenderBox2.CharEffect charEffect) {
        //     return true;
        // }

    }

}