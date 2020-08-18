using System;
using UIForia.Elements;
using UnityEngine;

namespace UIForia.Text {

    public static class SelectionRangeUtil {

        public static string InsertText(ref TextInfo textInfo, string source, string characters) {
            string retn = null;

            if (string.IsNullOrEmpty(characters)) {
                return source;
            }

            if (string.IsNullOrEmpty(source)) {
                textInfo.selectionOrigin = SelectionCursor.Invalid;
                textInfo.selectionCursor = new SelectionCursor(characters.Length - 1, SelectionEdge.Right);
                return characters;
            }

            if (textInfo.selectionOrigin.IsValid) {
                source = DeleteTextForwards(source, ref textInfo);
                textInfo.selectionOrigin = SelectionCursor.Invalid;
            }

            if (string.IsNullOrEmpty(source)) {
                textInfo.selectionCursor = new SelectionCursor(characters.Length - 1, SelectionEdge.Right);
                return characters;
            }

            SelectionCursor startCursor = textInfo.selectionCursor;

            int cursorIndex = source.Length > 0 ? Mathf.Clamp(startCursor.index, 0, source.Length - 1) : 0;
            SelectionEdge edge = startCursor.edge;
            if (startCursor.index == source.Length - 1) {
                if (startCursor.edge == SelectionEdge.Left) {
                    string text = source.Substring(0, cursorIndex);
                    string endText = source.Substring(cursorIndex);
                    retn = text + characters + endText;
                    cursorIndex += characters.Length;
                }
                else {
                    retn = source + characters;
                    cursorIndex += characters.Length;
                }
            }
            else if (startCursor.index == 0) {
                if (startCursor.edge == SelectionEdge.Left) {
                    retn = characters + source;
                    cursorIndex = characters.Length;
                    edge = SelectionEdge.Left;
                }
                else {
                    string text = source.Substring(0, cursorIndex);
                    string endText = source.Substring(cursorIndex);
                    retn = text + characters + endText;
                    cursorIndex += characters.Length;
                }
            }
            // todo optimize not to use substring here
            else if (startCursor.edge == SelectionEdge.Right) {
                string text = source.Substring(0, cursorIndex + 1);
                string endText = source.Substring(cursorIndex + 1);
                retn = text + characters + endText;
                cursorIndex += characters.Length;
            }
            else {
                string text = source.Substring(0, cursorIndex);
                string endText = source.Substring(cursorIndex);
                retn = text + characters + endText;
                cursorIndex += characters.Length;
            }

            textInfo.selectionCursor = new SelectionCursor(cursorIndex, edge);
            return retn;
        }


        public static string DeleteTextForwards(string source, ref TextInfo textInfo) {
            string retn = null;

            if (string.IsNullOrEmpty(source)) {
                textInfo.selectionOrigin = SelectionCursor.Invalid;
                return string.Empty;
            }

            SelectionCursor cursor = textInfo.selectionCursor;

            int cursorIndex = Mathf.Clamp(cursor.index, 0, source.Length - 1);

            if (textInfo.HasSelection) {
                RangeInt selectionRange = textInfo.GetSelectionRange();

                if (selectionRange.end == source.Length - 1) {
                    retn = source.Substring(0, selectionRange.start);
                }
                else {
                    string part0 = source.Substring(0, selectionRange.start);
                    string part1 = source.Substring(selectionRange.end);
                    retn = part0 + part1;
                }

                if (selectionRange.start == retn.Length) {
                    textInfo.selectionCursor = new SelectionCursor(selectionRange.start - 1, SelectionEdge.Right);
                }
                else {
                    textInfo.selectionCursor = new SelectionCursor(selectionRange.start, selectionRange.start == retn.Length ? SelectionEdge.Right : SelectionEdge.Left);
                }

                textInfo.selectionOrigin = SelectionCursor.Invalid;

                return retn;
            }
            else {
                if (cursorIndex == source.Length - 1 && cursor.edge == SelectionEdge.Right) {
                    return source;
                }
                else {
                    if (cursorIndex == source.Length - 1) {
                        retn = source.Remove(source.Length - 1);
                        textInfo.selectionCursor = new SelectionCursor(retn.Length - 1, SelectionEdge.Right);
                        textInfo.selectionOrigin = SelectionCursor.Invalid;
                    }
                    else if (textInfo.selectionCursor.edge == SelectionEdge.Right) {
                        string part0 = source.Substring(0, cursorIndex + 1);
                        string part1 = source.Substring(cursorIndex + 2);
                        retn = part0 + part1;
                        textInfo.selectionCursor = new SelectionCursor(cursorIndex, SelectionEdge.Right);
                    }
                    else {
                        string part0 = source.Substring(0, cursorIndex);
                        string part1 = source.Substring(cursorIndex + 1);
                        retn = part0 + part1;
                        textInfo.selectionCursor = new SelectionCursor(cursorIndex, SelectionEdge.Left);
                    }
                }
            }

            return retn;
        }

        public static string DeleteTextBackwards(string source, ref TextInfo textInfo) {
            if (string.IsNullOrEmpty(source)) {
                textInfo.selectionOrigin = SelectionCursor.Invalid;
                return string.Empty;
            }

            RangeInt selectionRange = textInfo.GetSelectionRange();

            // if there is no selection...
            if (selectionRange.start == 0 && selectionRange.length == 0) {
                SelectionCursor cursor = textInfo.selectionCursor;
                // ... and the cursor is valid
                if (cursor.IsValid) {
                    // ... and the cursor is on the right of the character
                    if (cursor.edge == SelectionEdge.Right) {
                        string part1 = source.Substring(0, cursor.index);
                        string part2 = source.Length > cursor.index + 1 ? source.Substring(cursor.index + 1) : string.Empty;

                        if (cursor.index == 0) {
                            textInfo.selectionCursor = new SelectionCursor(0, SelectionEdge.Left);
                        }
                        else {
                            textInfo.selectionCursor = new SelectionCursor(cursor.index - 1, SelectionEdge.Right);
                        }
                        
                        return part1 + part2;
                    }
                    // otherwise we're on the left edge
                    else if (cursor.index > 0) {
                        string part1 = source.Substring(0, cursor.index - 1);
                        string part2 = source.Length > cursor.index + 1 ? source.Substring(cursor.index) : string.Empty;

                        textInfo.selectionCursor = new SelectionCursor(cursor.index - 1, SelectionEdge.Left);

                        return part1 + part2;
                    }
                }
                
                textInfo.selectionOrigin = SelectionCursor.Invalid;
                return source;
            }
            else {
                if (selectionRange.length == 0) {
                    textInfo.selectionCursor = new SelectionCursor(selectionRange.start - 1, SelectionEdge.Left);
                    return source.Substring(selectionRange.start);
                }

                textInfo.selectionOrigin = SelectionCursor.Invalid;
                if (selectionRange.start == 0) {
                    textInfo.selectionCursor = new SelectionCursor(0, SelectionEdge.Left);
                }
                else {
                    textInfo.selectionCursor = new SelectionCursor(selectionRange.start - 1, SelectionEdge.Right);
                }
                string part1 = source.Substring(0, selectionRange.start);
                string part2 = source.Substring(selectionRange.end);
                return part1 + part2;
            }
        }


        public static string DeleteTextBackwards(string source, ref SelectionRange range) {
            if (string.IsNullOrEmpty(source)) {
                range = new SelectionRange(0);
                return string.Empty;
            }

            int cursorIndex = range.cursorIndex;

            if (range.HasSelection) {
                int min = (range.cursorIndex < range.selectIndex ? range.cursorIndex : range.selectIndex);
                int max = (range.cursorIndex > range.selectIndex ? range.cursorIndex : range.selectIndex);

                if (max - min >= source.Length) {
                    range = new SelectionRange(0);
                    return string.Empty;
                }

                if (max >= source.Length) {
                    range = new SelectionRange(min);
                    return source.Substring(0, min);
                }

                if (min == 0) {
                    range = new SelectionRange(0);
                    return source.Substring(max);
                }

                string part0 = source.Substring(0, min);
                string part1 = source.Substring(max);
                range = new SelectionRange(min);
                return part0 + part1;
            }
            else {
                if (cursorIndex == 0) {
                    return source;
                }

                cursorIndex = Mathf.Max(0, cursorIndex - 1);

                if (cursorIndex == 0) {
                    range = new SelectionRange(cursorIndex);
                    return source.Substring(1);
                }

                if (cursorIndex >= source.Length) {
                    range = new SelectionRange(source.Length);
                    return source.Substring(0, source.Length);
                }

                string part0 = source.Substring(0, cursorIndex);
                string part1 = source.Substring(cursorIndex + 1);
                range = new SelectionRange(cursorIndex);
                return part0 + part1;
            }
        }

    }

}