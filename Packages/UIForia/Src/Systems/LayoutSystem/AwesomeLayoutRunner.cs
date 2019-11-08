using System;
using System.Diagnostics;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine.Assertions;

namespace UIForia.Systems {

    public class AwesomeLayoutRunner {

        private int frameId;
        private readonly UIElement rootElement;
        internal LightList<UIElement> hierarchyRebuildList;
        internal LightList<AwesomeLayoutBox> ignoredList;

        public AwesomeLayoutRunner(UIElement rootElement) {
            this.rootElement = rootElement;
            rootElement.awesomeLayoutBox = new AwesomeRootLayoutBox();
            rootElement.awesomeLayoutBox.Initialize(rootElement, 0);
            this.hierarchyRebuildList = new LightList<UIElement>();
            this.ignoredList = new LightList<AwesomeLayoutBox>();
        }

        public void RunLayout() {
            GatherLayoutData();
            RunHorizontalLayout();
            frameId++;
        }

        public void RunHorizontalLayout() {
            LightStack<AwesomeLayoutBox> stack = LightStack<AwesomeLayoutBox>.Get();
            stack.Push(rootElement.awesomeLayoutBox);

            while (stack.size > 0) {
                AwesomeLayoutBox layoutBox = stack.array[--stack.size];

                if ((layoutBox.element.flags & UIElementFlags.DebugLayout) != 0 || (layoutBox.flags & AwesomeLayoutBoxFlags.RequireLayoutHorizontal) != 0) {
                    if ((layoutBox.element.flags & UIElementFlags.DebugLayout) != 0) {
                        Debugger.Break();
                    }

                    layoutBox.RunLayoutHorizontal(frameId);
                    layoutBox.element.layoutHistory.AddLayoutHorizontalCall(frameId);
                    layoutBox.flags &= ~AwesomeLayoutBoxFlags.RequireLayoutHorizontal;
                }

                stack.EnsureAdditionalCapacity(layoutBox.childCount);

                AwesomeLayoutBox ptr = layoutBox.firstChild;
                while (ptr != null) {
                    stack.array[stack.size++] = ptr;
                    ptr = ptr.nextSibling;
                }
            }

            for (int i = 0; i < ignoredList.size; i++) {
                // do the same as above
            }

            stack.Release();
        }

        public void RunVerticalLayout() { }

        public void GatherLayoutData() {
            hierarchyRebuildList.Clear();

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

                    if ((flags & UIElementFlags.LayoutAlignmentWidthDirty) != 0) {
                        // if element has identity align remove from toAlignHorizontal list
                        // otherwise add if not already member
                        flags &= ~UIElementFlags.LayoutAlignmentWidthDirty;
                    }

                    if ((flags & UIElementFlags.LayoutAlignmentHeightDirty) != 0) {
                        // if element has identity align remove from toAlignHorizontal list
                        // otherwise add if not already member
                        flags &= ~UIElementFlags.LayoutAlignmentHeightDirty;
                    }

                    if ((flags & UIElementFlags.LayoutTransformDirty) != 0) {
                        // if element has identity transform remove from toTransform list
                        // otherwise add if not already member
                        flags &= ~UIElementFlags.LayoutTransformDirty;
                    }
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

            RebuildHierarchy(frameId, hierarchyRebuildList, null);

            // we sort so that parents are done before their children
            ignoredList.Sort((a, b) => a.element.depthTraversalIndex - b.element.depthTraversalIndex);
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
                if (currentElement.awesomeLayoutBox.layoutBoxType == layoutType) {
                    return;
                }

                currentElement.awesomeLayoutBox.Destroy();
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

                MarkContentParentsHorizontalDirty(element);
                MarkContentParentsVerticalDirty(element);

                elementBox.SetChildren(childList);
                element.flags &= ~UIElementFlags.LayoutHierarchyDirty;
                childList.size = 0;
            }

            LightList<AwesomeLayoutBox>.Release(ref childList);
        }

        private void RunLayoutHorizontal() {
            LightStack<AwesomeLayoutBox> stack = LightStack<AwesomeLayoutBox>.Get();

            AwesomeLayoutBox rootBox = new AwesomeFlexLayoutBox();

            stack.array[stack.size++] = rootBox;

            while (stack.size > 0) { }

            LightStack<AwesomeLayoutBox>.Release(ref stack);
        }

    }

}