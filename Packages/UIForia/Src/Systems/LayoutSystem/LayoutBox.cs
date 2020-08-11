using System;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public struct LayoutHierarchyInfo {

        public ElementId parentId;
        public ElementId firstChildId;
        public ElementId lastChildId;
        public ElementId nextSiblingId;
        public ElementId prevSiblingId;
        public int childCount;
        public LayoutBehavior behavior;

    }
    
    public abstract class LayoutBox {

        public LayoutBoxFlags flags;
        
        public float finalWidth;
        public float finalHeight;
        public float cachedContentWidth;
        public float cachedContentHeight;
        public float cachedBlockWidth;
        public float cachedBlockHeight;

        public UIElement element;
   
        public ElementId layoutParentId;
        public ElementId elementId;
        
        public ElementSystem elementSystem;
        protected LayoutSystem layoutSystem;
        
        public float paddingBorderHorizontalStart;
        public float paddingBorderHorizontalEnd;
        public float paddingBorderVerticalStart;
        public float paddingBorderVerticalEnd;

        public float marginHorizontalStart;
        public float marginHorizontalEnd;
        public float marginVerticalStart;
        public float marginVerticalEnd;
        
        
        // Kill List
        
        // move to debug data
        internal int cacheMiss;
        internal int cacheHit;
        
        public LayoutBox firstChild;
        public LayoutBox nextSibling;
        public LayoutBox parent;
        //public ClipData clipData;
        //public float transformX;
        //public float transformY;
        //public ClipBehavior clipBehavior;
        //public ClipBounds clipBounds;
        //public int traversalIndex;
        //public int zIndex;
//
        //public OffsetMeasurement transformPositionX;
        //public OffsetMeasurement transformPositionY;
        //public float transformScaleX;
        //public float transformScaleY;
        //public float transformRotation;
        //public UIFixedLength transformPivotX;
        //public UIFixedLength transformPivotY;

        public void Initialize(LayoutSystem layoutSystem, ElementSystem elementSystem, UIElement element, int frameId) {
            this.elementId = element.id;
            this.elementSystem = elementSystem;
            this.layoutSystem = layoutSystem;
            this.element = element;
            this.finalWidth = -1;
            this.finalHeight = -1;
            this.cachedContentWidth = -1;
            this.cachedContentHeight = -1;
            this.cachedBlockWidth = -1;
            this.cachedBlockHeight = -1;
            this.cacheHit = 0;
            this.cacheMiss = 0;
            OnInitialize();
        }

        public int childCount {
            get => 0; //layoutHierarchyTable[elementId].childCount;
        }
        
        protected virtual void OnInitialize() { }

        public void Destroy() {
            OnDestroy();
            layoutSystem = null;
            elementSystem = null; 
            flags = 0;
            element = null;
            nextSibling = null;
            firstChild = null;
        }

        protected virtual void OnDestroy() { }

        protected abstract float ComputeContentWidth();

        protected abstract float ComputeContentHeight();

        public abstract void OnChildrenChanged();

        public void UpdateContentAreaWidth() {
            // flags &= ~LayoutBoxFlags.ContentAreaWidthChanged;
            Vector2 viewSize = element.View.Viewport.size;
            float emSize = element.style.GetResolvedFontSize();
            
            // todo -- improve
            
            ViewParameters viewParameters = new ViewParameters();
            viewParameters.viewWidth = viewSize.x;
            viewParameters.viewHeight = viewSize.y;
            
            float paddingLeft = MeasurementUtil.ResolveFixedSize(finalWidth, viewParameters, emSize, element.style.PaddingLeft);
            float paddingRight = MeasurementUtil.ResolveFixedSize(finalWidth, viewParameters, emSize, element.style.PaddingRight);
            float borderLeft = MeasurementUtil.ResolveFixedSize(finalWidth, viewParameters, emSize, element.style.BorderLeft);
            float borderRight = MeasurementUtil.ResolveFixedSize(finalWidth, viewParameters, emSize, element.style.BorderRight);

            // write to layout result here? would need to flag layout result for changes anyway
            // ref LayoutResult layoutResult = ref element.layoutResult;
            //
            // layoutResult.padding.left = paddingLeft;
            // layoutResult.padding.right = paddingRight;
            // layoutResult.border.left = borderLeft;
            // layoutResult.border.right = borderRight;
            //
            // paddingBorderHorizontalStart = paddingLeft + borderLeft;
            // paddingBorderHorizontalEnd = paddingRight + borderRight;
        }

        public void UpdateContentAreaHeight() {
            // flags &= ~LayoutBoxFlags.ContentAreaHeightChanged;
            Vector2 viewSize = element.View.Viewport.size;
            // float emSize = element.style.GetResolvedFontSize();
            // ref LayoutResult layoutResult = ref element.layoutResult;
            // // todo -- improve
            //
            // ViewParameters viewParameters = new ViewParameters();
            // viewParameters.viewWidth = viewSize.x;
            // viewParameters.viewHeight = viewSize.y;
            //
            // float paddingTop = MeasurementUtil.ResolveFixedSize(finalHeight, viewParameters, emSize, element.style.PaddingTop);
            // float paddingBottom = MeasurementUtil.ResolveFixedSize(finalHeight, viewParameters, emSize, element.style.PaddingBottom);
            // float borderTop = MeasurementUtil.ResolveFixedSize(finalHeight, viewParameters, emSize, element.style.BorderTop);
            // float borderBottom = MeasurementUtil.ResolveFixedSize(finalHeight, viewParameters, emSize, element.style.BorderBottom);
            //
            // layoutResult.padding.top = paddingTop;
            // layoutResult.padding.bottom = paddingBottom;
            // layoutResult.border.top = borderTop;
            // layoutResult.border.bottom = borderBottom;
            //
            // paddingBorderVerticalStart = paddingTop + borderTop;
            // paddingBorderVerticalEnd = paddingBottom + borderBottom;
        }

        public void ApplyLayoutHorizontalExplicit(float localX, float size, int frameId) {
            // ref LayoutResult layoutResult = ref element.layoutResult;
            //
            // layoutResult.alignedPosition.x = localX;
            // layoutResult.allocatedPosition.x = localX;
            // layoutResult.actualSize.width = size;
            // layoutResult.allocatedSize.width = size;
            // layoutResult.margin.left = 0;
            // layoutResult.margin.right = 0;
            // if (size != finalWidth) {
            //     flags |= LayoutBoxFlags.RequireLayoutHorizontal;
            //     finalWidth = size;
            // }
            //
            // UpdateContentAreaWidth();
        }
        
        public void ApplyLayoutHorizontal(float localX, float alignedPosition, in LayoutSize reportedSize, float size, float availableSize, LayoutFit defaultFit, int frameId) {
            LayoutFit fit = element.style.LayoutFitHorizontal;

            if (fit == LayoutFit.Default || fit == LayoutFit.Unset) {
                fit = defaultFit;
            }

            float newWidth = size;

            switch (fit) {
                case LayoutFit.Unset:
                case LayoutFit.None:
                case LayoutFit.Default:
                    newWidth = size;
                    break;

                case LayoutFit.Grow:
                    if (availableSize > size) {
                        newWidth = availableSize;
                        alignedPosition = localX;
                    }

                    break;

                case LayoutFit.Shrink:
                    if (availableSize < size) {
                        newWidth = availableSize;
                        alignedPosition = localX;
                    }

                    break;

                case LayoutFit.Fill:
                    newWidth = availableSize;
                    alignedPosition = localX;
                    break;

                case LayoutFit.FillParent:
                    if (parent == null) {
                        newWidth = size;
                    }
                    else {
                        newWidth = parent.finalWidth;
                        alignedPosition = 0; //localX;
                    }

                    break;
            }

            // write to layout result here? would need to flag layout result for changes anyway
            // ref LayoutResult layoutResult = ref element.layoutResult;
            //
            // float previousPosition = layoutResult.alignedPosition.x;
            //
            // // todo -- layout result change flags (and maybe history entry if enabled)
            // layoutResult.alignedPosition.x = alignedPosition;
            // layoutResult.allocatedPosition.x = localX;
            // layoutResult.actualSize.width = newWidth;
            // layoutResult.allocatedSize.width = availableSize;
            // layoutResult.margin.left = reportedSize.marginStart;
            // layoutResult.margin.right = reportedSize.marginEnd;
            //
            // // if ((flags & LayoutBoxFlags.RequireAlignmentHorizontal) == 0 && !Mathf.Approximately(previousPosition, alignedPosition)) {
            // //     flags |= LayoutBoxFlags.RequiresMatrixUpdate;
            // //     flags |= LayoutBoxFlags.RecomputeClipping;
            // // }
            //
            // // todo -- should probably be when content area size changes, not just overall size
            // if (newWidth != finalWidth) {
            //     flags |= LayoutBoxFlags.RequireLayoutHorizontal;
            //     finalWidth = newWidth;
            // }
            //
            // UpdateContentAreaWidth();
        }

        public void ApplyLayoutVertical(float localY, float alignedPosition, in LayoutSize reportedSize, float size, float availableSize, LayoutFit defaultFit, int frameId) {
            LayoutFit fit = element.style.LayoutFitVertical;
            if (fit == LayoutFit.Default || fit == LayoutFit.Unset) {
                fit = defaultFit;
            }

            float newHeight = size;

            switch (fit) {
                case LayoutFit.Unset:
                case LayoutFit.None:
                case LayoutFit.Default:
                    newHeight = size;
                    break;

                case LayoutFit.Grow:
                    if (availableSize > size) {
                        newHeight = availableSize;
                        alignedPosition = localY;
                    }

                    break;

                case LayoutFit.Shrink:
                    if (availableSize < size) {
                        newHeight = availableSize;
                        alignedPosition = localY;
                    }

                    break;

                case LayoutFit.Fill:
                    newHeight = availableSize;
                    alignedPosition = localY;
                    break;
            }

            // if aligned position changed -> flag for matrix recalc 
            // write to layout result here? would need to flag layout result for changes anyway
            LayoutResult layoutResult = default; //ref element.layoutResult;

            // todo -- layout result change flags (and maybe history entry if enabled)

            float previousPosition = layoutResult.alignedPosition.y;

            // layoutResult.alignedPosition.y = alignedPosition;
            // layoutResult.allocatedPosition.y = localY;
            //
            // layoutResult.actualSize.height = newHeight;
            // layoutResult.allocatedSize.height = availableSize;
            // layoutResult.pivot.y = newHeight * 0.5f; // todo -- resolve pivot

            // todo -- margin

            // if ((flags & LayoutBoxFlags.RequireAlignmentVertical) == 0 && !Mathf.Approximately(previousPosition, alignedPosition)) {
            //     flags |= LayoutBoxFlags.RequiresMatrixUpdate;
            // }

            if (newHeight != finalHeight) {
                flags |= LayoutBoxFlags.RequireLayoutVertical;
                //  element.layoutHistory.AddLogEntry(LayoutDirection.Vertical, frameId, LayoutReason.FinalSizeChanged, string.Empty);
                finalHeight = newHeight;
            }

            UpdateContentAreaHeight();
        }

        public void ApplyLayoutVerticalExplicit(float localY, float size, int frameId) {
            LayoutResult layoutResult = default; //ref element.layoutResult;

            // layoutResult.alignedPosition.y = localY;
            // layoutResult.allocatedPosition.y = localY;
            // layoutResult.actualSize.height = size;
            // layoutResult.allocatedSize.height = size;
            // layoutResult.margin.top = 0;
            // layoutResult.margin.bottom = 0;
            if (size != finalHeight) {
                flags |= LayoutBoxFlags.RequireLayoutHorizontal;
                finalHeight = size;
            }

            UpdateContentAreaHeight();
        }

        protected virtual float ResolveAutoWidth(LayoutBox child, float factor) {
            return child.GetContentWidth(factor);
        }

        public float GetContentWidth(float factor) {
            float width = 0;

            // todo -- this cached value is only valid if the current block size is the same as when the size was computed
            // probably makes sense to hold at least 2 versions of content cache, 1 for baseline one for 2nd pass (ie fit)
            if (cachedContentWidth >= 0) {
                float blockSize = ComputeBlockWidth(1);
                if (Math.Abs(blockSize - cachedBlockWidth) < Mathf.Epsilon) {
                    width = cachedContentWidth; // todo -- might not need to resolve size for padding / border in this case
                    cacheHit++;
                }
                else {
                    cacheMiss++;
                    cachedContentWidth = ComputeContentWidth();
                    cachedBlockWidth = blockSize;
                    width = cachedContentWidth;
                }
            }
            else {
                cachedContentWidth = ComputeContentWidth();
                width = cachedContentWidth;
                cacheMiss++;
            }

            float baseVal = width;
            // todo -- try not to fuck with style here
            // todo -- view and em size
            Vector2 viewSize = element.View.Viewport.size;
            float emSize = element.style.GetResolvedFontSize();
            
            ViewParameters viewParameters = new ViewParameters();
            viewParameters.viewWidth = viewSize.x;
            viewParameters.viewHeight = viewSize.y;
            
            baseVal += MeasurementUtil.ResolveFixedSize(width, viewParameters, emSize, element.style.PaddingLeft);
            baseVal += MeasurementUtil.ResolveFixedSize(width, viewParameters, emSize, element.style.PaddingRight);
            baseVal += MeasurementUtil.ResolveFixedSize(width, viewParameters, emSize, element.style.BorderRight);
            baseVal += MeasurementUtil.ResolveFixedSize(width, viewParameters, emSize, element.style.BorderLeft);

            if (baseVal < 0) baseVal = 0;

            float retn = factor * baseVal;

            return retn > 0 ? retn : 0;
        }

        public float GetContentHeight(float factor) {
            float height = 0;

            // todo -- this cached value is only valid if the current block size is the same as when the size was computed
            // probably makes sense to hold at least 2 versions of content cache, 1 for baseline one for 2nd pass (ie fit)
            if (cachedContentHeight >= 0) {
                float blockSize = ComputeBlockHeight(1);
                if (Math.Abs(blockSize - cachedBlockHeight) < Mathf.Epsilon) {
                    height = cachedContentHeight; // todo -- might not need to resolve size for padding / border in this case
                    cacheHit++;
                }
                else {
                    cacheMiss++;
                    cachedContentHeight = ComputeContentHeight();
                    cachedBlockHeight = blockSize;
                    height = cachedContentHeight;
                }
            }
            else {
                cachedContentHeight = ComputeContentHeight();
                height = cachedContentHeight;
                cacheMiss++;
            }

            float baseVal = height;
            // todo -- try not to fuck with style here
            // todo -- view and em size
            Vector2 viewSize = element.View.Viewport.size;
            float emSize = element.style.GetResolvedFontSize();
            
            ViewParameters viewParameters = new ViewParameters();
            viewParameters.viewWidth = viewSize.x;
            viewParameters.viewHeight = viewSize.y;
            
            baseVal += MeasurementUtil.ResolveFixedSize(height, viewParameters, emSize, element.style.PaddingTop);
            baseVal += MeasurementUtil.ResolveFixedSize(height, viewParameters, emSize, element.style.PaddingBottom);
            baseVal += MeasurementUtil.ResolveFixedSize(height, viewParameters, emSize, element.style.BorderTop);
            baseVal += MeasurementUtil.ResolveFixedSize(height, viewParameters, emSize, element.style.BorderBottom);

            if (baseVal < 0) baseVal = 0;

            float retn = factor * baseVal;

            return retn > 0 ? retn : 0;
        }

        public float ResolveWidth(in UIMeasurement measurement) {
            float value = measurement.value;

            switch (measurement.unit) {
                case UIMeasurementUnit.Auto: {
                    return parent.ResolveAutoWidth(this, measurement.value);
                }

                case UIMeasurementUnit.Content: {
                    return GetContentWidth(measurement.value);
                }

                case UIMeasurementUnit.FitContent:
                    throw new NotImplementedException();

                case UIMeasurementUnit.Pixel:
                    return value;

                case UIMeasurementUnit.Em:
                    return element.style.GetResolvedFontSize() * value;

                case UIMeasurementUnit.ViewportWidth:
                    return element.View.Viewport.width * value;

                case UIMeasurementUnit.ViewportHeight:
                    return element.View.Viewport.height * value;

                case UIMeasurementUnit.IntrinsicMinimum:
                    return 0; //GetIntrinsicMinWidth();

                case UIMeasurementUnit.IntrinsicPreferred:
                    return GetIntrinsicPreferredWidth();

                case UIMeasurementUnit.BlockSize: {
                    // ignored elements can use the output size of their parent since it has been resolved already
                    return ComputeBlockWidth(measurement.value);
                }

                case UIMeasurementUnit.Percentage:
                case UIMeasurementUnit.ParentContentArea: {
                    return ComputeBlockContentAreaWidth(measurement.value);
                }
            }

            return 0;
        }

        public virtual float GetIntrinsicPreferredWidth() {
            float width = 0;

            // todo -- this cached value is only valid if the current block size is the same as when the size was computed
            // probably makes sense to hold at least 2 versions of content cache, 1 for baseline one for 2nd pass (ie fit)
            if (cachedContentWidth >= 0) {
                width = cachedContentWidth; // todo -- might not need to resolve size for padding / border in this case
            }
            else {
                cachedContentWidth = ComputeContentWidth();
                width = cachedContentWidth;
            }

            float baseVal = width;
            // todo -- try not to fuck with style here
            // todo -- view and em size
            Vector2 viewSize = element.View.Viewport.size;
            float emSize = element.style.GetResolvedFontSize();
            
            // todo -- improve
            ViewParameters viewParameters = new ViewParameters();
            viewParameters.viewWidth = viewSize.x;
            viewParameters.viewHeight = viewSize.y;
            
            baseVal += MeasurementUtil.ResolveFixedSize(width, viewParameters, emSize, element.style.PaddingLeft);
            baseVal += MeasurementUtil.ResolveFixedSize(width, viewParameters, emSize, element.style.PaddingRight);
            baseVal += MeasurementUtil.ResolveFixedSize(width, viewParameters, emSize, element.style.BorderRight);
            baseVal += MeasurementUtil.ResolveFixedSize(width, viewParameters, emSize, element.style.BorderLeft);

            if (baseVal < 0) baseVal = 0;

            float retn = baseVal;

            return retn > 0 ? retn : 0;
        }

        public float ComputeBlockContentAreaWidth(float value) {
            // LayoutBox ptr = parent;
            // float paddingBorder = 0;
            //
            // // ignored elements can use the output size of their parent since it has been resolved already
            // if ((flags & LayoutBoxFlags.Ignored) != 0) {
            //     LayoutResult parentResult = elementSystem.layoutResultTable[layoutParentId]; //element.layoutResult.layoutParent;
            //     paddingBorder = parentResult.padding.left + parentResult.padding.right + parentResult.border.left + parentResult.border.right;
            //     return Math.Max(0, (parentResult.actualSize.width - paddingBorder) * value);
            // }
            //
            // while (ptr != null) {
            //     paddingBorder += ptr.paddingBorderHorizontalStart + ptr.paddingBorderHorizontalEnd;
            //
            //     if (ptr.CanProvideHorizontalBlockSize(this, out float blockSize)) {
            //         // ignore padding on provided element
            //         paddingBorder -= (ptr.paddingBorderHorizontalStart + ptr.paddingBorderHorizontalEnd);
            //         return Math.Max(0, (blockSize - paddingBorder) * value);
            //     }
            //
            //     if ((ptr.flags & LayoutBoxFlags.WidthBlockProvider) != 0) {
            //         Assert.AreNotEqual(-1, ptr.finalWidth);
            //         return Math.Max(0, (ptr.finalWidth - paddingBorder) * value);
            //     }
            //
            //     ptr = ptr.parent;
            // }
            //
            // return Math.Max(0, (element.View.Viewport.width - paddingBorder) * value);
            return default;
        }

        internal float ComputeBlockWidth(float value) {
            // if ((flags & LayoutBoxFlags.Ignored) != 0) {
            //     // LayoutResult parentResult = element.layoutResult.layoutParent;
            //     LayoutResult parentResult = elementSystem.layoutResultTable[layoutParentId];
            //     return Math.Max(0, parentResult.actualSize.width * value);
            // }
            //
            // LayoutBox ptr = parent;
            //
            // while (ptr != null) {
            //     if (ptr.CanProvideHorizontalBlockSize(this, out float blockSize)) {
            //         return Math.Max(0, blockSize * value);
            //     }
            //
            //     if ((ptr.flags & LayoutBoxFlags.WidthBlockProvider) != 0) {
            //         Assert.AreNotEqual(-1, ptr.finalWidth);
            //         return Math.Max(0, ptr.finalWidth * value);
            //     }
            //
            //     ptr = ptr.parent;
            // }
            //
            // return Math.Max(0, element.View.Viewport.width * value);
            return default;
        }

        public virtual bool CanProvideHorizontalBlockSize(LayoutBox child, out float blockSize) {
            blockSize = 0;
            return false;
        }

        public virtual bool CanProvideVerticalBlockSize(LayoutBox child, out float blockSize) {
            blockSize = 0;
            return false;
        }

        protected float ComputeBlockContentHeight(float value) {
            // LayoutBox ptr = parent;
            // float paddingBorder = 0;
            //
            // // ignored elements can use the output size of their parent since it has been resolved already
            // if ((flags & LayoutBoxFlags.Ignored) != 0) {
            //     // LayoutResult parentResult = element.layoutResult.layoutParent;
            //     LayoutResult parentResult = elementSystem.layoutResultTable[layoutParentId];
            //     paddingBorder = parentResult.padding.top + parentResult.padding.bottom + parentResult.border.top + parentResult.border.bottom;
            //     return Math.Max(0, (parentResult.actualSize.height - paddingBorder) * value);
            // }
            //
            // while (ptr != null) {
            //     paddingBorder += ptr.paddingBorderVerticalStart + ptr.paddingBorderVerticalEnd;
            //
            //     if (ptr.CanProvideVerticalBlockSize(this, out float blockSize)) {
            //         // ignore padding on provided element
            //         paddingBorder -= (ptr.paddingBorderVerticalStart + ptr.paddingBorderVerticalEnd);
            //         return Math.Max(0, (blockSize - paddingBorder) * value);
            //     }
            //
            //     if ((ptr.flags & LayoutBoxFlags.HeightBlockProvider) != 0) {
            //         Assert.AreNotEqual(-1, ptr.finalHeight);
            //         return Math.Max(0, (ptr.finalHeight - paddingBorder) * value);
            //     }
            //
            //     ptr = ptr.parent;
            // }
            //
            // return Math.Max(0, (element.View.Viewport.height - paddingBorder) * value);
            return default;
        }

        internal float ComputeBlockHeight(float value) {
            // LayoutBox ptr = parent;
            //
            // // ignored elements can use the output size of their parent since it has been resolved already
            // if ((flags & LayoutBoxFlags.Ignored) != 0) {
            //     LayoutResult parentResult = elementSystem.layoutResultTable[layoutParentId];
            //     // LayoutResult parentResult = element.layoutResult.layoutParent;
            //     return Math.Max(0, (parentResult.actualSize.height) * value);
            // }
            //
            // while (ptr != null) {
            //     if (ptr.CanProvideVerticalBlockSize(this, out float blockSize)) {
            //         // ignore padding on provided element
            //         return Math.Max(0, blockSize * value);
            //     }
            //
            //     if ((ptr.flags & LayoutBoxFlags.HeightBlockProvider) != 0) {
            //         Assert.AreNotEqual(-1, ptr.finalHeight);
            //         return Math.Max(0, ptr.finalHeight * value);
            //     }
            //
            //     ptr = ptr.parent;
            // }
            //
            // return Math.Max(0, element.View.Viewport.height * value);
            return default;
        }

        public float ResolveHeight(in UIMeasurement measurement) {
            float value = measurement.value;

            switch (measurement.unit) {
                case UIMeasurementUnit.Content: {
                    return GetContentHeight(measurement.value);
                }

                case UIMeasurementUnit.FitContent:
                    throw new NotImplementedException();

                case UIMeasurementUnit.Pixel:
                    return value;

                case UIMeasurementUnit.Em:
                    return element.style.GetResolvedFontSize() * value;

                case UIMeasurementUnit.ViewportWidth:
                    return element.View.Viewport.width * value;

                case UIMeasurementUnit.ViewportHeight:
                    return element.View.Viewport.height * value;

                case UIMeasurementUnit.IntrinsicMinimum: {
                    throw new NotImplementedException();
                }

                case UIMeasurementUnit.IntrinsicPreferred:
                    throw new NotImplementedException();

                case UIMeasurementUnit.BlockSize: {
                    return ComputeBlockHeight(value);
                }

                case UIMeasurementUnit.Percentage:
                case UIMeasurementUnit.ParentContentArea: {
                    return ComputeBlockContentHeight(value);
                }
            }

            return 0;
        }

        public abstract void RunLayoutHorizontal(int frameId);

        public abstract void RunLayoutVertical(int frameId);

        public void GetWidths(ref LayoutSize size) {

            // if((element.style.animationFlags & AnimationFlags.PreferredWidth) != 0) {
            //     AnimatedProperty animatedProperty;
            //     element.style.TryGetAnimatedProperty(StylePropertyId.PreferredWidth, out animatedProperty);
            //     float v0 = ResolveWidth(animatedProperty.value0.AsUIMeasurement);
            //     float v1 = ResolveWidth(animatedProperty.value1.AsUIMeasurement);
            //     size.preferred = Mathf.Lerp(v0, v1, animatedProperty.time);
            // }
            // else {
            size.preferred = ResolveWidth(element.style.PreferredWidth);
            // }

            size.minimum = ResolveWidth(element.style.MinWidth);
            size.maximum = ResolveWidth(element.style.MaxWidth);
            Vector2 viewSize = element.View.Viewport.size;
            float emSize = element.style.GetResolvedFontSize();
            ViewParameters viewParameters = new ViewParameters();
            viewParameters.viewWidth = viewSize.x;
            viewParameters.viewHeight = viewSize.y;
            size.marginStart = MeasurementUtil.ResolveFixedSize(0, viewParameters, emSize, element.style.MarginLeft);
            size.marginEnd = MeasurementUtil.ResolveFixedSize(0, viewParameters, emSize, element.style.MarginRight);
            // todo -- not sure this is right or desired
            // ref LayoutResult layoutResult = ref element.layoutResult;
            // layoutResult.margin.left = size.marginStart;
            // layoutResult.margin.right = size.marginEnd;
        }

        public void GetHeights(ref LayoutSize size) {
            size.preferred = ResolveHeight(element.style.PreferredHeight);
            size.minimum = ResolveHeight(element.style.MinHeight);
            size.maximum = ResolveHeight(element.style.MaxHeight);
            Vector2 viewSize = element.View.Viewport.size;
            float emSize = element.style.GetResolvedFontSize();
            ViewParameters viewParameters = new ViewParameters();
            viewParameters.viewWidth = viewSize.x;
            viewParameters.viewHeight = viewSize.y;
            size.marginStart = MeasurementUtil.ResolveFixedSize(0, viewParameters, emSize, element.style.MarginTop);
            size.marginEnd = MeasurementUtil.ResolveFixedSize(0, viewParameters, emSize, element.style.MarginBottom);
            // todo -- not sure this is right or desired
            // ref LayoutResult layoutResult = ref element.layoutResult;
            // layoutResult.margin.top = size.marginStart;
            // layoutResult.margin.bottom = size.marginEnd;
        }

        public virtual void OnStyleChanged(StyleProperty[] propertyList, int propertyCount) { }

        public virtual void OnChildStyleChanged(LayoutBox child, StyleProperty[] propertyList, int propertyCount) { }

        public void MarkContentParentsHorizontalDirty() {
            LayoutBox ptr = parent;

            while (ptr != null) {
                // once we hit a block provider we can safely stop traversing since the provider doesn't care about content size changing
                bool stop = (ptr.flags & LayoutBoxFlags.WidthBlockProvider) != 0;

                // can't break out if already flagged for layout because parent of parent might not be and might be content sized
                ptr.flags |= LayoutBoxFlags.RequireLayoutHorizontal;
                ptr.cachedContentWidth = -1;

                //  ptr.element.layoutHistory.AddLogEntry(LayoutDirection.Horizontal, frameId, reason);
                if (stop) break;
                ptr = ptr.parent;
            }
        }

        public void MarkContentParentsVerticalDirty() {
            LayoutBox ptr = parent;

            while (ptr != null) {
                // once we hit a block provider we can safely stop traversing since the provider doesn't care about content size changing
                bool stop = (ptr.flags & LayoutBoxFlags.HeightBlockProvider) != 0;

                // can't break out if already flagged for layout because parent of parent might not be and might be content sized
                ptr.flags |= LayoutBoxFlags.RequireLayoutVertical;
                ptr.cachedContentHeight = -1;
                //   ptr.element.layoutHistory.AddLogEntry(LayoutDirection.Vertical, frameId, reason);
                if (stop) break;
                ptr = ptr.parent;
            }
        }

        protected virtual bool IsAutoWidthContentBased() {
            return true;
        }

        public void MarkForLayoutHorizontal() {
            flags |= LayoutBoxFlags.RequireLayoutHorizontal;
            cachedContentWidth = -1;
            MarkContentParentsHorizontalDirty();
        }

        public void MarkForLayoutVertical() {
            flags |= LayoutBoxFlags.RequireLayoutVertical;
            cachedContentHeight = -1;
            MarkContentParentsVerticalDirty();
        }

        public void UpdateBlockProviderWidth() {
            UIMeasurementUnit pref = element.style.PreferredWidth.unit;
            UIMeasurementUnit min = element.style.MinWidth.unit;
            UIMeasurementUnit max = element.style.MaxWidth.unit;

            bool contentBased = (pref == UIMeasurementUnit.Content || min == UIMeasurementUnit.Content || max == UIMeasurementUnit.Content);

            bool intrinsic = (pref == UIMeasurementUnit.IntrinsicPreferred || min == UIMeasurementUnit.IntrinsicPreferred || max == UIMeasurementUnit.IntrinsicPreferred);

            bool autoSized = (pref == UIMeasurementUnit.Auto || min == UIMeasurementUnit.Auto || max == UIMeasurementUnit.Auto);

            if (contentBased || intrinsic || (autoSized && parent != null && parent.IsAutoWidthContentBased())) {
                flags &= ~LayoutBoxFlags.WidthBlockProvider;
            }
            else {
                flags |= LayoutBoxFlags.WidthBlockProvider;
            }
        }

        public void UpdateBlockProviderHeight() {
            bool contentBased = (element.style.PreferredHeight.unit == UIMeasurementUnit.Content || element.style.MinHeight.unit == UIMeasurementUnit.Content || element.style.MaxHeight.unit == UIMeasurementUnit.Content);

            if (contentBased) {
                flags &= ~LayoutBoxFlags.HeightBlockProvider;
            }
            else {
                flags |= LayoutBoxFlags.HeightBlockProvider;
            }
        }

        // public void UpdateClipper() {
        //     if (element.style.OverflowX != Overflow.Visible || element.style.OverflowY != Overflow.Visible) {
        //         flags |= LayoutBoxFlags.Clipper;
        //     }
        //     else {
        //         flags &= ~LayoutBoxFlags.Clipper;
        //     }
        // }

        public void UpdateRequiresHorizontalAlignment() {
            // UIStyleSet style = element.style;
            // AlignmentTarget alignment = style.AlignmentTargetX;
            //
            // if (alignment != AlignmentTarget.Unset) {
            //     flags |= LayoutBoxFlags.RequireAlignmentHorizontal;
            //     return;
            // }
            //
            // if (style.AlignmentOffsetX.value != 0) {
            //     flags |= LayoutBoxFlags.RequireAlignmentHorizontal;
            //     return;
            // }
            //
            // if (style.AlignmentOriginX.value != 0) {
            //     flags |= LayoutBoxFlags.RequireAlignmentHorizontal;
            //     return;
            // }
            //
            // if (style.AlignmentDirectionX != AlignmentDirection.Start) {
            //     flags |= LayoutBoxFlags.RequireAlignmentHorizontal;
            //     return;
            // }

            //   flags &= ~LayoutBoxFlags.RequireAlignmentHorizontal;
        }

        public void UpdateRequiresVerticalAlignment() {
            // UIStyleSet style = element.style;
            // AlignmentTarget alignment = style.AlignmentTargetY;
            //
            // if (alignment != AlignmentTarget.Unset) {
            //     flags |= LayoutBoxFlags.RequireAlignmentVertical;
            //     return;
            // }
            //
            // if (style.AlignmentOffsetY.value != 0) {
            //     flags |= LayoutBoxFlags.RequireAlignmentVertical;
            //     return;
            // }
            //
            // if (style.AlignmentOriginY.value != 0) {
            //     flags |= LayoutBoxFlags.RequireAlignmentVertical;
            //     return;
            // }
            //
            // if (style.AlignmentDirectionY != AlignmentDirection.Start) {
            //     flags |= LayoutBoxFlags.RequireAlignmentVertical;
            //     return;
            // }

            // flags &= ~LayoutBoxFlags.RequireAlignmentVertical;
        }

        public void Enable() {
            cachedContentWidth = -1;
            cachedContentHeight = -1;
            finalWidth = -1;
            finalHeight = -1;
//            element.layoutHistory = element.layoutHistory ?? new LayoutHistory(element);
//            element.layoutHistory.AddLogEntry(LayoutDirection.Horizontal, -1, LayoutReason.Initialized, boxName);
//            element.layoutHistory.AddLogEntry(LayoutDirection.Vertical, -1, LayoutReason.Initialized, boxName);
            flags |= LayoutBoxFlags.RequireLayoutHorizontal | LayoutBoxFlags.RequireLayoutVertical;

            if (element.style.LayoutBehavior == LayoutBehavior.Ignored) {
                flags |= LayoutBoxFlags.Ignored;
            }
            
            // UpdateBlockProviderWidth();
            // UpdateBlockProviderHeight();
            //
            // UpdateRequiresHorizontalAlignment();
            // UpdateRequiresVerticalAlignment();
            //
            // UpdateClipper();
        }

        internal void GetChildren(LightList<LayoutBox> list) {
            // for (int i = 0; i < element.children.size; i++) {
            //     var child = element.children.array[i];
            //     if (!child.isEnabled) continue;
            //     ref LayoutResult childLayoutResult = ref layoutSystem.elementSystem.layoutResultTable[child.id];
            //     switch (child.style.LayoutBehavior) {
            //         case LayoutBehavior.Ignored:
            //             child.layoutBox.parent = this;
            //           //  childLayoutResult.layoutParent = element.id; // todo -- multiple ignore levels?
            //             // ignoredList.Add(child.layoutBox);
            //             break;
            //
            //         case LayoutBehavior.TranscludeChildren:
            //             child.layoutBox.parent = this;
            //             //childLayoutResult.layoutParent = element.id; // todo -- multiple ignore levels?
            //             child.layoutBox.GetChildren(list);
            //             break;
            //
            //         default:
            //             list.Add(child.layoutBox);
            //             break;
            //     }
            // }

            // AwesomeLayoutBox ptr = firstChild;
            // while (ptr != null) {
            //     list.Add(ptr);
            //     ptr = ptr.nextSibling;
            // }
        }

        internal UIElement GetBlockWidthProvider() {
            if ((flags & LayoutBoxFlags.Ignored) != 0) {
                // LayoutResult parentResult = element.layoutResult.layoutParent;
                return elementSystem.instanceTable[layoutParentId.index]; //parentResult.element;
            }

            LayoutBox ptr = parent;

            while (ptr != null) {
                if (ptr.CanProvideHorizontalBlockSize(this, out float blockSize)) {
                    return ptr.element;
                }

                if ((ptr.flags & LayoutBoxFlags.WidthBlockProvider) != 0) {
                    return ptr.element;
                }

                ptr = ptr.parent;
            }

            return element.View.RootElement;
        }

        internal UIElement GetBlockHeightProvider() {
            if ((flags & LayoutBoxFlags.Ignored) != 0) {
                // LayoutResult parentResult = element.layoutResult.layoutParent;
                // return parentResult.element;
                return elementSystem.instanceTable[layoutParentId.index];
            }

            LayoutBox ptr = parent;

            while (ptr != null) {
                if (ptr.CanProvideVerticalBlockSize(this, out float blockSize)) {
                    return ptr.element;
                }

                if ((ptr.flags & LayoutBoxFlags.HeightBlockProvider) != 0) {
                    return ptr.element;
                }

                ptr = ptr.parent;
            }

            return element.View.RootElement;
        }

        public void Invalidate() {
            cachedBlockWidth = -1;
            cachedBlockHeight = -1;
            cachedContentWidth = -1;
            cachedContentHeight = -1;
            finalWidth = -1;
            finalHeight = -1;
            flags |= LayoutBoxFlags.RequireLayoutVertical | LayoutBoxFlags.RequireLayoutHorizontal;
        }

    }

    public struct LayoutSize {

        public float preferred;
        public float minimum;
        public float maximum;
        public float marginStart;
        public float marginEnd;

        public float Clamped {
            get {
                float f = preferred;
                if (preferred > maximum) {
                    f = maximum;
                }

                if (minimum > f) {
                    f = minimum;
                }

                return f;
            }
        }

        public float ClampedWithMargin {
            get => Clamped + marginStart + marginEnd;
        }

    }

}