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

    public class AwesomeLayoutRunner {

        private int frameId;
        internal readonly UIElement rootElement;
        internal LightList<UIElement> hierarchyRebuildList;
        internal LightList<UIElement> alignHorizontalList;
        internal LightList<UIElement> alignVerticalList;
        internal LightList<UIElement> transformList;
        internal LightList<AwesomeLayoutBox> ignoredList;
        internal LightStack<AwesomeLayoutBox> layoutStack;
        internal LightList<UIElement> enabledElements;
        
        public AwesomeLayoutRunner(UIElement rootElement) {
            this.rootElement = rootElement;
            this.rootElement.awesomeLayoutBox = new AwesomeRootLayoutBox();
            this.rootElement.awesomeLayoutBox.Initialize(rootElement, 0);
            this.hierarchyRebuildList = new LightList<UIElement>();
            this.ignoredList = new LightList<AwesomeLayoutBox>();
            this.alignHorizontalList = new LightList<UIElement>();
            this.alignVerticalList = new LightList<UIElement>();
            this.transformList = new LightList<UIElement>();
            this.layoutStack = new LightStack<AwesomeLayoutBox>();
            this.enabledElements = new LightList<UIElement>(32);
        }

        public void RunLayout() {
            GatherLayoutData();
            RunHorizontalLayout();
            ApplyHorizontalAlignments();
            RunVerticalLayout();
            ApplyVerticalAlignments();
            ApplyTransforms();
            ApplyLayoutResults();
            frameId++;
        }

        private void ApplyTransforms() { }

        public void GatherLayoutData() {
            hierarchyRebuildList.Clear();
            enabledElements.Clear();
            
            if (rootElement.isDisabled) return;

            LightStack<UIElement> stack = LightStack<UIElement>.Get();

            stack.array[stack.size++] = rootElement;

            while (stack.size > 0) {
                UIElement currentElement = stack.array[--stack.size];

                UIElementFlags flags = currentElement.flags;

                // if the element was just enabled or disabled we need to make sure the parent rebuilds it's hierarchy
                if ((flags & (UIElementFlags.EnabledThisFrame | UIElementFlags.DisabledThisFrame)) != 0) {
                    if (currentElement.isEnabled) {
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

                if (currentElement.isDisabled) {
                    currentElement.flags = flags; // might have changed above
                    continue;
                }
                
                enabledElements.Add(currentElement);

                if ((flags & UIElementFlags.LayoutFlags) != 0) {
                    
                    if ((flags & (UIElementFlags.LayoutTypeDirty | UIElementFlags.LayoutBehaviorDirty)) != 0) {
                        UpdateLayoutTypeOrBehavior(currentElement);
                        flags &= ~(UIElementFlags.LayoutTypeDirty | UIElementFlags.LayoutBehaviorDirty);

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

                stack.EnsureAdditionalCapacity(childCount);

                for (int i = childCount - 1; i >= 0; i--) {
                    stack.array[stack.size++] = childArray[i];
                }
            }

            LightStack<UIElement>.Release(ref stack);

            RebuildHierarchy(frameId, hierarchyRebuildList, ignoredList);

            // we sort so that parents are done before their children, may not be needed though
            ignoredList.Sort((a, b) => a.element.depthTraversalIndex - b.element.depthTraversalIndex);
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

                if (direction == AlignmentDirection.End) {
                    result.alignedPosition.x = (originBase + originSize) - (originOffset + offset) - box.finalWidth;
                }
                else {
                    result.alignedPosition.x = originBase + originOffset + offset;
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

                if (direction == AlignmentDirection.End) {
                    result.alignedPosition.y = (originBase + originSize) - (originOffset + offset) - box.finalHeight;
                }
                else {
                    result.alignedPosition.y = originBase + originOffset + offset;
                }
            }

            alignVerticalList.Clear();
        }

        private void ApplyLayoutResults() {
            LightStack<UIElement> stack = LightStack<UIElement>.Get();
            stack.Push(rootElement);

            while (stack.size > 0) {
                UIElement element = stack.array[--stack.size];
                LayoutResult result = element.layoutResult;

                // wrong?
                result.localPosition.x = result.alignedPosition.x;
                result.localPosition.y = result.alignedPosition.y;

                ref SVGXMatrix localMatrix = ref result.localMatrix;

                Vector2 pivot = default;
                float ca = 1;
                float sa = 0;
                float scaleX = 1;
                float scaleY = 1;

                localMatrix.m0 = ca * scaleX;
                localMatrix.m1 = sa * scaleX;
                localMatrix.m2 = -sa * scaleY;
                localMatrix.m3 = ca * scaleY;
                localMatrix.m4 = result.alignedPosition.x; // does not account for transform (yet)
                localMatrix.m5 = result.alignedPosition.y;

                if (element.parent != null) {
                    result.matrix = element.parent.layoutResult.matrix * localMatrix;
                }
                else {
                    result.matrix = localMatrix;
                }

                result.screenPosition = result.matrix.position; // todo -- this should be world space pivot position? 

                for (int i = 0; i < element.children.size; i++) {
                    UIElement child = element.children.array[i];
                    if (child.isEnabled) {
                        stack.Push(child);
                    }
                }
            }

            stack.Release();
        }

        // pivot default = center
        // padding / border / size changed (any dimension) -> re-render
        // transform changed -> re-render
        // layoutResult.aabb.position;
        // want to know what is transformed
        // want to know what is overflowing
        // want to know what is aligned out of parent bounds
        // build dynamic quad tree (every frame?)
        // screen position = top left of transformed box.
        // need 4 corner points of each box to really handle rotation and picking, or top / bottom w/ stored rotation value
        // 3d? kind of not a part of layout which is 2d, 3d animations etc require a perspective camera and more data, seems like a rendering issue not layout
        // maybe whats that a view is: 3d element

        public void RunHorizontalLayout() {
            RunHorizontalLayout(rootElement.awesomeLayoutBox);

            for (int i = 0; i < ignoredList.size; i++) {
                // todo -- only need to update sizes if ignored parent size changed
                AwesomeLayoutBox box = ignoredList.array[i];
                AwesomeLayoutBox.LayoutSize size = default;
                box.GetWidths(ref size);
                // todo -- account for margin on ignored element
                float outputSize = Mathf.Max(size.minimum, Mathf.Min(size.preferred, size.maximum));
                box.ApplyLayoutHorizontal(0, 0, outputSize, outputSize, LayoutFit.None, frameId);
                RunHorizontalLayout(box);
            }
        }

        private void RunHorizontalLayout(AwesomeLayoutBox rootBox) {
            layoutStack.Push(rootBox);

            while (layoutStack.size > 0) {
                AwesomeLayoutBox layoutBox = layoutStack.array[--layoutStack.size];

                bool needsDebugger = (layoutBox.element.flags & UIElementFlags.DebugLayout) != 0;

                if (needsDebugger || (layoutBox.flags & AwesomeLayoutBoxFlags.RequireLayoutHorizontal) != 0) {
#if DEBUG
                    if (needsDebugger) {
                        Debugger.Break();
                    }
#endif
                    layoutBox.RunLayoutHorizontal(frameId);
                    layoutBox.element.layoutHistory.AddLayoutHorizontalCall(frameId);
                    layoutBox.flags &= ~AwesomeLayoutBoxFlags.RequireLayoutHorizontal;
                }

                layoutStack.EnsureAdditionalCapacity(layoutBox.childCount);

                AwesomeLayoutBox ptr = layoutBox.firstChild;
                while (ptr != null) {
                    layoutStack.array[layoutStack.size++] = ptr;
                    ptr = ptr.nextSibling;
                }
            }
        }

        public void RunVerticalLayout() {
            RunVerticalLayout(rootElement.awesomeLayoutBox);

            for (int i = 0; i < ignoredList.size; i++) {
                // todo -- only need to update sizes if ignored parent size changed
                AwesomeLayoutBox box = ignoredList.array[i];
                AwesomeLayoutBox.LayoutSize size = default;
                box.GetHeights(ref size);
                float outputSize = Mathf.Max(size.minimum, Mathf.Min(size.preferred, size.maximum));
                box.ApplyLayoutVertical(0, 0, outputSize, outputSize, LayoutFit.None, frameId);
                RunVerticalLayout(box);
            }
        }

        private void RunVerticalLayout(AwesomeLayoutBox rootBox) {
            layoutStack.Push(rootBox);

            while (layoutStack.size > 0) {
                AwesomeLayoutBox layoutBox = layoutStack.array[--layoutStack.size];

                bool needsDebugger = (layoutBox.element.flags & UIElementFlags.DebugLayout) != 0;

                if (needsDebugger || (layoutBox.flags & AwesomeLayoutBoxFlags.RequireLayoutVertical) != 0) {
#if DEBUG
                    if (needsDebugger) {
                        Debugger.Break();
                    }
#endif
                    layoutBox.RunLayoutVertical(frameId);
                    layoutBox.element.layoutHistory.AddLayoutHorizontalCall(frameId);
                    layoutBox.flags &= ~AwesomeLayoutBoxFlags.RequireLayoutVertical;
                }

                layoutStack.EnsureAdditionalCapacity(layoutBox.childCount);

                AwesomeLayoutBox ptr = layoutBox.firstChild;
                while (ptr != null) {
                    layoutStack.array[layoutStack.size++] = ptr;
                    ptr = ptr.nextSibling;
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
                    break;
                case LayoutType.Flow:
                    break;
                case LayoutType.Flex:
                    currentElement.awesomeLayoutBox = new AwesomeFlexLayoutBox();
                    currentElement.awesomeLayoutBox.Initialize(currentElement, frameId);
                    break;
                case LayoutType.Fixed:
                    break;
                case LayoutType.Grid:
                    currentElement.awesomeLayoutBox = new AwesomeGridLayoutBox();
                    currentElement.awesomeLayoutBox.Initialize(currentElement, frameId);
                    break;
                case LayoutType.Radial:
                    break;
                case LayoutType.Stack:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RebuildHierarchy(int frameId, LightList<UIElement> list, LightList<AwesomeLayoutBox> ignoredBoxes) {
            // do this back to front so parent always works with final children
            LightList<AwesomeLayoutBox> childList = LightList<AwesomeLayoutBox>.Get();

            // input list is already in depth traversal order, so iterating backwards will effectively walk up the leaves
            for (int i = list.size - 1; i >= 0; i--) {
                UIElement element = list.array[i];
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
                            ignoredBoxes.Add(child.awesomeLayoutBox);
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
                
                if (element.layoutResult.matrix.IsTranslationOnly) {
                    
                }
                else {
                    
                }
                
                // if transform, alignment, or position changed, traverse and children dirty (will need to re-bucket these) 
                
            }
        }

    }

}