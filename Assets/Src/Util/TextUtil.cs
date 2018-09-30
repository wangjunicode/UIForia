using System;

namespace Src.Util {

    public static class TextUtil {

        [Flags]
        public enum FontStyle {

            Unset = 0,
            Normal = 1 << 1,
            Bold = 1 << 2,
            Italic = 1 << 3,
            Underline = 1 << 4,
            LowerCase = 1 << 5,
            UpperCase = 1 << 6,  
            SmallCaps = 1 << 7,  
            Strikethrough = 1 << 8,  
            Superscript = 1 << 9,  
            Subscript = 1 << 10,  
            Highlight = 1 << 11,  

        }

        public enum TextAnchor {

            Unset = 0,
            UpperLeft = 1,
            UpperCenter = 2,
            UpperRight = 3,
            MiddleLeft = 4,
            MiddleCenter = 5,
            MiddleRight = 6,
            LowerLeft = 7,
            LowerCenter = 8,
            LowerRight = 9

        }

        public static int StringToCharArray(string sourceText, ref int[] charBuffer, bool parseControlCharacters = false) {
            if (sourceText == null) {
                charBuffer = charBuffer ?? new int[0];
                return 0;
            }

            if (charBuffer.Length < sourceText.Length) {
                Array.Resize(ref charBuffer, sourceText.Length);
            }

            if (!parseControlCharacters) {
                for (int i = 0; i < sourceText.Length; i++) {
                    char current = sourceText[i];
                    charBuffer[i] = current;
                }

                return sourceText.Length;
            }

            int writeIndex = 0;

            for (int i = 0; i < sourceText.Length; i++) {
                char current = sourceText[i];

                if (current == 92 && sourceText.Length > i + 1) { // ascii '\'
                    switch ((int) sourceText[i + 1]) {
                        case 85: // \U00000000 for UTF-32 Unicode
                            if (sourceText.Length > i + 9) {
                                charBuffer[writeIndex] = GetUTF32(sourceText, i + 2);
                                i += 9;
                                writeIndex += 1;
                                continue;
                            }

                            break;
                        case 92: // \ escape

                            if (sourceText.Length <= i + 2) break;

                            charBuffer[writeIndex] = sourceText[i + 1];
                            charBuffer[writeIndex + 1] = sourceText[i + 2];
                            i += 2;
                            writeIndex += 2;
                            continue;
                        case 110: // \n LineFeed

                            charBuffer[writeIndex] = (char) 10;
                            i += 1;
                            writeIndex += 1;
                            continue;
                        case 114: // \r

                            charBuffer[writeIndex] = (char) 13;
                            i += 1;
                            writeIndex += 1;
                            continue;
                        case 116: // \t Tab

                            charBuffer[writeIndex] = (char) 9;
                            i += 1;
                            writeIndex += 1;
                            continue;

                        case 117: // \u0000 for UTF-16 Unicode
                            if (sourceText.Length > i + 5) {
                                charBuffer[writeIndex] = (char) GetUTF16(sourceText, i + 2);
                                i += 5;
                                writeIndex += 1;
                                continue;
                            }

                            break;
                    }
                }
//                handle UTF32
//                if (char.IsHighSurrogate(sourceText[i]) && char.IsLowSurrogate(sourceText[i + 1])) {
//                    charBuffer[writeIndex] = char.ConvertToUtf32(sourceText[i], sourceText[i + 1]);
//                    i += 1;
//                    writeIndex += 1;
//                    continue;
//                }

                // todo -- maybe handle <br/> here

                charBuffer[writeIndex] = sourceText[i];
                writeIndex += 1;
            }

            charBuffer[writeIndex] = (char) 0;
            return writeIndex + 1;
        }

        public static int HexToInt(char hex) {
            switch (hex) {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case 'A': return 10;
                case 'B': return 11;
                case 'C': return 12;
                case 'D': return 13;
                case 'E': return 14;
                case 'F': return 15;
                case 'a': return 10;
                case 'b': return 11;
                case 'c': return 12;
                case 'd': return 13;
                case 'e': return 14;
                case 'f': return 15;
            }

            return 15;
        }

        public static int GetUTF16(string text, int i) {
            int unicode = 0;
            unicode += HexToInt(text[i]) << 12;
            unicode += HexToInt(text[i + 1]) << 8;
            unicode += HexToInt(text[i + 2]) << 4;
            unicode += HexToInt(text[i + 3]);
            return unicode;
        }

        public static int GetUTF32(string text, int i) {
            int unicode = 0;
            unicode += HexToInt(text[i]) << 30;
            unicode += HexToInt(text[i + 1]) << 24;
            unicode += HexToInt(text[i + 2]) << 20;
            unicode += HexToInt(text[i + 3]) << 16;
            unicode += HexToInt(text[i + 4]) << 12;
            unicode += HexToInt(text[i + 5]) << 8;
            unicode += HexToInt(text[i + 6]) << 4;
            unicode += HexToInt(text[i + 7]);
            return unicode;
        }

    }

}