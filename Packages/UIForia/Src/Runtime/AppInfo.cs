using System;
using UIForia.Layout;
using UIForia.ListTypes;
using UIForia.Style;
using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia {

    internal unsafe partial struct AppInfo : IDisposable {

        public ShapeContext shapeContext;

        public int viewCount;
        public int elementCount;
        public int maxElementId;
        public int queryTableCount;

        public LayoutDebug layoutDebug;

        // copy from main app 
        [SoAGeneratorGroup("PerElementId")] public RuntimeTraversalInfo* runtimeInfoTable;
        [SoAGeneratorGroup("PerElementId")] public HierarchyInfo* hierarchyTable;
        [SoAGeneratorGroup("PerElementId")] public StyleState* states;
        [SoAGeneratorGroup("PerElementId")] public TemplateInfo* templateInfoTable;
        [SoAGeneratorGroup("PerElementId")] public int* elementIdToIndex;
        [SoAGeneratorGroup("PerElementId")] public ElementId* elementIdToParentId;
        [SoAGeneratorGroup("PerElementId")] public StyleInfo* styleInfoTable;
        [SoAGeneratorGroup("PerElementId")] public InstancePropertyInfo* instancePropertyInfoTable;
        [SoAGeneratorGroup("PerElementId")] public ushort* depthTable;
        [SoAGeneratorGroup("PerElementId")] public ushort* elementIdToViewId;

        [SoAGeneratorGroup("PerActiveElement")]
        public TraversalInfo* traversalTable;

        /// <summary>
        /// Maps an element ID's active index to its parent's active index. 
        ///
        /// parentIndexByActiveElementIndex
        /// ===============================
        /// index (* -> activeElementIds.index)
        /// </summary>
        [SoAGeneratorGroup("PerActiveElement")]
        public int* parentIndexByActiveElementIndex;

        [SoAGeneratorGroup("PerActiveElement")]
        public ElementId* indexToElementId;

        [SoAGeneratorGroup("PerActiveElement")]
        public StyleState* styleStateByActiveIndex;

        [SoAGeneratorGroup("PerActiveElement")]
        public int* childCountByActiveIndex;

        [SoAGeneratorGroup("PerActiveElement")]
        public int* siblingIndexByActiveIndex;

        public PropertyTables propertyTables;

        public StyleTables styleTables;

        public LayoutTree layoutTree;

        public PropertySolverGroup_LayoutSetup layoutSetup;
        public PropertySolverGroup_LayoutAndText layoutAndText;
        public PropertySolverGroup_ClippingAndTransformation clippingAndTransformation;
        public PropertySolverGroup_Rendering rendering;
        public PropertySolverGroup_HorizontalSpacing horizontalSpacing;
        public PropertySolverGroup_VerticalSpacing verticalSpacing;
        public PropertySolverGroup_LayoutBehaviorTypeFontSize layoutBehaviorTypeFontSize;
        public PropertySolverGroup_LayoutSizes layoutSizes;
        public PropertySolverGroup_TextMeasurement textMeasurement;

        public PropertyUpdateSet sharedPropertyUpdates;
        public TransitionUpdateSet transitionUpdates;
        public InstancePropertyUpdateSet instancePropertyUpdates;

        public TempBumpAllocatorPool tempBumpAllocatorPool;
        public LockedBumpAllocator perFrameBumpAllocator;

        // populated per-frame by layout data, allocated with per-frame bump
        // likely want to allocate all of these at the same time, after constructing layout hierarchy 

        public PerFrameLayoutData perFrameLayoutData;
        public PerFrameLayoutInfo perFrameLayoutInfo;
        public PerFrameLayoutOutput perFrameLayoutOutput;

        [Owned] public HeapAllocated<int> styleUsageIdGenerator;
        [Owned] public HeapAllocated<int> executionTokenGenerator;

        [Owned, BaseCapacity("elementCount / 2")]
        public DataList<PropertyContainer> instancePropertyTable;

        [Owned, BaseCapacity("elementCount * 4")]
        public DataList<StyleId> styleIdTable;

        [Owned, EnsureCapacity("elementCount / 2")] // todo -- make this a temp list that gets de-allocated
        public DataList<QuerySubscription>.Shared newQuerySubscriptions;

        [Owned, EnsureCapacity("maxElementId")]
        public DataList<StyleUsage>.Shared styleUsages;

        [Owned, EnsureCapacity("maxElementId")]
        public DataList<StyleUsageQueryResult>.Shared styleUsageResults;

        [Owned, EnsureCapacity("maxElementId")]
        public DataList<int>.Shared styleUsageToIndex;

        [Owned, BaseCapacity("64")] public DataList<int>.Shared styleUsageIdFreeList;

        [Owned, EnsureCapacity("queryTableCount")]
        public DataList<LongBoolMap>.Shared queryResultList;

        [Owned, EnsureCapacity("queryTableCount"), DisposeContents]
        public DataList<List_QuerySubscription>.Shared queryTableSubscriptionList;

        [Owned, EnsureCapacity("LongBoolMap.GetMapSize(maxElementId)"), MemClear]
        public DataList<ulong> invalidatedElementBuffer;

        [Owned, EnsureCapacity("LongBoolMap.GetMapSize(maxElementId)"), MemClear]
        public DataList<ulong> styleRebuildBuffer;

        [Owned, EnsureCapacity("LongBoolMap.GetMapSize(maxElementId)"), MemClear]
        public DataList<ulong> blockRebuildBuffer;

        [Owned, EnsureCapacity("LongBoolMap.GetMapSize(maxElementId)"), MemClear]
        public DataList<ulong> initMapBuffer;

        [Owned, EnsureCapacity("LongBoolMap.GetMapSize(maxElementId)"), MemClear]
        public DataList<ulong> activeMapBuffer;

        [Owned, EnsureCapacity("LongBoolMap.GetMapSize(queryTableCount)"), MemClear]
        public DataList<ulong> newQuerySubscriberMapBuffer;

        [Owned, BaseCapacity("8"), MemClear] public DataList<AnimationCommand> perFrameAnimationCommands;

        [Owned, BaseCapacity("8"), MemClear] public DataList<ColorVariable> colorVariableList;

        [Owned, BaseCapacity("8"), MemClear] public DataList<ValueVariable> valueVariableList;

        [Owned, BaseCapacity("8"), MemClear] public DataList<TextureVariable> textureVariableList;

        // don't clear this 
        [Owned, BaseCapacity("32")] public DataList<StyleBlockChanges> blockChanges;

        // don't clear this 
        [Owned, BaseCapacity("32")] public DataList<AnimationInstance> activeAnimationList;

        // explicitly managed lifecycle 
        public DataList<DataList<KeyFrameResult>> animationResultBuffer;

        // explicitly managed lifecycle 
        public DataList<DataList<InterpolatedStyleValue>> animationValueBuffer;
        
        [Dispose] public ulong* queryResultBuffer;

        private int queryResultBufferCapacity;

        [Owned, BaseCapacity("8")] public DataList<ElementId> viewRootIds;

        [Dispose] public TextShapeCache shapeCache;

        [Dispose] public StringTagger stringTagger;

        public byte* customPropertySolverBuffer; // explicitly managed life cycle 

        public DataList<CustomPropertyInfo> customPropertySolvers; // explicitly managed life cycle 
        
        public void Initialize(StyleDatabase styleDatabase, int elementCount, int maxElementId, int queryTableCount, int viewCount, TextServer.DebugCallback debugCallback) {

#if ENABLE_IL2CPP
            shapeContext = new ShapeContext(TextServer.CreateShapingContextIL2CPP());
#else
            shapeContext = new ShapeContext(TextServer.CreateShapingContext(ShapeLogSettings.All, debugCallback));
#endif
            layoutDebug = LayoutDebug.Create();

            // todo -- each of these has its own list allocator which is fine but should be sized appropriately since some groups are bigger than others
            // could either share a single allocator or have a few solvers share a few allocators
            layoutSetup.Initialize();
            layoutAndText.Initialize();
            layoutSizes.Initialize();
            clippingAndTransformation.Initialize();
            rendering.Initialize();
            horizontalSpacing.Initialize();
            verticalSpacing.Initialize();
            layoutBehaviorTypeFontSize.Initialize();
            textMeasurement.Initialize();

            styleTables = new StyleTables();
            layoutTree = new LayoutTree();
            perFrameLayoutInfo = new PerFrameLayoutInfo();
            perFrameLayoutOutput = new PerFrameLayoutOutput();
            perFrameLayoutData = new PerFrameLayoutData();
            sharedPropertyUpdates = new PropertyUpdateSet();
            transitionUpdates = new TransitionUpdateSet();
            instancePropertyUpdates = new InstancePropertyUpdateSet();
            perFrameBumpAllocator = new LockedBumpAllocator(TypedUnsafe.Kilobytes(256), Allocator.Persistent);
            tempBumpAllocatorPool = new TempBumpAllocatorPool(TypedUnsafe.Kilobytes(256));

            this.elementCount = elementCount;
            this.maxElementId = maxElementId;
            this.queryTableCount = queryTableCount;
            this.viewCount = viewCount;

            this.shapeCache = TextShapeCache.Create(0, 16);
            this.stringTagger = StringTagger.Create(0, 16);
            GeneratedInitialize();

            queryResultBuffer = TypedUnsafe.Malloc<ulong>(queryTableCount * maxElementId, Allocator.Persistent);
            queryResultBufferCapacity = queryTableCount * maxElementId;
            queryTableSubscriptionList.size = queryTableCount;

            for (int i = 0; i < queryTableCount; i++) {
                queryTableSubscriptionList[i] = new List_QuerySubscription(8, Allocator.Persistent);
            }

            animationResultBuffer = new DataList<DataList<KeyFrameResult>>(styleDatabase.PropertyTypeCount, Allocator.Persistent);
            animationValueBuffer = new DataList<DataList<InterpolatedStyleValue>>(styleDatabase.PropertyTypeCount, Allocator.Persistent);

            // todo -- don't love these being their own lists
            // todo -- only one of these two lists will ever be used per type, handle this better and only make 1 of them 
            for (int i = 0; i < styleDatabase.PropertyTypeCount; i++) {
                animationResultBuffer.Add(new DataList<KeyFrameResult>(8, Allocator.Persistent));
                animationValueBuffer.Add(new DataList<InterpolatedStyleValue>(8, Allocator.Persistent));
            }

        }

        public void EnsureCapacity(int elementCount, int maxElementId, int queryTableCount, int viewCount) {
            this.viewCount = viewCount;
            this.elementCount = elementCount;
            this.maxElementId = maxElementId;
            this.queryTableCount = queryTableCount;
            GeneratedEnsureCapacity();
            queryTableSubscriptionList.size = queryTableCount;

            int elementMapSize = LongBoolMap.GetMapSize(elementCount);
            int queryMapSize = LongBoolMap.GetMapSize(queryTableCount);

            if (queryTableCount * elementMapSize > queryResultBufferCapacity) {
                TypedUnsafe.Dispose(queryResultBuffer, Allocator.Persistent);
                queryResultBufferCapacity = queryTableCount * elementMapSize * 2;
                queryResultBuffer = TypedUnsafe.Malloc<ulong>(queryResultBufferCapacity, Allocator.Persistent);
            }

            queryResultList.size = queryTableCount;

            ulong* resultPtr = queryResultBuffer;
            for (int i = 0; i < queryTableCount; i++) {
                queryResultList[i] = new LongBoolMap(resultPtr, queryMapSize);
                resultPtr += elementMapSize;
            }

        }

        public void ClearPerFrameBumpAllocations() {
            perFrameBumpAllocator.ClearAndConsolidate();
            // memory disposed as part of the per-frame allocator so this is safe and not leaking
            sharedPropertyUpdates = default;
            transitionUpdates = default;
            instancePropertyUpdates = default;
            layoutTree = default;
            perFrameLayoutData = default;
            perFrameLayoutInfo = default;
        }

        public void Clear() {
            GeneratedClear();
            TypedUnsafe.MemClear(queryResultBuffer, queryResultBufferCapacity);
            for (int i = 0; i < animationResultBuffer.size; i++) {
                animationResultBuffer[i].size = 0;
                animationValueBuffer[i].size = 0;
            }
        }

        public void Dispose() {
            layoutDebug.Dispose();

            shapeContext.Dispose();
            perFrameBumpAllocator.Dispose();
            // solvers 
            textMeasurement.Dispose();
            layoutSetup.Dispose();
            verticalSpacing.Dispose();
            horizontalSpacing.Dispose();
            layoutAndText.Dispose();
            clippingAndTransformation.Dispose();
            rendering.Dispose();
            layoutBehaviorTypeFontSize.Dispose();
            layoutSizes.Dispose();

            tempBumpAllocatorPool.Dispose();
            SoA_Dispose();
            GeneratedDispose();

            for (int i = 0; i < animationResultBuffer.size; i++) {
                animationResultBuffer[i].Dispose();
                animationValueBuffer[i].Dispose();
            }

            animationResultBuffer.Dispose();
            animationValueBuffer.Dispose();

            for (int i = 0; i < customPropertySolvers.size; i++) {
                customPropertySolvers[i].Dispose();
            }
            
            TypedUnsafe.Dispose(customPropertySolverBuffer, Allocator.Persistent);
            customPropertySolverBuffer = default;

        }

        partial void GeneratedDispose();

        partial void GeneratedClear();

        partial void GeneratedEnsureCapacity();

        partial void GeneratedInitialize();

    }

    internal class OwnedAttribute : Attribute { }

    internal class DisposeAttribute : Attribute { }

    internal class MemClear : Attribute { }

    internal class DisposeContentsAttribute : Attribute { }

    internal class EnsureCapacity : Attribute {

        public string expression;

        public EnsureCapacity(string expression) {
            this.expression = expression;
        }

    }

    internal class BaseCapacity : Attribute {

        public string expression;

        public BaseCapacity(string expression) {
            this.expression = expression;
        }

    }

    // todo -- might want to / need to double buffer this in case user asks for data from here... or we make it  only available during painting? 

    internal class PerFrameBumpAllocateAttribute : Attribute { }

}