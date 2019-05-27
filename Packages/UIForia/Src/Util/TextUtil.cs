using SVGX;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Text {

    public struct WordInfo2 {

        public WordType type;
        public int charStart;
        public int charEnd;
        public float width;

    }

    public enum WordType {

        Whitespace,
        NewLine,
        Normal,
        SoftHyphen

    }

    public static class TextUtil {

        // whitespace processing needs to happen in two phases. the first is where we collapse whitespace and handle new lines
        // the second is what to do with trailing space and wrapping.
        public static int ProcessWhitespace(WhitespaceMode whitespaceMode, ref char[] buffer, char[] input, int inputSize = -1) {
            if (inputSize < 0) inputSize = input.Length;

            bool collapseSpaceAndTab = (whitespaceMode & WhitespaceMode.CollapseWhitespace) != 0;
            bool preserveNewLine = (whitespaceMode & WhitespaceMode.PreserveNewLines) != 0;
            bool trimStart = (whitespaceMode & WhitespaceMode.TrimStart) != 0;
            bool trimEnd = (whitespaceMode & WhitespaceMode.TrimEnd) != 0;

            bool collapsing = collapseSpaceAndTab;

            if (buffer == null) {
                buffer = ArrayPool<char>.GetMinSize(inputSize);
            }

            if (buffer.Length < input.Length) {
                ArrayPool<char>.Resize(ref buffer, inputSize);
            }

            int writeIndex = 0;
            int start = 0;
            int end = input.Length;

            if (trimStart) {
                for (int i = 0; i < end; i++) {
                    char c = input[i];

                    bool isWhiteSpace = c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085');

                    if (!isWhiteSpace) {
                        start = i;
                        break;
                    }
                }
            }

            if (trimEnd) {
                for (int i = end - 1; i >= start; i--) {
                    char c = input[i];

                    bool isWhiteSpace = c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085');

                    if (!isWhiteSpace) {
                        end = i + 1;
                        break;
                    }
                }
            }

            for (int i = start; i < end; i++) {
                char c = input[i];

                if (c == '\n' && !preserveNewLine) {
                    continue;
                }

                bool isWhiteSpace = c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085');

                if (c == '\n') {
                    if (preserveNewLine) {
                        buffer[writeIndex++] = c;
                        continue;
                    }
                }

                if (collapsing) {
                    if (!isWhiteSpace) {
                        buffer[writeIndex++] = c;
                        collapsing = false;
                    }
                }
                else if (isWhiteSpace) {
                    collapsing = collapseSpaceAndTab;
                    buffer[writeIndex++] = ' ';
                }
                else {
                    buffer[writeIndex++] = c;
                }
            }


            return writeIndex;
        }

        public static StructList<WordInfo2> BreakIntoWords(char[] buffer, int bufferSize = -1) {
            if (bufferSize < 0) bufferSize = buffer.Length;

            StructList<WordInfo2> retn = StructList<WordInfo2>.Get();
            WordInfo2 currentWord = new WordInfo2();
            WordType previousType = WordType.Normal;

            char c = buffer[0];

            if (c == '\n') {
                previousType = WordType.NewLine;
            }
            else if (c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085')) {
                previousType = WordType.Whitespace;
            }
            else if (c == 0xAD) {
                previousType = WordType.SoftHyphen;
            }
            else {
                previousType = WordType.Normal;
            }

            currentWord.type = previousType;
            currentWord.charStart = 0;
            currentWord.charEnd = 1;

            for (int i = 1; i < bufferSize; i++) {
                c = buffer[i];

                WordType type = WordType.Normal;

                if (c == '\n') {
                    type = WordType.NewLine;
                }
                else if (c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085')) {
                    type = WordType.Whitespace;
                }
                else if (c == 0xAD) {
                    type = WordType.SoftHyphen;
                }

                if (type == previousType) {
                    if (type == WordType.NewLine) {
                        retn.Add(currentWord);
                        currentWord.type = type;
                        currentWord.charStart = i;
                        currentWord.charEnd = i + 1;
                    }
                    else {
                        currentWord.charEnd++;
                    }
                }
                else {
                    retn.Add(currentWord);
                    currentWord.type = type;
                    currentWord.charStart = i;
                    currentWord.charEnd = i + 1;
                }

                previousType = type;
            }

            if (currentWord.charEnd > 0) {
                retn.Add(currentWord);
            }

            return retn;
        }

        public static void TransformText(TextTransform transform, char[] buffer, int count = -1) {
            if (count < 0) count = buffer.Length;

            switch (transform) {
                case TextTransform.UpperCase:
                case TextTransform.SmallCaps:
                    for (int i = 0; i < count; i++) {
                        buffer[i] = char.ToUpper(buffer[i]);
                    }

                    break;
                case TextTransform.LowerCase:
                    for (int i = 0; i < count; i++) {
                        buffer[i] = char.ToLower(buffer[i]);
                    }

                    break;
                case TextTransform.TitleCase:
                    for (int i = 0; i < count - 1; i++) {
                        if (char.IsLetter(buffer[i]) && char.IsWhiteSpace(buffer[i - 1])) {
                            buffer[i] = char.ToUpper(buffer[i]);
                        }
                    }

                    break;
            }
        }

        public static float GetPadding(in SVGXTextStyle textStyle, in Vector3 ratios) {
             float4 padding = Vector4.zero;

            float scaleRatio_A = ratios.x;
            float scaleRatio_B = ratios.y;
            float scaleRatio_C = ratios.z;
            
            float faceDilate = textStyle.faceDilate * scaleRatio_A;
            float faceSoftness = textStyle.outlineSoftness * scaleRatio_A;
            float outlineThickness = textStyle.outlineWidth * scaleRatio_A;

            float uniformPadding = outlineThickness + faceSoftness + faceDilate;

            float glowOffset = textStyle.glowOffset * scaleRatio_B;
            float glowOuter = textStyle.glowOuter * scaleRatio_B;

            uniformPadding = math.max(uniformPadding, faceDilate + glowOffset + glowOuter);

            float offsetX = textStyle.underlayX * scaleRatio_C;
            float offsetY = textStyle.underlayY * scaleRatio_C;
            float dilate = textStyle.underlayDilate * scaleRatio_C;
            float softness = textStyle.underlaySoftness * scaleRatio_C;

            // tmp does a max check here with 0, I don't think we need it though
            padding.x = faceDilate + dilate + softness - offsetX;
            padding.y = faceDilate + dilate + softness - offsetY;
            padding.z = faceDilate + dilate + softness + offsetX;
            padding.w = faceDilate + dilate + softness + offsetY;

            padding.x = math.max(padding.x, uniformPadding);
            padding.y = math.max(padding.y, uniformPadding);
            padding.z = math.max(padding.z, uniformPadding);
            padding.w = math.max(padding.w, uniformPadding);

            padding.x = math.min(padding.x, 1);
            padding.y = math.min(padding.y, 1);
            padding.z = math.min(padding.z, 1);
            padding.w = math.min(padding.w, 1);
            
            padding *= textStyle.fontAsset.gradientScale;

            // Set UniformPadding to the maximum value of any of its components.
            uniformPadding = math.max(padding.x, padding.y);
            uniformPadding = math.max(padding.z, uniformPadding);
            uniformPadding = math.max(padding.w, uniformPadding);

            return uniformPadding + 0.5f;
        }

        public static Vector3 ComputeRatios(in SVGXTextStyle textStyle) {
            FontAsset fontAsset = textStyle.fontAsset; 
            float gradientScale = fontAsset.gradientScale;
            float faceDilate = 0f; // todo -- from style
            float outlineThickness = textStyle.outlineWidth;
            float outlineSoftness = textStyle.outlineSoftness;
            float weight = (fontAsset.weightNormal > fontAsset.weightBold ? fontAsset.weightNormal : fontAsset.weightBold) / 4f;
            float ratioA_t = weight + faceDilate + outlineThickness + outlineSoftness;
            ratioA_t = ratioA_t > 1 ? 1 : ratioA_t;
            float ratioA = (gradientScale - 1f) / (gradientScale * ratioA_t);

            float glowOffset = textStyle.glowOffset;
            float glowOuter = textStyle.glowOuter;
            float ratioBRange = (weight + faceDilate) * (gradientScale - 1f);

            float ratioB_t = Mathf.Max(1, glowOffset + glowOuter);
            float ratioB = Mathf.Max(0, gradientScale - 1 - ratioBRange) / (gradientScale * ratioB_t);
            float underlayOffsetX = textStyle.underlayX;
            float underlayOffsetY = textStyle.underlayY;
            float underlayDilate = textStyle.underlayDilate;
            float underlaySoftness = textStyle.underlaySoftness;

            float ratioCRange = (weight = faceDilate) * (gradientScale - 1);
            float ratioC_t = Mathf.Max(1, Mathf.Max(Mathf.Abs(underlayOffsetX), Mathf.Abs(underlayOffsetY)) + underlayDilate + underlaySoftness);

            float ratioC = Mathf.Max(0, gradientScale - 1f - ratioCRange) / (gradientScale * ratioC_t);

            return new Vector3(ratioA, ratioB, ratioC);
        }

    }

}
//using System.Collections.Generic;
//using System.Text;
//using SVGX;
//using TMPro;
//using UIForia.Rendering;
//using UIForia.Text;
//using UnityEngine;
//using FontStyle = UIForia.Text.FontStyle;
//
//namespace UIForia.Util {
//
//    public static class TextUtil {
//
//        public struct TextSpan {
//
//            public readonly TMP_FontAsset fontAsset;
//            public readonly SVGXTextStyle textStyle;
//            public readonly string text;
//
//            public TextSpan(TMP_FontAsset fontAsset, SVGXTextStyle textStyle, string text) {
//                this.fontAsset = fontAsset;
//                this.textStyle = textStyle;
//                this.text = text;
//            }
//
//        }
//
//        public static string ProcessWrapString(string input, bool collapseSpaceAndTab = true, bool preserveNewLine = false) {
//            char[] buffer = null;
//            int count = ProcessWrap(input, collapseSpaceAndTab, preserveNewLine, ref buffer);
//            return new string(buffer, 0, count);
//        }
//
//       
//
//        public static int StringToCharArray(string sourceText, ref int[] charBuffer, bool parseControlCharacters = false) {
//            if (string.IsNullOrEmpty(sourceText)) {
//                charBuffer =  harBuffer ?? new int[0];
//                return 0;
//            }
//
//            if (charBuffer == null) {
//                charBuffer = charBuffer = ArrayPool<int>.GetMinSize(sourceText.Length);
//            }
//
//            if (charBuffer.Length < sourceText.Length) {
//                ArrayPool<int>.Resize(ref charBuffer, sourceText.Length);
//            }
//
//            if (!parseControlCharacters) {
//                for (int i = 0; i < sourceText.Length; i++) {
//                    char current = sourceText[i];
//                    charBuffer[i] = current;
//                }
//
//                return sourceText.Length;
//            }
//
//            int writeIndex = 0;
//
//            for (int i = 0; i < sourceText.Length; i++) {
//                char current = sourceText[i];
//
//                if (current == 92 && sourceText.Length > i + 1) { // ascii '\'
//                    switch ((int) sourceText[i + 1]) {
//                        case 85: // \U00000000 for UTF-32 Unicode
//                            if (sourceText.Length > i + 9) {
//                                charBuffer[writeIndex] = GetUTF32(sourceText, i + 2);
//                                i += 9;
//                                writeIndex += 1;
//                                continue;
//                            }
//
//                            break;
//                        case 92: // \ escape
//
//                            if (sourceText.Length <= i + 2) break;
//
//                            charBuffer[writeIndex] = sourceText[i + 1];
//                            charBuffer[writeIndex + 1] = sourceText[i + 2];
//                            i += 2;
//                            writeIndex += 2;
//                            continue;
//                        case 110: // \n LineFeed
//
//                            charBuffer[writeIndex] = (char) 10;
//                            i += 1;
//                            writeIndex += 1;
//                            continue;
//                        case 114: // \r
//
//                            charBuffer[writeIndex] = (char) 13;
//                            i += 1;
//                            writeIndex += 1;
//                            continue;
//                        case 116: // \t Tab
//
//                            charBuffer[writeIndex] = (char) 9;
//                            i += 1;
//                            writeIndex += 1;
//                            continue;
//
//                        case 117: // \u0000 for UTF-16 Unicode
//                            if (sourceText.Length > i + 5) {
//                                charBuffer[writeIndex] = (char) GetUTF16(sourceText, i + 2);
//                                i += 5;
//                                writeIndex += 1;
//                                continue;
//                            }
//
//                            break;
//                    }
//                }
////                handle UTF32
////                if (char.IsHighSurrogate(sourceText[i]) && char.IsLowSurrogate(sourceText[i + 1])) {
////                    charBuffer[writeIndex] = char.ConvertToUtf32(sourceText[i], sourceText[i + 1]);
////                    i += 1;
////                    writeIndex += 1;
////                    continue;
////                }
//
//                // todo -- maybe handle <br/> here
//
//                charBuffer[writeIndex] = sourceText[i];
//                writeIndex += 1;
//            }
//
//            charBuffer[writeIndex] = (char) 0;
//            return writeIndex + 1;
//        }
//
//        public static int HexToInt(char hex) {
//            switch (hex) {
//                case '0': return 0;
//                case '1': return 1;
//                case '2': return 2;
//                case '3': return 3;
//                case '4': return 4;
//                case '5': return 5;
//                case '6': return 6;
//                case '7': return 7;
//                case '8': return 8;
//                case '9': return 9;
//                case 'A': return 10;
//                case 'B': return 11;
//                case 'C': return 12;
//                case 'D': return 13;
//                case 'E': return 14;
//                case 'F': return 15;
//                case 'a': return 10;
//                case 'b': return 11;
//                case 'c': return 12;
//                case 'd': return 13;
//                case 'e': return 14;
//                case 'f': return 15;
//            }
//
//            return 15;
//        }
//
//        public static int GetUTF16(string text, int i) {
//            int unicode = 0;
//            unicode += HexToInt(text[i]) << 12;
//            unicode += HexToInt(text[i + 1]) << 8;
//            unicode += HexToInt(text[i + 2]) << 4;
//            unicode += HexToInt(text[i + 3]);
//            return unicode;
//        }
//
//        public static int GetUTF32(string text, int i) {
//            int unicode = 0;
//            unicode += HexToInt(text[i]) << 30;
//            unicode += HexToInt(text[i + 1]) << 24;
//            unicode += HexToInt(text[i + 2]) << 20;
//            unicode += HexToInt(text[i + 3]) << 16;
//            unicode += HexToInt(text[i + 4]) << 12;
//            unicode += HexToInt(text[i + 5]) << 8;
//            unicode += HexToInt(text[i + 6]) << 4;
//            unicode += HexToInt(text[i + 7]);
//            return unicode;
//        }
//
//        // line info is processed only when doing wrapping, not here
//        public static TextInfo ProcessText(string text, bool collapseWhitespace, bool preserveNewLines, TextTransform textTransform = TextTransform.None) {
//            char[] buffer = null;
//            int bufferLength = ProcessWrap(text, collapseWhitespace, preserveNewLines, ref buffer);
//
//            List<WordInfo> s_WordInfoList = ListPool<WordInfo>.Get();
//
//            WordInfo currentWord = new WordInfo();
//
//            bool inWhiteSpace = false;
//            CharInfo[] charInfos = ArrayPool<CharInfo>.GetExactSize(buffer.Length);
//            for (int i = 0; i < bufferLength; i++) {
//                int charCode = buffer[i];
//                charInfos[i] = new CharInfo();
//                charInfos[i].character = (char) charCode;
//
//                if ((char) charCode == '\n') {
//                    if (currentWord.charCount > 0) {
//                        s_WordInfoList.Add(currentWord);
//                        currentWord = new WordInfo();
//                        currentWord.startChar = i;
//                    }
//
//                    currentWord.charCount = 1;
//                    currentWord.spaceStart = 0;
//                    currentWord.isNewLine = true;
//                    s_WordInfoList.Add(currentWord);
//                    currentWord = new WordInfo();
//                    currentWord.startChar = i + 1;
//                }
//
//                if (!char.IsWhiteSpace((char) charCode)) {
//                    if (inWhiteSpace) {
//                        // new word starts
//                        s_WordInfoList.Add(currentWord);
//                        currentWord = new WordInfo();
//                        currentWord.startChar = i;
//                        inWhiteSpace = false;
//                    }
//
//                    currentWord.charCount++;
//                }
//                else {
//                    if (!inWhiteSpace) {
//                        inWhiteSpace = true;
//                        currentWord.spaceStart = currentWord.charCount;
//                    }
//
//                    currentWord.charCount++;
//                }
//            }
//
//            if (!inWhiteSpace) {
//                currentWord.spaceStart = currentWord.charCount;
//            }
//
//            s_WordInfoList.Add(currentWord);
//
//            if (textTransform == TextTransform.TitleCase) {
//                for (int i = 0; i < s_WordInfoList.Count; i++) {
//                    currentWord = s_WordInfoList[i];
//                    // todo -- make this better and respect sequences like 'the' and 'a' and 'is' which should not be capitalized
//                    charInfos[currentWord.startChar].character = char.ToUpper(charInfos[currentWord.startChar].character);
//                }
//            }
//
//            TextInfo retn = new TextInfo();
//            retn.wordCount = s_WordInfoList.Count;
//            retn.wordInfos = ArrayPool<WordInfo>.CopyFromList(s_WordInfoList);
//            retn.charInfos = charInfos;
//            retn.charCount = bufferLength;
//            return retn;
//        }
//
//        public static TextInfo CreateTextInfo(TextSpan textSpan) {
//            bool collapseSpaces = true; //style.computedStyle.TextCollapseWhiteSpace;
//            bool preserveNewlines = false; //style.computedStyle.TextPreserveNewLines;
//            TextInfo textInfo = ProcessText(textSpan.text, collapseSpaces, preserveNewlines, textSpan.textStyle.textTransform);
//            textInfo.spanCount = 1;
//            textInfo.spanInfos = ArrayPool<SpanInfo>.GetMinSize(1);
//            textInfo.spanInfos[0].wordCount = textInfo.wordCount;
//            textInfo.spanInfos[0].font = textSpan.fontAsset;
//            textInfo.spanInfos[0].textStyle = textSpan.textStyle;
//            ComputeCharacterAndWordSizes(textInfo);
//            return textInfo;
//        }
//        private static TMP_FontAsset GetFontAssetForWeight(SpanInfo spanInfo, int fontWeight) {
//            bool isItalic = (spanInfo.textStyle.fontStyle & FontStyle.Italic) != 0;
//
//            int weightIndex = fontWeight / 100;
//            TMP_FontWeights weights = spanInfo.font.fontWeights[weightIndex];
//            return isItalic ? weights.italicTypeface : weights.regularTypeface;
//        }
//
//
//        public static void ApplyLineAndWordOffsets(TextInfo textInfo) {
//            LineInfo[] lineInfos = textInfo.lineInfos;
//            WordInfo[] wordInfos = textInfo.wordInfos;
//            CharInfo[] charInfos = textInfo.charInfos;
//
//            for (int lineIdx = 0; lineIdx < textInfo.lineCount; lineIdx++) {
//                LineInfo currentLine = lineInfos[lineIdx];
//                float lineOffset = currentLine.position.y;
//                float wordAdvance = currentLine.position.x;
//
//                for (int w = currentLine.wordStart; w < currentLine.wordStart + currentLine.wordCount; w++) {
//                    WordInfo currentWord = wordInfos[w];
//                    currentWord.lineIndex = lineIdx;
//                    currentWord.position = new Vector2(wordAdvance, currentLine.position.y);
//
//                    for (int i = currentWord.startChar; i < currentWord.startChar + currentWord.charCount; i++) {
//                        float x0 = charInfos[i].topLeft.x + wordAdvance;
//                        float x1 = charInfos[i].bottomRight.x + wordAdvance;
//                        float y0 = charInfos[i].topLeft.y + lineOffset;
//                        float y1 = charInfos[i].bottomRight.y + lineOffset;
//                        charInfos[i].wordIndex = w;
//                        charInfos[i].lineIndex = lineIdx;
//                        charInfos[i].layoutTopLeft = new Vector2(x0, y0);
//                        charInfos[i].layoutBottomRight = new Vector2(x1, y1);
//                    }
//
//                    wordInfos[w] = currentWord;
//                    wordAdvance += currentWord.xAdvance;
//                }
//            }
//        }
//
//        public static List<LineInfo> Layout(TextInfo textInfo, float width) {
//            SpanInfo spanInfo = textInfo.spanInfos[0];
//
//            TMP_FontAsset asset = spanInfo.font;
//            float scale = (spanInfo.textStyle.fontSize / asset.fontInfo.PointSize) * asset.fontInfo.Scale;
//            float lh = (asset.fontInfo.Ascender + asset.fontInfo.Descender) * scale;
//
//            float lineOffset = 0;
//
//            LineInfo currentLine = new LineInfo();
//            WordInfo[] wordInfos = textInfo.wordInfos;
//            List<LineInfo> lineInfos = ListPool<LineInfo>.Get();
//
//            currentLine.position = new Vector2(0, lineOffset);
//            width = Mathf.Max(width, 0);
//
//            for (int w = 0; w < textInfo.wordCount; w++) {
//                WordInfo currentWord = wordInfos[w];
//
//                if (currentWord.isNewLine) {
//                    lineInfos.Add(currentLine);
//                    lineOffset += lh;
//                    //(currentLine.maxDescender + textInfo.charInfos[currentWord.startChar + currentWord.VisibleCharCount - 1].ascender + lineGap) * baseScale;
//                    currentLine = new LineInfo();
//                    currentLine.position = new Vector2(0, lineOffset);
//                    currentLine.wordStart = w + 1;
//                    continue;
//                }
//
//                if (currentWord.characterSize > width + 0.01f) {
//                    // we had words in this line already
//                    // finish the line and start a new one
//                    // line offset needs to to be bumped
//                    if (currentLine.wordCount > 0) {
//                        lineInfos.Add(currentLine);
//                        lineOffset += lh;
//                        //-currentLine.maxDescender + textInfo.charInfos[currentWord.startChar + currentWord.VisibleCharCount - 1].ascender + (lineGap) * baseScale;
//                    }
//
//                    currentLine = new LineInfo();
//                    currentLine.position = new Vector2(0, lineOffset);
//                    currentLine.wordStart = w;
//                    currentLine.wordCount = 1;
//                    //  currentLine.maxAscender = currentWord.ascender;
//                    //  currentLine.maxDescender = currentWord.descender;
//                    currentLine.width = currentWord.size.x;
//                    lineInfos.Add(currentLine);
//
//                    lineOffset += lh;
//                    //-currentLine.maxDescender + textInfo.charInfos[currentWord.startChar + currentWord.VisibleCharCount - 1].ascender + (lineGap) * baseScale;
//                    currentLine = new LineInfo();
//                    currentLine.wordStart = w + 1;
//                    currentLine.position = new Vector2(0, lineOffset);
//                }
//
//                else if (currentLine.width + currentWord.size.x > width + 0.01f) {
//                    // characters fit but space does not, strip spaces and start new line w/ next word
//                    if (currentLine.width + currentWord.characterSize < width + 0.01f) {
//                        currentLine.wordCount++;
//
//                        //   if (currentLine.maxAscender < currentWord.ascender) currentLine.maxAscender = currentWord.ascender;
//                        //   if (currentLine.maxDescender > currentWord.descender) currentLine.maxDescender = currentWord.descender;
//                        currentLine.width += currentWord.characterSize;
//                        lineInfos.Add(currentLine);
//
//                        lineOffset += lh;
//                        //-currentLine.maxDescender + textInfo.charInfos[currentWord.startChar + currentWord.VisibleCharCount - 1].ascender + (lineGap) * baseScale;
//
//                        currentLine = new LineInfo();
//                        currentLine.position = new Vector2(0, lineOffset);
//                        currentLine.wordStart = w + 1;
//                        continue;
//                    }
//
//                    lineInfos.Add(currentLine);
//                    lineOffset += lh;
//                    //-currentLine.maxDescender + textInfo.charInfos[currentWord.startChar + currentWord.VisibleCharCount - 1].ascender + (lineGap) * baseScale;
//                    currentLine = new LineInfo();
//                    currentLine.position = new Vector2(0, lineOffset);
//                    currentLine.wordStart = w;
//                    currentLine.wordCount = 1;
//                    currentLine.width = currentWord.size.x;
//                    //   currentLine.maxAscender = currentWord.ascender;
//                    //   currentLine.maxDescender = currentWord.descender;
//                }
//
//                else {
//                    currentLine.wordCount++;
//
//                    //   if (currentLine.maxAscender < currentWord.maxCharTop) currentLine.maxAscender = currentWord.maxCharTop;
//                    //  if (currentLine.maxDescender > currentWord.minCharBottom) currentLine.maxDescender = currentWord.minCharBottom;
//
//                    currentLine.width += currentWord.xAdvance;
//                }
//            }
//
//            if (currentLine.wordCount > 0) {
//                lineInfos.Add(currentLine);
//            }
//
//            return lineInfos;
//        }
//
//        private static void ComputeCharacterAndWordSizes(TextInfo textInfo) {
//            WordInfo[] wordInfos = textInfo.wordInfos;
//            CharInfo[] charInfos = textInfo.charInfos;
//
//            for (int spanIdx = 0; spanIdx < textInfo.spanCount; spanIdx++) {
//                SpanInfo spanInfo = textInfo.spanInfos[spanIdx];
//                TMP_FontAsset fontAsset = spanInfo.font;
//                Material fontAssetMaterial = fontAsset.material;
//
//                bool isUsingAltTypeface = false;
//                float boldAdvanceMultiplier = 1;
//
//                if ((spanInfo.textStyle.fontStyle & FontStyle.Bold) != 0) {
//                    fontAsset = GetFontAssetForWeight(spanInfo, 700);
//                    isUsingAltTypeface = true;
//                    boldAdvanceMultiplier = 1 + fontAsset.boldSpacing * 0.01f;
//                }
//
//                float smallCapsMultiplier = (spanInfo.textStyle.fontStyle & FontStyle.SmallCaps) == 0 ? 1.0f : 0.8f;
//                float fontScale = spanInfo.textStyle.fontSize * smallCapsMultiplier / fontAsset.fontInfo.PointSize * fontAsset.fontInfo.Scale;
//
//                //float yAdvance = fontAsset.fontInfo.Baseline * fontScale * fontAsset.fontInfo.Scale;
//                //float monoAdvance = 0;
//
//                float minWordSize = float.MaxValue;
//                float maxWordSize = float.MinValue;
//
//                float padding = ShaderUtilities.GetPadding(fontAsset.material, enableExtraPadding: false, isBold: false);
//                float stylePadding = 0;
//
//                if (!isUsingAltTypeface && (spanInfo.textStyle.fontStyle & FontStyle.Bold) == FontStyle.Bold) {
//                    if (fontAssetMaterial.HasProperty(ShaderUtilities.ID_GradientScale)) {
//                        float gradientScale = fontAssetMaterial.GetFloat(ShaderUtilities.ID_GradientScale);
//                        stylePadding = fontAsset.boldStyle / 4.0f * gradientScale * fontAssetMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A);
//
//                        // Clamp overall padding to Gradient Scale size.
//                        if (stylePadding + padding > gradientScale) {
//                            padding = gradientScale - stylePadding;
//                        }
//                    }
//
//                    boldAdvanceMultiplier = 1 + fontAsset.boldSpacing * 0.01f;
//                }
//                else if (fontAssetMaterial.HasProperty(ShaderUtilities.ID_GradientScale)) {
//                    float gradientScale = fontAssetMaterial.GetFloat(ShaderUtilities.ID_GradientScale);
//                    stylePadding = fontAsset.normalStyle / 4.0f * gradientScale *
//                                   fontAssetMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A);
//
//                    // Clamp overall padding to Gradient Scale size.
//                    if (stylePadding + padding > gradientScale) {
//                        padding = gradientScale - stylePadding;
//                    }
//                }
//
//                // todo -- handle tab
//                // todo -- handle sprites
//
//                for (int w = spanInfo.startWord; w < spanInfo.startWord + spanInfo.wordCount; w++) {
//                    WordInfo currentWord = wordInfos[w];
//                    float xAdvance = 0;
//                    // new lines are their own words (idea: give them an xAdvance of some huge number so they always get their own lines)
//
//                    for (int i = currentWord.startChar; i < currentWord.startChar + currentWord.charCount; i++) {
//                        int current = charInfos[i].character;
//
//                        TMP_Glyph glyph;
//                        TMP_FontAsset fontForGlyph = TMP_FontUtilities.SearchForGlyph(spanInfo.font, charInfos[i].character, out glyph);
//
//                        KerningPair adjustmentPair;
//                        GlyphValueRecord glyphAdjustments = new GlyphValueRecord();
//
//                        // todo -- if we end up doing character wrapping we probably want to ignore prev x kerning for line start
//                        if (i != textInfo.charCount - 1) {
//                            int next = charInfos[i + 1].character;
//                            fontAsset.kerningDictionary.TryGetValue((next << 16) + current, out adjustmentPair);
//                            if (adjustmentPair != null) {
//                                glyphAdjustments = adjustmentPair.firstGlyphAdjustments;
//                            }
//                        }
//
//                        if (i != 0) {
//                            int prev = charInfos[i - 1].character;
//                            fontAsset.kerningDictionary.TryGetValue((current << 16) + prev, out adjustmentPair);
//                            if (adjustmentPair != null) {
//                                glyphAdjustments += adjustmentPair.secondGlyphAdjustments;
//                            }
//                        }
//
//                        float currentElementScale = fontScale * glyph.scale;
//                        float topShear = 0;
//                        float bottomShear = 0;
//
//                        if (!isUsingAltTypeface && ((spanInfo.textStyle.fontStyle & FontStyle.Italic) != 0)) {
//                            float shearValue = fontAsset.italicStyle * 0.01f;
//                            topShear = glyph.yOffset * shearValue;
//                            bottomShear = (glyph.yOffset - glyph.height) * shearValue;
//                        }
//
//                        Vector2 topLeft;
//                        Vector2 bottomRight;
//
//                        // idea for auto sizing: multiply scale later on and just save base unscaled vertices
////                        topLeft.x = xAdvance + (glyph.xOffset - padding - stylePadding + glyphAdjustments.xPlacement) * currentElementScale;
////                        topLeft.y = yAdvance + (glyph.yOffset + padding + glyphAdjustments.yPlacement) * currentElementScale;
////                        bottomRight.x = topLeft.x + (glyph.width + padding * 2) * currentElementScale;
////                        bottomRight.y = topLeft.y - (glyph.height + padding * 2 + stylePadding * 2) * currentElementScale;
//
//                        topLeft.x = xAdvance + (glyph.xOffset - padding - stylePadding + glyphAdjustments.xPlacement) * currentElementScale;
//                        topLeft.y = ((fontAsset.fontInfo.Ascender) - (glyph.yOffset + padding)) * currentElementScale;
//                        bottomRight.x = topLeft.x + (glyph.width + padding * 2) * currentElementScale;
//                        bottomRight.y = topLeft.y + (glyph.height + padding * 2 + stylePadding * 2) * currentElementScale;
//
//                        if (currentWord.startChar + currentWord.VisibleCharCount >= i) {
//                            if (topLeft.y > currentWord.maxCharTop) {
//                                currentWord.maxCharTop = topLeft.y;
//                            }
//
//                            if (bottomRight.y < currentWord.minCharBottom) {
//                                currentWord.minCharBottom = bottomRight.y;
//                            }
//                        }
//
//                        FaceInfo faceInfo = fontAsset.fontInfo;
//                        Vector2 uv0;
//
//                        uv0.x = (glyph.x - padding - stylePadding) / faceInfo.AtlasWidth;
//                        uv0.y = 1 - (glyph.y + padding + stylePadding + glyph.height) / faceInfo.AtlasHeight;
//
//                        Vector2 uv1;
//                        uv1.x = (glyph.x + padding + stylePadding + glyph.width) / faceInfo.AtlasWidth;
//                        uv1.y = 1 - (glyph.y - padding - stylePadding) / faceInfo.AtlasHeight;
//
//                        charInfos[i].topLeft = topLeft;
//                        charInfos[i].bottomRight = bottomRight;
//                        charInfos[i].shearValues = new Vector2(topShear, bottomShear);
//
//                        charInfos[i].uv0 = uv0;
//                        charInfos[i].uv1 = uv1;
//
//                        charInfos[i].uv2 = new Vector2(currentElementScale, 0); // todo -- compute uv2s
//                        charInfos[i].uv3 = Vector2.one;
//
//                        float elementAscender = fontAsset.fontInfo.Ascender * currentElementScale / smallCapsMultiplier;
//                        float elementDescender = fontAsset.fontInfo.Descender * currentElementScale / smallCapsMultiplier;
//
//                        charInfos[i].ascender = elementAscender;
//                        charInfos[i].descender = elementDescender;
//
//                        currentWord.ascender = elementAscender > currentWord.ascender
//                            ? elementAscender
//                            : currentWord.ascender;
//                        currentWord.descender = elementDescender < currentWord.descender
//                            ? elementDescender
//                            : currentWord.descender;
//
//                        if ((spanInfo.textStyle.fontStyle & (FontStyle.Superscript | FontStyle.Subscript)) != 0) {
//                            float baseAscender = elementAscender / fontAsset.fontInfo.SubSize;
//                            float baseDescender = elementDescender / fontAsset.fontInfo.SubSize;
//
//                            currentWord.ascender = baseAscender > currentWord.ascender
//                                ? baseAscender
//                                : currentWord.ascender;
//                            currentWord.descender = baseDescender < currentWord.descender
//                                ? baseDescender
//                                : currentWord.descender;
//                        }
//
//                        if (i < currentWord.startChar + currentWord.spaceStart) {
//                            currentWord.characterSize = charInfos[i].bottomRight.x;
//                        }
//
//                        xAdvance += (glyph.xAdvance * boldAdvanceMultiplier + fontAsset.normalSpacingOffset +
//                                     glyphAdjustments.xAdvance) * currentElementScale;
//                    }
//
//                    currentWord.xAdvance = xAdvance;
//                    currentWord.size = new Vector2(xAdvance, currentWord.ascender); // was ascender - descender
//                    minWordSize = Mathf.Min(minWordSize, currentWord.size.x);
//                    maxWordSize = Mathf.Max(maxWordSize, currentWord.size.x);
//                    wordInfos[w] = currentWord;
//                }
//            }
//        }
//
//    }
//
//}