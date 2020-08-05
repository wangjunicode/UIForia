using System.Diagnostics;
using UIForia.Rendering;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Text {

    [BurstCompile]
    internal unsafe struct UpdateTextRenderRanges : IJob {

        public DataList<FontAssetInfo>.Shared fontAssetMap;
        public DataList<TextInfo> textInfoMap;
        public DataList<TextId> activeTextElementIds;

        public int rangeSizeLimit;

        public void Execute() {
            if (rangeSizeLimit <= 0) rangeSizeLimit = 150;
            Run(0, activeTextElementIds.size);
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

                faceTextureStack.size = 1;
                outlineTextureStack.size = 1;
                faceTextureStack[0] = 0;
                outlineTextureStack[0] = 0;

                TextId textId = activeTextElementIds[i];
                ref TextInfo textInfo = ref textInfoMap[textId.textInfoId];

                textInfo.renderRangeList.EnsureCapacity(4, Allocator.Persistent);
                // textInfo.renderedCharacters.EnsureCapacity(textInfo.renderingCharacterCount + 1, Allocator.Persistent); // +1 to account for invalid index 0

                textInfo.renderRangeList.size = 0;
                // textInfo.renderedCharacters.size = 1; // 0 is invalid

                ushort fontAssetId = (ushort) textInfo.textStyle.fontAssetId;

                RangeInt range = new RangeInt(-1, 0);

                ushort lineIdx = 0;
                bool active = false;
                // active can be set on any? non character symbol that supports it
                // or with an [enabled] tag
                UIForiaGlyph dummy = default;
                ref UIForiaGlyph glyph = ref dummy;
                FontAssetInfo* fontMap = fontAssetMap.GetArrayPointer();
                
                TextSymbol* prevSymbol = null;
                
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

                            // todo -- handle disabled characters
                            if ((symbol.charInfo.flags & CharacterFlags.Visible) == 0) {
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
                            glyph = ref fontMap[fontAssetId].glyphList[symbol.charInfo.glyphIndex];
                            
                            // textInfo.renderedCharacters.array[textInfo.renderedCharacters.size++] = new RenderedCharacterInfo() {
                            //     position = position,
                            //     // width and height are used just for bounds computation
                            //     width = glyph.width * symbol.charInfo.scale,    
                            //     height = glyph.height * symbol.charInfo.scale,
                            //     lineAscender = symbol.charInfo.maxAscender,
                            //     symbolIdx = c, // might not need symbol idx
                            //     renderedGlyphIndex = glyph.renderBufferIndex,
                            // };

                            range.length++;

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
                }

                for (int j = 0; j < textInfo.renderRangeList.size; j++) {
                    textInfo.renderRangeList.array[j].symbols = textInfo.symbolList.array;//.array;
                }

            }

            faceTextureStack.Dispose();
            outlineTextureStack.Dispose();
        }

        private void EndRenderRange() { }

        private void BeginUnderline() { }

        private void EndUnderline() { }

        private void OnLineEnd() { }

    }

}