using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Prototype;
using UIForia.Style;
using UIForia.Text;
using UIForia.UIInput;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace UIForia {

    internal struct InheritSorter : IComparer<ElementId> {

        public CheckedArray<TraversalInfo> traversalInfo;

        public int Compare(ElementId x, ElementId y) {
            int xDepth = traversalInfo[x.index].depth;
            int yDepth = traversalInfo[y.index].depth;
            return xDepth - yDepth;
        }

    }

    /// <summary>
    /// Sorts elements by their zIndex first, then by ftb index.
    /// Bigger ZIndex first, then later in hierarchy first. The first element is the top-most element.
    /// </summary>
    internal struct HierarchySorter : IComparer<ElementId> {

        public CheckedArray<TraversalInfo> traversalInfo;

        public int Compare(ElementId x, ElementId y) {
            int xZIndex = traversalInfo[x.index].zIndex;
            int yZIndex = traversalInfo[y.index].zIndex;

            if (xZIndex == yZIndex) {
                int xFtbIndex = traversalInfo[x.index].ftbIndex;
                int yFtbIndex = traversalInfo[y.index].ftbIndex;
                return yFtbIndex - xFtbIndex;
            }

            return yZIndex - xZIndex;
        }

    }

    public struct ParallelBatchSizes {

        public static int RunStyleQueries = 16;
        public static int UpdateBlockConditions = 16;
        public static int RemoveInvalidQuerySubscriptions = 16;

    }

    // todo -- delete this 
    public static class TempFontHolder {

        public static FontAssetId Roboto;
        public static FontAssetId OpenSans;

    }

    internal struct ScrollViewInput {

        public ElementId scrollElementId;
        public float targetScrollPercentageX;
        public float targetScrollPercentageY;
        public ElementId scrollIntoViewTarget;

    }

    internal struct ScrollViewOutput {

        public ElementId scrollElementId;
        public float scrollPercentageX;
        public float scrollPercentageY;
        public float overflowXPixels;
        public float overflowYPixels;
        public float contentWidthExtent;
        public float contentHeightExtent;

    }

    internal unsafe partial struct ApplicationLoop : IDisposable {

        [Owned] public AppInfo* appInfo;

        public JobHandle updateHandle;

        public QueryContext queryContext;

        private bool singleThreaded;

        public PrototypeTextRender textRender; // temp -- remove 
        public TextDataTable* textDataTable;

        public int elementMapSize;
        public int maxElementId;

        public void Initialize(UIApplication application) {
            this.textDataTable = application.textDataTable;

            maxElementId = application.MaxElementId;
            int elementCount = application.runtime.activeElementList.size;

            StyleDatabase styleDatabase = application.styleDatabase;
            DataList<QueryTable> queryTables = styleDatabase.queryTables;

            GeneratedInitialize();

            appInfo->Initialize(styleDatabase, elementCount, maxElementId, queryTables.size, application.viewList.size, (message) => Debug.Log("[UIForia:TextEngine] " + message));

            queryContext = new QueryContext() {
                styleDatabase = styleDatabase
            };

            // todo -- make this real by reading out of style db

            string dataPath = (Application.platform == RuntimePlatform.Android) ? Application.dataPath : Application.streamingAssetsPath;

            string roboto = Path.Combine(dataPath, @"Fonts", "Roboto-Regular.ttf");
            string openSans = Path.Combine(dataPath, @"Fonts", "OpenSans-Regular.ttf");

            bool TryLoadFromStreaming(string path, out byte[] bytes) {

                UnityWebRequest request = UnityWebRequest.Get(new Uri(path));

                UnityWebRequestAsyncOperation r = request.SendWebRequest();

                while (!request.downloadHandler.isDone) { }

                if (request.result == UnityWebRequest.Result.Success) {
                    bytes = request.downloadHandler.data;
                    return true;
                }

                bytes = default;
                return false;

            }

            byte[] bytes = default; 

            if (Application.platform == RuntimePlatform.Android) {
                TryLoadFromStreaming(Path.Combine("jar:file://", Application.streamingAssetsPath, @"Fonts", "Roboto-Regular.ttf"), out bytes);
            }
            else {
               bytes = File.ReadAllBytes(roboto);
            }

            TempFontHolder.Roboto = new FontAssetId(appInfo->shapeContext.CreateFontFromBytes(bytes));
            // TempFontHolder.OpenSans = new FontAssetId(appInfo->shapeContext.CreateFontFromFile(openSans));
            SDFFont baseFont = JsonUtility.FromJson<SDFFont>(Resources.Load<TextAsset>("RobotoTmp").text);
            application.textDataTable->fontTable.Add(default);
            application.textDataTable->fontTable.Add(SDFFontUnmanaged.Create(baseFont));

            this.textRender = new PrototypeTextRender(Object.FindObjectOfType<Camera>());

            int cnt = styleDatabase.GetCustomPropertyRange(out PropertyId start);

            appInfo->customPropertySolvers = new DataList<CustomPropertyInfo>(cnt, Allocator.Persistent);

            int totalSize = 0;
            for (int i = 0; i < cnt; i++) {
                PropertyId propertyId = (PropertyId) (start + i);
                CustomPropertyInfo info = new CustomPropertyInfo() {
                    propertyId = propertyId,
                    propertyType = styleDatabase.propertyTypeById[propertyId.index]
                };
                totalSize += info.GetSize();
                appInfo->customPropertySolvers.Add(info);
            }

            byte* buffer = appInfo->customPropertySolverBuffer = TypedUnsafe.Malloc<byte>(totalSize, Allocator.Persistent);

            int offset = 0;
            for (int i = 0; i < appInfo->customPropertySolvers.size; i++) {
                ref CustomPropertyInfo info = ref appInfo->customPropertySolvers.array[i];
                info.CreateSolver(buffer + offset);
                offset += info.GetSize();

                // PropertyId propertyId = (PropertyId) (start + i);
                // PropertyType typeId = styleDatabase.propertyTypeById[propertyId.index];
                //
                // switch (styleDatabase.propertyTypeById[propertyId.index]) {
                //
                //     case PropertyType.Float:
                //         
                //         offset += CreateCustomPropertySolver<CustomPropertySolver_Float>(propertyId, typeId, buffer, offset, ref  appInfo->customPropertySolvers);
                //
                //         break;
                //
                //     default:
                //         throw new ArgumentOutOfRangeException();
                // }
            }

        }

        private static int CreateCustomPropertySolver<T>(PropertyId propertyId, PropertyType propertyType, byte* buffer, int offset, ref DataList<CustomPropertyInfo> customPropertySolvers) where T : unmanaged, ICustomPropertySolver {

            T* solver = (T*) (buffer + offset);
            *solver = default;
            solver->Initialize();

            customPropertySolvers.Add(new CustomPropertyInfo() {
                solver = solver,
                propertyId = propertyId,
                propertyType = propertyType
            });

            return sizeof(T);

        }

        public void Dispose() {
            updateHandle.Complete();
            GeneratedDispose();
        }

        public void CopyThreadData(UIApplication application) {

            maxElementId = application.MaxElementId;

            TypedUnsafe.MemCpy(appInfo->runtimeInfoTable, application.runtimeInfoTable, maxElementId);
            TypedUnsafe.MemCpy(appInfo->states, application.styleStateTable, maxElementId);
            TypedUnsafe.MemCpy(appInfo->templateInfoTable, application.templateInfoTable, maxElementId);
            TypedUnsafe.MemCpy(appInfo->styleInfoTable, application.styleInfoTable, maxElementId);
            TypedUnsafe.MemCpy(appInfo->instancePropertyInfoTable, application.instancePropertyTable, maxElementId);

            TypedUnsafe.MemCpy(appInfo->depthTable, application.depthTable, maxElementId);
            TypedUnsafe.MemCpy(appInfo->elementIdToViewId, application.elementIdToViewId, maxElementId);
            TypedUnsafe.MemCpy(appInfo->elementIdToParentId, application.elementIdToParentId, maxElementId);

            application.styleDatabase.CopyTablePointers(ref appInfo->propertyTables);

            // reset any flags that are per-frame and copy the meta table from runtime table
            RestorePerFrameFlags.RestoreFlags(appInfo->runtimeInfoTable, application.runtimeInfoTable, maxElementId);

        }

        public void Update(float deltaTime, UIApplication application) {

            if (appInfo == null) {
                Initialize(application);
            }

            Size appSize = new Size(application.Width, application.Height);
            MouseState mouseState = application.mouseAdapter.GetMouseState(application.dpiScaleFactor, appSize);

            singleThreaded = true; // maybe compute based on element count?
            JobHandleExtension.SingleThreadedJobs = singleThreaded;
            // this is our thread barrier, we copy all the user side data into separate buffers here which we will use as our source of truth until next frame
            // this allows the user code to continue running while we update everything asynchronously

            maxElementId = application.MaxElementId;
            elementMapSize = LongBoolMap.GetMapSize(maxElementId);
            int activeElementCount = application.runtime.activeElementList.size;

            StyleDatabase styleDatabase = application.styleDatabase;

            appInfo->EnsureCapacity(activeElementCount, maxElementId, styleDatabase.queryTables.size, application.viewList.size);
            appInfo->Clear();

            fixed (StyleId* ptr = application.styleIdAllocator.memory) {
                appInfo->styleIdTable.CopyFrom(ptr, application.styleIdAllocator.MaxAllocation);
            }

            fixed (PropertyContainer* ptr = application.instancePropertyAllocator.memory) {
                appInfo->instancePropertyTable.CopyFrom(ptr, application.instancePropertyAllocator.MaxAllocation);
            }

            appInfo->SoA_SetSize_PerElementId(maxElementId, false);
            appInfo->SoA_SetSize_PerActiveElement(activeElementCount, false);
            appInfo->perFrameLayoutOutput.SoA_SetSize_PerActiveElementId(activeElementCount, false);

            TypedUnsafe.MemClear(appInfo->elementIdToIndex, maxElementId);

            // todo -- find a way to not do all these copies if user promises not to call UIForia stuff outside of binding loop

            appInfo->ClearPerFrameBumpAllocations();
            appInfo->perFrameLayoutData.Allocate(&appInfo->perFrameBumpAllocator, activeElementCount);

            CheckedArray<StyledAttributeChange> attributeChanges = appInfo->perFrameBumpAllocator.AllocateCheckedArray<StyledAttributeChange>(application.attributeChanges.size);

            CheckedArray<CheckedArray<bool>> styleConditionsByViewIndex = appInfo->perFrameBumpAllocator.AllocateCheckedArray<CheckedArray<bool>>(application.viewList.size);

            TypedUnsafe.MemCpy(attributeChanges.array, application.attributeChanges.array, application.attributeChanges.size);
            application.attributeChanges.size = 0;

            appInfo->tempBumpAllocatorPool.Consolidate();

            CopyThreadData(application);

            ElementMap initMap = new ElementMap(appInfo->initMapBuffer.GetArrayPointer(), elementMapSize);
            ElementMap enabledElementMap = new ElementMap(appInfo->activeMapBuffer.GetArrayPointer(), elementMapSize);
            ElementMap rebuildBlocksMap = new ElementMap(appInfo->blockRebuildBuffer.GetArrayPointer(), elementMapSize);
            ElementMap rebuildStylesMap = new ElementMap(appInfo->styleRebuildBuffer.GetArrayPointer(), elementMapSize);
            ElementMap deadOrDisabledMap = new ElementMap(appInfo->invalidatedElementBuffer.GetArrayPointer(), elementMapSize);

            LongBoolMap newQuerySubscriberMap = new LongBoolMap(appInfo->newQuerySubscriberMapBuffer.GetArrayPointer(), LongBoolMap.GetMapSize(styleDatabase.queryTables.size));

            // MaxElementId sized
            CheckedArray<StyleState> stateTable = new CheckedArray<StyleState>(appInfo->states, maxElementId);
            CheckedArray<HierarchyInfo> hierarchyTable = new CheckedArray<HierarchyInfo>(appInfo->hierarchyTable, maxElementId);
            CheckedArray<TraversalInfo> traversalTable = new CheckedArray<TraversalInfo>(appInfo->traversalTable, maxElementId);
            CheckedArray<TemplateInfo> templateInfoTable = new CheckedArray<TemplateInfo>(appInfo->templateInfoTable, maxElementId);
            CheckedArray<StyleInfo> styleInfoTable = new CheckedArray<StyleInfo>(appInfo->styleInfoTable, maxElementId);
            CheckedArray<InstancePropertyInfo> instanceProperties = new CheckedArray<InstancePropertyInfo>(appInfo->instancePropertyInfoTable, maxElementId);
            CheckedArray<ElementId> elementIdToParentId = new CheckedArray<ElementId>(appInfo->elementIdToParentId, maxElementId);
            CheckedArray<ushort> depthTable = new CheckedArray<ushort>(appInfo->depthTable, maxElementId);
            CheckedArray<ushort> elementIdToViewId = new CheckedArray<ushort>(appInfo->elementIdToViewId, maxElementId);
            CheckedArray<RuntimeTraversalInfo> runtimeInfoTable = new CheckedArray<RuntimeTraversalInfo>(appInfo->runtimeInfoTable, maxElementId);
            CheckedArray<int> activeIndexByElementId = new CheckedArray<int>(appInfo->elementIdToIndex, maxElementId);

            // activeElementCount sized
            CheckedArray<int> parentIndexByActiveElementIndex = new CheckedArray<int>(appInfo->parentIndexByActiveElementIndex, activeElementCount);
            CheckedArray<ElementId> elementIdByActiveIndex = new CheckedArray<ElementId>(application.runtime.activeElementList);
            CheckedArray<StyleState> styleStateByActiveIndex = new CheckedArray<StyleState>(appInfo->styleStateByActiveIndex, activeElementCount);
            CheckedArray<int> childCountByActiveIndex = new CheckedArray<int>(appInfo->childCountByActiveIndex, activeElementCount);
            CheckedArray<int> siblingIndexByActiveIndex = new CheckedArray<int>(appInfo->siblingIndexByActiveIndex, activeElementCount);

            // shared with main thread
            CheckedArray<QueryTable> queryTables = new CheckedArray<QueryTable>(styleDatabase.queryTables.GetArrayPointer(), styleDatabase.queryTables.size);
            CheckedArray<QueryPair> queryPairs = new CheckedArray<QueryPair>(styleDatabase.queries.GetArrayPointer(), styleDatabase.queries.size);
            CheckedArray<StyleDesc> styleDescriptions = new CheckedArray<StyleDesc>(styleDatabase.styles.GetArrayPointer(), styleDatabase.styles.size);

            // view sized
            CheckedArray<ElementId> viewRootIds = new CheckedArray<ElementId>(appInfo->viewRootIds.array, application.viewList.size);
            CheckedArray<Rect> viewRects = appInfo->perFrameBumpAllocator.AllocateCheckedArray<Rect>(application.viewList.size);

            int activeTagCount = styleDatabase.conditionTagger.ActiveTagCount();
            bool* styleConditionBuffer = appInfo->perFrameBumpAllocator.AllocateCleared<bool>(activeTagCount * application.viewList.size);

            for (int i = 0; i < viewRects.size; i++) {
                viewRects.array[i] = application.viewList[i].Viewport;
                viewRootIds.array[i] = application.viewList[i].RootElement.elementId;
                styleConditionsByViewIndex[i] = new CheckedArray<bool>(styleConditionBuffer, activeTagCount);
                styleConditionBuffer += activeTagCount;
            }

            StructList<StyleConditionEvaluatorEntry> evaluators = application.styleConditionEvaluators;
            DeviceInfo deviceInfo = new DeviceInfo(1f);
            for (int viewIndex = 0; viewIndex < application.viewList.size; viewIndex++) {
                CheckedArray<bool> viewConditions = styleConditionsByViewIndex[viewIndex];
                for (int evaluatorIndex = 0; evaluatorIndex < evaluators.size; evaluatorIndex++) {
                    StyleConditionEvaluatorEntry evaluator = evaluators[evaluatorIndex];
                    try {
                        viewConditions[evaluator.id] = evaluator.evaluator.Invoke(application.viewList[viewIndex], deviceInfo);
                    }
                    catch (Exception e) {
                        Debug.LogException(e);
                        // just in case
                        viewConditions[evaluator.id] = false;
                    }
                }
            }

            // maybe better as a function pointer? 
            new RemoveDeadVariables() {
                colorVariables = application.colorVariables,
                valueVariables = application.valueVariables,
                textureVariables = application.textureVariables,
                metaInfo = runtimeInfoTable,
            }.Run();

            appInfo->colorVariableList.CopyFrom(application.colorVariables);
            appInfo->valueVariableList.CopyFrom(application.valueVariables);
            appInfo->textureVariableList.CopyFrom(application.textureVariables);

            // variable lists
            CheckedArray<ColorVariable> colorVariables = new CheckedArray<ColorVariable>(appInfo->colorVariableList.GetArrayPointer(), appInfo->colorVariableList.size);
            CheckedArray<ValueVariable> valueVariables = new CheckedArray<ValueVariable>(appInfo->valueVariableList.GetArrayPointer(), appInfo->valueVariableList.size);
            CheckedArray<TextureVariable> textureVariables = new CheckedArray<TextureVariable>(appInfo->textureVariableList.GetArrayPointer(), appInfo->textureVariableList.size);

            ref TextDataTable textTable = ref application.textDataTable[0];

            InitTextTable(ref textTable);

            SolverParameters solverParameters = new SolverParameters() {
                maxElementId = maxElementId,
                deltaMS = (int) (deltaTime * 1000),
                longsPerElementMap = elementMapSize,

                defaultValueIndices = styleDatabase.defaultValueIndices,
                transitionDatabase = styleDatabase.transitionTable,

                traversalTable = traversalTable,
                elementIdToParentId = elementIdToParentId,

                invalidatedElementMap = deadOrDisabledMap,
                initMap = initMap,
                activeMap = enabledElementMap,
                rebuildBlocksMap = rebuildBlocksMap,
                sharedRebuildResult = &appInfo->sharedPropertyUpdates,
                transitionUpdateResult = &appInfo->transitionUpdates,
                instanceRebuildResult = &appInfo->instancePropertyUpdates,

                valueVariables = valueVariables,
                colorVariables = colorVariables,
                textureVariables = textureVariables,
                animationResultBuffer = appInfo->animationResultBuffer,
                animationValueBuffer = appInfo->animationValueBuffer

            };

            // todo -- i need to block the main thread while reading in style data but that doesn't mean we can't submit our scheduled jobs first, we just put a dependency down

            JobHandle createHierarchy = Schedule(new CreateHierarchyJob() {
                hierarchyTable = hierarchyTable,
                runtimeInfoTable = runtimeInfoTable,
                activeElementList = application.runtime.activeElementList,
                depthTable = depthTable,
                rootIds = viewRootIds,
                traversalTable = traversalTable,
            });

            JobHandle updateVariableLists = Schedule(createHierarchy, new UpdateVariableLists() {
                traversalInfo = traversalTable,
                colorVariables = colorVariables,
                valueVariables = valueVariables,
                textureVariables = textureVariables
            });

            JobHandle flattenActiveHierarchy = Schedule(new FlattenHierarchyJob() {
                stateTable = stateTable,
                activeElementIds = elementIdByActiveIndex,
                parentIndexByActiveElementIndex = parentIndexByActiveElementIndex,
                flattenedStateTable = styleStateByActiveIndex,
                activeIndexByElementId = activeIndexByElementId,
                elementIdToParentId = elementIdToParentId,
                childCountByActiveIndex = childCountByActiveIndex,
                siblingIndexByActiveIndex = siblingIndexByActiveIndex,
            });

            JobHandle setupElementMaps = Schedule(new SetupElementMaps() {
                deadOrDisabledMap = deadOrDisabledMap,
                rebuildStyleMap = rebuildStylesMap,
                initMap = initMap,
                activeMap = enabledElementMap,
                metaTable = runtimeInfoTable,
                activeElements = elementIdByActiveIndex
            });

            JobHandle processAttributeChanges = Schedule(new ProcessAttributeChanges() {
                meta = runtimeInfoTable,
                changes = attributeChanges,
                attrTables = styleDatabase.attrTables
            });

            // todo -- we should know which styles are used at this point, can use that to cull some of the queries 

            JobHandle runStyleQueries = flattenActiveHierarchy
                .And(createHierarchy)
                .And(processAttributeChanges)
                .And(setupElementMaps)
                .ThenParallel(new RunStyleQueries() {
                    parallel = new ParallelParams(queryTables.size, ParallelBatchSizes.RunStyleQueries),
                    results = appInfo->queryResultList,
                    childCounts = childCountByActiveIndex,
                    descendentCounts = default,
                    states = styleStateByActiveIndex,
                    hierarchyTable = hierarchyTable,
                    activeMap = enabledElementMap,
                    queryTables = queryTables,
                    activeElementIds = elementIdByActiveIndex,
                    activeIndexByElementId = activeIndexByElementId,
                    attributeTables = styleDatabase.attrTables,
                    parentIndexByActiveElementIndex = parentIndexByActiveElementIndex,
                    siblingIndexByActiveIndex = siblingIndexByActiveIndex,
                    styleConditionsByViewIndex = styleConditionsByViewIndex,
                });

            JobHandle solveSelectors = runStyleQueries;

            JobHandle removeInvalidQuerySubscriptions = ScheduleParallel(setupElementMaps, solveSelectors, new RemoveInvalidatedQuerySubscriptions() {
                parallel = new ParallelParams(queryTables.size, ParallelBatchSizes.RemoveInvalidQuerySubscriptions),
                rebuildStylesMap = rebuildStylesMap,
                deadOrDisabledMap = deadOrDisabledMap,
                queryTableState = appInfo->queryTableSubscriptionList,
            });

            JobHandle solveStyles = Schedule(solveSelectors, setupElementMaps, new CreateStyleUsagesAndQuerySubscriptions() {
                rebuildStylesMap = rebuildStylesMap,
                deadOrDisabledMap = deadOrDisabledMap,
                newSubscriberMap = newQuerySubscriberMap,

                styles = styleDescriptions,
                styleInfoTable = styleInfoTable,
                styleUsages = appInfo->styleUsages,

                queryPairs = queryPairs,
                newQuerySubscriptions = appInfo->newQuerySubscriptions,

                styleUsageToIndex = appInfo->styleUsageToIndex,
                styleUsageIdFreeList = appInfo->styleUsageIdFreeList,
                styleUsageQueryResults = appInfo->styleUsageResults,
                styleUsageIdGenerator = appInfo->styleUsageIdGenerator,
                styleIndexOffset = (int) styleDatabase.numberRangeStart,
                styleIdTable = appInfo->styleIdTable,

            });

            int deltaTimeMS = (int) (deltaTime * 1000f);

            if (application.activeScriptInstances.size > 0) {

                solveSelectors.Complete();
                solveStyles.Complete();
                flattenActiveHierarchy.Complete();

                queryContext.childCounts = childCountByActiveIndex;
                queryContext.activeIndexByElementId = activeIndexByElementId;
                queryContext.elementIdByActiveIndex = elementIdByActiveIndex;
                queryContext.stateByFlattenedElement = stateTable;
                queryContext.styleConditionsByViewIndex = styleConditionsByViewIndex;
                queryContext.siblingIndexByActiveIndex = siblingIndexByActiveIndex;
                queryContext.parentIndexByActiveElementIndex = parentIndexByActiveElementIndex;
                queryContext.runtimeInfoTable = runtimeInfoTable;
                queryContext.elementTable = application.instanceTable;

                // todo -- might need to re-visit attributes here if we allow it
                StructList<ElementId> rootScriptContextList = new StructList<ElementId>();

                // todo -- remove dead scripts & elements here

                for (int i = 0; i < application.activeScriptInstances.size; i++) {
                    UIScriptInstance scriptInstance = application.activeScriptInstances[i];

                    scriptInstance.context.appInfo = appInfo;
                    scriptInstance.context.queryContext = queryContext;

                    rootScriptContextList.size = 1;
                    rootScriptContextList.array[0] = scriptInstance.root.elementId;

                    scriptInstance.Update(deltaTimeMS, rootScriptContextList);

                    if (scriptInstance.IsComplete) {
                        scriptInstance.OnComplete();
                        application.activeScriptInstances.RemoveAt(i--);
                    }

                }

            }

            JobHandle solveInstanceStyles = Schedule(new SolveInstanceProperties() {
                activeElements = elementIdByActiveIndex,
                instancePropertyTable = appInfo->instancePropertyTable,
                properties = instanceProperties,
                totalPropertyTypeCount = styleDatabase.PropertyTypeCount,
                perFrameBumpAllocator = &appInfo->perFrameBumpAllocator,
                instancePropertyUpdates = &appInfo->instancePropertyUpdates
            });

            JobHandle updateBlockConditions = ScheduleParallel(removeInvalidQuerySubscriptions, solveStyles, new UpdateConditionResults() {
                parallel = new ParallelParams(queryTables.size, ParallelBatchSizes.UpdateBlockConditions),
                styleUsageResults = appInfo->styleUsageResults,
                results = appInfo->queryResultList,
                newQuerySubscriptions = appInfo->newQuerySubscriptions,
                querySubscriptionLists = appInfo->queryTableSubscriptionList,
                hasNewSubscriberMap = newQuerySubscriberMap,
                styleUsageToIndex = appInfo->styleUsageToIndex,
                elementIdToIndex = activeIndexByElementId
            });

            JobHandle computeSharedStyleRebuilds = Schedule(updateBlockConditions, new SolveSharedStyleRebuild() {
                recomputeBlocksMap = rebuildBlocksMap,
                styleUsageQueryResults = appInfo->styleUsageResults,
                styleUsages = appInfo->styleUsages,
                blockQueryRequirements = styleDatabase.blockQueryRequirements,
                rebuildStylesMap = rebuildStylesMap,
                blockChanges = &appInfo->blockChanges,
                executionTokenGenerator = appInfo->executionTokenGenerator,
                stateHooks = styleDatabase.stateHooks,
                styleBlocks = styleDatabase.styleBlocks,
                animationCommands = &appInfo->perFrameAnimationCommands
            });

            JobHandle tickAnimationSystem = Schedule(computeSharedStyleRebuilds, new RunAnimationJob() {
                frameDelta = deltaTimeMS,
                animationProperties = styleDatabase.animationProperties,
                animationKeyFrames = styleDatabase.animationKeyFrames,
                animationList = &appInfo->activeAnimationList,
                resultBuffer = appInfo->animationResultBuffer,
                commandsThisFrame = &appInfo->perFrameAnimationCommands,
                keyFrameRanges = styleDatabase.animationKeyFrameRanges,
                animationOptionsTable = styleDatabase.animationOptionsTable,
                propertyTypeCount = styleDatabase.PropertyTypeCount,
                // todo get the size from the style db instead
                usedPropertyMapSize = LongBoolMap.GetMapSize(PropertyParsers.PropertyCount)
            });

            JobHandle sharedPropertyUpdate = Schedule(computeSharedStyleRebuilds, new SolveSharedPropertyUpdate() {
                propertyDatabase = styleDatabase.propertyLocatorBuffer,
                totalPropertyTypeCount = styleDatabase.PropertyTypeCount,
                styleBlocks = styleDatabase.styleBlocks,
                sharedUpdates = &appInfo->sharedPropertyUpdates,
                rebuildBlocksMap = rebuildBlocksMap,
                perFrameBumpAllocator = &appInfo->perFrameBumpAllocator,
                styleUsages = appInfo->styleUsages,
                styleUsageQueryResults = appInfo->styleUsageResults,
                transitionDatabase = styleDatabase.transitionLocatorBuffer,
                transitionUpdates = &appInfo->transitionUpdates
            });

            JobHandle updateAnimationCurves = Schedule(tickAnimationSystem, new CurveJob() {
                resultBuffer = appInfo->animationResultBuffer,
                optionTable = styleDatabase.animationOptionsTable
            });

            JobHandle propertiesReady = JobHandle.CombineDependencies(
                JobHandle.CombineDependencies(updateVariableLists, updateAnimationCurves),
                JobHandle.CombineDependencies(solveInstanceStyles, sharedPropertyUpdate)
            );

            JobHandle solveLayoutSetup = propertiesReady.Then(new SolveLayoutSetupProperties() {
                solverGroup = &appInfo->layoutSetup,
                solverParameters = solverParameters,
                propertyTables = &appInfo->propertyTables,
                styleTables = &appInfo->styleTables,
                tempAllocatorPool = &appInfo->tempBumpAllocatorPool
            });

            JobHandle solveLayoutAndText = propertiesReady.Then(new SolveLayoutAndTextProperties() {
                solverGroup = &appInfo->layoutAndText,
                solverParameters = solverParameters,
                propertyTables = &appInfo->propertyTables,
                styleTables = &appInfo->styleTables,
                tempAllocatorPool = &appInfo->tempBumpAllocatorPool
            });

            JobHandle solveClipAndTransform = propertiesReady.Then(new SolveClippingTransformProperties() {
                solverGroup = &appInfo->clippingAndTransformation,
                solverParameters = solverParameters,
                propertyTables = &appInfo->propertyTables,
                styleTables = &appInfo->styleTables,
                tempAllocatorPool = &appInfo->tempBumpAllocatorPool
            });

            JobHandle solveRendering = propertiesReady.Then(new SolveRenderingProperties() {
                solverGroup = &appInfo->rendering,
                solverParameters = solverParameters,
                propertyTables = &appInfo->propertyTables,
                styleTables = &appInfo->styleTables,
                tempAllocatorPool = &appInfo->tempBumpAllocatorPool
            });

            JobHandle solveTextMeasurement = propertiesReady.Then(new SolveTextMeasurementProperties() {
                solverGroup = &appInfo->textMeasurement,
                solverParameters = solverParameters,
                propertyTables = &appInfo->propertyTables,
                styleTables = &appInfo->styleTables,
                tempAllocatorPool = &appInfo->tempBumpAllocatorPool
            });

            JobHandle updateTraversalInfoZIndex = solveClipAndTransform.Then(new UpdateTraversalInfoZIndexJob() {
                traversalTable = traversalTable,
                parentIndexByActiveElementIndex = parentIndexByActiveElementIndex,
                activeElementList = elementIdByActiveIndex,
                styleTables = &appInfo->styleTables
            });

            // todo -- break this into multiple jobs probably 
            JobHandle solveCustomProperties = Schedule(propertiesReady, new SolveCustomProperties() {
                solverParameters = solverParameters,
                propertyTables = &appInfo->propertyTables,
                tempAllocatorPool = &appInfo->tempBumpAllocatorPool,
                customProperties = appInfo->customPropertySolvers,
                valueVariables = valueVariables,
            });

            solveCustomProperties.Complete(); // todo -- remove this and schedule properly

            // this is a big bottle neck job w/ lots of dependencies 
            // could very likely be threaded per-view 
            JobHandle constructLayoutHierarchy = propertiesReady.Then(new ConstructLayoutHierarchy() {
                solverGroup = &appInfo->layoutBehaviorTypeFontSize,
                solverParameters = solverParameters,
                propertyTables = &appInfo->propertyTables,
                styleTables = &appInfo->styleTables,
                depthTable = depthTable,

                perFrameLayoutData = &appInfo->perFrameLayoutData,
                perFrameBumpAllocator = &appInfo->perFrameBumpAllocator,

                hierarchyTable = hierarchyTable,
                elementIdByIndex = elementIdByActiveIndex,
                activeIndexByElementId = activeIndexByElementId,
                parentIndexByActiveElementIndex = parentIndexByActiveElementIndex,
                output_layoutTree = &appInfo->layoutTree,
                templateInfoTable = templateInfoTable,
                textDataTable = application.textDataTable,

            });

            // todo -- get rid of constructLayoutHierarchy dependency, I think we just need to isolate the em table construction so we that we have it per-elementId not per flattenedIndex
            // todo -- could probably multi-thread this, we know how many texts are dirty up front, but would involve knowing tag ids sooner or sharing the tagger which might be bad 
            JobHandle measureCharacterGroups = constructLayoutHierarchy
                .And(solveTextMeasurement)
                .Then(new MeasureCharacterGroups() {
                    textTable = textTable,
                    stringTagger = appInfo->stringTagger,
                    shapeCache = appInfo->shapeCache,
                    emTable = &appInfo->perFrameLayoutData.emTable,
                    shapeContext = appInfo->shapeContext,
                    styleTables = &appInfo->styleTables,
                    elementIdToIndex = activeIndexByElementId,
                });

            JobHandle placeGridHorizontal = solveLayoutSetup
                .And(constructLayoutHierarchy)
                .Then(new GridLayoutSetup() {
                    targetBoxType = LayoutBoxType.GridHorizontal,
                    appWidth = application.Width,
                    appHeight = application.Height,
                    viewRects = viewRects,
                    layoutTree = &appInfo->layoutTree,
                    styleTables = &appInfo->styleTables,
                    mainAxisInfo = &appInfo->perFrameLayoutInfo.gridHorizontalMainAxis,
                    crossAxisInfo = &appInfo->perFrameLayoutInfo.gridHorizontalCrossAxis,
                    emTablePtr = &appInfo->perFrameLayoutData.emTable,
                    perFrameBumpAllocator = &appInfo->perFrameBumpAllocator,
                    styleDatabaseCellTable = styleDatabase.gridCellTable,
                    styleDatabaseTemplateTable = styleDatabase.gridTemplateTable,
                    elementIdToViewId = elementIdToViewId
                });

            JobHandle placeGridVertical = solveLayoutSetup
                .And(constructLayoutHierarchy)
                .Then(new GridLayoutSetup() {
                    targetBoxType = LayoutBoxType.GridVertical,
                    appWidth = application.Width,
                    appHeight = application.Height,
                    viewRects = viewRects,
                    layoutTree = &appInfo->layoutTree,
                    styleTables = &appInfo->styleTables,
                    mainAxisInfo = &appInfo->perFrameLayoutInfo.gridVerticalMainAxis,
                    crossAxisInfo = &appInfo->perFrameLayoutInfo.gridVerticalCrossAxis,
                    emTablePtr = &appInfo->perFrameLayoutData.emTable,
                    perFrameBumpAllocator = &appInfo->perFrameBumpAllocator,
                    styleDatabaseCellTable = styleDatabase.gridCellTable,
                    styleDatabaseTemplateTable = styleDatabase.gridTemplateTable,
                    elementIdToViewId = elementIdToViewId
                });

            // todo -- see if we can remove layoutSetup dependency 
            // could combine with solving horizontal & vertical spaces instead
            // will have to see how grid / text / radial end up 
            JobHandle borderSizes = solveLayoutSetup.And(constructLayoutHierarchy).Then(new SolveBorderSize() {
                // should just solve padding/border itself probably 
                layoutTree = &appInfo->layoutTree,
                styleTables = &appInfo->styleTables,
                emTablePtr = &appInfo->perFrameLayoutData.emTable,
                borderTopPtr = &appInfo->perFrameLayoutData.borderTops,
                borderRightPtr = &appInfo->perFrameLayoutData.borderRights,
                borderBottomPtr = &appInfo->perFrameLayoutData.borderBottoms,
                borderLeftPtr = &appInfo->perFrameLayoutData.borderLefts,
            });

            JobHandle solveSizes = constructLayoutHierarchy.Then(new SolveLayoutSizes() {
                // I think min/max sizes need to be handled here also 
                // note -- needs pref sizes & aspect ratio, can solve those here instead of waiting all layout setup properties 
                appWidth = application.Height, // todo -- use user defined size instead
                appHeight = application.Width, // todo -- use user defined size instead
                layoutTree = &appInfo->layoutTree,
                styleTables = &appInfo->styleTables,
                viewRects = viewRects,
                emTablePtr = &appInfo->perFrameLayoutData.emTable,
                lineHeightTablePtr = &appInfo->perFrameLayoutData.lineHeightTable,
                solvedWidthPtr = &appInfo->perFrameLayoutData.solvedWidths,
                solvedHeightPtr = &appInfo->perFrameLayoutData.solvedHeights,
                solvedMinWidthPtr = &appInfo->perFrameLayoutData.minWidths,
                solvedMaxWidthPtr = &appInfo->perFrameLayoutData.maxWidths,
                solvedMinHeightPtr = &appInfo->perFrameLayoutData.minHeights,
                solvedMaxHeightPtr = &appInfo->perFrameLayoutData.maxHeights,
                solverGroup = &appInfo->layoutSizes,
                solverParameters = solverParameters,
                propertyTables = &appInfo->propertyTables,
                templateInfo = templateInfoTable,
                elementIdToViewId = elementIdToViewId,
            });

            JobHandle horizontalSizes = constructLayoutHierarchy.Then(new SolveHorizontalSpacingSizes() {
                layoutTree = &appInfo->layoutTree,
                emTablePtr = &appInfo->perFrameLayoutData.emTable,
                styleTables = &appInfo->styleTables,
                solverGroup = &appInfo->horizontalSpacing,
                solverParameters = solverParameters,
                propertyTables = &appInfo->propertyTables,
                viewRects = viewRects,
                appWidth = application.Height, // todo -- use user defined size instead
                appHeight = application.Width, // todo -- use user defined size instead
                marginRightPtr = &appInfo->perFrameLayoutData.marginRights,
                marginLeftPtr = &appInfo->perFrameLayoutData.marginLefts,
                paddingRightPtr = &appInfo->perFrameLayoutData.paddingRights,
                paddingLeftPtr = &appInfo->perFrameLayoutData.paddingLefts,
                spaceBetweenPtr = &appInfo->perFrameLayoutData.spaceBetweenHorizontal,
                spaceCollapsePtr = &appInfo->perFrameLayoutData.spaceCollapseHorizontal,
                elementIdToViewId = elementIdToViewId
            });

            JobHandle verticalSizes = constructLayoutHierarchy.Then(new SolveVerticalSpacingSizes() {
                layoutTree = &appInfo->layoutTree,
                emTablePtr = &appInfo->perFrameLayoutData.emTable,
                styleTables = &appInfo->styleTables,
                solverGroup = &appInfo->verticalSpacing,
                solverParameters = solverParameters,
                propertyTables = &appInfo->propertyTables,
                viewRects = viewRects,
                appWidth = application.Height, // todo -- use user defined size instead
                appHeight = application.Width, // todo -- use user defined size instead
                marginTopPtr = &appInfo->perFrameLayoutData.marginTops,
                marginBottomPtr = &appInfo->perFrameLayoutData.marginBottoms,
                paddingTopPtr = &appInfo->perFrameLayoutData.paddingTops,
                paddingBottomPtr = &appInfo->perFrameLayoutData.paddingBottoms,
                spaceBetweenPtr = &appInfo->perFrameLayoutData.spaceBetweenVertical,
                spaceCollapsePtr = &appInfo->perFrameLayoutData.spaceCollapseVertical,
                elementIdToViewId = elementIdToViewId
            });

            JobHandle layout = horizontalSizes
                .And(horizontalSizes)
                .And(verticalSizes)
                .And(borderSizes)
                .And(placeGridHorizontal)
                .And(placeGridVertical)
                .And(solveSizes)
                .And(measureCharacterGroups)
                .Then(new PerformQuadPassLayout() {
                    layoutTree = &appInfo->layoutTree,
                    perFrameLayoutData = &appInfo->perFrameLayoutData,
                    perFrameLayoutBoxes = &appInfo->perFrameLayoutInfo,
                    perFrameLayoutOutput = &appInfo->perFrameLayoutOutput,
                    animationValueBuffer = appInfo->animationValueBuffer,
                    textDataTable = application.textDataTable,
                    viewRects = viewRects,
                });

            layout.Then(new GarbageCollectTextData() {
                frameId = application.runtime.currentFrameId,
                shapeCache = appInfo->shapeCache,
                tagger = appInfo->stringTagger,
            }).Complete();

            // when do we scroll into view? 
            // after alignment but before matrices?
            // does scroll into view break for aligned items? possibly, we would scroll it's default layout area into view, not its aligned area?
            // scroll into view would compute the alignment, so must happen before then
            // if 

            JobHandle horizontalAlignment = layout.Then(new ApplyHorizontalAlignments() {
                layoutOutput = &appInfo->perFrameLayoutOutput,
                activeMap = enabledElementMap,
                screenWidth = application.Width,
                screenHeight = application.Height,
                viewRects = viewRects,
                solverGroup = &appInfo->clippingAndTransformation,
                styleTables = &appInfo->styleTables,
                layoutTree = &appInfo->layoutTree,
                bumpPool = &appInfo->tempBumpAllocatorPool,
                emTablePtr = &appInfo->perFrameLayoutData.emTable,
                mousePosition = new float2(Input.mousePosition.x, application.Height - Input.mousePosition.y), // todo -- fix mouse
                elementIdToViewId = elementIdToViewId,
            });

            JobHandle verticalAlignment = layout.Then(new ApplyVerticalAlignments() {
                layoutOutput = &appInfo->perFrameLayoutOutput,
                activeMap = enabledElementMap,
                screenWidth = application.Width,
                screenHeight = application.Height,
                viewRects = viewRects,
                solverGroup = &appInfo->clippingAndTransformation,
                styleTables = &appInfo->styleTables,
                layoutTree = &appInfo->layoutTree,
                bumpPool = &appInfo->tempBumpAllocatorPool,
                emTablePtr = &appInfo->perFrameLayoutData.emTable,
                mousePosition = new float2(Input.mousePosition.x, application.Height - Input.mousePosition.y), // todo -- fix mouse 
                elementIdToViewId = elementIdToViewId,
            });

            JobHandle computeMatrices = horizontalAlignment.And(verticalAlignment).Then(new ComputeMatrices() {
                viewRects = viewRects,
                layoutTree = &appInfo->layoutTree,
                localPositions = appInfo->perFrameLayoutOutput.localPositions,
                localMatrices = appInfo->perFrameLayoutOutput.localMatrices,
                worldMatrices = appInfo->perFrameLayoutOutput.worldMatrices
            });

            // baked this into clipping for now 
            // JobHandle computeBounds = computeMatrices.Then(new ComputeBounds() {
            //     worldMatrices = appInfo->perFrameLayoutOutput.worldMatrices,
            //     bounds = appInfo->perFrameLayoutOutput.bounds,
            //     sizes = appInfo->perFrameLayoutOutput.sizes,
            // });

            JobHandle computeCulling = computeMatrices.Then(new ComputeClipping() {
                screenWidth = application.Width,
                screenHeight = application.Height,
                layoutTree = &appInfo->layoutTree,
                styleTables = &appInfo->styleTables,
                perFrameLayoutOutput = &appInfo->perFrameLayoutOutput,
                lockedBumpAllocator = &appInfo->perFrameBumpAllocator,
            });

            // Vector3 rawMouse = //  Input.mousePosition;
            // float2 mouse = new float2(rawMouse.x, application.Height - rawMouse.y); // todo -- dpi

            JobHandle inputQuery = JobHandle.CombineDependencies(computeCulling, updateTraversalInfoZIndex).Then(new InputQuery() {
                point = mouseState.mousePosition,
                maxElementId = maxElementId,
                elementIdByActiveIndex = elementIdByActiveIndex,
                elementIdToParentId = elementIdToParentId,
                styleTables = &appInfo->styleTables,
                traversalTable = traversalTable,
                layoutTree = &appInfo->layoutTree,
                lockedBumpAllocator = &appInfo->perFrameBumpAllocator,
                perFrameLayoutOutput = &appInfo->perFrameLayoutOutput,
            });

            JobHandle allPropertiesSolved = JobHandle.CombineDependencies(
                JobHandle.CombineDependencies(solveLayoutSetup, solveLayoutAndText),
                JobHandle.CombineDependencies(solveClipAndTransform, solveRendering)
            );

            // scrolling

            inputQuery.Complete();
            allPropertiesSolved.Complete();
            layout.Complete();

            appInfo->perFrameAnimationCommands.size = 0;

            application.ResizeLayoutIdBasedBuffer(activeElementCount);

            TypedUnsafe.MemCpy(application.layoutIndexByElementId, appInfo->layoutTree.elementIdToLayoutIndex);
            TypedUnsafe.MemCpy(application.layoutSizeByLayoutIndex, appInfo->perFrameLayoutOutput.sizes);
            TypedUnsafe.MemCpy(application.layoutBordersByLayoutIndex, appInfo->perFrameLayoutOutput.borders);
            TypedUnsafe.MemCpy(application.layoutPaddingsByLayoutIndex, appInfo->perFrameLayoutOutput.paddings);
            TypedUnsafe.MemCpy(application.layoutLocalPositionByLayoutIndex, appInfo->perFrameLayoutOutput.localPositions);
            TypedUnsafe.MemCpy(application.layoutBoundsByLayoutIndex, appInfo->perFrameLayoutOutput.bounds);
            TypedUnsafe.MemCpy(application.layoutLocalMatrixByLayoutIndex, appInfo->perFrameLayoutOutput.localMatrices);
            TypedUnsafe.MemCpy(application.layoutWorldMatrixByLayoutIndex, appInfo->perFrameLayoutOutput.worldMatrices);

            // todo -- find this a real home

            CheckedArray<InputQueryResult> mouseQueryResults = appInfo->perFrameLayoutOutput.mouseQueryResults;

            for (int i = 0; i < mouseQueryResults.size; i++) {
                if (mouseQueryResults.array[i].requiresCustomHandling) {
                    // todo -- if implements some interface, remove if check fails 
                }
            }

            application.runtime.ProcessMouseInput(mouseState, mouseQueryResults, enabledElementMap);
            // application.keyboardAdapter.keyEventQueue.Clear(); // weird?

            application.textEditor.Update();

            appInfo->perFrameLayoutOutput.mouseQueryResults = default;
        }

        // todo -- put this in a job or at the very least through burst 
        private void InitTextTable(ref TextDataTable textTable) { // make sure worker thread can hold info for main thread

            // if element for entry disabled or destroyed -> free worker memory if present
            // if content changed for entry -> free worker memory if present 

            if (textTable.workerEntries.size != textTable.mainThreadEntries.size) {
                int start = textTable.workerEntries.size;
                int length = textTable.mainThreadEntries.size - start;

                // init new entries to unused 

                textTable.workerEntries.SetSize(textTable.mainThreadEntries.size);
                TypedUnsafe.MemClear(textTable.workerEntries.GetPointer(start), length);

            }

            textTable.activeTextIndices.size = 0;
            textTable.activeTextIndices.EnsureCapacity(textTable.mainThreadEntries.size);

            textTable.elementIdToTextId.Clear();
            textTable.boxIndexToTextId.Clear();

            // 1 because 0 is an invalid id 
            for (int i = 1; i < textTable.mainThreadEntries.size; i++) {

                ref MainThreadTextEntry mainThreadEntry = ref textTable.mainThreadEntries.Get(i);
                ref TextDataEntry workerEntry = ref textTable.workerEntries.Get(i);

                if (ElementSystem.IsDeadOrDisabled(mainThreadEntry.elementId, appInfo->runtimeInfoTable)) {

                    if (workerEntry.buffer != null) {
                        textTable.workerThreadListData.Free(workerEntry.buffer, workerEntry.bufferCapacity);
                        workerEntry.buffer = null;
                        workerEntry.bufferCapacity = 0;
                    }

                    if (workerEntry.runList.array != null) {
                        int capacity = textTable.workerThreadListData.GetCapacityFromSize<TextCharacterRun>(workerEntry.runList.size);
                        textTable.workerThreadListData.Free(workerEntry.runList.array, capacity);
                        workerEntry.runList = default;
                    }

                    continue;
                }

                // todo -- this whole method could use some love I think 
                workerEntry.explicitlyDirty = textTable.mainThreadDirtyElements.Get(i) || workerEntry.buffer == null; // buffer will be null if was disabled and now enable 

                if (workerEntry.explicitlyDirty) {

                    int symbolCount = mainThreadEntry.symbolLength == 0 ? 1 : mainThreadEntry.symbolLength;

                    bool hasCursor = mainThreadEntry.cursorIndex > -1;
                    if (hasCursor) {
                        // before caret
                        // caret
                        // between caret-selection
                        // selection
                        // after selection 
                        symbolCount += 4;
                    }

                    int byteCount = mainThreadEntry.dataLength * sizeof(char) + symbolCount * sizeof(TextSymbol);

                    textTable.workerThreadListData.Reallocate(ref workerEntry.buffer, ref workerEntry.bufferCapacity, byteCount);
                    // runs will be reallocated in the update loop so we don't need to do anything for that

                    workerEntry.elementId = mainThreadEntry.elementId;
                    workerEntry.dataLength = mainThreadEntry.dataLength;
                    workerEntry.symbolLength = symbolCount;

                    CheckedArray<TextSymbol> symbols = workerEntry.GetSymbols();
                    CheckedArray<char> data = workerEntry.GetDataBuffer();

                    TypedUnsafe.MemCpy(data.array, mainThreadEntry.dataBuffer, mainThreadEntry.dataLength);

                    if (mainThreadEntry.symbolLength == 0) {
                        if (hasCursor) {

                            bool hasSelection = mainThreadEntry.selectionIndex > -1;
                            int selectionLength = hasSelection
                                ? math.abs(mainThreadEntry.selectionIndex - mainThreadEntry.cursorIndex)
                                : 0;

                            int fromIndex = hasSelection
                                ? math.min(mainThreadEntry.cursorIndex, mainThreadEntry.selectionIndex)
                                : mainThreadEntry.cursorIndex;

                            symbols[0] = new TextSymbol() {
                                symbolType = SymbolType.CharacterGroup,
                                dataLength = math.min(workerEntry.dataLength, fromIndex)
                            };
                            symbols[1] = new TextSymbol() {
                                symbolType = SymbolType.PushCursor,
                                dataLength = 0
                            };
                            symbols[2] = new TextSymbol() {
                                symbolType = SymbolType.CharacterGroup,
                                dataLength = selectionLength
                            };
                            symbols[3] = new TextSymbol() {
                                symbolType = SymbolType.PopCursor,
                                dataLength = 0
                            };
                            symbols[4] = new TextSymbol() {
                                symbolType = SymbolType.CharacterGroup,
                                dataLength = mainThreadEntry.dataLength - fromIndex - selectionLength
                            };
                        }
                        else {
                            symbols[0] = new TextSymbol() {
                                symbolType = SymbolType.CharacterGroup,
                                dataLength = mainThreadEntry.dataLength
                            };
                        }
                    }
                    else {
                        TypedUnsafe.MemCpy(symbols.array, mainThreadEntry.symbolPtr, mainThreadEntry.symbolLength);
                    }

                }

                textTable.elementIdToTextId.Add(workerEntry.elementId, new TextId(i));
                textTable.activeTextIndices.AddUnchecked(new TextId(i));

            }

            textTable.mainThreadDirtyElements.Clear();
        }

        private LightList<Mesh> meshes;
        private LightList<Material> materials;
        private Mesh cursorMesh;
        private Material cursorMaterial;

        private static readonly int s_Property = Shader.PropertyToID("_Color");
        private static readonly int s_BorderSize = Shader.PropertyToID("_BorderSize");
        private static readonly int s_ClipRect = Shader.PropertyToID("_ClipRect");
        private static readonly int s_ScreenHeight = Shader.PropertyToID("_ScreenHeight");

        public void RenderEditor(UIApplication application, CommandBuffer commandBuffer) {

            updateHandle.Complete();
            updateHandle = default;
            float screenHeight = application.Height;

            if (meshes == null) {
                meshes = new LightList<Mesh>();
                materials = new LightList<Material>();
            }

            if (meshes.size < appInfo->layoutTree.elementCount) {

                Material material = Resources.Load<Material>("LayoutDebug");

                int diff = appInfo->layoutTree.elementCount - meshes.size;
                meshes.EnsureAdditionalCapacity(diff);
                materials.EnsureAdditionalCapacity(diff);
                int start = meshes.size;
                meshes.size = appInfo->layoutTree.elementCount;
                materials.size = appInfo->layoutTree.elementCount;
                for (int i = start; i < appInfo->layoutTree.elementCount; i++) {
                    meshes[i] = new Mesh();
                    meshes[i].MarkDynamic();
                    materials[i] = new Material(material);
                }
            }

            for (int i = 0; i < appInfo->layoutTree.elementCount; i++) {

                Mesh mesh = meshes[i];
                Material material = materials[i];

                ElementId elementId = application.runtime.activeElementList[i];
                int layoutIndex = appInfo->layoutTree.elementIdToLayoutIndex[elementId.index];

                Color color = (Color) appInfo->styleTables.BackgroundColor[elementId.index];
                Size size = appInfo->perFrameLayoutOutput.sizes[layoutIndex];

                float4x4 matrix = appInfo->perFrameLayoutOutput.worldMatrices[layoutIndex];

                if (appInfo->layoutTree.nodeList[layoutIndex].layoutBoxType == LayoutBoxType.TextHorizontal) {
                    textDataTable->elementIdToTextId.TryGetValue(elementId, out TextId textId);
                    Color textColor = appInfo->styleTables.TextColor[elementId.index];
                    textRender.DrawText(appInfo->shapeCache, mesh, ref textDataTable[0], textId, matrix, null, textColor);

                    if (application.instanceTable[elementId.index] is SelectableText selectableText) {

                        if (selectableText.cursor.showCursor) {
                            float2 cursorPosition = selectableText.GetCursorPosition();
                            float cursorHeight = selectableText.GetCursorHeight();

                            cursorMesh = cursorMesh ?? new Mesh();
                            cursorMaterial = cursorMaterial ?? Resources.Load<Material>("LayoutDebug");

                            cursorMesh.vertices = new[] {
                                new Vector3(0, 0, 0),
                                new Vector3(1, 0, 0),
                                new Vector3(1, -cursorHeight, 0),
                                new Vector3(0, -cursorHeight, 0),
                            };

                            cursorMesh.uv = new[] {
                                new Vector2(0, 0),
                                new Vector2(1, 0),
                                new Vector2(1, 1),
                                new Vector2(0, 1),
                            };
                            // todo -- figure out pivot etc, likely wants to be rendered with pivot default at center 
                            AxisAlignedBounds2D aabb = appInfo->perFrameLayoutOutput.clippers[appInfo->perFrameLayoutOutput.clipperIndex[i]].aabb;

                            cursorMesh.triangles = new[] {0, 1, 2, 2, 3, 0};
                            cursorMaterial.SetColor(s_Property, Color.black);
                            cursorMaterial.SetVector(s_BorderSize, new Vector4(0, 0, 0, 0));
                            cursorMaterial.SetVector(s_ClipRect, new Vector4(aabb.xMin, aabb.yMin, aabb.xMax, aabb.yMax));
                            cursorMaterial.SetFloat(s_ScreenHeight, screenHeight);
                            float4x4 translate = float4x4.Translate(new float3(cursorPosition.x, cursorPosition.y, 0));
                            commandBuffer.DrawMesh(cursorMesh, math.mul(matrix, translate), cursorMaterial, 0, 0);

                        }
                    }

                }
                else {
                    if (size.height <= 0 || size.width <= 0) continue;
                    if (color.a <= 0) continue;

                    float borderTop = appInfo->perFrameLayoutData.borderTops[layoutIndex] / size.height;
                    float borderRight = 1 - appInfo->perFrameLayoutData.borderRights[layoutIndex] / size.width;
                    float borderBottom = 1 - appInfo->perFrameLayoutData.borderBottoms[layoutIndex] / size.height;
                    float borderLeft = appInfo->perFrameLayoutData.borderLefts[layoutIndex] / size.width;

                    AxisAlignedBounds2D aabb = appInfo->perFrameLayoutOutput.clippers[appInfo->perFrameLayoutOutput.clipperIndex[layoutIndex]].aabb;
                    material.SetColor(s_Property, color);
                    material.SetVector(s_BorderSize, new Vector4(borderTop, borderRight, borderBottom, borderLeft));
                    material.SetVector(s_ClipRect, new Vector4(aabb.xMin, aabb.yMin, aabb.xMax, aabb.yMax));
                    material.SetFloat(s_ScreenHeight, screenHeight);

                    mesh.vertices = new[] {
                        new Vector3(0, 0, 0),
                        new Vector3(size.width, 0, 0),
                        new Vector3(size.width, -size.height, 0),
                        new Vector3(0, -size.height, 0),
                    };

                    meshes[i].uv = new[] {
                        new Vector2(0, 0),
                        new Vector2(1, 0),
                        new Vector2(1, 1),
                        new Vector2(0, 1),
                    };
                    // todo -- figure out pivot etc, likely wants to be rendered with pivot default at center 

                    meshes[i].triangles = new[] {0, 1, 2, 2, 3, 0};

                    commandBuffer.DrawMesh(mesh, matrix, material, 0, 0);
                }

            }

        }

        public void Render(UIApplication application, Camera camera) {

            updateHandle.Complete();
            updateHandle = default;

            if (meshes == null) {
                meshes = new LightList<Mesh>();
                materials = new LightList<Material>();
            }

            if (meshes.size < appInfo->layoutTree.elementCount) {

                Material material = Resources.Load<Material>("LayoutDebug");

                int diff = appInfo->layoutTree.elementCount - meshes.size;
                meshes.EnsureAdditionalCapacity(diff);
                materials.EnsureAdditionalCapacity(diff);
                int start = meshes.size;
                meshes.size = appInfo->layoutTree.elementCount;
                materials.size = appInfo->layoutTree.elementCount;
                for (int i = start; i < appInfo->layoutTree.elementCount; i++) {
                    meshes[i] = new Mesh();
                    meshes[i].MarkDynamic();
                    materials[i] = new Material(material);
                }
            }

            camera.orthographic = true;
            camera.orthographicSize = application.Height * 0.5f;

            Vector3 position = camera.transform.position;
            camera.transform.position = new Vector3(application.Width / 2f, -application.Height / 2f, -2f);

            TempList<ElementId> renderOrder = TypedUnsafe.MallocUnsizedTempList<ElementId>(appInfo->layoutTree.elementCount, Allocator.Temp);

            for (int i = 0; i < appInfo->layoutTree.elementCount; i++) {
                renderOrder.array[renderOrder.size++] = application.runtime.activeElementList[i];
            }

            NativeSortExtension.Sort(renderOrder.array, renderOrder.size, new HierarchySorter() {
                traversalInfo = new CheckedArray<TraversalInfo>(appInfo->traversalTable, maxElementId)
            });

            for (int i = renderOrder.size - 1; i >= 0; i--) {

                Mesh mesh = meshes[i];
                Material material = materials[i];

                mesh.Clear(false);

                ElementId elementId = renderOrder[i];
                int layoutIndex = appInfo->layoutTree.elementIdToLayoutIndex[elementId.index];

                Color color = (Color) appInfo->styleTables.BackgroundColor[elementId.index];
                Size size = appInfo->perFrameLayoutOutput.sizes[layoutIndex];

                float4x4 matrix = appInfo->perFrameLayoutOutput.worldMatrices[layoutIndex];

                if (appInfo->layoutTree.nodeList[layoutIndex].layoutBoxType == LayoutBoxType.TextHorizontal) {
                    textDataTable->elementIdToTextId.TryGetValue(elementId, out TextId textId);
                    Color textColor = appInfo->styleTables.TextColor[elementId.index];
                    textRender.DrawText(appInfo->shapeCache, meshes[i], ref textDataTable[0], textId, matrix, null, textColor);

                    if (application.instanceTable[elementId.index] is SelectableText selectableText) {
                        float screenHeight = application.Height;

                        if (selectableText.cursor.showCursor) {
                            float2 cursorPosition = selectableText.GetCursorPosition();
                            float cursorHeight = selectableText.GetCursorHeight();

                            cursorMesh = cursorMesh ?? new Mesh();
                            cursorMaterial = cursorMaterial ?? Resources.Load<Material>("LayoutDebug");

                            cursorMesh.vertices = new[] {
                                new Vector3(0, 0, 0),
                                new Vector3(1, 0, 0),
                                new Vector3(1, -cursorHeight, 0),
                                new Vector3(0, -cursorHeight, 0),
                            };

                            cursorMesh.uv = new[] {
                                new Vector2(0, 0),
                                new Vector2(1, 0),
                                new Vector2(1, 1),
                                new Vector2(0, 1),
                            };
                            // todo -- figure out pivot etc, likely wants to be rendered with pivot default at center 
                            AxisAlignedBounds2D aabb = appInfo->perFrameLayoutOutput.clippers[appInfo->perFrameLayoutOutput.clipperIndex[i]].aabb;

                            cursorMesh.triangles = new[] {0, 1, 2, 2, 3, 0};
                            cursorMaterial.SetColor(s_Property, application.instanceTable[elementId.index].GetColor("FeatureShowcaseModule:caretColor"));
                            cursorMaterial.SetVector(s_BorderSize, new Vector4(0, 0, 0, 0));
                            cursorMaterial.SetVector(s_ClipRect, new Vector4(aabb.xMin, aabb.yMin, aabb.xMax, aabb.yMax));
                            cursorMaterial.SetFloat(s_ScreenHeight, screenHeight);
                            float4x4 translate = float4x4.Translate(new float3(cursorPosition.x, cursorPosition.y, 0));
                            cursorMaterial.SetPass(0);
                            Graphics.DrawMeshNow(cursorMesh, math.mul(matrix, translate));

                        }
                    }
                }
                else {
                    if (size.height <= 0 || size.width <= 0) continue;
                    if (color.a <= 0) continue;

                    float borderTop = appInfo->perFrameLayoutData.borderTops[layoutIndex] / size.height;
                    float borderRight = 1 - appInfo->perFrameLayoutData.borderRights[layoutIndex] / size.width;
                    float borderBottom = 1 - appInfo->perFrameLayoutData.borderBottoms[layoutIndex] / size.height;
                    float borderLeft = appInfo->perFrameLayoutData.borderLefts[layoutIndex] / size.width;

                    AxisAlignedBounds2D aabb = appInfo->perFrameLayoutOutput.clippers[appInfo->perFrameLayoutOutput.clipperIndex[layoutIndex]].aabb;
                    material.SetColor(s_Property, color);
                    material.SetVector(s_BorderSize, new Vector4(borderTop, borderRight, borderBottom, borderLeft));
                    material.SetVector(s_ClipRect, new Vector4(aabb.xMin, aabb.yMin, aabb.xMax, aabb.yMax));
                    material.SetFloat(s_ScreenHeight, Screen.height);

                    try {
                        meshes[i].vertices = new[] {
                            new Vector3(0, 0, 0),
                            new Vector3(size.width, 0, 0),
                            new Vector3(size.width, -size.height, 0),
                            new Vector3(0, -size.height, 0),
                        };

                        meshes[i].uv = new[] {
                            new Vector2(0, 0),
                            new Vector2(1, 0),
                            new Vector2(1, 1),
                            new Vector2(0, 1),
                        };
                        // todo -- figure out pivot etc, likely wants to be rendered with pivot default at center 

                        meshes[i].triangles = new[] {0, 1, 2, 2, 3, 0};

                        material.SetPass(0);
                    }
                    catch (Exception e) {
                        Debugger.Break();
                    }

                    Graphics.DrawMeshNow(mesh, matrix);
                }

            }

            camera.transform.position = position;

        }

        private JobHandle Schedule<T>(JobHandle dependency, in T job) where T : struct, IJob {
            if (singleThreaded) {
                job.Run();
                return default;
            }

            return job.Schedule(dependency);
        }

        private JobHandle Schedule<T>(JobHandle d0, JobHandle d1, in T job) where T : struct, IJob {
            if (singleThreaded) {
                job.Run();
                return default;
            }

            return job.Schedule(JobHandle.CombineDependencies(d0, d1));
        }

        private JobHandle Schedule<T>(in T job) where T : struct, IJob {
            if (singleThreaded) {
                job.Run();
                return default;
            }

            return job.Schedule();
        }

        private JobHandle ScheduleParallel<T>(in T job) where T : struct, IUIForiaParallel {
            int itemCount = job.parallel.itemCount < 1 ? 1 : job.parallel.itemCount;
            int batchSize = job.parallel.minBatchSize < 1 ? 1 : job.parallel.minBatchSize;
            if (singleThreaded) {
                job.Run();
                return default;
            }

            return job.ScheduleBatch(itemCount, batchSize);
        }

        private JobHandle ScheduleParallel<T>(JobHandle d0, JobHandle d1, in T job) where T : struct, IUIForiaParallel {
            int itemCount = job.parallel.itemCount < 1 ? 1 : job.parallel.itemCount;
            int batchSize = job.parallel.minBatchSize < 1 ? 1 : job.parallel.minBatchSize;
            if (singleThreaded) {
                job.Run();
                return default;
            }

            return job.ScheduleBatch(itemCount, batchSize, JobHandle.CombineDependencies(d0, d1));
        }

        private JobHandle ScheduleParallel<T>(JobHandle d0, in T job) where T : struct, IUIForiaParallel {
            int itemCount = job.parallel.itemCount < 1 ? 1 : job.parallel.itemCount;
            int batchSize = job.parallel.minBatchSize < 1 ? 1 : job.parallel.minBatchSize;
            if (singleThreaded) {
                job.Run();
                return default;
            }

            return job.ScheduleBatch(itemCount, batchSize, d0);
        }

        partial void GeneratedInitialize();

        partial void GeneratedClear();

        partial void GeneratedDispose();

        partial void CopyThreadDataGenerated();

        partial void GeneratedEnsureCapacity();

    }

}