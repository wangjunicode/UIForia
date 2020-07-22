using System;
using System.Collections.Concurrent;
using UIForia.Text;
using UIForia.Util;

namespace UIForia.Rendering {

    public unsafe class ManagedTextSpanInfo : IDisposable {

        internal TextInfo* textInfoPtr;
        internal UnmanagedSpanInfo* unmanagedSpanInfo;

        internal string content;
        internal string renderedContent;
        internal bool _isEnabled;
        internal ManagedTextInfo textInfo;
            
        public bool isEnabled {
            get => _isEnabled;
            set {
                if (value != _isEnabled) {
                    unmanagedSpanInfo->isEnabled = value;
                    textInfoPtr->spanDirty = true;
                }
            }
        }

        public void SetText(char[] array, int count) {
            SetText(array, 0, count);
        }

        public void SetText(char[] array, int offset, int count) {
                
        }

        public void SetText(string text) {
                
        }

        internal RichTextProcessor _textProcessor;

        internal ManagedTextSpanInfo(ManagedTextInfo textInfo) { }

        public RichTextProcessor textProcessor {
            get => _textProcessor;
            set {
                if (_textProcessor != value) {
                    _textProcessor = value;
                    if (_textProcessor != null) {
                        // process now or enqueue
                    }
                }
            }
        }

        internal static ConcurrentQueue<IntPtr> s_ReleaseQueue = new ConcurrentQueue<IntPtr>();
        internal LightList<TextEffect> textEffects; // todo -- dont resize in big steps
        public ManagedTextSpanInfo firstChild;
        public ManagedTextSpanInfo nextSibling;

        ~ManagedTextSpanInfo() {
            Dispose();
        }

        public string GetRawContent() {
            return content;
        }

        public string GetDisplayedContent() {

            if (renderedContent == null) {
                // build a string from the symbol list unmanagedSpanInfo->symbolList.size;
            }

            return renderedContent;
        }

        public void Dispose() {
            if (textInfo == null) {
                return;
            }

            s_ReleaseQueue.Enqueue((IntPtr) textInfoPtr);
        }

    }

}