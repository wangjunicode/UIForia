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

            LightStack<TextSpan>.Release(ref stack);
        }

        public Size Layout(Vector2 offset = default, float width = float.MaxValue) {
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
            // ApplyTextAlignment(maxWidth, style.TextAlignment);
            ApplyLineAndWordOffsets();
            metrics = new Size(maxWidth, height);
            return metrics;
        }

        private void ApplyLineAndWordOffsets(int lineStart = 0) {
            // todo -- this can be done partially in the case where we append or insert text 

            LineInfo2[] lines = lineInfoList.array;
            int lineCount = lineInfoList.size;

            // todo -- right / left / center align lines

            int spanCount = spanList.Count;
            TextSpan[] spans = spanList.Array;

            // todo -- if linestart != 0 this is wrong!
            for (int i = lines[lineStart].spanStart; i < spanCount; i++) {
                if (spans[i].geometryList == null) {
                    spans[i].geometryList = new StructList<TextGeometry>(0);
                }
                spans[i].geometryList.size = 0;
            }

            float lineOffsetY = 0;

            for (int lineIndex = lineStart; lineIndex < lineCount; lineIndex++) {
                int spanStart = lines[lineIndex].spanStart;
                int spanEnd = lines[lineIndex].spanEnd;

                float lineOffsetX = 0; // alignment
                float wordOffsetX = lineOffsetX;
                
                for (int s = spanStart; s < spanEnd; s++) {
                    TextSpan span = spans[s];
                    WordInfo2[] words = span.wordInfoList.array;
                    CharInfo2[] chars = span.charInfoList.array;

                    if (span.geometryList == null) {
                        span.geometryList = new StructList<TextGeometry>(0);
                    }
                    
                    int idx = span.geometryList.size;

                    int wordStart = s == spanStart ? lines[lineIndex].wordStart : 0;
                    int wordEnd = s == spanEnd - 1 ? lines[lineIndex].wordEnd : span.wordInfoList.size;
                    int geometrySize = 0;

                    for (int w = wordStart; w < wordEnd; w++) {
                        if (words[w].type == WordType.Normal) {
                            geometrySize += words[w].charEnd - words[w].charStart;
                        }
                    }

                    span.geometryList.EnsureAdditionalCapacity(geometrySize);
                    TextGeometry[] geometry = span.geometryList.array;

                    for (int w = wordStart; w < wordEnd; w++) {
                        if (words[w].type == WordType.Normal) {
                            int charStart = words[w].charStart;
                            int charEnd = words[w].charEnd;

                            for (int c = charStart; c < charEnd; c++) {
                                ref Vector2 topLeft = ref chars[c].topLeft;
                                ref Vector2 bottomRight = ref chars[c].bottomRight;
                                ref CharInfo2 charInfo = ref chars[c];
                                ref TextGeometry textGeometry = ref geometry[idx++];
                                textGeometry.topShear = charInfo.topShear;
                                textGeometry.bottomShear = charInfo.bottomShear;
                                textGeometry.topLeft.x = wordOffsetX + topLeft.x;
                                textGeometry.topLeft.y = lineOffsetY + topLeft.y;
                                textGeometry.bottomRight.x = wordOffsetX + bottomRight.x;
                                textGeometry.bottomRight.y = lineOffsetY + bottomRight.y;
                                textGeometry.topLeftTexCoord = chars[c].topLeftUV;
                                textGeometry.bottomRightTexCoord = chars[c].bottomRightUV;
                                textGeometry.scale = chars[c].scale;
                            }
                        }

                        wordOffsetX += words[w].width;
                    }

                    span.geometryList.size = idx;
                }

                lineOffsetY += lines[lineIndex].height;
            }
        }

        public float GetIntrinsicMinWidth() {
            float maxWord = 0;
            for (int i = 0; i < spanList.size; i++) {
                
                if (spanList.array[i].isEnabled) {

                    if (maxWord < spanList.array[i].longestWordSize) {
                        maxWord = spanList.array[i].longestWordSize;
                    }

                }
                
            }

            return maxWord;
        }
        
        private StructList<LineInfo2> RunLayout(StructList<LineInfo2> lines, float width) {
            if (!requiresLayout) return lines;

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

                int end = span.wordInfoList.size;
                for (int w = 0; w < end; w++) {
                    WordInfo2 wordInfo = wordInfos[w];

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
                                currentLine.height = wordInfo.height;
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
                                currentLine.height = wordInfo.height;
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

    }

}