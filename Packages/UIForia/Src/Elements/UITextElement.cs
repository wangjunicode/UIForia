using SVGX;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Text;
using UnityEngine;
using WhitespaceMode = UIForia.Text.WhitespaceMode;

namespace UIForia.Elements {

    public class UITextElement : UIElement, IStyleChangeHandler, IStylePropertiesWillChangeHandler, IStylePropertiesDidChangeHandler {

        internal string text;
        internal TextInfo2 textInfo;
        private bool shouldUpdateSpanStyle;
        private SVGXTextStyle spanStyle;
        public UITextElement(string text = "") {
            this.text = text ?? string.Empty;
            this.textInfo = new TextInfo2(new TextSpan(string.Empty));
            this.flags = flags | UIElementFlags.TextElement
                               | UIElementFlags.BuiltIn
                               | UIElementFlags.Primitive;
        }

        internal TextInfo2 TextInfo => textInfo;

        public override void OnCreate() {
            if (children != null) {
                for (int i = 0; i < children.Count; i++) {
                    TextSpanElement childSpan = (TextSpanElement) children[i];
                    childSpan.Initialize(textInfo);
                }
            }

        }

        public string GetText() {
            return text;
        }

        public void SetText(string newText) {
            if (this.text == newText) {
                return;
            }

            this.text = newText;
            
            spanStyle.font = style.TextFontAsset;
            spanStyle.fontSize = style.TextFontSize;
            spanStyle.fontStyle = style.TextFontStyle;
            spanStyle.textTransform = style.TextTransform;
            spanStyle.whitespaceMode = WhitespaceMode.CollapseWhitespace | WhitespaceMode.PreserveNewLines;
            textInfo.UpdateSpan(0, text, spanStyle);
            
        }

        public void SetText(int spanIndex, string text) {
            textInfo.UpdateSpan(spanIndex, text);
        }

        public override string GetDisplayName() {
            return "Text";
        }

        public SelectionRange AppendText(char character) {
            SetText(text + character);
            return new SelectionRange(textInfo.CharCount - 1, TextEdge.Right);
        }

        public SelectionRange AppendText(string str) {
            SetText(text + str);
            return new SelectionRange(textInfo.CharCount - 1, TextEdge.Right);
        }

        public SelectionRange DeleteTextBackwards(SelectionRange range) {
            if (text.Length == 0) {
                return range;
            }

            int cursorIndex = Mathf.Clamp(range.cursorIndex, 0, textInfo.CharCount - 1);
            if (range.HasSelection) {
                int min = (range.cursorIndex < range.selectIndex ? range.cursorIndex : range.selectIndex) + 1;
                int max = (range.cursorIndex > range.selectIndex ? range.cursorIndex : range.selectIndex) + 1;

                string part0 = text.Substring(0, min);
                string part1 = text.Substring(max);
                SetText(part0 + part1);
                return new SelectionRange(min, TextEdge.Left);
            }
            else {
                if (cursorIndex == 0 && range.cursorEdge == TextEdge.Left) {
                    return range;
                }

                // assume same line for the moment
                if (range.cursorEdge == TextEdge.Left) {
                    cursorIndex--;
                }

                cursorIndex = Mathf.Max(0, cursorIndex);

                if (cursorIndex == 0) {
                    SetText(text.Substring(1));
                    return new SelectionRange(0, TextEdge.Left);
                }
                else if (cursorIndex == textInfo.CharCount - 1) {
                    SetText(text.Substring(0, text.Length - 1));
                    return new SelectionRange(range.cursorIndex - 1, TextEdge.Right);
                }
                else {
                    string part0 = text.Substring(0, cursorIndex);
                    string part1 = text.Substring(cursorIndex + 1);
                    SetText(part0 + part1);

                    return new SelectionRange(cursorIndex - 1, TextEdge.Right);
                }
            }
        }

        public SelectionRange DeleteTextForwards(SelectionRange range) {
            if (text.Length == 0) {
                return range;
            }

            int cursorIndex = Mathf.Clamp(range.cursorIndex, 0, textInfo.CharCount - 1);
            if (range.HasSelection) {
                int min = (range.cursorIndex < range.selectIndex ? range.cursorIndex : range.selectIndex) + 1;
                int max = (range.cursorIndex > range.selectIndex ? range.cursorIndex : range.selectIndex) + 1;

                string part0 = text.Substring(0, min);
                string part1 = text.Substring(max);
                SetText(part0 + part1);
                return new SelectionRange(min, TextEdge.Left);
            }
            else {
                if (cursorIndex == textInfo.CharCount - 1 && range.cursorEdge == TextEdge.Right) {
                    return range;
                }
                else {
                    if (cursorIndex == textInfo.CharCount - 1) {
                        SetText(text.Remove(textInfo.CharCount - 1));
                        return new SelectionRange(textInfo.CharCount - 1, TextEdge.Right);
                    }
                    else {
                        string part0 = text.Substring(0, cursorIndex + 1);
                        string part1 = text.Substring(cursorIndex + 2);
                        SetText(part0 + part1);
                        return new SelectionRange(cursorIndex, TextEdge.Right);
                    }
                }
            }
        }

        public SelectionRange MoveCursorLeft(SelectionRange range, bool maintainSelection) {
            int selectionIndex = range.selectIndex;
            TextEdge selectionEdge = range.selectEdge;

            if (!maintainSelection && range.HasSelection) {
                selectionIndex = -1;
                return new SelectionRange(range.cursorIndex, range.cursorEdge);
            }
            else if (!maintainSelection) {
                selectionIndex = -1;
            }
            else if (!range.HasSelection) {
                selectionIndex = range.cursorIndex;
                selectionEdge = range.cursorEdge;
            }

            if (range.cursorEdge == TextEdge.Left) {
                if (range.cursorIndex == 0) {
                    return new SelectionRange(range.cursorIndex, range.cursorEdge, selectionIndex, selectionEdge);
                }

                return new SelectionRange(Mathf.Max(0, range.cursorIndex - 2), TextEdge.Right, selectionIndex, selectionEdge);
            }

            if (range.cursorIndex - 1 < 0) {
                return new SelectionRange(0, TextEdge.Left, selectionIndex, selectionEdge);
            }

            return new SelectionRange(range.cursorIndex - 1, TextEdge.Right, selectionIndex, selectionEdge);
        }

        public SelectionRange MoveCursorRight(SelectionRange range, bool maintainSelection) {
            int selectionIndex = range.selectIndex;
            TextEdge selectionEdge = range.selectEdge;

            if (!maintainSelection && range.HasSelection) {
                selectionIndex = -1;
                return new SelectionRange(range.cursorIndex, range.cursorEdge);
            }
            else if (!maintainSelection) {
                selectionIndex = -1;
            }
            else if (!range.HasSelection) {
                selectionIndex = range.cursorIndex;
                selectionEdge = range.cursorEdge;
            }

            if (range.cursorEdge == TextEdge.Left) {
                return new SelectionRange(range.cursorIndex, TextEdge.Right, selectionIndex, selectionEdge);
            }

            int cursorIndex = Mathf.Min(range.cursorIndex + 1, textInfo.CharCount - 1);

            if (cursorIndex == textInfo.CharCount - 1) {
                return new SelectionRange(cursorIndex, TextEdge.Right, selectionIndex, selectionEdge);
            }

            return new SelectionRange(cursorIndex, TextEdge.Right, selectionIndex, selectionEdge);
        }

        public void InsertText(SelectionRange range, char character) { }

        public void InsertText(int characterIndex, string str) { }


        private WordInfo GetWordAtPoint(Vector2 point) {
            return FindNearestWord(FindNearestLine(point), point);
        }

        public SelectionRange DeleteRange(SelectionRange selectionRange) {
            return new SelectionRange();
        }

        public string GetSubstring(SelectionRange selectionRange) {
            if (!selectionRange.HasSelection) {
                return string.Empty;
            }

            int start = Mathf.Min(selectionRange.cursorIndex, selectionRange.selectIndex);
            int end = Mathf.Max(selectionRange.cursorIndex, selectionRange.selectIndex);

            char[] chars = new char[end - start];
            int idx = 0;
            for (int i = start; i < end; i++) {
                //  chars[idx++] = textInfo.charInfos[i].character;
            }

            return new string(chars);
        }

        public SelectionRange SelectAll() {
            return new SelectionRange(textInfo.CharCount - 1, TextEdge.Right, 0, TextEdge.Left);
        }

        private TextEdge FindCursorEdge(int charIndex, Vector2 point) {
            if (string.IsNullOrEmpty(text) || charIndex > text.Length - 1) {
                return TextEdge.Left;
            }

            CharInfo charInfo = textInfo.charInfoList.Array[charIndex];
            float width = charInfo.bottomRight.x - charInfo.topLeft.x;
            if (point.x > charInfo.topLeft.x + (width * 0.5f)) {
                return TextEdge.Right;
            }

            return TextEdge.Left;
        }

        private int FindNearestCharacterIndex(WordInfo wordInfo, Vector2 point) {
            int closestIndex = wordInfo.startChar;
            float closestDistance = float.MaxValue;
            CharInfo[] charInfos = textInfo.charInfoList.Array;
//
//            for (int i = wordInfo.startChar; i < wordInfo.startChar + wordInfo.CharCount; i++) {
//                CharInfo charInfo = charInfos[i];
//                float x1 = charInfo.topLeft.x;
//                float x2 = charInfo.bottomRight.x;
//
//                if (point.x >= x1 && point.x <= x2) {
//                    return i;
//                }
//
//                float distToY1 = Mathf.Abs(point.x - x1);
//                float distToY2 = Mathf.Abs(point.x - x2);
//                if (distToY1 < closestDistance) {
//                    closestIndex = i;
//                    closestDistance = distToY1;
//                }
//
//                if (distToY2 < closestDistance) {
//                    closestIndex = i;
//                    closestDistance = distToY2;
//                }
//            }

            return closestIndex;
        }

        private int FindNearestCharacterIndex(Vector2 point) {
            LineInfo nearestLine = FindNearestLine(point);
            WordInfo nearestWord = FindNearestWord(nearestLine, point);
            return FindNearestCharacterIndex(nearestWord, point);
        }

        private LineInfo FindNearestLine(Vector2 point) {
            return default; //textInfo.lineInfos[FindNearestLineIndex(point)];
        }

        private int FindNearestLineIndex(Vector2 point) {
            return 0;
//            float lh = GetLineHeight();
//            if (point.y <= textInfo.lineInfos[0].position.y) {
//                return 0;
//            }
//
//            if (point.y >= textInfo.lineInfos[textInfo.lineInfos.Length - 1].position.y) {
//                return textInfo.lineInfos.Length - 1;
//            }
//
//            for (int i = 0; i < textInfo.lineCount; i++) {
//                LineInfo line = textInfo.lineInfos[i];
//
//                if (line.position.y <= point.y && line.position.y + lh >= point.y) {
//                    return i;
//                }
//            }
//
//
//            return 0; // should never reach this
//            float closestDistance = float.MaxValue;
//            for (int i = 0; i < textInfo.lineCount; i++) {
//                LineInfo line = textInfo.lineInfos[i];
//                float y1 = -line.maxAscender;
//                float y2 = -line.maxDescender;
//
//                if (point.y >= y1 && point.y <= y2) {
//                    return i;
//                }
//
//                float distToY1 = Mathf.Abs(point.y - y1);
//                float distToY2 = Mathf.Abs(point.y - y2);
//                if (distToY1 < closestDistance) {
//                    closestIndex = i;
//                    closestDistance = distToY1;
//                }
//
//                if (distToY2 < closestDistance) {
//                    closestIndex = i;
//                    closestDistance = distToY2;
//                }
//            }
//
//            return closestIndex;
        }

        private WordInfo FindNearestWord(LineInfo line, Vector2 point) {
            int closestIndex = 0;
            float closestDistance = float.MaxValue;
            for (int i = line.wordStart; i < line.wordStart + line.wordCount; i++) {
                WordInfo word = default; //textInfo.wordInfos[i];
                float x1 = word.position.x;
                float x2 = word.position.x + word.xAdvance;
                if (point.x >= x1 && point.x <= x2) {
                    return word;
                }

                float distToX1 = Mathf.Abs(point.x - x1);
                float distToX2 = Mathf.Abs(point.x - x2);
                if (distToX1 < closestDistance) {
                    closestIndex = i;
                    closestDistance = distToX1;
                }

                if (distToX2 < closestDistance) {
                    closestIndex = i;
                    closestDistance = distToX2;
                }
            }

            return default; //textInfo.wordInfos[closestIndex];
        }

        public SelectionRange GetSelectionAtPoint(Vector2 point) {
            if (string.IsNullOrEmpty(text)) {
                return new SelectionRange(0, TextEdge.Left, -1, TextEdge.Left);
            }

            int charIndex = FindNearestCharacterIndex(point);
            return new SelectionRange(charIndex, FindCursorEdge(charIndex, point));
        }

        public SelectionRange SelectWordAtPoint(Vector2 point) {
            WordInfo wordInfo = GetWordAtPoint(point);
            return new SelectionRange(
                wordInfo.startChar + wordInfo.VisibleCharCount - 1,
                TextEdge.Right,
                wordInfo.startChar
            );
        }

        public SelectionRange ValidateSelectionRange(SelectionRange range) {
            int cursorIdx = (range.cursorIndex < textInfo.CharCount) ? range.cursorIndex : textInfo.CharCount - 1;
            int selectIdx = (range.selectIndex < textInfo.CharCount) ? range.selectIndex : textInfo.CharCount - 1;
            return new SelectionRange(cursorIdx, range.cursorEdge, selectIdx, range.selectEdge);
        }

        public SelectionRange SelectToPoint(SelectionRange range, Vector2 point) {
            int charIndex = FindNearestCharacterIndex(point);
            return new SelectionRange(
                charIndex,
                FindCursorEdge(charIndex, point),
                range.selectIndex,
                range.selectEdge
            );
        }

        public Vector2 GetCursorPosition(SelectionRange selectionRange) {
            if (string.IsNullOrEmpty(text) || selectionRange.cursorIndex >= textInfo.CharCount) {
                return Vector2.zero;
            }

//            CharInfo charInfo = textInfo.charInfos[selectionRange.cursorIndex];
//            LineInfo lineInfo = textInfo.lineInfos[charInfo.lineIndex];
//
//            return new Vector2(selectionRange.cursorEdge == TextEdge.Right
//                    ? charInfo.bottomRight.x
//                    : charInfo.topLeft.x,
//                lineInfo.position.y
//            );
            return default;
        }

        public SelectionRange BeginSelection(Vector2 point) {
            int selectIdx = FindNearestCharacterIndex(point);
            TextEdge selectEdge = FindCursorEdge(selectIdx, point);
            return new SelectionRange(selectIdx, selectEdge, selectIdx, selectEdge);
        }



        // size, font, style, whitespace, transform, alignment
        public void OnStylePropertyChanged(in StyleProperty property) {
            switch (property.propertyId) {
                case StylePropertyId.TextFontSize:
                    shouldUpdateSpanStyle = true;
                    spanStyle.fontSize = property.AsInt;
                    break;
                case StylePropertyId.TextFontStyle:
                    shouldUpdateSpanStyle = true;
                    spanStyle.fontStyle = property.AsFontStyle;
                    break;
                case StylePropertyId.TextAlignment:
                    shouldUpdateSpanStyle = true;
                    spanStyle.alignment = property.AsTextAlignment;
                    break;
                case StylePropertyId.TextFontAsset:
                    shouldUpdateSpanStyle = true;
                    spanStyle.font = property.AsFont;
                    break;
                case StylePropertyId.TextTransform:
                    shouldUpdateSpanStyle = true;
                    spanStyle.textTransform = property.AsTextTransform;
                    break;
                // todo -- support this
                // case StylePropertyId.WhiteSpaceMode:
            }
        }

        public void OnStylePropertiesWillChange() {
            shouldUpdateSpanStyle = false;
        }

        public void OnStylePropertiesDidChange() {
            if (shouldUpdateSpanStyle) {
                textInfo.UpdateSpanStyle(0, spanStyle);
                shouldUpdateSpanStyle = false;
            }
        }

    }

}