using System;
using System.Diagnostics;
using UIForia.Graphics;
using UIForia.Rendering;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Text {

    [BurstCompile]
    internal unsafe struct UpdateTextRenderBounds : IJob {

        public DataList<TextId> activeTextElementInfos;
        public DataList<FontAssetInfo>.Shared fontAssetMap;

        public void Execute() {

            Run(0, activeTextElementInfos.size);

        }

        private void Run(int start, int end) {

            for (int i = start; i < end; i++) {

                ComputeBounds(ref activeTextElementInfos[i].textInfo[0]);

            }

        }

        private void ComputeBounds(ref TextInfo textInfo) {

            FontAssetInfo* fontAssetMapArray = fontAssetMap.GetArrayPointer();

            for (int i = 0; i < textInfo.renderRangeList.size; i++) {

                ref TextRenderRange renderRange = ref textInfo.renderRangeList.array[i];

                // if (!renderRange.needsBoundsUpdate) {
                //     continue;
                // }

                float xMin = float.MaxValue;
                float yMin = float.MaxValue;
                float xMax = float.MinValue;
                float yMax = float.MinValue;

                switch (renderRange.type) {

                    case TextRenderType.Characters: {

                        int idx = renderRange.characterRange.start;

                        while (idx != -1) {

                            ref TextSymbol symbol = ref textInfo.symbolList.array[idx];
                            ref BurstCharInfo charInfo = ref symbol.charInfo;

                            if (charInfo.effectIdx != 0) {

                                // todo -- 

                            }
                            else {

                                ref UIForiaGlyph glyph = ref fontAssetMapArray[charInfo.fontAssetId].glyphList[charInfo.glyphIndex];

                                float localMinX = charInfo.position.x;
                                float localMinY = charInfo.position.y;
                                float localMaxX = charInfo.position.x + (charInfo.scale * glyph.width);
                                float localMaxY = charInfo.position.y + (charInfo.scale * glyph.height);

                                if (localMinX < xMin) xMin = localMinX;
                                if (localMinX > xMax) xMax = localMinX;

                                if (localMaxX < xMin) xMin = localMaxX;
                                if (localMaxX > xMax) xMax = localMaxX;

                                if (localMinY < yMin) yMin = localMinY;
                                if (localMinY > yMax) yMax = localMinY;

                                if (localMaxY < yMin) yMin = localMaxY;
                                if (localMaxY > yMax) yMax = localMaxY;

                            }

                            idx = symbol.charInfo.nextRenderIdx;

                        }

                        renderRange.localBounds = new AxisAlignedBounds2D(xMin, yMin, xMax, yMax);

                        break;
                    }

                    case TextRenderType.Underline:
                        break;

                    case TextRenderType.Sprite:
                        break;

                    case TextRenderType.Image:
                        break;

                    case TextRenderType.Element:
                        break;

                }

            }
        }

    }

}