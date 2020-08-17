using System;
using UIForia.Attributes;
using UIForia.ListTypes;
using UIForia.Text;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Elements {

    [TemplateTagName("Text")]
    public unsafe class UITextElement : UIElement {

        internal string text;
        internal ITextProcessor _processor;
        internal TextInfo* textInfo;
        internal int lastUpdateFrame;

        ~UITextElement() {
            OnDestroy();
        }

        private void EnsureTextInfo() {
            if (textInfo != null) {
                return;
            }

            textInfo = TypedUnsafe.Malloc<TextInfo>(1, Allocator.Persistent);
            *textInfo = default;
        }

        public override void OnCreate() {
            EnsureTextInfo();
        }

        public override void OnDestroy() {
            if (textInfo == null) {
                return;
            }

            textInfo->Dispose();
            TypedUnsafe.Dispose(textInfo, Allocator.Persistent);
            textInfo = null;
        }

        // setting this and text in same frame will run update twice, improve this
        public ITextProcessor processor {
            get => _processor;
            set {
                if (_processor != value) {
                    _processor = value;
                    UpdateText();
                }
            }
        }

        public string GetText() {
            return text;
        }

        private void UpdateText() {
            EnsureTextInfo();
            application.textSystem.UpdateText(this);
        }

        public void SetTextFromCharacters(char[] newText, int length) {
            if (text == null || text.Length != length) {
                // needs update
                text = new string(newText, 0, length);
                UpdateText();
                return;
            }

            fixed (char* oldTextPtr = text)
            fixed (char* newTextPtr = newText) {
                if (UnsafeUtility.MemCmp(oldTextPtr, newTextPtr, 2 * length) != 0) {
                    text = new string(newText, 0, length);
                    UpdateText();
                }
            }
        }

        public void SetText(string newText) {
            if (newText == null) newText = string.Empty;

            if (text == null || text.Length != newText.Length) {
                // needs update
                text = newText;
                UpdateText();
                return;
            }

            fixed (char* oldTextPtr = text)
            fixed (char* newTextPtr = newText) {
                if (UnsafeUtility.MemCmp(oldTextPtr, newTextPtr, 2 * newText.Length) != 0) {
                    text = newText;
                    UpdateText();
                }
            }
        }

        public override string GetDisplayName() {
            return "Text";
        }

        public SelectionRange SelectWordAtPoint(Vector2 mouse) {
            throw new NotImplementedException();
        }

        public SelectionRange SelectLineAtPoint(Vector2 mouse) {
            throw new NotImplementedException();
        }

        public bool FontHasCharacter(char c) {
            if (textInfo == null) return false;

            // todo -- search fallback fonts
            // todo -- because of rich text, we need to know the 'active' font to search

            if (application.textSystem.TryGetFontAsset(textInfo->fontAssetId, out FontAssetInfo fontAssetInfo)) {
                return fontAssetInfo.TryGetGlyph((int) c, out UIForiaGlyph glyph);
            }

            return false;
        }

        public float2 GetCursorPosition(int cursorIndex) {
            if (textInfo == null) return default;

            List_TextSymbol list = textInfo->symbolList;

            int charIdx = 0;
            for (int i = 0; i < list.size; i++) {
                ref TextSymbol symbol = ref list.array[i];
                if (symbol.type != TextSymbolType.Character) {
                    continue;
                }

                if (charIdx == cursorIndex) {
                    ref BurstCharInfo charInfo = ref symbol.charInfo;
                    if (charInfo.wordIndex >= textInfo->layoutSymbolList.size) {
                        return default;
                    }

                    ref WordInfo wordInfo = ref textInfo->layoutSymbolList.array[charInfo.wordIndex].wordInfo;
                    return charInfo.position + new float2(wordInfo.x, wordInfo.y);
                }

                charIdx++;
            }

            return default;
        }

        public SelectionRange MoveToStartOfLine(SelectionRange selectionRange, bool evtShift) {
            throw new NotImplementedException();
        }

        public SelectionRange MoveToEndOfLine(SelectionRange selectionRange, bool evtShift) {
            throw new NotImplementedException();
        }

        public SelectionRange MoveCursorLeft(SelectionRange selectionRange, bool evtShift, bool evtCommand) {
            throw new NotImplementedException();
        }

        public SelectionRange MoveCursorRight(SelectionRange selectionRange, bool evtShift, bool evtCommand) {
            throw new NotImplementedException();
        }

        public string GetSelectedString(SelectionRange selectionRange) {
            throw new NotImplementedException();
        }

        public Rect GetLineRect(int lineRangeStart) {
            throw new NotImplementedException();
        }

        public Vector2 GetSelectionPosition(SelectionRange inputElementSelectionRange) {
            throw new NotImplementedException();
        }

        public int GetIndexAtPoint(Vector2 point) {
            if (textInfo == null) return -1;
            return textInfo->GetIndexAtPoint(application.ResourceManager.fontAssetMap, point);
        }

        public SelectionCursor GetSelectionCursorAtPoint(Vector2 point) {
            if (textInfo == null) return new SelectionCursor();
            return textInfo->GetSelectionCursorAtPoint(application.ResourceManager.fontAssetMap, point);
        }

        public Rect GetCursorRect() {
            if (textInfo == null) return default;
            return textInfo->GetCursorRect(application.ResourceManager.fontAssetMap);
        }

        public void SetSelection(SelectionCursor caretCursor, SelectionCursor selectionCursor) {
            if (textInfo == null) return;
            textInfo->selectionStartCursor = caretCursor;
            textInfo->selectionEndCursor = selectionCursor;
        }

        public RangeInt GetSelectionRange(out bool isRightEdge) {
            isRightEdge = false;
            if (textInfo == null) return default;
            return textInfo->GetSelectionRange(out isRightEdge);
        }

    }

    public struct SelectionCursor {

        public int index;
        public SelectionEdge edge;

        public SelectionCursor(int index, SelectionEdge edge) {
            this.index = index;
            this.edge = edge;
        }

        public static SelectionCursor Invalid => new SelectionCursor(-1, SelectionEdge.Left);

    }

}