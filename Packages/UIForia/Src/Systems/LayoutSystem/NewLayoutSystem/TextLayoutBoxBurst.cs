using UIForia.Elements;
using UIForia.Graphics;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UIForia.Layout {

    internal unsafe struct TextLayoutBoxBurst : ILayoutBox {

        public ElementId elementId;
        public TextInfo* textInfo;

        // todo -- probably want to use the same proxy layoutbox setup as scroll/image
        // we'll have to figure out how content width or height gets computed since 
        // content size might have two meanings, one for children one for the text itself
        // might be auto / content like it is for image
        
        public void RunHorizontal(BurstLayoutRunner* runner) {

            ref List_TextLineInfo buffer = ref UnsafeUtility.AsRef<List_TextLineInfo>(runner->lineInfoBuffer);
            ref LayoutInfo layoutInfo = ref runner->GetHorizontalLayoutInfo(elementId);

            float width = layoutInfo.finalSize - layoutInfo.paddingBorderStart - layoutInfo.paddingBorderStart;
            textInfo->requiresRenderRangeUpdate = true;
            if (textInfo->isRichText) {
                TextInfo.RunLayoutHorizontal_RichText(ref textInfo[0], ref buffer, math.max(0, width));
            }
            else {
                TextInfo.RunLayoutHorizontal_WordsOnly(ref textInfo[0], ref buffer, math.max(0, width));
            }

            textInfo->lineInfoList.CopyFrom(buffer.array, buffer.size, Allocator.Persistent);

            // TextInfo.ApplyTextAlignment(ref textInfo, );
            TextAlignment alignment = textInfo->alignment;

            for (int i = 0; i < buffer.size; i++) {
                TextLineInfo lineInfo = buffer[i];
                float position = 0;
                switch (alignment) {

                    default:
                    case TextAlignment.Unset:
                    case TextAlignment.Left:
                        break;

                    case TextAlignment.Right:
                        position = width - lineInfo.width;
                        break;

                    case TextAlignment.Center:
                        position = (width - lineInfo.width) * 0.5f;
                        break;
                }

                for (int w = lineInfo.wordStart; w < lineInfo.wordStart + lineInfo.wordCount; w++) {
                    ref TextLayoutSymbol layoutSymbol = ref textInfo->layoutSymbolList.array[w];
                    TextLayoutSymbolType type = layoutSymbol.type;

                    if (type == TextLayoutSymbolType.Word) {
                        layoutSymbol.wordInfo.x = position;
                        position += layoutSymbol.wordInfo.width;
                    }
                    else if (type == TextLayoutSymbolType.HorizontalSpace) {
                        position += layoutSymbol.width;
                    }
                }
            }

        }

        public void RunVertical(BurstLayoutRunner* runner) {
            int fontAssetId = textInfo->fontAssetId;
            ref FontAssetInfo fontAsset = ref runner->GetFontAsset(fontAssetId);
            float fontSize = runner->GetResolvedFontSize(elementId);
            textInfo->requiresRenderRangeUpdate = true;

            TextInfo.RunLayoutVertical_WordsOnly(fontAsset, fontSize, ref textInfo[0]);

            for (int i = 0; i < textInfo->lineInfoList.size; i++) {
                TextLineInfo lineInfo = textInfo->lineInfoList.array[i];
                float maxAscender = 0;
                for (int w = lineInfo.wordStart; w < lineInfo.wordStart + lineInfo.wordCount; w++) {
                    ref TextLayoutSymbol layoutSymbol = ref textInfo->layoutSymbolList.array[w];
                    TextLayoutSymbolType type = layoutSymbol.type;
                    if (type == TextLayoutSymbolType.Word) {
                        maxAscender = maxAscender > layoutSymbol.wordInfo.maxAscender ? maxAscender : layoutSymbol.wordInfo.maxAscender;
                    }
                }

                for (int w = lineInfo.wordStart; w < lineInfo.wordStart + lineInfo.wordCount; w++) {
                    ref TextLayoutSymbol layoutSymbol = ref textInfo->layoutSymbolList.array[w];
                    TextLayoutSymbolType type = layoutSymbol.type;
                    if (type == TextLayoutSymbolType.Word) {
                        for (int c = layoutSymbol.wordInfo.charStart; c < layoutSymbol.wordInfo.charEnd; c++) {
                            ref TextSymbol symbol = ref textInfo->symbolList.array[c];
                            if (symbol.type == TextSymbolType.Character) {
                                // todo -- need to offset the render y position by diff of line ascenders to handle multiple fonts or multiple sizes on a line 
                                // symbol.charInfo.renderPosition.y += (maxAscender - );
                                symbol.charInfo.renderPosition.x = layoutSymbol.wordInfo.x + symbol.charInfo.position.x;
                                symbol.charInfo.renderPosition.y = layoutSymbol.wordInfo.y + symbol.charInfo.position.y;
                            }
                        }
                    }
                }
            }
            
        }

        public float ComputeContentWidth(ref BurstLayoutRunner runner, in BlockSize blockSize) {
            ref List_TextLineInfo buffer = ref UnsafeUtility.AsRef<List_TextLineInfo>(runner.lineInfoBuffer);

            //TextInfo.RunLayoutHorizontal_WordsOnly(ref textInfo, ref buffer, blockSize.insetSize);
            TextInfo.RunLayoutHorizontal_RichText(ref textInfo[0], ref buffer, blockSize.insetSize);

            float max = 0;

            for (int i = 0; i < buffer.size; i++) {
                max = math.max(buffer.array[i].width, max);
            }

            // did the work, might as well copy result into the line list. It might change next time we compute but if it doesnt we can use the result
            // how do i know if I can use the result though? maybe i computed that result last frame?

            return max;
        }

        public float ComputeContentHeight(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {

            int fontAssetId = textInfo->fontAssetId;
            ref FontAssetInfo fontAsset = ref layoutRunner.GetFontAsset(fontAssetId);
            float fontSize = layoutRunner.GetResolvedFontSize(elementId);

            TextInfo.RunLayoutVertical_WordsOnly(fontAsset, fontSize, ref textInfo[0]);

            ref List_TextLineInfo lineInfoList = ref textInfo->lineInfoList;

            if (lineInfoList.size == 0) {
                return 0;
            }

            return lineInfoList.GetLast().y + lineInfoList.GetLast().height;
        }

        public void OnStylePropertiesChanged(LayoutSystem layoutSystem, UIElement element, StyleProperty[] properties, int propertyCount) { }

        public void OnChildStyleChanged(LayoutSystem layoutSystem, ElementId childId, StyleProperty[] properties, int propertyCount) { }

        public float GetActualContentWidth(ref BurstLayoutRunner runner) {
            return 0;
        }

        public float GetActualContentHeight(ref BurstLayoutRunner runner) {
            return 0;
        }

        public void Dispose() { }

        public void OnInitialize(LayoutSystem layoutSystem, UIElement element, UIElement proxy) {
            UITextElement textElement = (UITextElement) element;
            this.elementId = textElement.id;
            this.textInfo = textElement.textInfo;
        }

        // never called
        public float ResolveAutoWidth(ref BurstLayoutRunner runner, ElementId elementId, in BlockSize blockSize) {
            return 0;
        }

        // never called
        public float ResolveAutoHeight(ref BurstLayoutRunner runner, ElementId elementId, in BlockSize blockSize) {
            return 0;
        }

        public void OnChildrenChanged(LayoutSystem layoutSystem) {
        }

    }

}