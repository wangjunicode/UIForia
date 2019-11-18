using System;
using System.Collections.Generic;
using System.Diagnostics;
using SVGX;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = System.Diagnostics.Debug;

namespace UIForia.Systems {

    public struct AxisAlignedBounds {

        public float xMin;
        public float yMin;
        public float xMax;
        public float yMax;

    }

    public struct OrientedBounds {

        public Vector2 p0;
        public Vector2 p1;
        public Vector2 p2;
        public Vector2 p3;

    }

    // this is crazy, but by using a struct as our array type, even though it just contains a reference, 
    // causes a massive performance increase. The reason is we avoid mono doing `Object.virt_stelemref_class_small_idepth`,
    // which is a complete undocumented part of mono that runs when you assign a reference type to an array slot.
    // By having a struct type on the array (even though its just a wrapper around our reference type),
    // we can increase the performance of array usage DRAMATICALLY. Example: Converting from using LightStack<UIElement>
    // to StructStack<ElemRef> improved performance of layout by over 300%!!!!!!!!!

    // source code reference is in the mono project: mono/mono/metadata/marshal.c
    // there is even a comment there that says: 
    // "Arrays are sealed but are covariant on their element type, We can't use any of the fast paths."
    // this means that arrays of reference types are always way slower than arrays of values type because of 
    // polymorphism and interfaces. Anyway, with this optimization and some other small changes
    // we brought the no-op layout run time of around 4000 elements from ~2.5ms down to ~0.2ms with deep profiling on

    // the bonus here is that this probably incurs no performance overhead with il2cpp because the struct is the same
    // size as the pointer so dereferencing it should yield identical performance (I haven't verified this though)

    // as far as I can tell, reading from ref typed arrays do not have this stelemref overhead, only writing.
    internal struct ElemRef {

        public UIElement element;

        public ElemRef(UIElement element) {
            this.element = element;
        }

    }

    internal struct BoxRef {

        public AwesomeLayoutBox box;

        public BoxRef(AwesomeLayoutBox box) {
            this.box = box;
        }

    }

    public class AwesomeLayoutRunner {

        private int frameId;
        internal readonly UIElement rootElement;
        internal LightList<UIElement> hierarchyRebuildList;
        internal LightList<UIElement> alignHorizontalList;
        internal LightList<UIElement> alignVerticalList;
        internal LightList<UIElement> transformList;
        internal LightList<AwesomeLayoutBox> ignoredList;
        internal LightList<UIElement> enabledElements;
        internal StructStack<ElemRef> elemRefStack;
        internal StructStack<BoxRef> boxRefStack;
        internal LightList<UIElement> matrixUpdateList;

        public AwesomeLayoutRunner(UIElement rootElement) {
            this.rootElement = rootElement;
            this.rootElement.awesomeLayoutBox = new AwesomeRootLayoutBox();
            this.rootElement.awesomeLayoutBox.Initialize(rootElement, 0);
            this.hierarchyRebuildList = new LightList<UIElement>();
            this.ignoredList = new LightList<AwesomeLayoutBox>();
            this.alignHorizontalList = new LightList<UIElement>();
            this.alignVerticalList = new LightList<UIElement>();
            this.transformList = new LightList<UIElement>();
            this.enabledElements = new LightList<UIElement>(32);
            this.elemRefStack = new StructStack<ElemRef>(32);
            this.boxRefStack = new StructStack<BoxRef>(32);
            this.matrixUpdateList = new LightList<UIElement>();
        }

        public void RunLayout() {
            frameId = rootElement.Application.frameId;

            if (rootElement.isDisabled) {
                return;
            }

            hierarchyRebuildList.Clear();
            enabledElements.Clear();

            GatherLayoutData();
            RebuildHierarchy();
            PerformLayout();
            ApplyHorizontalAlignments();
            ApplyVerticalAlignments();
            ApplyTransforms();
            ApplyLayoutResults();
            ApplyBoxSizeChanges();
        }

        private void ApplyTransforms() {
            for (int i = 0; i < transformList.size; i++) {
                UIElement currentElement = transformList.array[i];
                LayoutResult result = currentElement.layoutResult;

                result.transformMatrix = SVGXMatrix.TRS(
                    currentElement.awesomeLayoutBox.transformX,
                    currentElement.awesomeLayoutBox.transformY,
                    currentElement.style.TransformRotation,
                    currentElement.style.TransformScaleX,
                    currentElement.style.TransformScaleY
                );
            }

            transformList.Clear();
        }

        public void GatherLayoutData() {
            elemRefStack.array[elemRefStack.size++].element = rootElement;

            while (elemRefStack.size > 0) {
                UIElement currentElement = elemRefStack.array[--elemRefStack.size].element;

                UIElementFlags flags = currentElement.flags;

                bool enabled = (flags & UIElementFlags.AliveEnabledAncestorEnabled) == (UIElementFlags.AliveEnabledAncestorEnabled);

                // if the element was just enabled or disabled we need to make sure the parent rebuilds it's hierarchy
                if (currentElement.enableStateChangedFrameId == frameId) {
                    if (enabled) {
                        flags |= UIElementFlags.LayoutHierarchyDirty;
                        hierarchyRebuildList.Add(currentElement);
                    }

                    // because we operate depth first the parent has already been processed
                    // if it wasn't already marked to be the parent isn't already marked to rebuild it's hierarchy by itself or
                    // a previous sibling of currentElement, mark it as such and add it to the rebuild list
                    if (currentElement.parent != null && (currentElement.parent.flags & UIElementFlags.LayoutHierarchyDirty) == 0) {
                        currentElement.parent.flags |= UIElementFlags.LayoutHierarchyDirty;
                        hierarchyRebuildList.Add(currentElement.parent);
                    }
                }

                if (!enabled) {
                    currentElement.flags = flags; // might have changed above
                    continue;
                }

                if ((flags & UIElementFlags.LayoutFlags) != 0) {
                    if ((flags & UIElementFlags.LayoutTypeOrBehaviorDirty) != 0) {
                        UpdateLayoutTypeOrBehavior(currentElement);
                        flags &= ~UIElementFlags.LayoutTypeOrBehaviorDirty;

                        if ((flags & UIElementFlags.LayoutHierarchyDirty) == 0) {
                            flags |= UIElementFlags.LayoutHierarchyDirty;
                            hierarchyRebuildList.Add(currentElement);
                        }
                    }

                    // if padding or border is dirty, we need a layout but our size didn't change so our parent doesn't need to layout even if it is content based
                    if ((flags & UIElementFlags.LayoutBorderPaddingHorizontalDirty) != 0) {
                        currentElement.awesomeLayoutBox.flags |= AwesomeLayoutBoxFlags.RequireLayoutHorizontal;
                        currentElement.layoutHistory.AddLogEntry(LayoutDirection.Horizontal, frameId, LayoutReason.BorderPaddingChanged);
                        flags &= ~UIElementFlags.LayoutBorderPaddingHorizontalDirty;
                    }

                    // if padding or border is dirty, we need a layout but our size didn't change so our parent doesn't need to layout even if it is content based
                    if ((flags & UIElementFlags.LayoutBorderPaddingVerticalDirty) != 0) {
                        currentElement.awesomeLayoutBox.flags |= AwesomeLayoutBoxFlags.RequireLayoutVertical;
                        currentElement.layoutHistory.AddLogEntry(LayoutDirection.Vertical, frameId, LayoutReason.BorderPaddingChanged);
                        flags &= ~UIElementFlags.LayoutBorderPaddingVerticalDirty;
                    }

                    if ((flags & UIElementFlags.LayoutSizeWidthDirty) != 0) {
                        currentElement.awesomeLayoutBox.flags |= AwesomeLayoutBoxFlags.RequireLayoutHorizontal;
                        currentElement.layoutHistory.AddLogEntry(LayoutDirection.Horizontal, frameId, LayoutReason.StyleSizeChanged);
                        flags &= ~UIElementFlags.LayoutSizeWidthDirty;

                        MarkContentParentsHorizontalDirty(currentElement);
                    }

                    if ((flags & UIElementFlags.LayoutSizeHeightDirty) != 0) {
                        currentElement.awesomeLayoutBox.flags |= AwesomeLayoutBoxFlags.RequireLayoutVertical;
                        currentElement.layoutHistory.AddLogEntry(LayoutDirection.Vertical, frameId, LayoutReason.StyleSizeChanged);
                        flags &= ~UIElementFlags.LayoutSizeHeightDirty;

                        MarkContentParentsVerticalDirty(currentElement);
                    }

                    if ((flags & UIElementFlags.LayoutTransformDirty) != 0) {
                        // if element has identity transform remove from toTransform list
                        // otherwise add if not already member
                        transformList.Add(currentElement); // mark to get transform recomputed
                        // mark to update matrix, don't actually add it to the list, that will happen later
                        // because many things can cause this flag to be set, be sure we only add it to list once.
                        currentElement.awesomeLayoutBox.flags |= AwesomeLayoutBoxFlags.RequiresMatrixUpdate;
                        flags &= ~UIElementFlags.LayoutTransformDirty;
                    }
                }

                if ((currentElement.awesomeLayoutBox.flags & AwesomeLayoutBoxFlags.RequireAlignmentHorizontal) != 0) {
                    alignHorizontalList.Add(currentElement);
                }

                if ((currentElement.awesomeLayoutBox.flags & AwesomeLayoutBoxFlags.RequireAlignmentVertical) != 0) {
                    alignVerticalList.Add(currentElement);
                }

                currentElement.flags = flags; // write changes back to element

                UIElement[] childArray = currentElement.children.array;
                int childCount = currentElement.children.size;

                if (elemRefStack.size + childCount > elemRefStack.array.Length) {
                    elemRefStack.EnsureAdditionalCapacity(childCount);
                }

                for (int i = childCount - 1; i >= 0; i--) {
                    elemRefStack.array[elemRefStack.size++].element = childArray[i];
                }
            }

            // we sort so that parents are done before their children, may not be needed though
            // ignoredList.Sort((a, b) => a.element.depthTraversalIndex - b.element.depthTraversalIndex);
        }

        private void ApplyHorizontalAlignments() {
            float viewportWidth = rootElement.View.Viewport.width;
            float viewportHeight = rootElement.View.Viewport.height;
            UIView view = rootElement.View;

            for (int i = 0; i < alignHorizontalList.size; i++) {
                UIElement element = alignHorizontalList.array[i];
                AwesomeLayoutBox box = element.awesomeLayoutBox;
                LayoutResult result = element.layoutResult;

                // todo -- cache these values on layout box or make style reads fast
                OffsetMeasurement originX = box.element.style.AlignmentOriginX;
                OffsetMeasurement offsetX = box.element.style.AlignmentOffsetX;
                AlignmentDirection direction = box.element.style.AlignmentDirectionX;
                AlignmentBehavior alignmentTargetX = element.style.AlignmentBehaviorX;

                float originBase = MeasurementUtil.ResolveOriginBaseX(result, view.position.x, alignmentTargetX, direction);
                float originSize = MeasurementUtil.ResolveOffsetOriginSizeX(result, viewportWidth, alignmentTargetX);
                float originOffset = MeasurementUtil.ResolveOffsetMeasurement(element, viewportWidth, viewportHeight, originX, originSize);
                float offset = MeasurementUtil.ResolveOffsetMeasurement(element, viewportWidth, viewportHeight, offsetX, box.finalWidth);

                float previousPosition = result.alignedPosition.x;

                if (direction == AlignmentDirection.End) {
                    result.alignedPosition.x = (originBase + originSize) - (originOffset + offset) - box.finalWidth;
                }
                else {
                    result.alignedPosition.x = originBase + originOffset + offset;
                }

                if (!Mathf.Approximately(previousPosition, result.alignedPosition.x)) {
                    if ((box.flags & AwesomeLayoutBoxFlags.RequiresMatrixUpdate) != 0) {
                        box.flags |= AwesomeLayoutBoxFlags.RequiresMatrixUpdate;
                        matrixUpdateList.Add(box.element);
                    }
                }
            }

            alignHorizontalList.Clear();
        }

        private void ApplyVerticalAlignments() {
            float viewportWidth = rootElement.View.Viewport.width;
            float viewportHeight = rootElement.View.Viewport.height;
            UIView view = rootElement.View;

            for (int i = 0; i < alignVerticalList.size; i++) {
                UIElement element = alignVerticalList.array[i];
                AwesomeLayoutBox box = element.awesomeLayoutBox;
                LayoutResult result = element.layoutResult;

                // todo -- cache these values on layout box or make style reads fast
                OffsetMeasurement originY = box.element.style.AlignmentOriginY;
                OffsetMeasurement offsetY = box.element.style.AlignmentOffsetY;
                AlignmentDirection direction = box.element.style.AlignmentDirectionY;
                AlignmentBehavior alignmentTargetY = element.style.AlignmentBehaviorY;

                float originBase = MeasurementUtil.ResolveOriginBaseY(result, view.position.y, alignmentTargetY, direction);
                float originSize = MeasurementUtil.ResolveOffsetOriginSizeY(result, viewportWidth, alignmentTargetY);
                float originOffset = MeasurementUtil.ResolveOffsetMeasurement(element, viewportWidth, viewportHeight, originY, originSize);
                float offset = MeasurementUtil.ResolveOffsetMeasurement(element, viewportWidth, viewportHeight, offsetY, box.finalHeight);

                float previousPosition = result.alignedPosition.y;

                if (direction == AlignmentDirection.End) {
                    result.alignedPosition.y = (originBase + originSize) - (originOffset + offset) - box.finalHeight;
                }
                else {
                    result.alignedPosition.y = originBase + originOffset + offset;
                }

                if (!Mathf.Approximately(previousPosition, result.alignedPosition.y)) {
                    if ((box.flags & AwesomeLayoutBoxFlags.RequiresMatrixUpdate) != 0) {
                        box.flags |= AwesomeLayoutBoxFlags.RequiresMatrixUpdate;
                        matrixUpdateList.Add(box.element);
                    }
                }
            }

            alignVerticalList.Clear();
        }

        private void PerformLayoutStep(AwesomeLayoutBox rootBox) {
            boxRefStack.Push(new BoxRef() {box = rootBox});

            float viewWidth = rootBox.element.View.Viewport.width;
            float viewHeight = rootBox.element.View.Viewport.height;

            while (boxRefStack.size > 0) {
                AwesomeLayoutBox layoutBox = boxRefStack.array[--boxRefStack.size].box;
#if DEBUG
                bool needsDebugger = (layoutBox.element.flags & UIElementFlags.DebugLayout) != 0;

                if (needsDebugger) {
                    Debugger.Break();
                }

                if (needsDebugger || (layoutBox.flags & AwesomeLayoutBoxFlags.RequireLayoutHorizontal) != 0) {
                    layoutBox.RunLayoutHorizontal(frameId);
                    layoutBox.element.layoutHistory.AddLayoutHorizontalCall(frameId);
                    layoutBox.flags &= ~AwesomeLayoutBoxFlags.RequireLayoutHorizontal;
                }

                if (needsDebugger || (layoutBox.flags & AwesomeLayoutBoxFlags.RequireLayoutVertical) != 0) {
                    layoutBox.RunLayoutVertical(frameId);
                    layoutBox.element.layoutHistory.AddLayoutVerticalCall(frameId);
                    layoutBox.flags &= ~AwesomeLayoutBoxFlags.RequireLayoutVertical;
                }
#else
                if ((layoutBox.flags & AwesomeLayoutBoxFlags.RequireLayoutHorizontal) != 0) {
                    layoutBox.RunLayoutHorizontal(frameId);
                    layoutBox.flags &= ~AwesomeLayoutBoxFlags.RequireLayoutHorizontal;
                }

                if ((layoutBox.flags & AwesomeLayoutBoxFlags.RequireLayoutVertical) != 0) {
                    layoutBox.RunLayoutVertical(frameId);
                    layoutBox.flags &= ~AwesomeLayoutBoxFlags.RequireLayoutVertical;
                }
#endif

                if ((layoutBox.element.flags & UIElementFlags.LayoutTransformNotIdentity) != 0) {
                    float x = MeasurementUtil.ResolveOffsetMeasurement(layoutBox.element, viewWidth, viewHeight, layoutBox.element.style.TransformPositionX, layoutBox.finalWidth);
                    float y = MeasurementUtil.ResolveOffsetMeasurement(layoutBox.element, viewWidth, viewHeight, layoutBox.element.style.TransformPositionY, layoutBox.finalHeight);
                    if (!Mathf.Approximately(x, layoutBox.transformX) || !Mathf.Approximately(y, layoutBox.transformY)) {
                        layoutBox.transformX = x;
                        layoutBox.transformY = y;
                        layoutBox.flags |= AwesomeLayoutBoxFlags.RequiresMatrixUpdate;
                    }
                }

                // if we need to update this element's matrix then add to the list
                // this can happen if the parent assigned a different position to 
                // this element (and the element doesn't have an alignment override)
                if ((layoutBox.flags & AwesomeLayoutBoxFlags.RequiresMatrixUpdate) != 0) {
                    matrixUpdateList.Add(layoutBox.element);
                }

                if (boxRefStack.size + layoutBox.childCount > boxRefStack.array.Length) {
                    boxRefStack.EnsureAdditionalCapacity(layoutBox.childCount);
                }

                AwesomeLayoutBox ptr = layoutBox.firstChild;
                while (ptr != null) {
                    boxRefStack.array[boxRefStack.size++].box = ptr;
                    ptr = ptr.nextSibling;
                }
            }
        }

        private void PerformLayout() {
            PerformLayoutStep(rootElement.awesomeLayoutBox);

            for (int i = 0; i < ignoredList.size; i++) {
                AwesomeLayoutBox ignoredBox = ignoredList.array[i];
                AwesomeLayoutBox.LayoutSize size = default;
                // todo -- account for margin on ignored element

                ignoredBox.GetWidths(ref size);
                float outputSize = size.Clamped;
                ignoredBox.ApplyLayoutHorizontal(0, 0, outputSize, ignoredBox.parent?.finalWidth ?? outputSize, LayoutFit.None, frameId);

                ignoredBox.GetHeights(ref size);
                outputSize = size.Clamped;
                ignoredBox.ApplyLayoutVertical(0, 0, outputSize, ignoredBox.parent?.finalHeight ?? outputSize, LayoutFit.None, frameId);

                PerformLayoutStep(ignoredBox);
            }
        }

        private void ApplyLayoutResults() {
            int size = matrixUpdateList.size;
            UIElement[] array = matrixUpdateList.array;

            for (int i = 0; i < size; i++) {
                UIElement startElement = array[i];
                AwesomeLayoutBox box = startElement.awesomeLayoutBox;

                if ((box.flags & AwesomeLayoutBoxFlags.RequiresMatrixUpdate) == 0) {
                    continue;
                }

                elemRefStack.Push(new ElemRef(startElement));

                while (elemRefStack.size > 0) {
                    UIElement currentElement = elemRefStack.array[--elemRefStack.size].element;
                    AwesomeLayoutBox currentBox = currentElement.awesomeLayoutBox;
                    LayoutResult result = currentElement.layoutResult;
                    currentBox.flags &= ~AwesomeLayoutBoxFlags.RequiresMatrixUpdate;

                    result.localPosition.x = result.alignedPosition.x;
                    result.localPosition.y = result.alignedPosition.y;

                    ref SVGXMatrix localMatrix = ref result.localMatrix;

                    float px = MeasurementUtil.ResolveFixedSize(result.actualSize.width, 0, 0, 0, currentElement.style.TransformPivotX);
                    float py = MeasurementUtil.ResolveFixedSize(result.actualSize.height, 0, 0, 0, currentElement.style.TransformPivotY);

                    float rotation = currentElement.style.TransformRotation * Mathf.Deg2Rad;
                    float ca = Mathf.Cos(rotation);
                    float sa = Mathf.Sin(rotation);
                    float scaleX = currentElement.style.TransformScaleX;
                    float scaleY = currentElement.style.TransformScaleY;

                    localMatrix.m0 = ca * scaleX;
                    localMatrix.m1 = sa * scaleX;
                    localMatrix.m2 = -sa * scaleY;
                    localMatrix.m3 = ca * scaleY;
                    localMatrix.m4 = result.alignedPosition.x;
                    localMatrix.m5 = result.alignedPosition.y;

                    localMatrix = SVGXMatrix.TRS(result.alignedPosition.x, result.alignedPosition.y, rotation, scaleX, scaleY);
                    
                    if (px != 0 || py != 0) {
                        SVGXMatrix pivot = new SVGXMatrix(1, 0, 0, 1, px, py);
                        SVGXMatrix pivotBack = pivot;
                        pivotBack.m4 = -px;
                        pivotBack.m5 = -py;
                        localMatrix = pivot * localMatrix * pivotBack;
                    }
                       
                    if (currentElement.parent != null) {
                        // result.matrix = element.parent.layoutResult.matrix * localMatrix;
                        ref SVGXMatrix m = ref result.matrix;
                        ref SVGXMatrix left = ref currentElement.parent.layoutResult.matrix;
                        ref SVGXMatrix right = ref localMatrix;
                        m.m0 = left.m0 * right.m0 + left.m2 * right.m1;
                        m.m1 = left.m1 * right.m0 + left.m3 * right.m1;
                        m.m2 = left.m0 * right.m2 + left.m2 * right.m3;
                        m.m3 = left.m1 * right.m2 + left.m3 * right.m3;
                        m.m4 = left.m0 * right.m4 + left.m2 * right.m5 + left.m4;
                        m.m5 = left.m1 * right.m4 + left.m3 * right.m5 + left.m5;
                    }
                    else {
                        result.matrix = localMatrix;
                    }

                    result.screenPosition.x = result.matrix.m4;
                    result.screenPosition.y = result.matrix.m5;

                    int childCount = currentElement.children.size;
                    if (elemRefStack.size + childCount > elemRefStack.array.Length) {
                        elemRefStack.EnsureAdditionalCapacity(childCount);
                    }

                    for (int childIdx = 0; childIdx < childCount; childIdx++) {
                        UIElement child = currentElement.children.array[childIdx];
                        bool enabled = (child.flags & UIElementFlags.AliveEnabledAncestorEnabled) == (UIElementFlags.AliveEnabledAncestorEnabled);
                        if (enabled) {
                            elemRefStack.array[elemRefStack.size++].element = child;
                        }
                    }
                }
            }

            matrixUpdateList.Clear();
        }

        private void ApplyBoxSizeChanges() {
            // for anything that had a matrix update or a size change we need to recompute bounding boxes

            elemRefStack.array[elemRefStack.size++].element = rootElement;

            while (elemRefStack.size > 0) {
                UIElement currentElement = elemRefStack.array[--elemRefStack.size].element;
                LayoutResult result = currentElement.layoutResult;

                const float x = 0;
                const float y = 0;

                float width = result.actualSize.width;
                float height = result.actualSize.height;

                OrientedBounds orientedBounds = default;
                ref SVGXMatrix m = ref result.matrix;

                // inlined svgxMatrix.Transform(point), takes runtime for ~4000 elements from 4.5ms to 0.47ms w/ deep profile on
                orientedBounds.p0.x = m.m0 * x + m.m2 * y + m.m4;
                orientedBounds.p0.y = m.m1 * x + m.m3 * y + m.m5;

                orientedBounds.p1.x = m.m0 * width + m.m2 * y + m.m4;
                orientedBounds.p1.y = m.m1 * width + m.m3 * y + m.m5;

                orientedBounds.p2.x = m.m0 * width + m.m2 * height + m.m4;
                orientedBounds.p2.y = m.m1 * width + m.m3 * height + m.m5;

                orientedBounds.p3.x = m.m0 * x + m.m2 * height + m.m4;
                orientedBounds.p3.y = m.m1 * x + m.m3 * height + m.m5;

                result.orientedBounds = orientedBounds;

                int childCount = currentElement.children.size;
                if (elemRefStack.size + childCount > elemRefStack.array.Length) {
                    elemRefStack.EnsureAdditionalCapacity(childCount);
                }

                for (int childIdx = 0; childIdx < childCount; childIdx++) {
                    UIElement child = currentElement.children.array[childIdx];
                    bool enabled = (child.flags & UIElementFlags.AliveEnabledAncestorEnabled) == (UIElementFlags.AliveEnabledAncestorEnabled);
                    if (enabled) {
                        elemRefStack.array[elemRefStack.size++].element = child;
                    }
                }
            }
        }

        private void MarkContentParentsHorizontalDirty(UIElement currentElement) {
            AwesomeLayoutBox ptr = currentElement.awesomeLayoutBox.parent;

            while (ptr != null) {
                // once we hit a block provider we can safely stop traversing since the provider doesn't care about content size changing
                if ((ptr.flags & AwesomeLayoutBoxFlags.WidthBlockProvider) != 0) {
                    break;
                }

                // can't break out if already flagged for layout because parent of parent might not be and might be content sized
                ptr.flags |= AwesomeLayoutBoxFlags.RequireLayoutHorizontal;
                ptr.element.layoutHistory.AddLogEntry(LayoutDirection.Horizontal, frameId, LayoutReason.DescendentStyleSizeChanged);
                ptr = ptr.parent;
            }
        }

        private void MarkContentParentsVerticalDirty(UIElement currentElement) {
            AwesomeLayoutBox ptr = currentElement.awesomeLayoutBox.parent;

            while (ptr != null) {
                // once we hit a block provider we can safely stop traversing since the provider doesn't care about content size changing
                if ((ptr.flags & AwesomeLayoutBoxFlags.HeightBlockProvider) != 0) {
                    break;
                }

                // can't break out if already flagged for layout because parent of parent might not be and might be content sized
                ptr.flags |= AwesomeLayoutBoxFlags.RequireLayoutVertical;
                ptr.element.layoutHistory.AddLogEntry(LayoutDirection.Vertical, frameId, LayoutReason.DescendentStyleSizeChanged);
                ptr = ptr.parent;
            }
        }

        private void UpdateLayoutTypeOrBehavior(UIElement currentElement) {
            if (currentElement == rootElement) return;
            // if had box and was ignored
            // if behavior is no longer ignored
            // we need to remove the box from the ignored list

            // if became transcluded -> give it a transcluded box
            LayoutBehavior layoutBehavior = currentElement.style.LayoutBehavior;

            // we don't have a layout box yet, create it here
            if (currentElement.awesomeLayoutBox == null) {
                // if we are transcluded, create a transcluded box and exit 
                if (layoutBehavior == LayoutBehavior.TranscludeChildren) {
                    currentElement.awesomeLayoutBox = new AwesomeTranscludedLayoutBox();
                    currentElement.awesomeLayoutBox.Initialize(currentElement, frameId);
                    return;
                }

                // otherwise create the normal box
                UpdateLayoutBoxType(currentElement);
            }
            else {
                // if we have a layout box and it is currently ignored, if the behavior changed to something that isn't ignored,
                // we need to remove that box from the ignored list
                bool isIgnored = (currentElement.awesomeLayoutBox.flags & AwesomeLayoutBoxFlags.Ignored) == 0;
                if (isIgnored && layoutBehavior != LayoutBehavior.Ignored) {
                    ignoredList.Remove(currentElement.awesomeLayoutBox);
                }

                // if we already have a layout box we need to check for change in type and in behavior
                // if behavior is transcluded, check if box was previously transcluded
                if (layoutBehavior == LayoutBehavior.TranscludeChildren) {
                    if (!(currentElement.awesomeLayoutBox is AwesomeTranscludedLayoutBox)) {
                        // if not already transcluded, destroy the old box and create the new one
                        currentElement.awesomeLayoutBox.Destroy();
                        currentElement.awesomeLayoutBox = new AwesomeTranscludedLayoutBox();
                        currentElement.awesomeLayoutBox.Initialize(currentElement, frameId);
                        return;
                    }
                }

                UpdateLayoutBoxType(currentElement);
            }

            Debug.Assert(currentElement.awesomeLayoutBox != null, "currentElement.awesomeLayoutBox != null");

            // if we have a layout box and it has no ignored flag, that is is not in the list
            // if the behavior is ignored add it to the ignored list
            if (layoutBehavior == LayoutBehavior.Ignored && (currentElement.awesomeLayoutBox.flags & AwesomeLayoutBoxFlags.Ignored) == 0) {
                currentElement.awesomeLayoutBox.flags |= AwesomeLayoutBoxFlags.Ignored;
                currentElement.awesomeLayoutBox.parent = currentElement.parent.awesomeLayoutBox;
                ignoredList.Add(currentElement.awesomeLayoutBox);
            }
        }

        private void UpdateLayoutBoxType(UIElement currentElement) {
            LayoutType layoutType = currentElement.style.LayoutType;

            if (currentElement.awesomeLayoutBox != null) {
                if (currentElement.awesomeLayoutBox is AwesomeTextLayoutBox) {
                    return;
                }

                if (currentElement.awesomeLayoutBox.layoutBoxType == layoutType) {
                    return;
                }

                currentElement.awesomeLayoutBox.Destroy();
            }

            if (currentElement is UITextElement) {
                currentElement.awesomeLayoutBox = new AwesomeTextLayoutBox();
                currentElement.awesomeLayoutBox.Initialize(currentElement, frameId);
                return;
            }

            switch (layoutType) {
                case LayoutType.Unset:
                case LayoutType.Flex:
                    currentElement.awesomeLayoutBox = new AwesomeFlexLayoutBox();
                    currentElement.awesomeLayoutBox.Initialize(currentElement, frameId);
                    break;
                case LayoutType.Grid:
                    currentElement.awesomeLayoutBox = new AwesomeGridLayoutBox();
                    currentElement.awesomeLayoutBox.Initialize(currentElement, frameId);
                    break;
                case LayoutType.Radial:
                    throw new NotImplementedException();
                case LayoutType.Stack:
                    currentElement.awesomeLayoutBox = new AwesomeStackLayoutBox();
                    currentElement.awesomeLayoutBox.Initialize(currentElement, frameId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RebuildHierarchy() {
            // do this back to front so parent always works with final children
            LightList<AwesomeLayoutBox> childList = LightList<AwesomeLayoutBox>.Get();

            // input list is already in depth traversal order, so iterating backwards will effectively walk up the leaves
            for (int i = hierarchyRebuildList.size - 1; i >= 0; i--) {
                UIElement element = hierarchyRebuildList.array[i];
                Assert.IsTrue(element.isEnabled);
                AwesomeLayoutBox elementBox = element.awesomeLayoutBox;

                LightList<UIElement> elementChildList = element.children;

                for (int j = 0; j < elementChildList.size; j++) {
                    UIElement child = elementChildList.array[j];
                    if (!child.isEnabled) {
                        continue;
                    }

                    switch (child.style.LayoutBehavior) { // todo -- flag on box instead would be faster
                        case LayoutBehavior.Unset:
                        case LayoutBehavior.Normal:
                            childList.Add(child.awesomeLayoutBox);
                            break;
                        case LayoutBehavior.Ignored:
                            child.layoutResult.layoutParent = element.layoutResult; // not 100% sure of this
                            ignoredList.Add(child.awesomeLayoutBox);
                            break;
                        case LayoutBehavior.TranscludeChildren:
                            childList.AddRange(((AwesomeTranscludedLayoutBox) child.awesomeLayoutBox).GetChildren());
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                element.layoutHistory.AddLogEntry(LayoutDirection.Horizontal, frameId, LayoutReason.HierarchyChanged, string.Empty);
                element.layoutHistory.AddLogEntry(LayoutDirection.Vertical, frameId, LayoutReason.HierarchyChanged, string.Empty);

                elementBox.flags |= (AwesomeLayoutBoxFlags.RequireLayoutHorizontal | AwesomeLayoutBoxFlags.RequireLayoutVertical);

                element.awesomeLayoutBox.MarkContentParentsHorizontalDirty(frameId, LayoutReason.DescendentStyleSizeChanged);
                element.awesomeLayoutBox.MarkContentParentsVerticalDirty(frameId, LayoutReason.DescendentStyleSizeChanged);

                elementBox.SetChildren(childList);
                element.flags &= ~UIElementFlags.LayoutHierarchyDirty;
                childList.size = 0;
            }

            LightList<AwesomeLayoutBox>.Release(ref childList);
        }

        // this is a really good candidate for doing in parallel with rendering since this data is not required while rendering
        public void QueryPoint(Vector2 point, IList<UIElement> retn) {
            for (int i = 0; i < enabledElements.size; i++) {
                UIElement element = enabledElements.array[i];

                // if offscreen or clipped or invisible continue
                // how do i know if the thing is clipped or not?
                // only real way is via render + read back, but thats nasty
                // cant use the render box result can I?
                // divide into large and small?
                // if area is big then just check it
                // if area is small partition it into buckets?

                // first step might be to find all elements who's aabb contains or overlaps point
                // then figure out which of those elements real geometry contains the point via polygon check
                // then figure out which of those elements are culled, remove those

                // traverse from the root

                // if element is culled and is clipper continue


                // when we encounter a clipper push it on our stack

                // visit children

                // pop clipper if needed

                if (element.layoutResult.matrix.IsTranslationOnly) { }
                else { }

                // if transform, alignment, or position changed, traverse and children dirty (will need to re-bucket these) 
            }
        }

    }

}