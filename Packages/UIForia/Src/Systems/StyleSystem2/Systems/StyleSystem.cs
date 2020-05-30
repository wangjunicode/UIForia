using System;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia {

    public unsafe class StyleSystem2 : IDisposable {

        public ListAllocator<StyleId> styleIdAllocator;
        public ElementTable<StyleSetData> styleSets;
        public ElementTable<StyleResult> instanceResults;
        public ElementTable<StyleResult> sharedResults;
        public ElementTable<StyleResult> selectorResults;
        public ElementTable<StyleResult> animationResults;
        public ElementTable<StyleResult> finalResults;

        public DataList<StyleStateUpdate>.Shared stateUpdates;
        public DataList<SharedStyleUpdate>.Shared sharedChangeSets;
        public DataList<ElementId>.Shared instanceChangeSets;

        public IntMap<List_InstanceStyleProperty> instanceMap;
        public IntMap<List_ElementId> styleIndex;

        public StyleDatabase styleDatabase;

        // can probably be 1 buffer
        public DataList<ElementId> activeStateIndex;
        public DataList<ElementId> focusStateIndex;
        public DataList<ElementId> hoverStateIndex;

        public PagedList<StyleId> updatedStyleIdBuffer; // cleared every frame

        // 1 allocator per table we want to write to? or 1 allocator total?
        // could buffer results, sort by block size and write in parallel?
        // would need 2 passes, 1 to free and 1 to write
        // or we'd need to schedule write jobs to be ordered
        private int elementCapacity;
        private byte* backingStore;
        public Pow2AllocatorSet stylePropertyListAllocator;

        public StyleSystem2(int initialElementCount, StyleDatabase styleDatabase) {

            this.styleDatabase = styleDatabase;
            initialElementCount = initialElementCount < 256 ? 256 : initialElementCount;

            this.styleIdAllocator = CreateStyleIdListAllocator();
            this.styleIndex = new IntMap<List_ElementId>(64, Allocator.Persistent);

            this.sharedChangeSets = new DataList<SharedStyleUpdate>.Shared(32, Allocator.Persistent);
            this.instanceChangeSets = new DataList<ElementId>.Shared(32, Allocator.Persistent);
            this.stateUpdates = new DataList<StyleStateUpdate>.Shared(32, Allocator.Persistent);
            this.updatedStyleIdBuffer = new PagedList<StyleId>(512, Allocator.Persistent);

            this.activeStateIndex = new DataList<ElementId>(32, Allocator.Persistent);
            this.focusStateIndex = new DataList<ElementId>(32, Allocator.Persistent);
            this.hoverStateIndex = new DataList<ElementId>(32, Allocator.Persistent);

            ResizeBackingBuffer(initialElementCount);

            const int largestBlockPerPages = 128;
            int minCapacity = TypedUnsafe.ByteSize<PropertyId, PropertyData>(8);
            int maxCapacity = TypedUnsafe.ByteSize<PropertyId, PropertyData>(VertigoStyleSystem.k_MaxStyleProperties);
            stylePropertyListAllocator = Pow2AllocatorSet.CreateFromSizeRange(minCapacity, maxCapacity, largestBlockPerPages);
        }
        
        public void Initialize(StyleDatabase styleDatabase) {
            this.styleDatabase = styleDatabase;
            this.styleIdAllocator.Dispose();
            this.sharedChangeSets.size = 0;
            this.instanceChangeSets.size = 0;
            this.stateUpdates.size = 0;
            this.updatedStyleIdBuffer.Clear();
            this.activeStateIndex.size = 0;
            this.focusStateIndex.size = 0;
            this.hoverStateIndex.size = 0;
        }

        // maybe this should return a job struct? probably best done in a job since we'll want to clean the index of dead elements also (or leave them, not sure)
        public void EndFrame() {

            updatedStyleIdBuffer.Clear();

            // start at 1 to let 0 mean invalid

            for (int i = 1; i < sharedChangeSets.size; i++) {
                styleSets[sharedChangeSets[i].elementId].styleChangeIndex = 0;
            }

            for (int i = 1; i < stateUpdates.size; i++) {
                styleSets[stateUpdates[i].elementId].stateChangeIndex = 0;
            }

            for (int i = 1; i < instanceChangeSets.size; i++) {
                styleSets[instanceChangeSets[i]].instanceChangeIndex = 0;
            }

            instanceChangeSets.size = 1;
            sharedChangeSets.size = 1;
            stateUpdates.size = 1;

        }

        public void CreateElement(ElementId elementId) {

            int index = elementId.index;

            if (elementCapacity < elementId.index) {
                // todo find a different growth scheme, power of 2 is wasteful for element counts
                // max index + 512 or 1024 or something like that is better
                int capacity = index < 256 ? 256 : index;
                ResizeBackingBuffer(BitUtil.EnsurePowerOfTwo(capacity));
            }

            styleSets[elementId].state = StyleState2.Normal; // anyway to avoid this? would have to do it in bulk on create / resize. or we just treat normal as 0? we're never NOT in normal state
            // data is re-initialized on free

        }

        // this will totally ignore the existence of duplicate styles in the input list
        // the burst code is better suited to handling this and I expect the instance of 
        // duplicate style usages to be damn close to 0 so I want want to architect around it
        public void SetSharedStyles(ElementId elementId, StyleId* newStyles, int count) {

            ref StyleSetData styleSet = ref styleSets[elementId];
            ref List_StyleId currentStyles = ref styleSet.sharedStyles;

            // this will hit most of the time
            if (currentStyles.size == count && UnsafeUtility.MemCmp(currentStyles.array, newStyles, sizeof(StyleId) * count) == 0) {
                return;
            }

            count = RemoveDuplicateAndInvalidStyles(newStyles, count);

            int changeSetIndex = styleSet.styleChangeIndex;

            if (changeSetIndex == 0) {
                styleSet.styleChangeIndex = sharedChangeSets.size;
                StyleId* styles = updatedStyleIdBuffer.Reserve(currentStyles.size + count);
                TypedUnsafe.MemCpy(styles, currentStyles.array, currentStyles.size);
                TypedUnsafe.MemCpy(styles + currentStyles.size, newStyles, count);

                sharedChangeSets.Add(new SharedStyleUpdate {
                    elementId = elementId,
                    originalState = styleSet.state,
                    updatedState = styleSet.state,
                    styles = styles,
                    originalStyleCount = currentStyles.size,
                    updatedStyleCount = (ushort) count
                });

            }
            else {
                ref SharedStyleUpdate update = ref sharedChangeSets[changeSetIndex];
                update.updatedStyleCount = (ushort) count;

                if (update.styles == null) {
                    StyleId* styles = updatedStyleIdBuffer.Reserve(currentStyles.size + count);
                    TypedUnsafe.MemCpy(styles, currentStyles.array, currentStyles.size);
                    TypedUnsafe.MemCpy(styles + currentStyles.size, newStyles, count);
                    update.styles = styles;
                    update.originalStyleCount = currentStyles.size;
                    update.updatedStyleCount = count;
                }
                else if (count < update.updatedStyleCount) {
                    TypedUnsafe.MemCpy(update.styles + update.originalStyleCount, newStyles, count);
                }
                else {
                    StyleId* ptr = updatedStyleIdBuffer.Reserve(update.originalStyleCount + count);
                    TypedUnsafe.MemCpy(ptr, update.styles, update.originalStyleCount);
                    TypedUnsafe.MemCpy(ptr + update.originalStyleCount, newStyles, count);
                    update.styles = ptr;
                }
            }

            styleIdAllocator.Replace(ref currentStyles, newStyles, count);
        }

        // selectors need to know overall style changes which means shared styles changed or state changed
        // might as well process them all the same and discard in burst phase if we dont actually need an update
        // instance styles can have their own run sequence and can just read the state update, ignoring style data

        public void EnterState(ElementId elementId, StyleState2 state) {
            ref StyleSetData styleSet = ref styleSets[elementId];

            if ((styleSet.state & state) != 0) {
                return;
            }

            int changeSetIndex = styleSet.styleChangeIndex;
            int stateChangeSetIndex = styleSet.stateChangeIndex;

            // if we didnt have a change set before we can just borrow the existing style data
            // this can be safely referenced and overwritten later if we get a real shared style change
            if (changeSetIndex == 0) {
                styleSet.styleChangeIndex = sharedChangeSets.size;

                sharedChangeSets.Add(new SharedStyleUpdate {
                    elementId = elementId,
                    originalState = styleSet.state,
                    updatedState = styleSet.state | state,
                    updatedStyleCount = 0,
                    originalStyleCount = 0
                });
            }
            else {
                ref SharedStyleUpdate update = ref sharedChangeSets[changeSetIndex];
                update.updatedState |= state;
            }

            // dont expect many of these per frame, make sense to hold a state change set id?
            if (stateChangeSetIndex == 0) {
                styleSet.stateChangeIndex = stateUpdates.size;
                stateUpdates.Add(new StyleStateUpdate() {
                    elementId = elementId,
                    originalState = styleSet.state,
                    updatedState = styleSet.state | state
                });
            }
            else {
                ref StyleStateUpdate stateUpdate = ref stateUpdates[stateChangeSetIndex];
                stateUpdate.updatedState |= state;
            }

        }

        public void ExitState(ElementId elementId, StyleState2 state) {
            ref StyleSetData styleSet = ref styleSets[elementId];

            if ((styleSet.state & state) == 0) {
                return;
            }

            int changeSetIndex = styleSet.styleChangeIndex;
            int stateChangeSetIndex = styleSet.stateChangeIndex;

            // if we didnt have a change set before we can just borrow the existing style data
            // this can be safely referenced and overwritten later if we get a real shared style change
            if (changeSetIndex == 0) {
                styleSet.styleChangeIndex = sharedChangeSets.size;
                sharedChangeSets.Add(new SharedStyleUpdate {
                    elementId = elementId,
                    updatedStyleCount = 0,
                    originalStyleCount = 0,
                    originalState = styleSet.state,
                    updatedState = styleSet.state & ~state
                });
            }
            else {
                ref SharedStyleUpdate update = ref sharedChangeSets[changeSetIndex];
                update.updatedState &= ~state;
            }

            if (stateChangeSetIndex == 0) {
                styleSet.stateChangeIndex = stateUpdates.size;
                stateUpdates.Add(new StyleStateUpdate() {
                    elementId = elementId,
                    originalState = styleSet.state,
                    updatedState = styleSet.state & ~state
                });
            }
            else {
                ref StyleStateUpdate stateUpdate = ref stateUpdates[stateChangeSetIndex];
                stateUpdate.updatedState &= ~state;
            }

        }

        public void UnsetInstanceStyle(ElementId elementId, PropertyId propertyId, StyleState2 state) {
            // if (ElementSystem.IsDead(elementId)) return;

            if (!instanceMap.TryGetPointer((int)elementId, out List_InstanceStyleProperty* listptr)) {
                return;
            }

            ref List_InstanceStyleProperty list = ref *listptr;

            for (int i = 0; i < list.size; i++) {

                if (list.array[i].propertyId == propertyId && list.array[i].state == state) {
                    
                    ListAllocator<InstanceStyleProperty>.Create(stylePropertyListAllocator).Remove(ref list, i);
                    EnsureInstanceChangeSet(elementId);
                    
                    if (list.size == 0) {
                        instanceMap.Remove((int)elementId);
                    }
                    
                    return;
                }

            }
           
        }

        public void SetInstanceStyle(ElementId elementId, PropertyId propertyId, PropertyData propertyData, StyleState2 state) {
            // if (ElementSystem.IsDead(elementId)) return;

            ref List_InstanceStyleProperty list = ref instanceMap.GetOrCreateReference((int)elementId);

            for (int i = 0; i < list.size; i++) {

                if (list.array[i].propertyId == propertyId && list.array[i].state == state) {
                    if (list.array[i].data.value == propertyData.value) {
                        return;
                    }

                    list.array[i].data = propertyData;
                    EnsureInstanceChangeSet(elementId);
                    return;
                }

            }

            ListAllocator<InstanceStyleProperty>.Create(stylePropertyListAllocator).Add(ref list, new InstanceStyleProperty() {
                data = propertyData,
                state = state,
                propertyId = propertyId
            });

            EnsureInstanceChangeSet(elementId);

        }

        private void EnsureInstanceChangeSet(ElementId elementId) {
            ref StyleSetData styleSet = ref styleSets[elementId];
            if (styleSet.instanceChangeIndex == 0) {
                styleSet.instanceChangeIndex = instanceChangeSets.size;
                instanceChangeSets.Add(elementId);
            }
        }

        // destroy should be given a list of element indices sorted lowest-> highest
        // called every frame before frame start or maybe as a job(s) that run after frame but before next ui tick starts

        public void DestroyElement(ElementId elementId) {

            if(instanceMap.TryRemove((int)elementId, out List_InstanceStyleProperty list)) {
                ListAllocator<List_InstanceStyleProperty>.Create(stylePropertyListAllocator).Free(ref list);
            }
            
            if (sharedResults[elementId].properties != null) {
                // free
            }

            if (sharedResults[elementId].properties != null) {
                // free
            }

            if (sharedResults[elementId].properties != null) {
                // free
            }

            if (sharedResults[elementId].properties != null) {
                // free
            }

            if (sharedResults[elementId].properties != null) {
                // free
            }

        }

        private int RemoveDuplicateAndInvalidStyles(StyleId* newStyles, int count) {
            if (count <= 0) return 0;
            if (count > 16) count = 16;

            // todo -- diagnostic for more then 16 styles 

            StyleIdBuffer16 buffer = new StyleIdBuffer16();
            StyleId* idBuffer = (StyleId*) &buffer;
            int idx = 0;

            int maxStyleIndex = styleDatabase.styleCount;

            // technically this is n^2 but the list size is typically 1 - 4 elements, so who cares?
            for (int i = count - 1; i >= 0; i--) {

                ref StyleId target = ref newStyles[i];

                if (target.index >= maxStyleIndex) {
                    continue;
                }

                bool found = false;
                for (int j = 0; j < idx; j++) {
                    if (idBuffer[j] == target) {
                        found = true;
                    }
                }

                if (!found) {
                    idBuffer[idx++] = target;
                }

            }

            TypedUnsafe.MemCpy(newStyles, idBuffer, idx);

            return idx;
        }

        public void Dispose() {
            sharedChangeSets.Dispose();
            updatedStyleIdBuffer.Dispose();

            stateUpdates.Dispose();

            activeStateIndex.Dispose();
            focusStateIndex.Dispose();
            hoverStateIndex.Dispose();

            styleIdAllocator.Dispose();
            stylePropertyListAllocator.Dispose();

            if (backingStore != null) {
                UnsafeUtility.Free(backingStore, Allocator.Persistent);
                backingStore = null;
            }

        }

        // todo -- I might consolidate these allocators across the board
        private static ListAllocator<StyleId> CreateStyleIdListAllocator() {
            FixedAllocatorDesc* blocks = stackalloc FixedAllocatorDesc[5];
            blocks[0] = new FixedAllocatorDesc<StyleId>(4, 128, 1);
            blocks[1] = new FixedAllocatorDesc<StyleId>(8, 64, 1);
            blocks[2] = new FixedAllocatorDesc<StyleId>(16, 32, 0);
            blocks[3] = new FixedAllocatorDesc<StyleId>(32, 8, 0);
            blocks[4] = new FixedAllocatorDesc<StyleId>(64, 4, 0);
            return ListAllocator<StyleId>.Create(blocks, 5);
        }

        private void ResizeBackingBuffer(int newCapacity) {

            backingStore = TypedUnsafe.ResizeSplitBuffer(
                ref styleSets.array,
                ref instanceResults.array,
                ref sharedResults.array,
                ref selectorResults.array,
                // ref animationResults.array,
                ref finalResults.array,
                elementCapacity,
                newCapacity,
                Allocator.Persistent,
                true
            );

            elementCapacity = newCapacity;

        }

        // used to get a stack allocated list of 16 style ids
        private struct StyleIdBuffer16 {

            public fixed int ids[16];

        }



    }

    public struct InstanceStyleProperty {

        public PropertyId propertyId;
        public StyleState2 state;
        public PropertyData data;

    }

}