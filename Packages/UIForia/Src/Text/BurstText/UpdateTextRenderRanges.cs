using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Graphics;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Text {

    [BurstCompile]
    internal unsafe struct UpdateTextRenderRanges : IJob {

        public DataList<FontAssetInfo>.Shared fontAssetMap;
        public DataList<TextId> activeTextElementInfos;

        public int rangeSizeLimit;
        private float minSelectedPosition;
        private float maxSelectedPosition;

        public void Execute() {
            if (rangeSizeLimit <= 0) rangeSizeLimit = 150;
            Run(0, activeTextElementInfos.size);
        }

        public enum TextDecorationType {

            Underline,
            Overline,
            Strikethrough,
            BackgroundHighlight,
            ForegroundHighlight

        }

        public struct TextDecoration {

            public TextDecorationType decorationType;

        }

        public void Run(int start, int end) {
            DataList<int> faceTextureStack = new DataList<int>(16, Allocator.Temp);
            DataList<int> outlineTextureStack = new DataList<int>(16, Allocator.Temp);

            for (int i = start; i < end; i++) {
                // if !needsDrawUpdate, continue
                // if !rich text, do something simpler, still needs to break lines and handle text decoration per line

                TextId textId = activeTextElementInfos[i];
                ref TextInfo textInfo = ref textId.textInfo[0];
                if (!textInfo.requiresRenderRangeUpdate) {
                    continue;
                }

                textInfo.requiresRenderRangeUpdate = false;
                faceTextureStack.size = 1;
                outlineTextureStack.size = 1;
                faceTextureStack[0] = 0;
                outlineTextureStack[0] = 0;

                textInfo.renderRangeList.EnsureCapacity(4, Allocator.Persistent);
                textInfo.renderRangeList.size = 0;

                ushort fontAssetId = (ushort) textInfo.fontAssetId;

                RangeInt range = new RangeInt(-1, 0);

                ushort lineIdx = 0;
                bool active = false;
                // active can be set on any? non character symbol that supports it
                // or with an [enabled] tag
                FontAssetInfo* fontMap = fontAssetMap.GetArrayPointer();

                TextSymbol* prevSymbol = null;

                RangeInt selectionRange = textInfo.GetSelectionRange();
                int charIdx = 0;
                bool hasSelection = selectionRange.length > 0;
                minSelectedPosition = -1;
                maxSelectedPosition = -1;

                int lineStartCharIdx = -1;
                
                for (int c = 0; c < textInfo.symbolList.size; c++) {
                    ref TextSymbol symbol = ref textInfo.symbolList.array[c];

                    switch (symbol.type) {
                        case TextSymbolType.TexturePush:
                            // faceTextureStack.Add(symbol.textureId);
                            break;

                        case TextSymbolType.TexturePop:
                            faceTextureStack.size--;
                            break;

                        case TextSymbolType.Character: {
                            if (symbol.charInfo.lineIndex != lineIdx) {
                                lineStartCharIdx = charIdx;
                                
                                if (hasSelection && minSelectedPosition != -1) {
                                    maxSelectedPosition = textInfo.lineInfoList[lineIdx].x + textInfo.lineInfoList[lineIdx].width;
                                    SubmitSelectionHighlight(ref textInfo, lineIdx);
                                }

                                lineIdx = symbol.charInfo.lineIndex;

                                if (range.length > 0) {
                                    textInfo.renderRangeList.Add(new TextRenderRange() {
                                        characterRange = range,
                                        type = TextRenderType.Characters,
                                        fontAssetId = fontAssetId,
                                    });
                                }

                                range.start = -1;
                                range.length = 0;
                                prevSymbol = null;
                                // foreach active decoration
                                // decoration.EndLine();
                            }

                            if (hasSelection) {
                                if (charIdx >= selectionRange.start && charIdx < selectionRange.end) {
                                    if (charIdx == selectionRange.start || charIdx == lineStartCharIdx) {
                                        minSelectedPosition = GetCharacterLeft(symbol, textInfo);
                                    }

                                    if (charIdx == selectionRange.end - 1) {
                                        maxSelectedPosition = GetCharacterRight(fontMap, symbol, textInfo, fontAssetId);
                                        SubmitSelectionHighlight(ref textInfo, lineIdx);
                                    }
                                }
                            }

                            // todo -- handle disabled characters
                            if ((symbol.charInfo.flags & CharacterFlags.Visible) == 0) {
                                charIdx++;
                                continue;
                            }

                            if (symbol.charInfo.fontAssetId != fontAssetId) {
                                // todo -- handle fallback font ids
                            }

                            if (range.length >= rangeSizeLimit) {
                                prevSymbol = null;
                                textInfo.renderRangeList.Add(new TextRenderRange() {
                                    characterRange = range,
                                    type = TextRenderType.Characters, // not totally sure this is right, but leave it for now
                                    fontAssetId = fontAssetId,
                                    texture0 = default,
                                    texture1 = default,
                                });
                                range.start = -1; //textInfo.renderedCharacters.size;
                                range.length = 0;
                            }

                            if (range.start == -1) {
                                range.start = c;
                            }
                            else {
                                prevSymbol->charInfo.nextRenderIdx = c;
                            }

                            // need to know where to get this char data later on for effects to work on
                            // maybe computable, but id like charInfo to be 28 bytes so right now this isnt a waste
                            symbol.charInfo.nextRenderIdx = -1;

                            prevSymbol = textInfo.symbolList.array + c;
                            // could do this in a post step for better locality 

                            range.length++;
                            charIdx++;
                            break;
                        }

                        case TextSymbolType.Sprite: {
                            if (range.length > 0) {
                                // sprite should take an element style i think it would be cool if this would literally be an element
                                // regardless, should accept a full material I think
                                // probably wont try to batch with standard element rendering? guess theres no reason not to if it just takes a style
                                textInfo.renderRangeList.Add(new TextRenderRange() {
                                    type = TextRenderType.Characters,
                                    characterRange = range,
                                    fontAssetId = fontAssetId,
                                    texture0 = default,
                                    texture1 = default
                                });

                                range.start = -1; // textInfo.renderedCharacters.size;
                                range.length = 0;
                            }

                            // text render range maybe not the best name
                            textInfo.renderRangeList.Add(new TextRenderRange() {
                                type = TextRenderType.Sprite
                            });

                            break;
                        }

                        case TextSymbolType.TextTransformPush: {
                            break; // todo -- handle breaking underline | strikethrough
                        }

                        case TextSymbolType.TextTransformPop: {
                            break; // todo -- handle breaking underline | strikethrough
                        }

                        case TextSymbolType.UnderlinePush: {
                            // I dont have material data right now
                            // material buffer will need to go through decoration calls and set material index I think
                            // decorationList.Add(new Underline());

                            // underline takes whatever material settings were enabled at its push or line start

                            // todo -- overline too
                            // todo -- underline target = end of line, fixed width, fixed char count, visible char count
                            // width & direction would be cool
                            // that probably isnt a push then, probably a new symbol type 

                            // should handle its own coloring somehow
                            // also need to know where the line ends, not the last rendered character but the actual end of line including whitespace and sprites
                            // if color set it could use its own material? or simply encode color in a value not used?
                            // material makes the most sense but id have to know how to pack it in i guess
                            // probably nothing wrong with PushMaterial PushUnderline PopMaterial ... PopUnderline. just keep an underline material stack so i know what to use
                            // insert characters with type Underline start/mid/end next time we break render range 
                            break;
                        }

                        case TextSymbolType.UnderlinePop: {
                            // find last decoration with type = underline. End it. remove from list. enqueue it to be added after next render break
                            break;
                        }

                        case TextSymbolType.FontPush: {
                            // just treat font pushes as new renders for now, eventually diff this against 
                            // FontAssetInfo fontAssetInfo = fontAssetMap[symbol.fontId];

                            if (range.length > 0) {
                                textInfo.renderRangeList.Add(new TextRenderRange() {
                                    type = TextRenderType.Characters,
                                    characterRange = range,
                                    fontAssetId = fontAssetId,
                                    texture0 = default,
                                    texture1 = default
                                });
                            }

                            range.start = -1; //  textInfo.renderedCharacters.size;
                            range.length = 0;
                            break;
                        }

                        case TextSymbolType.FontPop: {
                            if (range.length > 0) {
                                textInfo.renderRangeList.Add(new TextRenderRange());
                            }

                            range.start = -1; //textInfo.renderedCharacters.size;
                            range.length = 0;
                            break;
                        }
                    }

                    // font changing is a new draw 
                    // line changing is a new draw
                    // textures changing is a new draw (not yet supported outside spans / text infos)
                }

                // OnLineEnd();
                if (range.length > 0) {
                    textInfo.renderRangeList.Add(new TextRenderRange() {
                        type = TextRenderType.Characters,
                        characterRange = range,
                        fontAssetId = fontAssetId,
                        texture0 = default,
                        texture1 = default
                    });
                    SubmitSelectionHighlight(ref textInfo, lineIdx);
                }

                for (int j = 0; j < textInfo.renderRangeList.size; j++) {
                    ref TextRenderRange render = ref textInfo.renderRangeList.array[j];
                    render.idx = (ushort) j;
                    if (render.type == TextRenderType.Characters) {
                        render.symbols = textInfo.symbolList.array;
                    }
                }

                NativeSortExtension.Sort(textInfo.renderRangeList.array, textInfo.renderRangeList.size, new Cmp());
            }

            faceTextureStack.Dispose();
            outlineTextureStack.Dispose();
        }

        private struct Cmp : IComparer<TextRenderRange> {

            public int Compare(TextRenderRange x, TextRenderRange y) {
                // note -- cast is needed or we subtract ushorts which might overflow incorrectly
                // ReSharper disable once RedundantCast
                if (x.type == y.type) return (int) x.idx - (int) y.idx;
                return (int) x.type - (int) y.type;
            }

        }

        private void SubmitSelectionHighlight(ref TextInfo textInfo, ushort lineIdx) {
            if (minSelectedPosition != -1 && maxSelectedPosition != -1) {
                textInfo.renderRangeList.Add(new TextRenderRange() {
                    type = TextRenderType.Highlight,
                    localBounds = new AxisAlignedBounds2D() {
                        xMin = minSelectedPosition - 1,
                        xMax = maxSelectedPosition + 1,
                        yMin = textInfo.lineInfoList.array[lineIdx].y - 1,
                        yMax = textInfo.lineInfoList.array[lineIdx].y + textInfo.lineInfoList.array[lineIdx].height
                    }
                });
                minSelectedPosition = -1;
                maxSelectedPosition = -1;
            }
        }

        private float GetCharacterLeft(in TextSymbol symbol, in TextInfo textInfo) {
            return textInfo.layoutSymbolList.array[symbol.charInfo.wordIndex].wordInfo.x + symbol.charInfo.position.x;
        }

        private float GetCharacterRight(FontAssetInfo* fontMap, in TextSymbol symbol, in TextInfo textInfo, int fontAssetId) {
            ref UIForiaGlyph glyph = ref fontMap[fontAssetId].glyphList[symbol.charInfo.glyphIndex];
            return textInfo.layoutSymbolList.array[symbol.charInfo.wordIndex].wordInfo.x + symbol.charInfo.position.x + (glyph.width * symbol.charInfo.scale);
        }

        private void EndRenderRange() { }

        private void BeginUnderline() { }

        private void EndUnderline() { }

        private void OnLineEnd() { }

    }

}