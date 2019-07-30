using System;
using SVGX;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Text {

    public abstract class TextLayoutPolygon {

        public abstract bool LineCast(float y, out Vector2 intersection);

        public abstract Rect GetBounds();

    }

    public class TextInfo {

        private static int s_SpanIdGenerator;

        internal StructList<LineInfo2> lineInfoList;
        internal LightList<TextSpan> spanList;
        internal TextSpan rootSpan;

        private bool requiresLayout;
        private Size metrics;
        internal bool requiresSpanListRebuild;

        private IntrinsicSizes intrinsics;


        public TextInfo(string content, in SVGXTextStyle style = default, bool inheritStyleProperties = false) {
            this.rootSpan = new TextSpan();
            this.rootSpan.textInfo = this;
            this.spanList = new LightList<TextSpan>();
            this.lineInfoList = StructList<LineInfo2>.Get();
            this.requiresLayout = true;
            this.requiresSpanListRebuild = true;
            rootSpan.inheritedStyle = new SVGXInheritedTextStyle() {
                alignment = TextAlignment.Left,
                textTransform = TextTransform.None,
                whitespaceMode = WhitespaceMode.CollapseWhitespace,
                textColor = new Color32(0, 0, 0, 255),
                faceDilate = 0,
                fontAsset = FontAsset.defaultFontAsset,
                fontSize = 18,
                fontStyle = FontStyle.Normal,
                glowColor = new Color32(0, 0, 0, 0),
                underlayColor = new Color32(0, 0, 0, 0),
                glowOffset = 0,
                underlayX = 0,
                underlayY = 0,
                underlayDilate = 0,
                underlaySoftness = 0,
                glowOuter = 0,
                outlineColor = new Color32(0, 0, 0, 0),
                outlineSoftness = 0,
                outlineWidth = 0
            };
            rootSpan.inheritStyleProperties = inheritStyleProperties;
            rootSpan.textStyle = style;
            rootSpan.SetText(content);
        }

        public bool LayoutDirty => requiresLayout;

        internal void SpanRequiresLayout(TextSpan span) {
            requiresLayout = true;
        }

        private void RebuildSpanList() {
            if (!requiresSpanListRebuild) return;
            
            requiresSpanListRebuild = false;
            spanList.QuickClear();

            LightStack<TextSpan> stack = LightStack<TextSpan>.Get();

            stack.Push(rootSpan);

            while (stack.size > 0) {
                TextSpan span = stack.PopUnchecked();
                spanList.Add(span);
                span.Rebuild();

                TextSpan ptr = span.firstChild;

                // todo -- might be backwards
                while (ptr != null) {
                    stack.Push(ptr);
                    ptr = ptr.nextSibling;
                }
            }

            UpdateIntrinsics();
            
            LightStack<TextSpan>.Release(ref stack);
        }

        // todo -- optimize for change types where possible and do partial layout for affect spans only
        public Size Layout(Vector2 offset, float width) {
            lineInfoList.size = 0;
            RebuildSpanList();
            RunLayout(lineInfoList, width);
            LineInfo2 lastLine = lineInfoList[lineInfoList.Count - 1];
            float maxWidth = 0;

            LineInfo2[] lineInfos = lineInfoList.Array;
            for (int i = 0; i < lineInfoList.Count; i++) {
                lineInfos[i].x += offset.x;
                lineInfos[i].y += offset.y;
                maxWidth = Mathf.Max(maxWidth, lineInfos[i].width);
            }

            float height = lastLine.y + lastLine.height;
            // todo -- handle alignment across multiple spans
//            ApplyTextAlignment(maxWidth, rootSpan.alignment);
            ApplyLineAndWordOffsets(width, rootSpan.alignment);
            metrics = new Size(maxWidth, height);
            return metrics;
        }

        private void ApplyLineAndWordOffsets(float totalWidth, TextAlignment alignment, int lineStart = 0) {
            // todo -- this can be done partially in the case where we append or insert text 

            LineInfo2[] lines = lineInfoList.array;
            int lineCount = lineInfoList.size;
            
            TextSpan[] spans = spanList.Array;
            
            float lineOffsetY = 0;

            for (int lineIndex = lineStart; lineIndex < lineCount; lineIndex++) {
                int spanStart = lines[lineIndex].spanStart;
                int spanEnd = lines[lineIndex].spanEnd;

                float lineOffsetX = 0;
                switch (alignment) {
                    case TextAlignment.Unset:     
                    case TextAlignment.Left:
                        break;
                    case TextAlignment.Right:
                        lineOffsetX = totalWidth - lines[lineIndex].width;
                        break;
                }
                
                float wordOffsetX = lineOffsetX;

                for (int s = spanStart; s < spanEnd; s++) {
                    TextSpan span = spans[s];
                    
                    span.geometryVersion++;
                    
                    WordInfo2[] words = span.wordInfoList.array;
                    CharInfo2[] chars = span.charInfoList.array;
                    
                    int wordStart = s == spanStart ? lines[lineIndex].wordStart : 0;
                    int wordEnd = s == spanEnd - 1 ? lines[lineIndex].wordEnd : span.wordInfoList.size;
                    
                    for (int w = wordStart; w < wordEnd; w++) {
                        ref WordInfo2 wordInfo = ref words[w];
                        
                        if (wordInfo.type == WordType.Normal) {
                            int charStart = wordInfo.charStart;
                            int charEnd = wordInfo.charEnd;

                            for (int c = charStart; c < charEnd; c++) {
                                ref CharInfo2 charInfo = ref chars[c];
                                charInfo.layoutX = wordOffsetX;
                                charInfo.layoutY = lineOffsetY;
                                charInfo.lineIndex = lineIndex;
                                charInfo.visible = true;
                            }
                        }

                        wordOffsetX += wordInfo.width;
                    }

                }

                lineOffsetY += lines[lineIndex].height;
            }
        }

        private void RunSizingHeightLayout(float width) {
           throw new NotImplementedException();
        }

        // todo -- introduce faster version that just outputs size and not a filled line info list
        private StructList<LineInfo2> RunLayout(StructList<LineInfo2> lines, float width) {
            lines.size = 0;

            LineInfo2 currentLine = new LineInfo2();

            // cast line through shape from top bottom and center of current line
            // find right-most intersection point use that as result

            currentLine.spanStart = 0;
            currentLine.wordStart = 0;

            int spanCount = spanList.Count;
            TextSpan[] spans = spanList.Array;

            for (int spanIndex = 0; spanIndex < spanCount; spanIndex++) {
                TextSpan span = spans[spanIndex];
                WordInfo2[] wordInfos = span.wordInfoList.array;

                // todo -- if text set to nowrap or pre-wrap need different layout algorithm
                // todo -- use different algorithm for text with blocking spans in it

                float baseLineHeight = span.textStyle.fontAsset.faceInfo.LineHeight;

                int end = span.wordInfoList.size;
                for (int w = 0; w < end; w++) {
                    ref WordInfo2 wordInfo = ref wordInfos[w];

                    switch (wordInfo.type) {
                        case WordType.Whitespace:
                            if (currentLine.width + wordInfo.width > width) {
                                currentLine.spanEnd = spanIndex;
                                currentLine.wordEnd = w;
                                lines.Add(currentLine);
                                currentLine = new LineInfo2(spanIndex, w + 1);
                            }
                            else {
                                if (currentLine.wordCount != 0) {
                                    currentLine.wordCount++;
                                    currentLine.width += wordInfo.width;
                                }
                                else if ((span.textStyle.whitespaceMode & WhitespaceMode.TrimLineStart) != 0 && currentLine.wordStart == w) {
                                    currentLine.wordStart++;
                                }
                            }

                            break;

                        case WordType.NewLine:
                            currentLine.wordEnd = w;
                            currentLine.spanEnd = spanIndex;
                            lines.Add(currentLine);
                            currentLine = new LineInfo2(spanIndex, w + 1);
                            currentLine.height = wordInfo.height; // or LineHeight?
                            break;

                        case WordType.Normal:
                            // if word is longer than the line, put on its own line
                            if (wordInfo.width > width) {
                                if (currentLine.wordCount > 0) {
                                    currentLine.spanEnd = spanIndex;
                                    currentLine.wordEnd = w;
                                    lines.Add(currentLine);
                                }

                                currentLine = new LineInfo2(spanIndex, w, wordInfo.width);
                                currentLine.wordCount = 1;
                                currentLine.spanEnd = spanIndex;
                                currentLine.wordEnd = w + 1;
                                if (wordInfo.height > currentLine.height) currentLine.height = wordInfo.height;

                                lines.Add(currentLine);

                                currentLine = new LineInfo2(spanIndex, w + 1);
                            }
                            // if word is too long for the current line, break to next line
                            else if (wordInfo.width + currentLine.width > width + 0.5) {
                                currentLine.spanEnd = spanIndex;
                                currentLine.wordEnd = w;
                                lines.Add(currentLine);
                                currentLine = new LineInfo2(spanIndex, w, wordInfo.width);
                                currentLine.wordCount = 1;
                                if (wordInfo.height > currentLine.height) currentLine.height = wordInfo.height;
                            }
                            else {
                                currentLine.width += wordInfo.width;
                                currentLine.wordCount++;
                                if (wordInfo.height > currentLine.height) currentLine.height = wordInfo.height;
                            }

                            break;

                        case WordType.SoftHyphen:
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            if (currentLine.wordCount > 0) {
                currentLine.spanEnd = spanCount - 1;
                currentLine.wordEnd = spans[spanCount - 1].wordInfoList.size;
                lines.Add(currentLine);
            }

            LineInfo2[] lineArray = lines.array;
            for (int i = 0; i < lines.size; i++) {
                lineArray[i].spanEnd++;
            }

            return lines;
        }

        public void SetStyle(in SVGXTextStyle style) {
            rootSpan.SetStyle(style);
        }

        public void SetOutlineWidth(float? outlineWidth) {
            rootSpan.SetOutlineWidth(outlineWidth);
        }

        public void SetOutlineSoftness(float? outlineSoftness) {
            rootSpan.SetOutlineSoftness(outlineSoftness);
        }

        public void SetFontSize(float? fontSize) {
            rootSpan.SetFontSize(fontSize);
        }

        public void SetTextColor(Color32? textColor) {
            rootSpan.SetTextColor(textColor);
        }

        public void SetOutlineColor(Color32? outlineColor) {
            rootSpan.SetOutlineColor(outlineColor);
        }

        public void SetGlowColor(Color32? glowColor) {
            rootSpan.SetGlowColor(glowColor);
        }

        public void SetUnderlayColor(Color32? underlayColor) {
            rootSpan.SetUnderlayColor(underlayColor);
        }

        public void SetFaceDilate(float? faceDilate) {
            rootSpan.SetFaceDilate(faceDilate);
        }

        public void SetUnderlayX(float? underlayX) {
            rootSpan.SetUnderlayX(underlayX);
        }

        public void SetUnderlayY(float? underlayY) {
            rootSpan.SetUnderlayY(underlayY);
        }

        public void SetUnderlayDilate(float? dilate) {
            rootSpan.SetUnderlayDilate(dilate);
        }

        public void SetUnderlaySoftness(float? softness) {
            rootSpan.SetUnderlaySoftness(softness);
        }

        public void SetFontStyle(FontStyle? fontStyle) {
            rootSpan.SetFontStyle(fontStyle);
        }

        public void SetAlignment(TextAlignment? alignment) {
            rootSpan.SetTextAlignment(alignment);
        }

        public void SetFont(FontAsset font) {
            rootSpan.SetFont(font);
        }

        public void SetTextTransform(TextTransform? transform) {
            rootSpan.SetTextTransform(transform);
        }

        public void SetWhitespaceMode(WhitespaceMode? whitespaceMode) {
            rootSpan.SetWhitespaceMode(whitespaceMode);
        }

        public TextSpan InsertSpan(string text, SVGXTextStyle getTextStyle) {
            throw new NotImplementedException();
        }

        private float ComputeIntrinsicMinWidth() {
            int spanCount = spanList.Count;
            TextSpan[] spans = spanList.Array;

            float maxWidth = 0;

            for (int spanIndex = 0; spanIndex < spanCount; spanIndex++) {
                TextSpan span = spans[spanIndex];
                WordInfo2[] wordInfos = span.wordInfoList.array;

                int end = span.wordInfoList.size;

                for (int w = 0; w < end; w++) {
                    ref WordInfo2 wordInfo = ref wordInfos[w];

                    switch (wordInfo.type) {
                        case WordType.Whitespace:
                        case WordType.NewLine:
                            break;

                        case WordType.SoftHyphen:
                        case WordType.Normal:
                            if (wordInfo.width > maxWidth) {
                                maxWidth = wordInfo.width;
                            }

                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return maxWidth;
        }

        private void UpdateIntrinsics() {
            intrinsics.minWidth = ComputeIntrinsicMinWidth();
            StructList<LineInfo2> lines = StructList<LineInfo2>.Get();
            RunLayout(lines, intrinsics.minWidth);
            intrinsics.minHeight = lines[lines.size - 1].y + lines[lines.size - 1].height;
            lines.size = 0;
            RunLayout(lines, float.MaxValue);

            float maxWidth = 0;
            for (int i = 0; i < lines.size; i++) {
                if (maxWidth < lines.array[i].width) {
                    maxWidth = lines.array[i].width;
                }
            }

            intrinsics.prefWidth = maxWidth;
            intrinsics.prefHeight = lines[lines.size - 1].y + lines[lines.size - 1].height;

            StructList<LineInfo2>.Release(ref lines);
        }

       
        public float GetIntrinsicWidth() {
            if (requiresSpanListRebuild) {
                RebuildSpanList();
            }

            return intrinsics.prefWidth;
        }

        public float GetIntrinsicHeight() {
            if (requiresSpanListRebuild) {
                RebuildSpanList();
            }

            return intrinsics.prefHeight;
        }

        public float GetIntrinsicMinWidth() {
            if (requiresSpanListRebuild) {
                RebuildSpanList();
            }

            return intrinsics.minWidth;
        }

        public float GetIntrinsicMinHeight() {
            if (requiresSpanListRebuild) {
                RebuildSpanList();
            }

            return intrinsics.minHeight;
        }

        public float ComputeHeightForWidth(float width, BlockSize blockWidth, BlockSize blockHeight) {
            // todo -- if has span content that is not text we need to use block width & height to resolve their sizes

            // can't use intrinsics here if we have content that is not text

            if (requiresSpanListRebuild) {
                RebuildSpanList();
            }

            if (Mathf.Approximately(width, intrinsics.minWidth)) {
                return intrinsics.minHeight;
            }

            if (Mathf.Approximately(width, intrinsics.prefWidth)) {
                return intrinsics.prefHeight;
            }

            StructList<LineInfo2> lines = StructList<LineInfo2>.Get();

            RunLayout(lines, float.MaxValue);

            float retn = lines[lines.size - 1].y + lines[lines.size - 1].height;

            lines.Release();

            return retn;
        }

        // todo -- if any span has content need to use block width to resolve it since it will be a layout box most likely
        public float ComputeContentWidth(float blockWidth) {
            return GetIntrinsicWidth();
        }


        public struct IntrinsicSizes {

            public float minWidth;
            public float prefWidth;
            public float minHeight;
            public float prefHeight;

        }

    }

}