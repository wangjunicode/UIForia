using System;
using UIForia.Prototype;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEngine;

namespace UIForia.Text {

    internal unsafe struct TextSplitter {

        public struct State : IDisposable {

            public DataList<ushort> fontStack;
            public DataList<float> fontSizeStack;

            public DataList<WhitespaceMode> whitespaceModeStack;
            public DataList<VerticalAlignment> verticalAlignStack;

            public static State Create(Allocator allocator) {
                State retn = new State {
                    verticalAlignStack = new DataList<VerticalAlignment>(8, allocator),
                    fontStack = new DataList<ushort>(8, allocator),
                    fontSizeStack = new DataList<float>(8, allocator),
                    whitespaceModeStack = new DataList<WhitespaceMode>(8, allocator)
                };
                return retn;
            }

            public void Dispose() {
                fontStack.Dispose();
                fontSizeStack.Dispose();
                whitespaceModeStack.Dispose();
                verticalAlignStack.Dispose();
            }

            public void Reset(TextLayoutInfo textLayoutInfo) {
                fontStack.size = 0;
                fontSizeStack.size = 0;
                whitespaceModeStack.size = 0;
                verticalAlignStack.size = 0;
                verticalAlignStack.Add(textLayoutInfo.verticalAlignment);
                whitespaceModeStack.Add(textLayoutInfo.whitespaceMode);
                fontStack.Add((ushort) textLayoutInfo.fontAssetId.id);
                fontSizeStack.Add(textLayoutInfo.fontSize);
            }

        }

        public static string[] MakeStrings(DataList<CharacterRunDeprecate>.Shared runs, DataList<char> buffer) {
            string[] retn = new string[runs.size];

            for (int i = 0; i < runs.size; i++) {
                CharacterRunDeprecate run = runs[i];
                retn[i] = new string(buffer.GetPointer(run.start), 0, run.length);
            }

            return retn;
        }

        public static string[] MakeStrings(DataList<CharacterRunDeprecate> runs, DataList<char> buffer) {
            string[] retn = new string[runs.size];

            for (int i = 0; i < runs.size; i++) {
                CharacterRunDeprecate run = runs[i];
                retn[i] = new string(buffer.GetPointer(run.start), 0, run.length);
            }

            return retn;
        }

        private static CharacterRunFlags ClassifyCharacter(char c) {
            if (c == '\n') return CharacterRunFlags.NewLine;
            return IsWhiteSpace(c) ? CharacterRunFlags.Whitespace : CharacterRunFlags.Characters;
        }

        public static bool IsWhiteSpace(char c) {
            return c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085');
        }

        public static void Split(ref DataList<char> contentBuffer, CheckedArray<TextSymbol> symbolBuffer, CheckedArray<char> dataBuffer, in TextLayoutInfo layoutInfo, ref DataList<TextCharacterRun> runList, ref DataList<int> contentLengthBuffer, ref State state, DataList<SDFFontUnmanaged> fontTable) {

            RangeInt retn = new RangeInt(runList.size, 0);

            state.Reset(layoutInfo);

            ushort fontId = (ushort) layoutInfo.fontAssetId.id;
            if (fontId == 0) {
                fontId = 1;
            }

            float fontSize = layoutInfo.fontSize;

            WhitespaceMode whitespaceMode = layoutInfo.whitespaceMode;
            VerticalAlignment verticalAlignment = layoutInfo.verticalAlignment;

            bool breakGroups = true; // init to true so we dont' try to combine different texts together that shouldn't be combined 

            int nextSymbolPosition = 0;

            ref SDFFontUnmanaged font = ref fontTable.Get(fontId);

            float lineHeight = fontSize - (font.descent * fontSize);

            for (int symbolIdx = 0; symbolIdx < symbolBuffer.size; symbolIdx++) {

                int currentSymbolOffset = nextSymbolPosition;
                ref TextSymbol symbol = ref symbolBuffer.Get(symbolIdx);
                nextSymbolPosition += symbol.dataLength;

                switch (symbol.symbolType) {

                    case SymbolType.PushVerticalAlignment: {
                        breakGroups = true;
                        verticalAlignment = *(VerticalAlignment*) dataBuffer.GetPointer(currentSymbolOffset);
                        state.verticalAlignStack.Add(verticalAlignment);
                        break;
                    }

                    case SymbolType.PopVerticalAlignment: {
                        breakGroups = true;
                        if (state.verticalAlignStack.size == 1) {
                            break;
                        }

                        state.verticalAlignStack.Pop();
                        verticalAlignment = state.verticalAlignStack.GetLast();
                        break;
                    }

                    case SymbolType.PushWhitespaceMode: {
                        whitespaceMode = (WhitespaceMode) (*(ushort*) dataBuffer.GetPointer(currentSymbolOffset));
                        state.whitespaceModeStack.Add(whitespaceMode);
                        break;
                    }

                    case SymbolType.PopWhitespaceMode: {
                        if (state.whitespaceModeStack.size == 1) {
                            break;
                        }

                        state.whitespaceModeStack.Pop();
                        whitespaceMode = state.whitespaceModeStack.GetLast();
                        break;
                    }

                    case SymbolType.PushHitRegion: {
                        symbol.metaInfo = breakGroups ? -1 : contentLengthBuffer.GetLast();
                        break;
                    }

                    case SymbolType.PopHitRegion: {
                        symbol.metaInfo = breakGroups ? -1 : contentLengthBuffer.GetLast();
                        break;
                    }

                    case SymbolType.PushColor: {
                        symbol.metaInfo = breakGroups ? -1 : contentLengthBuffer.GetLast();
                        break;
                    }

                    case SymbolType.PopColor: {
                        symbol.metaInfo = breakGroups ? -1 : contentLengthBuffer.GetLast();
                        break;
                    }

                    case SymbolType.HorizontalSpace: {
                        breakGroups = true;

                        RichText.TextSpacer spaceSize = *(RichText.TextSpacer*) dataBuffer.GetPointer(currentSymbolOffset);

                        contentLengthBuffer.Add(0);

                        runList.Add(new TextCharacterRun() {
                            verticalAlignment = verticalAlignment,
                            width = spaceSize.width,
                            height = spaceSize.height,
                            StretchParts = (ushort) spaceSize.stretchParts,
                            fontId = fontId,
                            fontSize = fontSize,
                            directionByte = HarfbuzzDirectionByte.LeftToRight,
                            flags = CharacterRunFlags.HorizontalSpace,
                            length = 0
                        });

                        break;
                    }

                    // is this the same as horizontal space? 
                    case SymbolType.InlineSpace: {

                        breakGroups = true;

                        RichText.TextSpacer spaceSize = *(RichText.TextSpacer*) dataBuffer.GetPointer(currentSymbolOffset);

                        contentLengthBuffer.Add(0);

                        runList.Add(new TextCharacterRun() {
                            verticalAlignment = verticalAlignment,
                            width = spaceSize.width,
                            height = spaceSize.height,
                            StretchParts = (ushort) spaceSize.stretchParts,
                            fontId = fontId,
                            fontSize = fontSize,
                            directionByte = HarfbuzzDirectionByte.LeftToRight,
                            flags = CharacterRunFlags.InlineSpace,
                            length = 0
                        });
                        break;
                    }

                    case SymbolType.PushFontSize: {
                        float size = *(float*) dataBuffer.GetPointer(currentSymbolOffset);
                        if (size == fontSize) {
                            continue;
                        }

                        state.fontSizeStack.Add(size);
                        fontSize = size;
                        breakGroups = true;
                        lineHeight = fontSize - (font.descent * fontSize);
                        break;
                    }

                    case SymbolType.PopFontSize: {
                        if (state.fontSizeStack.size == 1) {
                            break;
                        }

                        state.fontSizeStack.Pop();
                        fontSize = state.fontSizeStack.GetLast();
                        breakGroups = true;
                        lineHeight = fontSize - (font.descent * fontSize);
                        break;
                    }

                    case SymbolType.PushFont: {
                        ushort nextFontId = (ushort) dataBuffer[currentSymbolOffset];
                        if (nextFontId == 0) {
                            break;
                        }

                        if (nextFontId != fontId) {
                            state.fontStack.Add(nextFontId);
                            fontId = nextFontId;
                            font = ref fontTable.Get(fontId);
                            lineHeight = fontSize - (font.descent * fontSize);
                        }

                        breakGroups = true;
                        break;
                    }

                    case SymbolType.PopFont: {
                        if (state.fontStack.size == 1) {
                            break;
                        }

                        state.fontStack.Pop();
                        fontId = state.fontStack.GetLast();
                        font = ref fontTable.Get(fontId);
                        lineHeight = fontSize - (font.descent * fontSize);
                        breakGroups = true;
                        break;
                    }
                    
                    case SymbolType.PushCursor:
                        symbol.metaInfo = breakGroups ? -1 : contentLengthBuffer.GetLast();
                        break;
                    
                    case SymbolType.PopCursor:
                        symbol.metaInfo = breakGroups ? -1 : contentLengthBuffer.GetLast();
                        break;

                    case SymbolType.CharacterGroup: {

                        if (symbol.dataLength <= 0) {
                            symbol.metaInfo = TextSymbol.k_EmptyCharacterGroup;
                            continue;
                        }

                        char c = dataBuffer[currentSymbolOffset];

                        int loopStart = currentSymbolOffset;
                        int loopEnd = currentSymbolOffset + symbol.dataLength;

                        // if not whitespace & not a forced break & last char group is not whitespace, try to combine prev char group with this one instead of making a new one
                        if (!breakGroups && runList.size > 0 && ClassifyCharacter(c) == CharacterRunFlags.Characters && runList.GetLast().IsCharacterRun) {

                            while (loopStart < loopEnd && ClassifyCharacter(dataBuffer[loopStart]) == CharacterRunFlags.Characters) {
                                loopStart++;
                            }

                            int itemCount = loopStart - currentSymbolOffset;
                            contentLengthBuffer.GetLast() += itemCount;
                            contentBuffer.AddRange(dataBuffer.GetPointer(currentSymbolOffset), itemCount);
                            ref TextCharacterRun textCharacterRun = ref runList.GetLast();
                            textCharacterRun.length += (ushort) itemCount;
                        }

                        breakGroups = false;

                        // empty group, likely because we consumed them all by merging with the previous run 
                        if (loopStart == loopEnd) {
                            symbol.metaInfo = TextSymbol.k_EmptyCharacterGroup;
                            break;
                        }

                        for (int index = loopStart; index < loopEnd; index++) {

                            c = dataBuffer[index];

                            int start = index;

                            CharacterRunFlags flags = ClassifyCharacter(c);

                            index++;

                            while (index < loopEnd && ClassifyCharacter(dataBuffer[index]) == flags) {
                                index++;
                            }

                            int lastIndex = index;
                            index--; // account for while loop over iterating by 1

                            switch (flags) {

                                case CharacterRunFlags.Whitespace: {

                                    if ((whitespaceMode & WhitespaceMode.CollapseWhitespace) != 0) {

                                        if (runList.size != 0 && runList.GetLast().IsWhiteSpace) {
                                            break; // noop                                    
                                        }

                                        contentLengthBuffer.Add(1);

                                        runList.Add(new TextCharacterRun() {
                                            verticalAlignment = verticalAlignment,
                                            fontId = fontId,
                                            fontSize = fontSize,
                                            flags = CharacterRunFlags.Whitespace,
                                            directionByte = HarfbuzzDirectionByte.LeftToRight,
                                            length = 1,
                                            height = lineHeight
                                        });

                                        contentBuffer.Add(' ');

                                    }
                                    else {

                                        contentLengthBuffer.Add(lastIndex - start);

                                        runList.Add(new TextCharacterRun() {
                                            verticalAlignment = verticalAlignment,
                                            fontId = fontId,
                                            fontSize = fontSize,
                                            flags = CharacterRunFlags.Whitespace,
                                            directionByte = HarfbuzzDirectionByte.LeftToRight,
                                            length = (ushort) (lastIndex - start),
                                            height = lineHeight
                                        });

                                        contentBuffer.AddRange(dataBuffer.GetPointer(start), lastIndex - start);
                                    }

                                    break;
                                }

                                case CharacterRunFlags.NewLine: {
                                    if ((whitespaceMode & WhitespaceMode.PreserveNewLines) == 0) {

                                        if (runList.size != 0 && runList.GetLast().IsWhiteSpace) {
                                            // do nothing, we already had a space and dont need to add a new one
                                            break;
                                        }

                                        contentLengthBuffer.Add(1);

                                        // add a single whitespace 
                                        runList.Add(new TextCharacterRun() {
                                            verticalAlignment = verticalAlignment,
                                            fontId = fontId,
                                            fontSize = fontSize,
                                            flags = CharacterRunFlags.Whitespace,
                                            directionByte = HarfbuzzDirectionByte.LeftToRight,
                                            length = 1,
                                            height = lineHeight
                                        });

                                        contentBuffer.Add(' ');

                                    }
                                    else {

                                        ushort newLineCount = (ushort) (((whitespaceMode & WhitespaceMode.CollapseMultipleNewLines) == 0) ? lastIndex - start : 1); // number of new lines to add

                                        for (int newLineIndex = 0; newLineIndex < newLineCount; newLineIndex++) {
                                            contentLengthBuffer.Add(0); // unused but needed for offsets

                                            runList.Add(new TextCharacterRun() {
                                                verticalAlignment = verticalAlignment,
                                                fontId = fontId,
                                                fontSize = fontSize,
                                                flags = CharacterRunFlags.NewLine,
                                                directionByte = HarfbuzzDirectionByte.LeftToRight,
                                                length = 1,
                                                height = lineHeight
                                            });
                                        }

                                    }

                                    break;
                                }

                                case CharacterRunFlags.Characters: {

                                    contentLengthBuffer.Add(lastIndex - start);

                                    runList.Add(new TextCharacterRun() {
                                        verticalAlignment = verticalAlignment,
                                        fontId = fontId,
                                        fontSize = fontSize,
                                        flags = CharacterRunFlags.Characters,
                                        directionByte = HarfbuzzDirectionByte.LeftToRight,
                                        length = (ushort) (lastIndex - start),
                                        height = lineHeight
                                    });

                                    contentBuffer.AddRange(dataBuffer.GetPointer(start), lastIndex - start);
                                    break;
                                }

                            }

                        }

                        breakGroups = (runList.GetLast().flags & CharacterRunFlags.Characters) == 0;
                        runList.GetLast().flags |= CharacterRunFlags.LastInGroup;

                        break;
                    }
                }

            }

            retn.length = runList.size - retn.start;

            // not sure if this lives here or not 
            for (int i = retn.start; i < runList.size; i++) {
                runList[i].shapeResultIndex = -1;
            }

            // if ((CharacterRun2.flags & (CharacterRun2Flags.ReserveSpace | CharacterRun2Flags.HorizontalSpace)) != 0) {
            //     
            //     RichText.TextSpacer spaceSize = *(RichText.TextSpacer*) dataBuffer.GetPointer(CharacterRun2.start);
            //     
            //     resultList[i] = new TextLayoutResult() {
            //         width = spaceSize.width,
            //         height = spaceSize.height,
            //         StretchParts = (ushort)spaceSize.stretchParts,
            //         shapeResultIndex = -1,
            //     };
            //     continue;
            // }
            //
            // if ((CharacterRun2.flags & CharacterRun2Flags.NewLine) != 0) {
            //     resultList[i] = new TextLayoutResult() {
            //         shapeResultIndex = -1,
            //         width = 0,
            //     };
            //     continue;
            // }

        }

    }

}