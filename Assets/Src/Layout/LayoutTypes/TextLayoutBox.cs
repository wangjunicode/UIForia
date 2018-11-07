using System.Collections.Generic;
using TMPro;
using UIForia.Systems;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using TextAlignment = UIForia.Text.TextAlignment;

namespace UIForia.Layout.LayoutTypes {

    public class TextLayoutBox : LayoutBox {

        public TextLayoutBox(LayoutSystem layoutSystem, UIElement element)
            : base(layoutSystem, element) { }

        protected override float ComputeContentWidth() {
            TextInfo textInfo = ((UITextElement) element).textInfo;
            List<LineInfo> lineInfos = RunLayout(textInfo, float.MaxValue);

            ((UITextElement) element).textInfo = textInfo;

            float maxWidth = 0;
            for (int i = 0; i < lineInfos.Count; i++) {
                maxWidth = Mathf.Max(maxWidth, lineInfos[i].width);
            }

            ListPool<LineInfo>.Release(ref lineInfos);
            
            return maxWidth;
        }

        protected override float ComputeContentHeight(float width) {
            TextInfo textInfo = ((UITextElement) element).textInfo;
            List<LineInfo> lineInfos = RunLayout(textInfo, width);
            LineInfo lastLine = lineInfos[lineInfos.Count - 1];
            ListPool<LineInfo>.Release(ref lineInfos);
            TMP_FontAsset asset = style.TextFontAsset;
            float scale = (style.TextFontSize / asset.fontInfo.PointSize) * asset.fontInfo.Scale;
            float lh = (asset.fontInfo.Ascender - asset.fontInfo.Descender) *scale; 
            return lastLine.position.y + lh;
        }

        public override void RunLayout() {
            TextInfo textInfo = ((UITextElement) element).textInfo;
            List<LineInfo> lineInfos = RunLayout(textInfo, allocatedWidth);
            textInfo.lineInfos = ArrayPool<LineInfo>.CopyFromList(lineInfos);
            textInfo.lineCount = lineInfos.Count;
            ((UITextElement) element).textInfo = textInfo;

            float maxWidth = 0;
            float topOffset = PaddingTop + BorderTop;
            float leftOffset = PaddingLeft + BorderLeft;
            for (int i = 0; i < lineInfos.Count; i++) {
                textInfo.lineInfos[i].position = new Vector2(
                    lineInfos[i].position.x + leftOffset,
                    lineInfos[i].position.y + topOffset
                );
                maxWidth = Mathf.Max(maxWidth, lineInfos[i].width);
            }

            actualWidth = maxWidth + PaddingHorizontal + BorderHorizontal;
            TMP_FontAsset asset = style.TextFontAsset;
            float scale = (style.TextFontSize / asset.fontInfo.PointSize) * asset.fontInfo.Scale;
            float lh = (asset.fontInfo.Ascender - asset.fontInfo.Descender) *scale; 
            actualHeight = textInfo.lineInfos[textInfo.lineInfos.Length - 1].position.y + lh + PaddingBottom + BorderBottom;

            ApplyTextAlignment(allocatedWidth, textInfo, style.TextAlignment);

            ListPool<LineInfo>.Release(ref lineInfos);
        }

        // todo -- when starting a new line, if the last line has spaces as the final characters, consider stripping them

        public static float GetLineOffset(TMP_FontAsset asset) {
            FaceInfo fontInfo = asset.fontInfo;
            return 0f;//(fontInfo.LineHeight - (fontInfo.Ascender - fontInfo.Descender))* 0.5f;
        }
        
        private List<LineInfo> RunLayout(TextInfo textInfo, float width) {
            TMP_FontAsset asset = style.TextFontAsset;
            float scale = (style.TextFontSize / asset.fontInfo.PointSize) * asset.fontInfo.Scale;
            float lh = (asset.fontInfo.Ascender + asset.fontInfo.Descender) * scale;//asset.fontInfo.LineHeight * scale;

            float lineOffset = GetLineOffset(asset);
            
            LineInfo currentLine = new LineInfo();
            WordInfo[] wordInfos = textInfo.wordInfos;
            List<LineInfo> lineInfos = ListPool<LineInfo>.Get();

            currentLine.position = new Vector2(0, lineOffset);
            width = Mathf.Max(width - PaddingHorizontal + BorderHorizontal, 0);
            
            for (int w = 0; w < textInfo.wordCount; w++) {
                WordInfo currentWord = wordInfos[w];

                if (currentWord.isNewLine) {
                    lineInfos.Add(currentLine);
                    lineOffset += lh;
                    //(currentLine.maxDescender + textInfo.charInfos[currentWord.startChar + currentWord.VisibleCharCount - 1].ascender + lineGap) * baseScale;
                    currentLine = new LineInfo();
                    currentLine.position = new Vector2(0, lineOffset);
                    currentLine.wordStart = w + 1;
                    continue;
                }

                if (currentWord.characterSize > width + 0.01f) {
                    // we had words in this line already
                    // finish the line and start a new one
                    // line offset needs to to be bumped
                    if (currentLine.wordCount > 0) {
                        lineInfos.Add(currentLine);
                        lineOffset += lh;
                        //-currentLine.maxDescender + textInfo.charInfos[currentWord.startChar + currentWord.VisibleCharCount - 1].ascender + (lineGap) * baseScale;
                    }

                    currentLine = new LineInfo();
                    currentLine.position = new Vector2(0, lineOffset);
                    currentLine.wordStart = w;
                    currentLine.wordCount = 1;
                  //  currentLine.maxAscender = currentWord.ascender;
                  //  currentLine.maxDescender = currentWord.descender;
                    currentLine.width = currentWord.size.x;
                    lineInfos.Add(currentLine);

                    lineOffset += lh;
                    //-currentLine.maxDescender + textInfo.charInfos[currentWord.startChar + currentWord.VisibleCharCount - 1].ascender + (lineGap) * baseScale;
                    currentLine = new LineInfo();
                    currentLine.wordStart = w + 1;
                    currentLine.position = new Vector2(0, lineOffset);
                }

                else if (currentLine.width + currentWord.size.x > width + 0.01f) {
                    // characters fit but space does not, strip spaces and start new line w/ next word
                    if (currentLine.width + currentWord.characterSize < width + 0.01f) {
                        currentLine.wordCount++;

                     //   if (currentLine.maxAscender < currentWord.ascender) currentLine.maxAscender = currentWord.ascender;
                     //   if (currentLine.maxDescender > currentWord.descender) currentLine.maxDescender = currentWord.descender;
                        currentLine.width += currentWord.characterSize;
                        lineInfos.Add(currentLine);

                        lineOffset += lh;
                        //-currentLine.maxDescender + textInfo.charInfos[currentWord.startChar + currentWord.VisibleCharCount - 1].ascender + (lineGap) * baseScale;

                        currentLine = new LineInfo();
                        currentLine.position = new Vector2(0, lineOffset);
                        currentLine.wordStart = w + 1;
                        continue;
                    }

                    lineInfos.Add(currentLine);
                    lineOffset += lh;
                    //-currentLine.maxDescender + textInfo.charInfos[currentWord.startChar + currentWord.VisibleCharCount - 1].ascender + (lineGap) * baseScale;
                    currentLine = new LineInfo();
                    currentLine.position = new Vector2(0, lineOffset);
                    currentLine.wordStart = w;
                    currentLine.wordCount = 1;
                    currentLine.width = currentWord.size.x;
                 //   currentLine.maxAscender = currentWord.ascender;
                 //   currentLine.maxDescender = currentWord.descender;
                }

                else {
                    currentLine.wordCount++;

                 //   if (currentLine.maxAscender < currentWord.maxCharTop) currentLine.maxAscender = currentWord.maxCharTop;
                  //  if (currentLine.maxDescender > currentWord.minCharBottom) currentLine.maxDescender = currentWord.minCharBottom;

                    currentLine.width += currentWord.xAdvance;
                }
            }

            if (currentLine.wordCount > 0) {
                lineInfos.Add(currentLine);
            }

            return lineInfos;
        }

        private static void ApplyTextAlignment(float allocatedWidth, TextInfo textInfo, Text.TextAlignment alignment) {
            LineInfo[] lineInfos = textInfo.lineInfos;

            // find max line width
            // if line width < allocated width use allocated width

            float lineWidth = allocatedWidth;
            for (int i = 0; i < textInfo.lineCount; i++) {
                lineWidth = Mathf.Max(lineWidth, lineInfos[i].width);
            }

            switch (alignment) {
                case TextAlignment.Center:

                    for (int i = 0; i < textInfo.lineCount; i++) {
                        float offset = (lineWidth - lineInfos[i].width) * 0.5f;
                        if (offset <= 0) break;
                        lineInfos[i].position = new Vector2(offset, lineInfos[i].position.y);
                    }

                    break;
                case TextAlignment.Right:

                    for (int i = 0; i < textInfo.lineCount; i++) {
                        float offset = (lineWidth - lineInfos[i].width);
                        lineInfos[i].position = new Vector2(offset, lineInfos[i].position.y);
                    }

                    break;
            }
        }

        public void OnTextContentUpdated() {
            RequestContentSizeChangeLayout();
        }

    }

}