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

            TextInfo.RunLayoutHorizontal_WordsOnly(ref textInfo, ref buffer, math.max(0, width));

            textInfo.lineInfoList.CopyFrom(buffer.array, buffer.size, Allocator.Persistent);

            // // now I need to handle alignment 
            
            for (int i = 0; i < buffer.size; i++) {
                TextLineInfo lineInfo = buffer[i];
                float position = 0;
                for (int w = lineInfo.wordStart; w < lineInfo.wordStart + lineInfo.wordCount; w++) {
                    ref TextLayoutSymbol layoutSymbol = ref textInfo.layoutSymbolList.array[w];
                    layoutSymbol.wordInfo.x = position;
                    position += layoutSymbol.wordInfo.width;
                }
            }
        }

        public void RunVertical(BurstLayoutRunner* runner) {
            ref TextInfo textInfo = ref runner->GetTextInfo(textElementInfoIndex);
            int fontAssetId = textInfo.textStyle.fontAssetId;
            ref FontAssetInfo fontAsset = ref runner->GetFontAsset(fontAssetId);
            float fontSize = runner->GetResolvedFontSize(elementId);

            TextInfo.RunLayoutVertical_WordsOnly(fontAsset, fontSize, ref textInfo);

            // for (int i = 0; i < textInfo.lineInfoList.size; i++) {
            //     ref TextLineInfo lineInfo = ref textInfo.lineInfoList[i];
            //
            //     int first = -1;
            //
            //     // dont need line indices if all im doing is building rendered character infos
            //     // if text changes, ill run layout
            //     // if something affecting size changes, (font, font size, style, etc) ill run layout
            //     // better to build render info now while i know I have to do it rather than every frame
            //
            //     // for (int w = lineInfo.wordStart; w < lineInfo.wordStart + lineInfo.wordCount; w++) {
            //     //     ref TextLayoutSymbol wordInfo = ref textInfo.layoutSymbolList[w];
            //     //     if (wordInfo.type == TextLayoutSymbolType.Word) {
            //     //
            //     //         if (first < 0) {
            //     //             first = wordInfo.wordInfo.charStart;
            //     //             lineInfo.globalCharacterStartIndex = wordInfo.wordInfo.charStart;
            //     //         }
            //     //
            //     //         lineInfo.globalCharacterEndIndex = wordInfo.wordInfo.charEnd;
            //     //
            //     //         for (int c = wordInfo.wordInfo.charStart; c < wordInfo.wordInfo.charEnd; c++) {
            //     //             ref BurstCharInfo charInfo = ref textInfo.symbolList[c].charInfo;
            //     //             // textInfo.renderedCharacters.Add(new TextRenderBox2.CharRenderInfo() {
            //     //             //     position = new float2(charInfo.topLeft.x + wordInfo.wordInfo.x, charInfo.topLeft.y + wordInfo.wordInfo.x),
            //     //             //     glyphIndex = charInfo.glyphIndex,
            //     //             //     lineIndex = 0, // can move this to lines now 
            //     //             //     materialIndex = 0, // compute later? already computed by text engine?
            //     //             //     
            //     //             // });
            //     //             charInfo.lineIndex = (ushort) i;
            //     //         }
            //     //
            //     //     }
            //     //
            //     // }
            //
            // }

        }

        public float ComputeContentWidth(ref BurstLayoutRunner runner, in BlockSize blockSize) {
            ref List_TextLineInfo buffer = ref UnsafeUtilityEx.AsRef<List_TextLineInfo>(runner.lineInfoBuffer);
            ref TextInfo textInfo = ref runner.GetTextInfo(textElementInfoIndex);

            TextInfo.RunLayoutHorizontal_WordsOnly(ref textInfo, ref buffer, blockSize.insetSize);

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