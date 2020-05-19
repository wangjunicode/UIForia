using System;
using System.Collections.Generic;
using Packages.UIForia.Util.Unsafe;
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
        private const int ImplicitStyleSheetIndex = 0;

        private readonly int dbIndex;

        public ByteBuffer buffer_staticStyleProperties; 

        // maybe introduce a new Table type that grows at a non doubling rate after initializing
        public DataList<ModuleInfo> table_ModuleInfo;
        public DataList<StaticStyleInfo> table_StyleInfo;
        public DataList<StyleSheetInfo> table_StyleSheetInfo;

        public ModuleTable<ModuleCondition> conditionTable;

        public DataList<int> join_StyleIndex__ModuleIndex;
        public DataList<int> join_StyleIndex__StyleSheetIndex;

        public UnmanagedLongMap<int> table_StyleSheetMap;
        public UnmanagedLongMap<StyleId> table_StyleIdMap;

        public LightList<string> join_StyleIndex__StyleName;
        public LightList<string> join_StyleSheetIndex__Name;

        public SelectorDatabase selectorDatabase;
        public StaticStringTable staticStringTable;

        private StructList<PendingSelectorFilter> selectorFiltersToBuild;
        private IList<Module> moduleList;

        internal static readonly List<StyleDatabase> s_DebugDatabases = new List<StyleDatabase>();

        public unsafe BurstableStyleDatabase GetBurstable() {
            return new BurstableStyleDatabase() {
                conditionTable = conditionTable,
                sharedStyleTable = new StyleTable<StaticStyleInfo>(table_StyleInfo.GetArrayPointer()),
                staticStyleProperties = buffer_staticStyleProperties.data,
                selectorStyleTable = new SelectorTable<SelectorStyleEffect>() // todo -- hook up selector styles here
            };
        }

        public void Dispose() {

            table_ModuleInfo.Dispose();
            table_StyleSheetInfo.Dispose();
            buffer_staticStyleProperties.Dispose();
            staticStringTable.Dispose();
            join_StyleIndex__ModuleIndex.Dispose();
            join_StyleIndex__StyleSheetIndex.Dispose();

            table_StyleInfo.Dispose();
            table_StyleSheetMap.Dispose();
            table_StyleIdMap.Dispose();
            selectorDatabase.Dispose();
            s_DebugDatabases.Remove(this);

        }

        private Module rootModule;

        public int styleCount {
            get => table_StyleInfo.size; // todo -- this will change
        }

        public StyleDatabase(Module rootModule, int initialStyleCount = 64, int initialPropertyCount = 512) {
            this.rootModule = rootModule;
            this.moduleList = ModuleSystem.GetModuleList();

            // for compiled output we know this already
            buffer_staticStyleProperties = new ByteBuffer(TypedUnsafe.SizeOf<StaticPropertyId, PropertyData>(initialPropertyCount), Allocator.Persistent);

            join_StyleIndex__StyleName = new LightList<string>();

            join_StyleSheetIndex__Name = new LightList<string>();

            staticStringTable = StaticStringTable.Create();

            table_ModuleInfo = new DataList<ModuleInfo>(8, Allocator.Persistent);
            table_StyleSheetMap = new UnmanagedLongMap<int>(16, Allocator.Persistent);
            table_StyleIdMap = new UnmanagedLongMap<StyleId>(128, Allocator.Persistent);

            // these could all live in a single buffer since they grow identically.
            table_StyleInfo = new DataList<StaticStyleInfo>(initialStyleCount, Allocator.Persistent);
            join_StyleIndex__ModuleIndex = new DataList<int>(initialStyleCount, Allocator.Persistent);
            join_StyleIndex__StyleSheetIndex = new DataList<int>(initialStyleCount, Allocator.Persistent);

            table_StyleSheetInfo = new DataList<StyleSheetInfo>(8, Allocator.Persistent);
            selectorFiltersToBuild = new StructList<PendingSelectorFilter>(64);

            selectorDatabase = SelectorDatabase.Create();

            BuildModuleData();

            NameKey styleSheetHash = MurmurHash3.Hash(k_ImplicitStyleSheetName);

            // implicit style sheet
            table_StyleSheetInfo.Add(new StyleSheetInfo(0, table_ModuleInfo[0].nameKey));

            join_StyleSheetIndex__Name.Add(k_ImplicitStyleSheetName);

            table_StyleSheetMap.Add(MakeStyleSheetKey(table_ModuleInfo[0].nameKey, styleSheetHash), 0);

            AddStyle("__INVALID__", ImplicitStyleSheetIndex, 0, default, default); // 0 index should be considered invalid

            this.dbIndex = s_DebugDatabases.Count;
            s_DebugDatabases.Add(this);
        }

        private void BuildModuleData() {

            table_ModuleInfo.SetSize(moduleList.Count);

            for (int i = 0; i < moduleList.Count; i++) {
                table_ModuleInfo[i] = new ModuleInfo(MurmurHash3.Hash(moduleList[i].GetModuleName()));
            }

            // todo -- condition table should be initialized here

        }

        public void Initialize() {
            BuildPendingSelectorFilters();
        }

        internal bool TryGetStyleSheet(Module module, string sheetName, out StyleSheetInterface styleSheetInterface) {
            NameKey moduleName = MurmurHash3.Hash(module.GetModuleName());
            long key = BitUtil.IntsToLong(moduleName, MurmurHash3.Hash(sheetName));

            if (table_StyleSheetMap.TryGetValue(key, out int idx)) {
                styleSheetInterface = new StyleSheetInterface(this, idx);
                return true;
            }

            styleSheetInterface = default;
            return false;
        }

        public StyleSheetInterface GetStyleSheet<TModuleType>(string sheetName) where TModuleType : Module {

            int moduleIndex = GetModuleIndex(typeof(TModuleType));

            NameKey styleSheetName = MurmurHash3.Hash(sheetName);
            NameKey moduleName = table_ModuleInfo[moduleIndex].nameKey;
            long key = MakeStyleSheetKey(moduleName, styleSheetName);

            if (table_StyleSheetMap.TryGetValue(key, out int idx)) {
                return new StyleSheetInterface(this, idx);
            }

            return default;
        }

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
            if (styleIndex > join_StyleIndex__StyleName.size) {
                return default;
            }

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

            join_StyleSheetIndex__Name.Add(styleSheetName);

            StyleSheetBuilder builder = new StyleSheetBuilder(this, moduleIndex, styleSheetIndex);

            action.Invoke(builder);

            table_StyleSheetInfo.Add(new StyleSheetInfo(moduleIndex, styleSheetNameKey));

            return styleSheetIndex;
        }

        // Assumes style data is already in the buffer
        internal int AddStyle(string name, int styleSheetIndex, int moduleIndex, StyleState2 selectorStates, StaticStyleInfo styleInfo) {
            long styleKey = MakeStyleKey(styleSheetIndex, name);
            int styleIndex = table_StyleInfo.size;

            StyleState2 states = default;

            if (styleInfo.activeCount > 0) states |= StyleState2.Active;
            if (styleInfo.focusCount > 0) states |= StyleState2.Focused;
            if (styleInfo.hoverCount > 0) states |= StyleState2.Hover;
            if (styleInfo.normalCount > 0) states |= StyleState2.Normal;

            StyleId styleId = new StyleId((ushort) styleIndex, states, selectorStates, dbIndex); // todo -- selectors

            if (!table_StyleIdMap.TryAddValue(styleKey, styleId)) {
                // todo -- diagnostics
                throw new Exception("Duplicate style in sheet: " + name);
            }

            Assert.IsTrue(
                (join_StyleIndex__ModuleIndex.size == join_StyleIndex__StyleSheetIndex.size) &&
                (join_StyleIndex__StyleName.size == join_StyleIndex__StyleSheetIndex.size) &&
                (join_StyleIndex__StyleSheetIndex.size == table_StyleInfo.size)
            );

            table_StyleInfo.Add(styleInfo);

            join_StyleIndex__ModuleIndex.Add(moduleIndex);
            join_StyleIndex__StyleSheetIndex.Add(styleSheetIndex);
            join_StyleIndex__StyleName.Add(name);

            return styleIndex;

        }

        internal int AddStyleFromBuilder(string name, int moduleIndex, int styleSheetIndex, Action<StyleBuilder> action) {

            StyleBuilder builder = new StyleBuilder(); // todo optimize if i use this for non test code

            action?.Invoke(builder);

            int totalPropertyCount = builder.GetSharedStylePropertyCount();

            buffer_staticStyleProperties.EnsureAdditionalCapacity<StaticPropertyId, PropertyData>(totalPropertyCount);

            int baseOffset = buffer_staticStyleProperties.GetWritePosition();

            PropertyRange activeRange = WriteSharedProperties(builder.activeGroup?.properties, ref buffer_staticStyleProperties, baseOffset);
            PropertyRange focusRange = WriteSharedProperties(builder.focusGroup?.properties, ref buffer_staticStyleProperties, baseOffset);
            PropertyRange hoverRange = WriteSharedProperties(builder.hoverGroup?.properties, ref buffer_staticStyleProperties, baseOffset);
            PropertyRange normalRange = WriteSharedProperties(builder.normalGroup?.properties, ref buffer_staticStyleProperties, baseOffset);

            StyleState2 selectorStates = default;

            if (builder.activeGroup?.selectorBuilders != null) {
                selectorStates |= StyleState2.Active;
            }

            if (builder.focusGroup?.selectorBuilders != null) {
                selectorStates |= StyleState2.Focused;
            }

            if (builder.hoverGroup?.selectorBuilders != null) {
                selectorStates |= StyleState2.Hover;
            }

            if (builder.normalGroup?.selectorBuilders != null) {
                selectorStates |= StyleState2.Normal;
            }

            int styleIndex = AddStyle(name, styleSheetIndex, moduleIndex, selectorStates, new StaticStyleInfo() {
                moduleId = new ModuleId(moduleIndex),
                propertyOffsetInBytes = baseOffset,
                activeCount = activeRange.count,
                activeOffset = activeRange.offset,
                focusCount = focusRange.count,
                focusOffset = focusRange.offset,
                hoverCount = hoverRange.count,
                hoverOffset = hoverRange.offset,
                normalCount = normalRange.count,
                normalOffset = normalRange.offset,
                totalPropertyCount = (ushort) (activeRange.count + focusRange.count + hoverRange.count + normalRange.count),
                dataBaseId = (ushort) this.dbIndex
            });

            Module module = moduleList[moduleIndex];

            if (builder.activeGroup?.selectorBuilders != null) {
                BuildSelectors(styleIndex, StyleState2.Active, module, styleSheetIndex, builder.activeGroup.selectorBuilders);
            }

            if (builder.focusGroup?.selectorBuilders != null) {
                BuildSelectors(styleIndex, StyleState2.Focused, module, styleSheetIndex, builder.focusGroup.selectorBuilders);
            }

            if (builder.hoverGroup?.selectorBuilders != null) {
                BuildSelectors(styleIndex, StyleState2.Hover, module, styleSheetIndex, builder.hoverGroup.selectorBuilders);
            }

            if (builder.normalGroup?.selectorBuilders != null) {
                BuildSelectors(styleIndex, StyleState2.Normal, module, styleSheetIndex, builder.normalGroup.selectorBuilders);
            }

            return styleIndex;

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

                        ProcessedType processedType = pendingFilter.module.ResolveTagName(moduleName, tagName, default); // todo -- diagnostic

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
                        builtFilter.key = staticStringTable.GetOrCreateReference(pendingFilter.key);
                        break;
                    }

                    case SelectorFilterType.ElementsWithAttribute_ValueEquals:
                    case SelectorFilterType.ElementsWithAttribute_ValueContains:
                    case SelectorFilterType.ElementsWithAttribute_ValueEndsWith:
                    case SelectorFilterType.ElementsWithAttribute_ValueStartsWith: {
                        builtFilter.key = staticStringTable.GetOrCreateReference(pendingFilter.key);
                        builtFilter.value = staticStringTable.GetOrCreateReference(pendingFilter.value);
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
                if (!TryGetStyleSheet(module, styleSheetName, out StyleSheetInterface sheet)) {
                    // todo -- diagnostic
                    throw new Exception($"Unable to resolve style sheet in module {module.GetModuleName()} with name {styleSheetName}");
                }

                if (!sheet.TryGetStyle(styleNameExpression, out StyleId styleId)) {
                    // todo -- diagnostic
                    throw new Exception($"Unable to resolve style in module {module.GetModuleName()} with stylesheet {styleSheetName} with style name {styleNameExpression}");
                }

                return styleId.id;

            }
            else {
                if (TryGetStyleId(MakeStyleKey(styleSheetIndex, styleNameExpression), out StyleId styleId)) {
                    return styleId.id;
                }

            }

            throw new Exception($"Unable to resolve style from expression {original}");
        }

        internal string Debug_ResolveStyleName(ushort styleIndex) {

            if (styleIndex > join_StyleIndex__StyleName.size) {
                return "unresolved style";
            }

            int sheetIdx = join_StyleIndex__StyleSheetIndex[styleIndex];
            string sheetName = join_StyleSheetIndex__Name[sheetIdx];
            string styleName = join_StyleIndex__StyleName[styleIndex];
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

    }

}