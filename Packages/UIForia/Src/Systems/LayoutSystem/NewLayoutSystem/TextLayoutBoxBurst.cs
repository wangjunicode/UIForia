using UIForia.Elements;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Layout {

    internal unsafe struct TextLayoutBoxBurst : ILayoutBox {

        public ElementId elementId;
        public int textElementInfoIndex;

        public void RunHorizontal(BurstLayoutRunner* runner) {

            ref List_TextLineInfo buffer = ref UnsafeUtilityEx.AsRef<List_TextLineInfo>(runner->lineInfoBuffer);
            ref TextInfo textInfo = ref runner->GetTextInfo(textElementInfoIndex);
            ref LayoutInfo layoutInfo = ref runner->GetHorizontalLayoutInfo(elementId);

            float width = layoutInfo.finalSize - layoutInfo.paddingBorderStart - layoutInfo.paddingBorderStart;

            if (textInfo.isRichText) {
                TextInfo.RunLayoutHorizontal_RichText(ref textInfo, ref buffer, math.max(0, width));
            }
            else {
                TextInfo.RunLayoutHorizontal_WordsOnly(ref textInfo, ref buffer, math.max(0, width));
            }

            textInfo.lineInfoList.CopyFrom(buffer.array, buffer.size, Allocator.Persistent);

            // now I need to handle alignment 
            
            for (int i = 0; i < buffer.size; i++) {
                TextLineInfo lineInfo = buffer[i];
                float position = 0;
                for (int w = lineInfo.wordStart; w < lineInfo.wordStart + lineInfo.wordCount; w++) {
                    ref TextLayoutSymbol layoutSymbol = ref textInfo.layoutSymbolList.array[w];
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
            ref TextInfo textInfo = ref runner->GetTextInfo(textElementInfoIndex);
            int fontAssetId = textInfo.textStyle.fontAssetId;
            ref FontAssetInfo fontAsset = ref runner->GetFontAsset(fontAssetId);
            float fontSize = runner->GetResolvedFontSize(elementId);

            TextInfo.RunLayoutVertical_WordsOnly(fontAsset, fontSize, ref textInfo);
            
            for (int i = 0; i < textInfo.lineInfoList.size; i++) {
                TextLineInfo lineInfo =  textInfo.lineInfoList.array[i];
                float maxAscender = 0;
                for (int w = lineInfo.wordStart; w < lineInfo.wordStart + lineInfo.wordCount; w++) {
                    ref TextLayoutSymbol layoutSymbol = ref textInfo.layoutSymbolList.array[w];
                    TextLayoutSymbolType type = layoutSymbol.type;
                    if (type == TextLayoutSymbolType.Word) {
                        maxAscender = maxAscender > layoutSymbol.wordInfo.maxAscender ?maxAscender : layoutSymbol.wordInfo.maxAscender; 
                    }
                }
                
                for (int w = lineInfo.wordStart; w < lineInfo.wordStart + lineInfo.wordCount; w++) {
                    ref TextLayoutSymbol layoutSymbol = ref textInfo.layoutSymbolList.array[w];
                    TextLayoutSymbolType type = layoutSymbol.type;
                    if (type == TextLayoutSymbolType.Word) {
                        for (int c = layoutSymbol.wordInfo.charStart; c < layoutSymbol.wordInfo.charEnd; c++) {
                            ref TextSymbol symbol = ref textInfo.symbolList.array[c];
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
            ref List_TextLineInfo buffer = ref UnsafeUtilityEx.AsRef<List_TextLineInfo>(runner.lineInfoBuffer);
            ref TextInfo textInfo = ref runner.GetTextInfo(textElementInfoIndex);

            //TextInfo.RunLayoutHorizontal_WordsOnly(ref textInfo, ref buffer, blockSize.insetSize);
            TextInfo.RunLayoutHorizontal_RichText(ref textInfo, ref buffer, blockSize.insetSize);
            

            float max = 0;

            for (int i = 0; i < buffer.size; i++) {
                max = math.max(buffer.array[i].width, max);
            }

            // did the work, might as well copy result into the line list. It might change next time we compute but if it doesnt we can use the result
            // how do i know if I can use the result though? maybe i computed that result last frame?

            return max;
        }

        public float ComputeContentHeight(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            ref TextInfo textInfo = ref layoutRunner.GetTextInfo(textElementInfoIndex);

            int fontAssetId = textInfo.textStyle.fontAssetId;
            ref FontAssetInfo fontAsset = ref layoutRunner.GetFontAsset(fontAssetId);
            float fontSize = layoutRunner.GetResolvedFontSize(elementId);

            TextInfo.RunLayoutVertical_WordsOnly(fontAsset, fontSize, ref textInfo);

            ref List_TextLineInfo lineInfoList = ref textInfo.lineInfoList;

            return lineInfoList.GetLast().y + lineInfoList.GetLast().height;
        }

        public void Dispose() { }

        public void OnInitialize(LayoutSystem layoutSystem, UIElement element) {
            UITextElement textElement = (UITextElement) element;
            this.elementId = textElement.id;
            this.textElementInfoIndex = textElement.textInfoId;
        }

        // never called
        public float ResolveAutoWidth(ref BurstLayoutRunner runner, ElementId elementId, UIMeasurement measurement, in BlockSize blockSize) {
            return 0;
        }

        // never called
        public float ResolveAutoHeight(ref BurstLayoutRunner runner, ElementId elementId, UIMeasurement measurement, in BlockSize blockSize) {
            return 0;
        }

    }

}