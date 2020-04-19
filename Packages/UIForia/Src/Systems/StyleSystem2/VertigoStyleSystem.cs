using System;
using UIForia.Elements;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia {

    // currently sizeof(SharedStyleChangeSet) = 32 bytes
    public unsafe struct SharedStyleChangeSet {

        // the first {oldStyleCount} elements in *styles are old, the rest are new
        public int elementId;
        public StyleId* styles;
        public ushort newStyleCount;
        public ushort oldStyleCount;
        public StyleState2 newState;
        public StyleState2 originalState;

        public StyleId* oldStyles {
            get => styles;
        }

        public StyleId* newStyles {
            get => styles + oldStyleCount;
        }

    }

    public class VertigoStyleSystem {

        internal static LightList<VertigoStyleSheet> s_DebugSheets;

        internal const ushort k_ImplicitModuleId = 0;

        public UIElement rootElement;

        internal readonly LightList<VertigoStyleSheet> styleSheets;

        internal LightList<StyleSet> changedStyleSets;
        internal UnsafeList<StyleId> changeSetStyleBuffer; // stack based buffer allocator, clear on frame end
        internal UnsafeList<SharedStyleChangeSet> sharedStyleChangeSets; // stack based buffer allocator, clear on frame end

        public VertigoStyleSystem() {
            changeSetStyleBuffer = new UnsafeList<StyleId>(128, Allocator.Persistent);
            sharedStyleChangeSets = new UnsafeList<SharedStyleChangeSet>(32, Allocator.Persistent);
            changedStyleSets = new LightList<StyleSet>(32);
            styleSheets = new LightList<VertigoStyleSheet>();
            s_DebugSheets = styleSheets;
        }

        internal bool TryResolveStyle(string sheetName, string styleName, out StyleId styleId) {
            styleId = default;
            VertigoStyleSheet sheet = GetStyleSheet(sheetName);
            return sheet != null && sheet.TryGetStyle(styleName, out styleId);
        }
        
        public void Destroy() {
            sharedStyleChangeSets.Dispose();
            changeSetStyleBuffer.Dispose();
            for (int i = 0; i < styleSheets.size; i++) {
                styleSheets.array[i].Destroy();
            }
        }

        public void AddStyleSheet(string name, Action<VertigoStyleSheet> sheetAction) {
            // todo - ensure name is unique
            VertigoStyleSheet sheet = new VertigoStyleSheet(name, new StyleSheetId(k_ImplicitModuleId, (ushort) styleSheets.size));
            styleSheets.Add(sheet);
            sheetAction?.Invoke(sheet);
        }

        public void AddStyleSheet(Action<VertigoStyleSheet> sheetAction) {
            AddStyleSheet(string.Empty, sheetAction);
        }

        internal void AddStyleSheet(VertigoStyleSheet[] styleSheets) {
            // this is the compile case where we want to batch add these
        }

        public struct SharedStyleJobData {

            public UnsafeList<GucciSystem.StyleUpdate> addedList;
            public UnsafeList<GucciSystem.StyleUpdate> removedList;
            public UnsafeSpan<SharedStyleChangeSet> changeSets;

        }

        public unsafe void OnUpdate() {

            int changeCount = changedStyleSets.size;
            int batchSize = 1;
            int changeProcessBatchCount = 1;
            
            NativeArray<SharedStyleJobData> data = new NativeArray<SharedStyleJobData>();
            
            for (int i = 0; i < changeProcessBatchCount; i++) {
                
                ProcessSharedStyleUpdatesJob job = new ProcessSharedStyleUpdatesJob();
                
                var h0 = job.Schedule();
                // var h1 = new RemoveInactiveSelectors(batchData).Schedule(h0);

            }

            // end of frame
            for (int i = 0; i < changedStyleSets.size; i++) {
                // can be parallel but probably doesn't need to be
                changedStyleSets.array[i].stateAndSharedStyleChangeSetId = ushort.MaxValue;
            }

            changedStyleSets.size = 0;
            sharedStyleChangeSets.size = 0;
            changeSetStyleBuffer.size = 0;
        }

        public VertigoStyleSheet GetStyleSheet(string sheetName) {
            // cannot sort the list, if this is a common use case then keep a dictionary 
            for (int i = 0; i < styleSheets.size; i++) {
                if (styleSheets.array[i].name == sheetName) {
                    return styleSheets.array[i];
                }
            }

            return null;
        }

        // assumes at least 1 of the groups changed or order was altered in some way
        public unsafe void SetSharedStyles(StyleSet styleSet, ref StackLongBuffer16 newStyles) {

            if (styleSet.stateAndSharedStyleChangeSetId == ushort.MaxValue) {
                changedStyleSets.Add(styleSet);
                styleSet.stateAndSharedStyleChangeSetId = (ushort) sharedStyleChangeSets.size;
                SharedStyleChangeSet changeSetData = default;
                changeSetData.elementId = styleSet.element.id;
                changeSetData.originalState = styleSet.state;
                sharedStyleChangeSets.Add(changeSetData);
            }

            ref SharedStyleChangeSet changeSet = ref sharedStyleChangeSets.array[styleSet.stateAndSharedStyleChangeSetId];

            changeSet.styles = changeSetStyleBuffer.GetSlicePointer(newStyles.size + styleSet.sharedStyles.size);
            changeSet.newStyleCount = (ushort) newStyles.size;
            changeSet.oldStyleCount = (ushort) styleSet.sharedStyles.size;

            int idx = 0;
            for (int i = 0; i < changeSet.oldStyleCount; i++) {
                changeSet.styles[idx++] = styleSet.sharedStyles.array[i];
            }

            for (int i = 0; i < newStyles.size; i++) {
                changeSet.styles[idx++] = newStyles.array[i];
            }

        }

    }

}