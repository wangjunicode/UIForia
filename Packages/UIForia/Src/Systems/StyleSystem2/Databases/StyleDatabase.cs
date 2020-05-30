using System;
using System.Collections.Generic;
using Packages.UIForia.Util.Unsafe;
using UIForia.NewStyleParsing;
using UIForia.Parsing;
using UIForia.Src;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEngine.Assertions;

namespace UIForia {

    public class StyleDatabase : IDisposable {

        private const string k_ImplicitStyleSheetName = "_IMPLICIT_STYLE_SHEET_";

        private readonly ushort dbIndex;

        public ByteBuffer buffer_staticStyleProperties;

        public StringInternSystem internSystem;
        public DataList<StaticStyleInfo> table_StyleInfo;
        public DataList<StyleSheetInfo> table_StyleSheetInfo;

        // maybe introduce a new Table type that grows at a non doubling rate after initializing
        public DataList<ModuleInfo> table_ModuleInfo;
        public UnmanagedLongMap<int> table_StyleSheetMap;
        public UnmanagedLongMap<StyleId> table_StyleIdMap;

        public SelectorDatabase selectorDatabase;

        private StructList<PendingSelectorFilter> selectorFiltersToBuild;
        private IList<Module> moduleList;

        internal static readonly List<StyleDatabase> s_DebugDatabases = new List<StyleDatabase>();

        public StyleDatabase(StringInternSystem internSystem) {
            this.internSystem = internSystem;
            this.moduleList = ModuleSystem.GetModuleList();

            // todo -- move this to a builder, doesn't belong here
            // selectorFiltersToBuild = new StructList<PendingSelectorFilter>(64);

            // todo -- defer to init when we know how big our data is
            // selectorDatabase = SelectorDatabase.Create();

            // AddStyle("__INVALID__", 0, default); // 0 index should be considered invalid

            this.dbIndex = (ushort) s_DebugDatabases.Count;
            s_DebugDatabases.Add(this);
        }

        public unsafe BurstableStyleDatabase GetBurstable() {
            return new BurstableStyleDatabase() {
                sharedStyleTable = new StyleTable<StaticStyleInfo>(table_StyleInfo.GetArrayPointer()),
                staticStyleProperties = buffer_staticStyleProperties.data,
                selectorStyleTable = new SelectorTable<SelectorStyleEffect>() // todo -- hook up selector styles here
            };
        }

        public void Dispose() {

            table_ModuleInfo.Dispose();
            table_StyleSheetInfo.Dispose();
            buffer_staticStyleProperties.Dispose();

            table_StyleInfo.Dispose();
            table_StyleSheetMap.Dispose();
            table_StyleIdMap.Dispose();
            selectorDatabase.Dispose();
            s_DebugDatabases.Remove(this);

        }

        public int styleCount {
            get => table_StyleInfo.size; // todo -- this will change
        }

        public void Initialize() {
            BuildPendingSelectorFilters();
        }

        // internal bool TryGetStyleSheet(Module module, string sheetName, out StyleSheetInterface styleSheetInterface) {
        //     
        //     int moduleIndex = module.index;
        //     int styleSheetNameId = internSystem.GetIndex(sheetName);
        //     
        //     if (styleSheetNameId < 0) {
        //         styleSheetInterface = default;
        //         return false;
        //     }
        //     
        //     long key = MakeStyleSheetKey(moduleIndex, styleSheetNameId);
        //     
        //     if (table_StyleSheetMap.TryGetValue(key, out int idx)) {
        //         styleSheetInterface = new StyleSheetInterface(this, idx);
        //         return true;
        //     }
        //
        //     styleSheetInterface = default;
        //     return false;
        // }
        //
        // public StyleSheetInterface GetStyleSheet<TModuleType>(string sheetName) where TModuleType : Module {
        //
        //     int moduleIndex = ModuleSystem.GetModule<TModuleType>().index;
        //
        //     int styleSheetNameId = internSystem.GetIndex(sheetName);
        //     long key = MakeStyleSheetKey(moduleIndex, styleSheetNameId);
        //
        //     if (table_StyleSheetMap.TryGetValue(key, out int idx)) {
        //         return new StyleSheetInterface(this, idx);
        //     }
        //
        //     return default;
        // }

        public unsafe StyleProperty2 GetSelectorPropertyValue(ushort selectorIndex, PropertyId propertyId) {
            SelectorStyleEffect selectorEffect = selectorDatabase.table_SelectorStyles[selectorIndex];

            StaticPropertyId* keys = (StaticPropertyId*) (buffer_staticStyleProperties.data + selectorEffect.baseOffsetInBytes);
            PropertyData* values = (PropertyData*) (keys + selectorEffect.totalPropertyCount);

            // todo -- not accounting for module conditions
            for (int k = 0; k < selectorEffect.totalPropertyCount; k++) {

                if (keys[k].propertyId == propertyId) {
                    return new StyleProperty2(propertyId, values[k].value);
                }

            }

            return default;
        }

        public unsafe PropertyData GetPropertyValue(ushort styleIndex, PropertyId propertyId, StyleState2 state) {

            StaticStyleInfo staticStyle = table_StyleInfo[styleIndex];

            int offset = 0;
            int count = 0;

            switch (state) {
                case StyleState2.Normal:
                    offset = staticStyle.normalOffset;
                    count = staticStyle.normalCount;
                    break;

                case StyleState2.Hover:
                    offset = staticStyle.hoverOffset;
                    count = staticStyle.hoverCount;
                    break;

                case StyleState2.Focused:
                    offset = staticStyle.focusOffset;
                    count = staticStyle.focusCount;
                    break;

                case StyleState2.Active:
                    offset = staticStyle.activeOffset;
                    count = staticStyle.activeCount;
                    break;
            }

            StaticPropertyId* keys = (StaticPropertyId*) (buffer_staticStyleProperties.data + staticStyle.propertyOffsetInBytes + offset);
            PropertyData* values = (PropertyData*) (keys + staticStyle.totalPropertyCount);

            for (int k = 0; k < count; k++) {
                // todo -- not accounting for module conditions

                if (keys[k].propertyId == propertyId) {
                    return values[k];
                }

            }

            return default;

        }

        public void AddStyleSheet(Type moduleType, string name, Action<StyleSheetBuilder> action) {
            AddStyleSheet(name, GetModuleIndex(moduleType), action);
        }

        internal int AddStyleSheet(string styleSheetName, int moduleIndex, Action<StyleSheetBuilder> action) {
            if (action == null) return -1;

            int styleSheetIndex = table_StyleSheetInfo.size;

            NameKey styleSheetNameKey = MurmurHash3.Hash(styleSheetName);
            NameKey moduleNameKey = table_ModuleInfo[moduleIndex].nameKey;
            long styleSheetKey = MakeStyleSheetKey(moduleNameKey, styleSheetNameKey);

            if (!table_StyleSheetMap.TryAddValue(styleSheetKey, styleSheetIndex)) {
                // todo -- diagnostic
                throw new Exception("Duplicate style in module: " + styleSheetName);
            }

            StyleSheetBuilder builder = new StyleSheetBuilder(this, moduleIndex, styleSheetIndex);

            action.Invoke(builder);

            table_StyleSheetInfo.Add(new StyleSheetInfo(moduleIndex, styleSheetNameKey));

            return styleSheetIndex;
        }

        // Assumes style data is already in the buffer
        // internal int AddStyle(string name, StyleState2 selectorStates, StaticStyleInfo styleInfo) { }

        internal int AddStyleFromBuilder(string name, int moduleIndex, int styleSheetIndex, Action<StyleBuilder> action) {
            throw new NotImplementedException();

            // StyleBuilder builder = new StyleBuilder(); // todo optimize if i use this for non test code
            //
            // action?.Invoke(builder);
            //
            // int totalPropertyCount = builder.GetSharedStylePropertyCount();
            //
            // buffer_staticStyleProperties.EnsureAdditionalCapacity<StaticPropertyId, PropertyData>(totalPropertyCount);
            //
            // int baseOffset = buffer_staticStyleProperties.GetWritePosition();
            //
            // PropertyRange activeRange = WriteSharedProperties(builder.activeGroup?.properties, ref buffer_staticStyleProperties, baseOffset);
            // PropertyRange focusRange = WriteSharedProperties(builder.focusGroup?.properties, ref buffer_staticStyleProperties, baseOffset);
            // PropertyRange hoverRange = WriteSharedProperties(builder.hoverGroup?.properties, ref buffer_staticStyleProperties, baseOffset);
            // PropertyRange normalRange = WriteSharedProperties(builder.normalGroup?.properties, ref buffer_staticStyleProperties, baseOffset);
            //
            // StyleState2 selectorStates = default;
            //
            // if (builder.activeGroup?.selectorBuilders != null) {
            //     selectorStates |= StyleState2.Active;
            // }
            //
            // if (builder.focusGroup?.selectorBuilders != null) {
            //     selectorStates |= StyleState2.Focused;
            // }
            //
            // if (builder.hoverGroup?.selectorBuilders != null) {
            //     selectorStates |= StyleState2.Hover;
            // }
            //
            // if (builder.normalGroup?.selectorBuilders != null) {
            //     selectorStates |= StyleState2.Normal;
            // }

            // int styleIndex = AddStyle(name, selectorStates, new StaticStyleInfo() {
            //     moduleIndex = (ushort) moduleIndex,
            //     styleSheetIndex = (ushort) styleSheetIndex,
            //     propertyOffsetInBytes = baseOffset,
            //     activeCount = activeRange.count,
            //     activeOffset = activeRange.offset,
            //     focusCount = focusRange.count,
            //     focusOffset = focusRange.offset,
            //     hoverCount = hoverRange.count,
            //     hoverOffset = hoverRange.offset,
            //     normalCount = normalRange.count,
            //     normalOffset = normalRange.offset,
            //     totalPropertyCount = (ushort) (activeRange.count + focusRange.count + hoverRange.count + normalRange.count),
            //     dataBaseId = (ushort) this.dbIndex
            // });

            // Module module = moduleList[moduleIndex];
            //
            // if (builder.activeGroup?.selectorBuilders != null) {
            //     BuildSelectors(styleIndex, StyleState2.Active, module, styleSheetIndex, builder.activeGroup.selectorBuilders);
            // }
            //
            // if (builder.focusGroup?.selectorBuilders != null) {
            //     BuildSelectors(styleIndex, StyleState2.Focused, module, styleSheetIndex, builder.focusGroup.selectorBuilders);
            // }
            //
            // if (builder.hoverGroup?.selectorBuilders != null) {
            //     BuildSelectors(styleIndex, StyleState2.Hover, module, styleSheetIndex, builder.hoverGroup.selectorBuilders);
            // }
            //
            // if (builder.normalGroup?.selectorBuilders != null) {
            //     BuildSelectors(styleIndex, StyleState2.Normal, module, styleSheetIndex, builder.normalGroup.selectorBuilders);
            // }
            //
            // return styleIndex;

        }

        private void BuildFiltersForQuery(int queryIndex, int start, int end, ref DataList<SelectorFilter> buffer) {

            for (int i = start; i < end; i++) {
                PendingSelectorFilter pendingFilter = selectorFiltersToBuild.array[i];

                SelectorFilter builtFilter = new SelectorFilter {
                    filterType = pendingFilter.filterType
                };

                SelectorFilterType filterType = builtFilter.filterType & ~SelectorFilterType.Inverted;

                switch (filterType) {

                    case SelectorFilterType.ElementsWithTag: {

                        int colonIdx = pendingFilter.key.IndexOf(':');
                        string moduleName = null;
                        string tagName = pendingFilter.key;

                        if (colonIdx != -1) {
                            moduleName = pendingFilter.key.Substring(0, colonIdx - 1);
                            tagName = pendingFilter.key.Substring(colonIdx + 1);
                        }

                        // todo -- this ain't right
                        ProcessedType processedType = pendingFilter.module.ResolveTagName(moduleName, tagName, default, null, new LineInfo()); // todo -- diagnostic & line number

                        if (processedType == null) {
                            // todo -- diagnostic
                            continue;
                        }

                        builtFilter.key = processedType.id; // todo -- not sure how this resolves generics for us

                        break;
                    }

                    case SelectorFilterType.ElementsWithState: {
                        builtFilter.key = (int) pendingFilter.state;
                        break;
                    }

                    case SelectorFilterType.ElementsWithStyle: {
                        // somehow want to defer this if not found yet. 
                        builtFilter.key = ResolveStyleId(pendingFilter.module, pendingFilter.styleSheetIndex, pendingFilter.key);

                        if (builtFilter.key < 0) {
                            // todo -- diagnostic
                            continue;
                        }

                        break;
                    }

                    case SelectorFilterType.ElementsWithAttribute: {
                        builtFilter.key = internSystem.AddConstant(pendingFilter.key);
                        break;
                    }

                    case SelectorFilterType.ElementsWithAttribute_ValueEquals:
                    case SelectorFilterType.ElementsWithAttribute_ValueContains:
                    case SelectorFilterType.ElementsWithAttribute_ValueEndsWith:
                    case SelectorFilterType.ElementsWithAttribute_ValueStartsWith: {
                        builtFilter.key = internSystem.AddConstant(pendingFilter.key);
                        builtFilter.value = internSystem.AddConstant(pendingFilter.value);
                        break;
                    }
                }

                buffer.Add(builtFilter);
            }

            selectorDatabase.SetFiltersForQuery(queryIndex, ref buffer);

        }

        private void BuildPendingSelectorFilters() {

            if (selectorFiltersToBuild == null) return;

            DataList<SelectorFilter> buffer = new DataList<SelectorFilter>(16, Allocator.TempJob);

            for (int i = 0; i < selectorFiltersToBuild.size; i++) {

                buffer.SetSize(0);

                int queryIndex = selectorFiltersToBuild.array[i].queryIndex;
                int count = 1;

                while (i + count < selectorFiltersToBuild.size && selectorFiltersToBuild.array[i + count].queryIndex == queryIndex) {
                    count++;
                }

                BuildFiltersForQuery(queryIndex, i, i + count, ref buffer);

                i += count;

            }

            selectorFiltersToBuild = null;

            buffer.Dispose();
        }

        private void BuildSelectors(int styleIndex, StyleState2 state, Module module, int styleSheetIndex, LightList<SelectorBuilder> selectorBuilders) {

            if (selectorBuilders == null || selectorBuilders.size == 0) {
                return;
            }

            DataList<SelectorTypeIndex> indexBuffer = new DataList<SelectorTypeIndex>(16, Allocator.Temp);

            for (int i = 0; i < selectorBuilders.size; i++) {

                SelectorBuilder selector = selectorBuilders.array[i];

                if (selector.queryBuilder == null) {
                    continue;
                }

                SelectorQuery query = new SelectorQuery() {
                    source = selector.queryBuilder.source,
                    filterCount = 0,
                    filters = null
                };

                if (selector.queryBuilder.whereFn != null) {
                    query.whereFilterId = selectorDatabase.table_WhereFilters.size;
                    selectorDatabase.table_WhereFilters.Add(selector.queryBuilder.whereFn);
                }
                else {
                    query.whereFilterId = 0;
                }

                bool usesIndices = false;
                int queryIndex = selectorDatabase.table_SelectorQueries.size;

                for (int f = 0; f < selector.queryBuilder.filters.size; f++) {

                    if ((selector.queryBuilder.filters[f].filterType & SelectorFilterType.Inverted) == 0) {
                        usesIndices = true;
                    }

                    selectorFiltersToBuild.Add(new PendingSelectorFilter() {
                        filterType = selector.queryBuilder.filters[f].filterType,
                        key = selector.queryBuilder.filters[f].key,
                        value = selector.queryBuilder.filters[f].value,
                        queryIndex = queryIndex,
                        module = module,
                        state = selector.queryBuilder.filters[f].state,
                        styleSheetIndex = styleSheetIndex
                    });

                }

                indexBuffer.Add(new SelectorTypeIndex() {
                    index = (ushort) queryIndex,
                    source = query.source,
                    usesIndices = usesIndices
                });

                int baseOffset = buffer_staticStyleProperties.GetWritePosition();

                PropertyRange selectorStyleEffect = WriteSharedProperties(selector.properties, ref buffer_staticStyleProperties, baseOffset);

                selectorDatabase.table_SelectorQueries.Add(query);
                selectorDatabase.table_SelectorStyles.Add(new SelectorStyleEffect() {
                    baseOffsetInBytes = baseOffset,
                    offset = selectorStyleEffect.offset,
                    totalPropertyCount = selectorStyleEffect.count,
                    enterEventCount = 0,
                    exitEventCount = 0
                });

                Assert.AreEqual(selectorDatabase.table_SelectorQueries.size, selectorDatabase.table_SelectorStyles.size);

            }

            selectorDatabase.BuildSelectorIndices(styleIndex, state, ref indexBuffer);

            indexBuffer.Dispose();

        }

        // @import.two
        // @module:sheet.two
        // "module:styleSheet:two"
        // "styleSheet.two"; (usually an import)
        // "two"
        private int ResolveStyleId(Module module, int styleSheetIndex, string styleNameExpression) {
            // if no module provided
            int colonIndex = styleNameExpression.IndexOf(':');
            int dotIndex = styleNameExpression.IndexOf('.');

            string original = styleNameExpression;

            if (colonIndex != -1) {
                if (dotIndex == -1 || dotIndex < colonIndex) {
                    // todo -- diagnostic
                    throw new Exception($"Invalid style lookup expression: '{styleNameExpression}'. Expected format module:styleSheet.styleName");
                }

                string moduleName = styleNameExpression.Substring(0, colonIndex - 1);
                styleNameExpression = styleNameExpression.Substring(colonIndex + 1);

                module = module.ResolveModuleName(moduleName);
                if (module == null) {
                    // todo -- diagnostic
                    throw new Exception($"Unable to resolve module by alias {moduleName}");
                }
            }

            if (dotIndex != -1) {
                string styleSheetName = styleNameExpression.Substring(0, dotIndex - 1);
                styleNameExpression = styleSheetName.Substring(dotIndex + 1);
                // todo -- this needs to work with style packages, because sheets aren't named except in the builder, which is silly
                // if (!TryGetStyleSheet(module, styleSheetName, out StyleSheetInterface sheet)) {
                //     // todo -- diagnostic
                //     throw new Exception($"Unable to resolve style sheet in module {module.GetModuleName()} with name {styleSheetName}");
                // }

                //if (!sheet.TryGetStyle(styleNameExpression, out StyleId styleId)) {
                //    // todo -- diagnostic
                //    throw new Exception($"Unable to resolve style in module {module.GetModuleName()} with stylesheet {styleSheetName} with style name {styleNameExpression}");
                //}

                // return styleId.id;

            }
            else {
                if (TryGetStyleId(MakeStyleKey(styleSheetIndex, styleNameExpression), out StyleId styleId)) {
                    return styleId.id;
                }

            }

            throw new Exception($"Unable to resolve style from expression {original}");
        }

        internal string Debug_ResolveStyleName(ushort styleIndex) {

            ref StaticStyleInfo styleInfo = ref table_StyleInfo[styleIndex];
            ref StyleSheetInfo sheetInfo = ref table_StyleSheetInfo[styleInfo.styleSheetIndex];

            string sheetName = internSystem.GetString(sheetInfo.nameKey);
            string styleName = internSystem.GetString(styleInfo.styleSheetIndex);

            return sheetName + " / " + styleName;
        }

        internal StyleId GetStyleId(long styleKey) {
            table_StyleIdMap.TryGetValue(styleKey, out StyleId styleId);
            return styleId;
        }

        internal bool TryGetStyleId(long styleKey, out StyleId styleId) {
            return table_StyleIdMap.TryGetValue(styleKey, out styleId);
        }

        private int GetModuleIndex(Type type) {

            int moduleIndex = -1;

            for (int i = 0; i < moduleList.Count; i++) {
                if (moduleList[i].type == type) {
                    moduleIndex = i;
                    break;
                }
            }

            if (moduleIndex < 0) {
                // todo -- diagnostics
                throw new Exception($"Cannot find a module with type {type}");
            }

            return moduleIndex;
        }

        internal static StyleDatabase GetDatabase(int i) {
            return s_DebugDatabases[i];
        }

        internal static long MakeStyleSheetKey(NameKey moduleName, NameKey styleSheetName) {
            return BitUtil.IntsToLong(moduleName, styleSheetName);
        }

        internal static long MakeStyleKey(int styleSheetIndex, string styleName) {
            return BitUtil.IntsToLong(styleSheetIndex, MurmurHash3.Hash(styleName));
        }

        private static PropertyRange WriteSharedProperties(StructList<StyleProperty2> properties, ref ByteBuffer byteBuffer, int baseOffset) {
            int offset = byteBuffer.GetWritePosition() - baseOffset;
            if (properties == null) {
                return new PropertyRange(offset, 0);
            }

            for (int i = 0; i < properties.size; i++) {
                byteBuffer.Write(new StaticPropertyId(properties.array[i].propertyId));
            }

            for (int i = 0; i < properties.size; i++) {
                byteBuffer.Write(new PropertyData(properties.array[i].longVal));
            }

            return new PropertyRange(offset, properties.size);
        }

        private struct PropertyRange {

            public ushort count;
            public ushort offset;

            public PropertyRange(int offset, int count) {
                this.count = (ushort) count;
                this.offset = (ushort) offset;
            }

        }

        private struct PendingSelectorFilter {

            public SelectorFilterType filterType;
            public string key;
            public string value;
            public StyleState2 state;
            public Module module;
            public int queryIndex;
            public int styleSheetIndex;

        }

        public static int MakeSelectorKey(StyleId styleId, StyleState2 state) {
            return BitUtil.SetHighLowBits(styleId.index, (int) state);
        }

        private bool isInitialized;

        public void Initialize(LightList<StyleFile> styleFiles) {
            int totalStyleCount = 0;
            int totalPropertyCount = 0;

            for (int i = 0; i < styleFiles.size; i++) {
                ref StyleFile file = ref styleFiles.array[i];

                for (int j = 0; j < file.compileResult.styles.Length; j++) {
                    totalStyleCount++;
                    totalPropertyCount += file.compileResult.propertyCount;
                }

            }

            totalStyleCount += (totalStyleCount / 4);
            int styleSheetCount = styleFiles.size + (styleFiles.size / 4);
            int propertyCount = totalPropertyCount + (totalPropertyCount / 4);
            int propertyByteSize = TypedUnsafe.ByteSize<PropertyId, PropertyData>(propertyCount);

            if (!isInitialized) {
                isInitialized = true;
                // todo -- dataList will always have pow2 size, make something different for this that isnt overallocating like mad
                table_StyleInfo = new DataList<StaticStyleInfo>(totalStyleCount, Allocator.Persistent);
                table_StyleSheetInfo = new DataList<StyleSheetInfo>(styleSheetCount, Allocator.Persistent);
                buffer_staticStyleProperties = new ByteBuffer(propertyByteSize, Allocator.Persistent);
                table_StyleSheetMap = new UnmanagedLongMap<int>(styleSheetCount, Allocator.Persistent);
                table_StyleIdMap = new UnmanagedLongMap<StyleId>(totalStyleCount, Allocator.Persistent);
            }
            else {
                table_StyleInfo.size = 0;
                table_StyleSheetInfo.size = 0;
                table_StyleInfo.EnsureCapacity(totalStyleCount);
                table_StyleSheetInfo.EnsureCapacity(styleSheetCount);
                buffer_staticStyleProperties.Reset(propertyByteSize);
                table_StyleSheetMap.Clear();
                table_StyleIdMap.Clear();
            }

            int propertyByteOffset = 0;

            for (int i = 0; i < styleFiles.size; i++) {
                ref StyleFile file = ref styleFiles.array[i];

                ushort styleSheetId = (ushort) i;

                int styleSheetLocationId = internSystem.AddConstWithoutBurst(file.filePath);

                // todo -- continue here, sort out style file names and mapping

                long key = MakeStyleSheetKey(file.module.index, styleSheetLocationId);

                table_StyleSheetMap.Add(key, i);

                for (int j = 0; j < file.compileResult.styles.Length; j++) {
                    CompiledSharedStyle style = file.compileResult.styles[j];
                    // todo -- selectors

                    int styleIndex = table_StyleInfo.size;
                    long styleKey = MakeStyleKey(styleSheetId, style.styleName);

                    StyleState2 states = default;

                    if (style.activeCount > 0) states |= StyleState2.Active;
                    if (style.focusCount > 0) states |= StyleState2.Focused;
                    if (style.hoverCount > 0) states |= StyleState2.Hover;
                    if (style.normalCount > 0) states |= StyleState2.Normal;

                    StyleId styleId = new StyleId((ushort) styleIndex, states, default, dbIndex); // todo -- selectors

                    // style sheet would have already ensured name was valid
                    table_StyleIdMap.TryAddValue(styleKey, styleId);

                    table_StyleInfo.Add(new StaticStyleInfo {
                        moduleIndex = file.module.index,
                        styleSheetIndex = styleSheetId,
                        activeOffset = style.activeOffset,
                        activeCount = style.activeOffset,
                        normalOffset = style.normalOffset,
                        normalCount = style.normalCount,
                        hoverOffset = style.hoverOffset,
                        hoverCount = style.hoverCount,
                        focusOffset = style.focusOffset,
                        focusCount = style.focusCount,
                        totalPropertyCount = style.totalPropertyCount,
                        propertyOffsetInBytes = propertyByteOffset,
                        dataBaseId = dbIndex
                    });

                    propertyByteOffset += TypedUnsafe.ByteSize<StaticPropertyId, PropertyData>(style.totalPropertyCount);
                }

                buffer_staticStyleProperties.WriteRange(file.compileResult.properties, file.compileResult.propertyCount);

            }
        }

    }

}