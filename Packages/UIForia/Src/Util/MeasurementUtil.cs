using System;
using System.Diagnostics;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Systems;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Util {

    internal static class MeasurementUtil {

        public static ResolvedGridCellSize ResolveGridCellSize(CheckedArray<Rect> viewRects, float appWidth, float appHeight, int viewIdx, GridCellDefinition size, float emSize) {
            ResolvedGridCellSize retn = default;

            retn.stretch = size.stretch;
            retn.unit = ResolveGridCellSizeUnit.Pixel;

            switch (size.unit) {

                case 0:
                case GridTemplateUnit.Stretch:
                case GridTemplateUnit.Pixel:
                    retn.value = size.value;
                    break;

                case GridTemplateUnit.Em:
                    retn.value = emSize * size.value;
                    break;

                case GridTemplateUnit.ViewportWidth:
                    retn.value = size.value * viewRects[viewIdx].width;
                    break;

                case GridTemplateUnit.ViewportHeight:
                    retn.value = size.value * viewRects[viewIdx].height;
                    break;

                case GridTemplateUnit.ApplicationWidth:
                    retn.value = size.value * appWidth;
                    break;

                case GridTemplateUnit.ApplicationHeight:
                    retn.value = size.value * appHeight;
                    break;

                case GridTemplateUnit.MinContent:
                    retn.value = size.value;
                    retn.unit = ResolveGridCellSizeUnit.MinContent;
                    break;

                case GridTemplateUnit.MaxContent:
                    retn.value = size.value;
                    retn.unit = ResolveGridCellSizeUnit.MaxContent;
                    break;

                case GridTemplateUnit.Percent:
                    retn.value = size.value;
                    retn.unit = ResolveGridCellSizeUnit.Percent;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return retn;

        }

        public static ResolvedSpacerSize ResolveSpacerSize(CheckedArray<Rect> viewRects, float appWidth, float appHeight, int viewIdx, UISpaceSize space, float emSize) {

            ResolvedSpacerSize retn = default;

            retn.stretch = space.stretch;

            switch (space.unit) {

                case UISpaceSizeUnit.Unset:
                case UISpaceSizeUnit.Stretch:
                case UISpaceSizeUnit.Pixel:
                    retn.value = space.fixedValue;
                    break;

                case UISpaceSizeUnit.Em:
                    retn.value = emSize * space.fixedValue;
                    break;

                case UISpaceSizeUnit.ViewportWidth:
                    retn.value = space.fixedValue * viewRects[viewIdx].width;
                    break;

                case UISpaceSizeUnit.ViewportHeight:
                    retn.value = space.fixedValue * viewRects[viewIdx].height;
                    break;

                case UISpaceSizeUnit.ApplicationWidth:
                    retn.value = space.fixedValue * appWidth;
                    break;

                case UISpaceSizeUnit.ApplicationHeight:
                    retn.value = space.fixedValue * appHeight;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return retn;
        }

        public static float ResolveTransformPivot(float baseSize, in ViewParameters viewParameters, float emSize, UIFixedLength fixedSize) {
            switch (fixedSize.unit) {

                default:
                case UIFixedUnit.Unset:
                case UIFixedUnit.Pixel:
                    return fixedSize.value;

                case UIFixedUnit.Percent:
                    return baseSize * fixedSize.value;

                case UIFixedUnit.Em:
                    return emSize;

                case UIFixedUnit.ViewportWidth:
                    return viewParameters.viewWidth;

                case UIFixedUnit.ViewportHeight:
                    return viewParameters.viewHeight;
            }
        }

        public static float ResolveFixedLayoutSize(in ViewParameters viewParameters, float emSize, UIFixedLength fixedSize) {
            switch (fixedSize.unit) {

                default:
                case UIFixedUnit.Unset:
                case UIFixedUnit.Percent: // percent not supported in layouts
                case UIFixedUnit.Pixel:
                    return fixedSize.value;

                case UIFixedUnit.Em:
                    return fixedSize.value * emSize;

                case UIFixedUnit.ViewportWidth:
                    return viewParameters.viewWidth;

                case UIFixedUnit.ViewportHeight:
                    return viewParameters.viewHeight;
            }
        }

        [DebuggerStepThrough]
        public static float ResolveFixedSize(float baseSize, in ViewParameters viewParameters, float emSize, UIFixedLength fixedSize) {
            switch (fixedSize.unit) {
                case UIFixedUnit.Pixel:
                    return fixedSize.value;

                case UIFixedUnit.Percent:
                    return baseSize * fixedSize.value;

                case UIFixedUnit.ViewportHeight:
                    return viewParameters.viewHeight * fixedSize.value;

                case UIFixedUnit.ViewportWidth:
                    return viewParameters.viewWidth * fixedSize.value;

                case UIFixedUnit.Em:
                    return emSize * fixedSize.value;

                default:
                    return 0;
            }
        }

    }

}