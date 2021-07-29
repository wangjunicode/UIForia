using System;
using UIForia.Elements;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia.Layout {
    
    internal struct LayoutDebugBuffer : IDisposable {

        public DataList<SizeTypeFlags> horizontalFlags;
        public DataList<SizeTypeFlags> verticalFlags;
        public DataList<LayoutSizes> horizontalSizes;
        public DataList<LayoutSizes> verticalSizes;

        public LayoutDebugBuffer(int initialCapacity, Allocator allocator) {
            this.horizontalFlags = new DataList<SizeTypeFlags>(initialCapacity, allocator);
            this.verticalFlags = new DataList<SizeTypeFlags>(initialCapacity, allocator);
            this.horizontalSizes = new DataList<LayoutSizes>(initialCapacity, allocator);
            this.verticalSizes = new DataList<LayoutSizes>(initialCapacity, allocator);
        }

        public void Dispose() {
            horizontalFlags.Dispose();
            verticalFlags.Dispose();
            horizontalSizes.Dispose();
            verticalSizes.Dispose();
        }

    }

    internal unsafe struct LayoutDebug : IDisposable {

        public LayoutDebugBuffer initialBuffer;
        public LayoutDebugBuffer bottomUpFirstBuffer;
        public LayoutDebugBuffer topDownFirstBuffer;
        public LayoutDebugBuffer bottomUpSecondBuffer;
        public LayoutDebugBuffer topDownSecondBuffer;
        private bool hasSecondaryPasses;

        public static LayoutDebug Create() {
            return new LayoutDebug() {
                initialBuffer = new LayoutDebugBuffer(64, Allocator.Persistent),
                bottomUpFirstBuffer = new LayoutDebugBuffer(64, Allocator.Persistent),
                topDownFirstBuffer = new LayoutDebugBuffer(64, Allocator.Persistent),
                bottomUpSecondBuffer = new LayoutDebugBuffer(64, Allocator.Persistent),
                topDownSecondBuffer = new LayoutDebugBuffer(64, Allocator.Persistent),
            };
        }

        public void SetInitialData(CheckedArray<SizeTypeFlags> horizontalFlags, CheckedArray<SizeTypeFlags> verticalFlags) {
            hasSecondaryPasses = false;
            initialBuffer.horizontalFlags.size = 0;
            initialBuffer.verticalFlags.size = 0;
            initialBuffer.horizontalFlags.AddRange(horizontalFlags.GetArrayPointer(), horizontalFlags.size);
            initialBuffer.verticalFlags.AddRange(verticalFlags.GetArrayPointer(), verticalFlags.size);

            initialBuffer.horizontalSizes.SetSize(horizontalFlags.size);
            initialBuffer.verticalSizes.SetSize(verticalFlags.size);

            TypedUnsafe.MemClear(initialBuffer.horizontalSizes.GetArrayPointer(), initialBuffer.horizontalSizes.size);
            TypedUnsafe.MemClear(initialBuffer.verticalSizes.GetArrayPointer(), initialBuffer.verticalSizes.size);
        }

        public void SetBottomUpFirstPass(CheckedArray<SizeTypeFlags> horizontalFlags, CheckedArray<SizeTypeFlags> verticalFlags, CheckedArray<LayoutSizes> horizontalSizes, CheckedArray<LayoutSizes> verticalSizes) {
            hasSecondaryPasses = false;
            bottomUpFirstBuffer.horizontalFlags.size = 0;
            bottomUpFirstBuffer.verticalFlags.size = 0;
            bottomUpFirstBuffer.horizontalFlags.AddRange(horizontalFlags.GetArrayPointer(), horizontalFlags.size);
            bottomUpFirstBuffer.verticalFlags.AddRange(verticalFlags.GetArrayPointer(), verticalFlags.size);
            bottomUpFirstBuffer.verticalSizes.size = 0;
            bottomUpFirstBuffer.horizontalSizes.size = 0;
            bottomUpFirstBuffer.horizontalSizes.AddRange(horizontalSizes.GetArrayPointer(), horizontalSizes.size);
            bottomUpFirstBuffer.verticalSizes.AddRange(verticalSizes.GetArrayPointer(), verticalSizes.size);
        }

        public void SetTopDownFirstPass(CheckedArray<SizeTypeFlags> horizontalFlags, CheckedArray<SizeTypeFlags> verticalFlags, CheckedArray<LayoutSizes> horizontalSizes, CheckedArray<LayoutSizes> verticalSizes) {
            hasSecondaryPasses = false;
            topDownFirstBuffer.horizontalFlags.size = 0;
            topDownFirstBuffer.verticalFlags.size = 0;
            topDownFirstBuffer.horizontalFlags.AddRange(horizontalFlags.GetArrayPointer(), horizontalFlags.size);
            topDownFirstBuffer.verticalFlags.AddRange(verticalFlags.GetArrayPointer(), verticalFlags.size);

            topDownFirstBuffer.verticalSizes.size = 0;
            topDownFirstBuffer.horizontalSizes.size = 0;
            topDownFirstBuffer.horizontalSizes.AddRange(horizontalSizes.GetArrayPointer(), horizontalSizes.size);
            topDownFirstBuffer.verticalSizes.AddRange(verticalSizes.GetArrayPointer(), verticalSizes.size);
        }

        public void SetBottomUpSecondPass(CheckedArray<SizeTypeFlags> horizontalFlags, CheckedArray<SizeTypeFlags> verticalFlags, CheckedArray<LayoutSizes> horizontalSizes, CheckedArray<LayoutSizes> verticalSizes) {
            hasSecondaryPasses = true;
            bottomUpSecondBuffer.horizontalFlags.size = 0;
            bottomUpSecondBuffer.verticalFlags.size = 0;
            bottomUpSecondBuffer.horizontalFlags.AddRange(horizontalFlags.GetArrayPointer(), horizontalFlags.size);
            bottomUpSecondBuffer.verticalFlags.AddRange(verticalFlags.GetArrayPointer(), verticalFlags.size);

            bottomUpSecondBuffer.verticalSizes.size = 0;
            bottomUpSecondBuffer.horizontalSizes.size = 0;
            bottomUpSecondBuffer.horizontalSizes.AddRange(horizontalSizes.GetArrayPointer(), horizontalSizes.size);
            bottomUpSecondBuffer.verticalSizes.AddRange(verticalSizes.GetArrayPointer(), verticalSizes.size);
        }

        public void SetTopDownSecondPass(CheckedArray<SizeTypeFlags> horizontalFlags, CheckedArray<SizeTypeFlags> verticalFlags, CheckedArray<LayoutSizes> horizontalSizes, CheckedArray<LayoutSizes> verticalSizes) {
            hasSecondaryPasses = true;
            topDownSecondBuffer.horizontalFlags.size = 0;
            topDownSecondBuffer.verticalFlags.size = 0;
            topDownSecondBuffer.horizontalFlags.AddRange(horizontalFlags.GetArrayPointer(), horizontalFlags.size);
            topDownSecondBuffer.verticalFlags.AddRange(verticalFlags.GetArrayPointer(), verticalFlags.size);

            topDownSecondBuffer.verticalSizes.size = 0;
            topDownSecondBuffer.horizontalSizes.size = 0;
            topDownSecondBuffer.horizontalSizes.AddRange(horizontalSizes.GetArrayPointer(), horizontalSizes.size);
            topDownSecondBuffer.verticalSizes.AddRange(verticalSizes.GetArrayPointer(), verticalSizes.size);
        }

        public void Dispose() {
            initialBuffer.Dispose();
            bottomUpFirstBuffer.Dispose();
            topDownFirstBuffer.Dispose();
            bottomUpSecondBuffer.Dispose();
            topDownSecondBuffer.Dispose();
            this = default;
        }

        public string Dump(UIElement root, in ApplicationLoop loop, bool colors = true) {
            return "";
            IndentedStringBuilder builder = new IndentedStringBuilder(2048);
            builder.enableColors = colors;
            builder.Append("Initial Values");
            builder.NewLine();
            builder.Indent();
            DumpInitialValues(builder, root, loop);
            builder.Outdent();

            builder.Append("Bottom Up First Pass");
            builder.NewLine();
            builder.Indent();
            DumpPass(builder, root, loop, (LayoutDebugBuffer*) UnsafeUtility.AddressOf(ref bottomUpFirstBuffer), (LayoutDebugBuffer*) UnsafeUtility.AddressOf(ref initialBuffer));
            builder.Outdent();

            builder.Append("Top Down First Pass");
            builder.NewLine();
            builder.Indent();
            DumpPass(builder, root, loop, (LayoutDebugBuffer*) UnsafeUtility.AddressOf(ref topDownFirstBuffer), (LayoutDebugBuffer*) UnsafeUtility.AddressOf(ref bottomUpFirstBuffer));
            builder.Outdent();

            if (hasSecondaryPasses) {

                builder.Append("Bottom Up Second Pass");
                builder.NewLine();
                builder.Indent();
                DumpPass(builder, root, loop, (LayoutDebugBuffer*) UnsafeUtility.AddressOf(ref bottomUpSecondBuffer), (LayoutDebugBuffer*) UnsafeUtility.AddressOf(ref topDownFirstBuffer));
                builder.Outdent();

                builder.Append("Top Down Second Pass");
                builder.NewLine();
                builder.Indent();
                DumpPass(builder, root, loop, (LayoutDebugBuffer*) UnsafeUtility.AddressOf(ref topDownSecondBuffer), (LayoutDebugBuffer*) UnsafeUtility.AddressOf(ref bottomUpSecondBuffer));
                builder.Outdent();
            }

            return builder.ToString();
        }

        private static void DumpPass(IndentedStringBuilder builder, UIElement element, in ApplicationLoop loop, LayoutDebugBuffer* current, LayoutDebugBuffer* previous) {
            int index = loop.appInfo->elementIdToIndex[element.elementId.index];

      //      LayoutBoxRef boxRef = default; // loop.appInfo->perFrameLayoutData.layoutBoxRefs[index];

            // element.ToString() [layoutType] (prefW = minW = x, maxH = y, prefW = minW = x, maxH = y) { resolvedX + flags, resolvedY + flags }
            builder.Append(index + " ");
            builder.AppendInline(element.ToString());
          //  builder.AppendInline(ColorUtil.sienna, " [" + boxRef.boxType + "]");
            builder.AppendInline(" (");

            // SolvedConstraint minWidth = loop.appInfo->perFrameLayoutData.minWidths[index];
            // SolvedConstraint minHeight = loop.appInfo->perFrameLayoutData.minHeights[index];
            // SolvedConstraint maxWidth = loop.appInfo->perFrameLayoutData.maxWidths[index];
            // SolvedConstraint maxHeight = loop.appInfo->perFrameLayoutData.maxHeights[index];

            UISizeConstraint styleMinWidth = loop.appInfo->styleTables.MinWidth[element.elementId.index];
            UISizeConstraint styleMaxWidth = loop.appInfo->styleTables.MaxWidth[element.elementId.index];

            UISizeConstraint styleMinHeight = loop.appInfo->styleTables.MinHeight[element.elementId.index];
            UISizeConstraint styleMaxHeight = loop.appInfo->styleTables.MaxHeight[element.elementId.index];

            UIMeasurement stylePrefWidth = loop.appInfo->styleTables.PreferredWidth[element.elementId.index];
            UIMeasurement stylePrefHeight = loop.appInfo->styleTables.PreferredHeight[element.elementId.index];

            bool hasMinWidth = (styleMinWidth.unit != UISizeConstraintUnit.Pixel || styleMinWidth.value != 0);
            bool hasMaxWidth = (styleMaxWidth.unit != UISizeConstraintUnit.Pixel || styleMaxWidth.value != float.MaxValue);
            bool hasMinHeight = (styleMinHeight.unit != UISizeConstraintUnit.Pixel || styleMinHeight.value != 0);
            bool hasMaxHeight = (styleMaxHeight.unit != UISizeConstraintUnit.Pixel || styleMaxHeight.value != float.MaxValue);

            bool onlyPrefWidth = !hasMinWidth && !hasMaxWidth;
            bool onlyPrefHeight = !hasMinHeight && !hasMaxHeight;

            if (onlyPrefWidth) {
                builder.AppendInline(UIMeasurement.ToStyleString(stylePrefWidth));
            }
            else {
                if (hasMinWidth) {
                    builder.AppendInline("min=");
                    builder.AppendInline(UISizeConstraint.ToStyleString(styleMinWidth));
                    builder.AppendInline(" ");
                    if (hasMaxWidth) {
                        builder.AppendInline(" ");
                    }
                }

                if (hasMaxWidth) {
                    builder.AppendInline("max=");
                    builder.AppendInline(UISizeConstraint.ToStyleString(styleMaxWidth));
                    builder.AppendInline(" ");
                }

                builder.AppendInline("pref=");
                builder.AppendInline(UIMeasurement.ToStyleString(stylePrefWidth));
            }

            builder.AppendInline(", ");

            if (onlyPrefHeight) {
                builder.AppendInline(UIMeasurement.ToStyleString(stylePrefHeight));
            }
            else {
                if (hasMinWidth) {
                    builder.AppendInline("min=");
                    builder.AppendInline(UISizeConstraint.ToStyleString(styleMinHeight));
                    builder.AppendInline(" ");
                }

                if (hasMaxWidth) {
                    builder.AppendInline("max=");
                    builder.AppendInline(UISizeConstraint.ToStyleString(styleMaxHeight));
                    builder.AppendInline(" ");
                }

                builder.AppendInline("pref=");
                builder.AppendInline(UIMeasurement.ToStyleString(stylePrefHeight));
            }

            builder.AppendInline(") {");

            SizeTypeFlags horizontalFlag = current->horizontalFlags[index];
            SizeTypeFlags verticalFlag = current->verticalFlags[index];
            SizeTypeFlags lastHorizontal = previous->horizontalFlags[index];
            SizeTypeFlags lastVertical = previous->verticalFlags[index];

            LayoutSizes horizontalSizes = current->horizontalSizes[index];
            LayoutSizes verticalSizes = current->verticalSizes[index];

            Color hFlagColor = horizontalFlag == lastHorizontal ? ColorUtil.firebrick : ColorUtil.black;
            Color vFlagColor = verticalFlag == lastVertical ? ColorUtil.firebrick : ColorUtil.black;

            builder.AppendInline(hFlagColor, horizontalFlag == SizeTypeFlags.Resolved ? "Resolved" : horizontalFlag.ToString());
            builder.AppendInline(" -> ");
            
            Color valueColor = Color.black;
            
            if (horizontalFlag == SizeTypeFlags.LayoutComplete) {
                builder.AppendInline(ColorUtil.darkgreen, loop.appInfo->perFrameLayoutOutput.sizes[index].width.ToString());
            }
            else if (horizontalFlag == SizeTypeFlags.LayoutReady || horizontalFlag == SizeTypeFlags.Resolved) {
                builder.AppendInline(valueColor, horizontalSizes.baseSize.ToString());
                uint horizontalStretch = horizontalSizes.stretch;
                if (horizontalStretch > 0) {
                    builder.AppendInline(valueColor, " " + horizontalStretch + "s");
                }
            }
            else {
                builder.AppendInline(valueColor, "?");
            }

            builder.AppendInline(", ");

            builder.AppendInline(vFlagColor, verticalFlag == SizeTypeFlags.Resolved ? "Resolved" : verticalFlag.ToString());
            builder.AppendInline(" -> ");

            if (verticalFlag == SizeTypeFlags.LayoutComplete) {
                builder.AppendInline(ColorUtil.darkgreen, loop.appInfo->perFrameLayoutOutput.sizes[index].height.ToString());
            }
            else if (verticalFlag == SizeTypeFlags.LayoutReady || verticalFlag == SizeTypeFlags.Resolved) {
                builder.AppendInline(valueColor, verticalSizes.baseSize.ToString());
                uint verticalStretch = verticalSizes.stretch;
                if (verticalStretch > 0) {
                    builder.AppendInline(valueColor, " " + verticalStretch + "s");
                }
            }
            else {
                builder.AppendInline(valueColor, "?");
            }

            builder.AppendInline("}");

            builder.NewLine();
            builder.Indent();
            // UIElement ptr = element.GetFirstChild();
            // while (ptr != null) {
            //
            //     if (ptr.isEnabled) { // todo -- handle transclusion i guess 
            //         DumpPass(builder, ptr, loop, current, previous);
            //     }
            //
            //     ptr = ptr.GetNextSibling();
            // }

            builder.Outdent();
        }
        
        private void DumpInitialValues(IndentedStringBuilder builder, UIElement element, in ApplicationLoop loop) {

            int index = loop.appInfo->elementIdToIndex[element.elementId.index];

          //loop.appInfo->perFrameLayoutData.layoutBoxRefs[index];

            // element.ToString() [layoutType] (prefW = minW = x, maxH = y, prefW = minW = x, maxH = y) { resolvedX + flags, resolvedY + flags }
            builder.Append(index + " ");
            builder.AppendInline(element.ToString());
           // builder.AppendInline(ColorUtil.sienna, " [" + boxRef.boxType + "]");
            builder.AppendInline(" (");

            UISizeConstraint styleMinWidth = loop.appInfo->styleTables.MinWidth[element.elementId.index];
            UISizeConstraint styleMaxWidth = loop.appInfo->styleTables.MaxWidth[element.elementId.index];

            UISizeConstraint styleMinHeight = loop.appInfo->styleTables.MinHeight[element.elementId.index];
            UISizeConstraint styleMaxHeight = loop.appInfo->styleTables.MaxHeight[element.elementId.index];

            UIMeasurement stylePrefWidth = loop.appInfo->styleTables.PreferredWidth[element.elementId.index];
            UIMeasurement stylePrefHeight = loop.appInfo->styleTables.PreferredHeight[element.elementId.index];

            // builder.Append(initialBuffer.horizontalFlags[index].ToString());

            bool hasMinWidth = (styleMinWidth.unit != UISizeConstraintUnit.Pixel || styleMinWidth.value != 0);
            bool hasMaxWidth = (styleMaxWidth.unit != UISizeConstraintUnit.Pixel || styleMaxWidth.value != float.MaxValue);
            bool hasMinHeight = (styleMinHeight.unit != UISizeConstraintUnit.Pixel || styleMinHeight.value != 0);
            bool hasMaxHeight = (styleMaxHeight.unit != UISizeConstraintUnit.Pixel || styleMaxHeight.value != float.MaxValue);

            bool onlyPrefWidth = !hasMinWidth && !hasMaxWidth;
            bool onlyPrefHeight = !hasMinHeight && !hasMaxHeight;

            if (onlyPrefWidth) {
                builder.AppendInline(UIMeasurement.ToStyleString(stylePrefWidth));
            }
            else {
                if (hasMinWidth) {
                    builder.AppendInline("min=");
                    builder.AppendInline(UISizeConstraint.ToStyleString(styleMinWidth));
                    builder.AppendInline(" ");
                    if (hasMaxWidth) {
                        builder.AppendInline(" ");
                    }
                }

                if (hasMaxWidth) {
                    builder.AppendInline("max=");
                    builder.AppendInline(UISizeConstraint.ToStyleString(styleMaxWidth));
                    builder.AppendInline(" ");
                }

                builder.AppendInline("pref=");
                builder.AppendInline(UIMeasurement.ToStyleString(stylePrefWidth));
            }

            builder.AppendInline(", ");

            if (onlyPrefHeight) {
                builder.AppendInline(UIMeasurement.ToStyleString(stylePrefHeight));
            }
            else {
                if (hasMinWidth) {
                    builder.AppendInline("min=");
                    builder.AppendInline(UISizeConstraint.ToStyleString(styleMinHeight));
                    builder.AppendInline(" ");
                }

                if (hasMaxWidth) {
                    builder.AppendInline("max=");
                    builder.AppendInline(UISizeConstraint.ToStyleString(styleMaxHeight));
                    builder.AppendInline(" ");
                }

                builder.AppendInline("pref=");
                builder.AppendInline(UIMeasurement.ToStyleString(stylePrefHeight));
            }

            builder.AppendInline(") {");

            SizeTypeFlags horizontalFlag = initialBuffer.horizontalFlags[index];
            SizeTypeFlags verticalFlag = initialBuffer.verticalFlags[index];
            if (horizontalFlag == SizeTypeFlags.Resolved) {
                builder.AppendInline(ColorUtil.aliceblue, "Resolved");
            }
            else {
                builder.AppendInline(ColorUtil.firebrick, horizontalFlag.ToString());
            }

            builder.AppendInline(", ");
            if (verticalFlag == SizeTypeFlags.Resolved) {
                builder.AppendInline(ColorUtil.aliceblue, "Resolved");
            }
            else {
                builder.AppendInline(ColorUtil.firebrick, verticalFlag.ToString());
            }

            builder.AppendInline("}");

            builder.NewLine();
            builder.Indent();
            // UIElement ptr = element.GetFirstChild();
            // while (ptr != null) {
            //
            //     if (ptr.isEnabled) { // todo -- handle transclusion i guess 
            //         DumpInitialValues(builder, ptr, loop);
            //     }
            //
            //     ptr = ptr.GetNextSibling();
            // }

            builder.Outdent();

        }

    }

}