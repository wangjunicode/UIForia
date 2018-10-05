using System.Collections.Generic;
using Src.Systems;
using Src.Text;
using Src.Util;
using TMPro;
using UnityEngine;

namespace Src.Layout.LayoutTypes {

    public class TextContainerLayoutBox : LayoutBox {

        public TextContainerLayoutBox(LayoutSystem2 layoutSystem, UIElement element)
            : base(layoutSystem, element) { }

        public override void RunLayout() {

            TextInfo textInfo = ((UITextElement)element).textInfo;
            List<LineInfo> lineInfos = RunLayout(textInfo, allocatedWidth);
            LineInfo lastLine = lineInfos[lineInfos.Count - 1];

            textInfo.lineInfos = ArrayPool<LineInfo>.CopyFromList(lineInfos);
            textInfo.lineCount = lineInfos.Count;
            ((UITextElement) element).textInfo = textInfo;
            
            float maxWidth = 0;
            for (int i = 0; i < lineInfos.Count; i++) {
                maxWidth = Mathf.Max(maxWidth, lineInfos[i].size.x);
            }

            actualWidth = maxWidth;
            actualHeight = lastLine.position.y + lastLine.Height + 3f;
            ListPool<LineInfo>.Release(lineInfos);
        }

        protected override Size RunContentSizeLayout() {
            TextInfo textInfo = ((UITextElement)element).textInfo;

            List<LineInfo> lineInfos = RunLayout(textInfo, float.MaxValue);
            LineInfo lastLine = lineInfos[lineInfos.Count - 1];

            float maxWidth = 0;
            for (int i = 0; i < lineInfos.Count; i++) {
                maxWidth = Mathf.Max(maxWidth, lineInfos[i].size.x);
            }

            ListPool<LineInfo>.Release(lineInfos);
            return new Size(maxWidth, -lastLine.position.y + lastLine.Height + 3f); // todo -- 3 is line gap
        }

        // note -- all alignment / justification happens in the renderer
        // might need to do this for text selection also..basically do it once on demand
        private static List<LineInfo> RunLayout(TextInfo textInfo, float width) {
            float lineOffset = 0;
            SpanInfo spanInfo = textInfo.spanInfos[0];

            LineInfo currentLine = new LineInfo();
            WordInfo[] wordInfos = textInfo.wordInfos;
            List<LineInfo> lineInfos = ListPool<LineInfo>.Get();
                       

            TMP_FontAsset font = spanInfo.font ? spanInfo.font : TMP_FontAsset.defaultFontAsset;
            float lineGap = font.fontInfo.LineHeight - (font.fontInfo.Ascender - font.fontInfo.Descender);
            float baseScale = (spanInfo.fontSize / font.fontInfo.PointSize * font.fontInfo.Scale);
            float lineHeight = (font.fontInfo.LineHeight + lineGap) * baseScale;
            // todo -- might want to use an optional 'lineHeight' setting instead of just computing the line height

            currentLine.position.y = lineGap;
            for (int w = 0; w < textInfo.wordCount; w++) {
                WordInfo currentWord = wordInfos[w];

//                string s = "";
//                float lineWidth = currentLine.size.x;
//                float wordWidth = currentWord.size.x;
//                float widthIfAdded = lineWidth + wordWidth;
//                
//                for (int i = 0; i < currentWord.charCount; i++) {
//                    s += textInfo.charInfos[currentWord.startChar + i].character;
//                }
               
                if (currentWord.isNewLine) {
                    lineInfos.Add(currentLine);
                    lineOffset -= (currentLine.maxDescender + textInfo.charInfos[currentWord.startChar + currentWord.charCount].ascender + lineGap) * baseScale;
                    currentLine = new LineInfo();
                    currentLine.position = new Vector2(0, lineOffset);
                    currentLine.wordStart = w + 1;
                    continue;
                }

                if (currentWord.characterSize > width) {
                    // we had words in this line already
                    // finish the line and start a new one
                    // line offset needs to to be bumped
                    if (currentLine.wordCount > 0) {
                        lineInfos.Add(currentLine);
                        lineOffset -= -currentLine.maxDescender + textInfo.charInfos[currentWord.startChar + currentWord.charCount].ascender + (lineGap) * baseScale;
                    }
                    currentLine = new LineInfo();
                    currentLine.position = new Vector2(0, lineOffset);
                    currentLine.wordStart = w;
                    currentLine.wordCount = 1;
                    currentLine.maxAscender = currentWord.ascender;
                    currentLine.maxDescender = currentWord.descender;
                    currentLine.size = new Vector2(currentWord.size.x, currentLine.Height);
                    lineInfos.Add(currentLine);

                    lineOffset -= -currentLine.maxDescender + textInfo.charInfos[currentWord.startChar + currentWord.charCount].ascender + (lineGap) * baseScale;
                    currentLine = new LineInfo();
                    currentLine.wordStart = w + 1;
                    currentLine.position = new Vector2(0, lineOffset);
                }

                else if (currentLine.size.x + currentWord.size.x > width) {

                    // characters fit but space does not, strip spaces and start new line w/ next word
                    if (currentLine.size.x + currentWord.characterSize < width) {
                        currentLine.wordCount++;
                        
                        if (currentLine.maxAscender < currentWord.ascender) currentLine.maxAscender = currentWord.ascender;
                        if (currentLine.maxDescender > currentWord.descender) currentLine.maxDescender = currentWord.descender;
                        currentLine.size = new Vector2(currentLine.size.x + currentWord.characterSize, currentLine.Height);
                        lineInfos.Add(currentLine);
                        
                        lineOffset -= -currentLine.maxDescender + textInfo.charInfos[currentWord.startChar + currentWord.charCount].ascender + (lineGap) * baseScale;
                        
                        currentLine = new LineInfo();
                        currentLine.position = new Vector2(0, lineOffset);
                        currentLine.wordStart = w + 1;
                        continue;
                    }
                    lineInfos.Add(currentLine);
                    lineOffset -= -currentLine.maxDescender + textInfo.charInfos[currentWord.startChar + currentWord.charCount].ascender + (lineGap) * baseScale;
                    currentLine = new LineInfo();
                    currentLine.position = new Vector2(0, lineOffset);
                    currentLine.wordStart = w;
                    currentLine.wordCount = 1;
                    currentLine.size.x = currentWord.size.x;
                    currentLine.maxAscender = currentWord.ascender;
                    currentLine.maxDescender = currentWord.descender;
                }

                else {
                    currentLine.wordCount++;
                    if (currentLine.maxAscender < currentWord.ascender) currentLine.maxAscender = currentWord.ascender;
                    if (currentLine.maxDescender > currentWord.descender) currentLine.maxDescender = currentWord.descender;
                    currentLine.size = new Vector2(currentLine.size.x + currentWord.xAdvance, currentLine.Height);
                }
            }

            if (currentLine.wordCount > 0) {
                lineInfos.Add(currentLine);
            }

            return lineInfos;
        }

        public void SetTextContent(string text) {
          
        }

    }

 
}