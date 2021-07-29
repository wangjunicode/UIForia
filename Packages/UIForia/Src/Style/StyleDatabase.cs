using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UIForia.Compilers;
using UIForia.Layout.LayoutTypes;
using UIForia.Parsing;
using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UIForia.Style {

    /// <summary>
    /// Add properties to this class and hit the "regenerate styles" menu item in unity. It will
    /// generate de/serializers for all properties in here. Make sure to initialize new properties
    /// in the Construct() method below. 
    /// </summary>
    internal unsafe partial class StyleDatabase : IDisposable {

        public const int k_MaxBlockCountPerStyle = 64;

        // todo -- a lot of these can be converted to checked arrays probably 
        /// <summary>
        /// Managed by Job: <see cref="CreateStyleUsagesAndQuerySubscriptions"/>
        /// </summary>
        public DataList<StyleDesc> styles;

        /// <summary>
        /// Read by Job: <see cref="SolveSharedPropertyUpdate"/>
        /// Filled during compilation from the <see cref="UIForiaStyleCompiler"/> via <see cref="AddStylesFromCompilation"/>
        /// </summary>
        public DataList<StyleBlock> styleBlocks;

        public DataList<StateHook> stateHooks;

        /// <summary>
        /// Indexable 1:1 with styleBlocks. All style blocks are stored in a flat list. Nested blocks will have
        /// the required bits set for their parents. 
        /// </summary>
        public DataList<BitSet> blockQueryRequirements;

        public DataList<QueryPair> queries; // queryId, index to put result into 
        public DataList<QueryTable> queryTables; // id, querytype, data needed to run query

        /// <summary>
        /// Default values for each property type (e.g. font size = 18px by default).
        /// </summary>
        public DataList<int> defaultValueIndices;

        /// <summary>
        /// Untyped reference of which properties are in a style block.
        /// Maps property types and values.
        /// </summary>
        public DataList<PropertyLocator> propertyLocatorBuffer;

        public DataList<TransitionLocator> transitionLocatorBuffer;
        public DataList<TransitionDefinition> transitionTable;

        public DataList<GridTemplate> gridTemplateTable;
        public DataList<GridCellDefinition> gridCellTable;
        public DataList<StyleNameMapping> nameMappings;

        public DataList<AnimationOptions> animationOptionsTable;

        public Dictionary<ModuleAndNameKey, AnimationReference> animationMap;
        public DataList<AnimationKeyFrameRange> animationKeyFrameRanges;
        public DataList<PropertyKeyFrames> animationProperties;
        public DataList<KeyFrameInfo> animationKeyFrames;

        internal Dictionary<ModuleAndNameKey, PropertyId> customPropertyIdMap;
        internal DataList<CustomPropertyType> customPropertyTypes;
        internal DataList<PropertyType> propertyTypeById;

        [NonSerialized] internal LightList<IStylePropertyParser> customPropertyParsers;

        [NonSerialized] // runtime only, maybe this belongs elsewhere
        public CheckedArray<AttributeMemberTable> attrTables;

        public Dictionary<string, StyleSheetInfo> styleSheetMap;

        [NonSerialized] // only needed for compilation comments
        private Dictionary<uint, StyleMeta> styleMetaMap;

        public StringTagger attributeTagger;
        public StringTagger variableTagger;
        public StringTagger conditionTagger;
        public StringTagger tagger;

        internal uint numberRangeStart;
        internal uint numberRangeEnd;

        internal const int k_NumberRangeSize = 250_000;

        // todo -- ideally we can serialize this stuff and not re-compile when re-booting apps or in production 

        internal const int Version = 1;

        internal static StyleDatabase s_DebuggedStyleDatabase;

        internal StyleDatabase(bool createFromDeserialized = false) {
            s_DebuggedStyleDatabase = this;

            if (!createFromDeserialized) {
                Construct();
            }
        }

        internal StyleDatabase(byte[] compilationStyleDatabase) {
            s_DebuggedStyleDatabase = this;
            ManagedByteBuffer buffer = new ManagedByteBuffer(compilationStyleDatabase);
            Deserialize(ref buffer);
        }

        internal void Serialize(ref ManagedByteBuffer buffer) {
            buffer.Write(Version);
            SerializeDatabase(ref buffer);
        }

        internal void Deserialize(ref ManagedByteBuffer buffer) {
            buffer.Read(out int version);
            if (version != Version) {
                throw new Exception("Version mismatch when deserializing style database, you need to recompile your application.");
            }

            DeserializeDatabase(ref buffer);
        }

        private void DeserializeStyleSheetMap(ref ManagedByteBuffer buffer) {
            buffer.Read(out int count);
            if (styleSheetMap != null) {
                styleSheetMap.Clear();
            }
            else {
                styleSheetMap = new Dictionary<string, StyleSheetInfo>(count);
            }

            for (int i = 0; i < count; i++) {
                StyleSheetInfo sheetInfo = new StyleSheetInfo();
                buffer.Read(out sheetInfo.fileName);
                buffer.Read(out sheetInfo.moduleName);
                buffer.Read(out sheetInfo.nameMappingRange.start);
                buffer.Read(out sheetInfo.nameMappingRange.length);
                styleSheetMap[sheetInfo.fileName] = sheetInfo;
            }
        }

        private void SerializeStyleSheetMap(ref ManagedByteBuffer buffer) {

            buffer.Write(styleSheetMap.Count);

            foreach (KeyValuePair<string, StyleSheetInfo> kvp in styleSheetMap) {
                buffer.Write(kvp.Value.fileName);
                buffer.Write(kvp.Value.moduleName);
                buffer.Write(kvp.Value.nameMappingRange.start);
                buffer.Write(kvp.Value.nameMappingRange.length);
            }

        }

        private void SerializeAnimationMap(ref ManagedByteBuffer writer) {

            writer.Write(animationMap.Count);

            foreach (KeyValuePair<ModuleAndNameKey, AnimationReference> kvp in animationMap) {
                writer.Write(kvp.Key.moduleName);
                writer.Write(kvp.Key.name);
                writer.Write(kvp.Value.animationId.id);
                writer.Write(kvp.Value.optionId.id);
            }

        }

        private void DeserializeAnimationMap(ref ManagedByteBuffer buffer) {
            buffer.Read(out int count);
            if (animationMap != null) {
                animationMap.Clear();
            }
            else {
                animationMap = new Dictionary<ModuleAndNameKey, AnimationReference>(count);
            }

            for (int i = 0; i < count; i++) {
                buffer.Read(out string moduleName);
                buffer.Read(out string name);
                buffer.Read(out int animId);
                buffer.Read(out int optionId);
                animationMap.Add(new ModuleAndNameKey(moduleName, name), new AnimationReference() {
                    animationId = new AnimationId(animId),
                    optionId = new OptionId(optionId)
                });
            }
        }

        private void Construct() {
            this.attributeTagger = StringTagger.Create(0, int.MaxValue); // we never release tagged attributes because we know them all at compile time
            this.variableTagger = StringTagger.Create(0, int.MaxValue); // we never release tagged attributes because we know them all at compile time
            this.conditionTagger = StringTagger.Create(0, int.MaxValue); // we never release tagged attributes because we know them all at compile time
            this.tagger = StringTagger.Create(0, int.MaxValue); // never release 

            this.styleSheetMap = new Dictionary<string, StyleSheetInfo>(); // maybe just an index to fileList? 
            this.styleBlocks = new DataList<StyleBlock>(256, Allocator.Persistent);
            this.styleMetaMap = new Dictionary<uint, StyleMeta>(31);
            this.styles = new DataList<StyleDesc>(64, Allocator.Persistent);
            this.stateHooks = new DataList<StateHook>(32, Allocator.Persistent);
            this.animationOptionsTable = new DataList<AnimationOptions>(16, Allocator.Persistent);

            this.nameMappings = new DataList<StyleNameMapping>(64, Allocator.Persistent);

            this.queries = new DataList<QueryPair>(64, Allocator.Persistent);
            this.queryTables = new DataList<QueryTable>(32, Allocator.Persistent);
            this.blockQueryRequirements = new DataList<BitSet>(256, Allocator.Persistent);

            this.propertyLocatorBuffer = new DataList<PropertyLocator>(1024, Allocator.Persistent);
            this.transitionLocatorBuffer = new DataList<TransitionLocator>(32, Allocator.Persistent);
            this.transitionTable = new DataList<TransitionDefinition>(16, Allocator.Persistent);

            this.gridCellTable = new DataList<GridCellDefinition>(16, Allocator.Persistent);
            this.gridTemplateTable = new DataList<GridTemplate>(8, Allocator.Persistent);

            this.animationMap = new Dictionary<ModuleAndNameKey, AnimationReference>();
            this.animationKeyFrameRanges = new DataList<AnimationKeyFrameRange>(8, Allocator.Persistent);
            this.animationProperties = new DataList<PropertyKeyFrames>(32, Allocator.Persistent);
            this.animationKeyFrames = new DataList<KeyFrameInfo>(64, Allocator.Persistent);
            this.customPropertyIdMap = new Dictionary<ModuleAndNameKey, PropertyId>();
            this.customPropertyParsers = new LightList<IStylePropertyParser>();

            styles.size = 1; // 0 is invalid
            styles[0] = default;
            numberRangeStart = 1;
            numberRangeEnd = numberRangeStart + k_NumberRangeSize;

            // setup default grid template
            gridCellTable.Add(new GridCellDefinition(1, GridTemplateUnit.MaxContent));
            gridTemplateTable.Add(new GridTemplate() {
                offset = 0, cellCount = 1
            });

            defaultValueIndices = new DataList<int>(PropertyParsers.PropertyCount, Allocator.Persistent);

            queryTables.Add(new QueryTable() {
                queryType = QueryType.Root,
                queryId = new QueryId(0),
                queryData = default
            });

            queryTables.Add(new QueryTable() {
                queryType = QueryType.State,
                queryId = new QueryId(1),
                queryData = new QueryData() {
                    stateInfo = new QueryDataStateInfo() {
                        state = StyleState.Hover
                    }
                }
            });

            queryTables.Add(new QueryTable() {
                queryType = QueryType.State,
                queryId = new QueryId(2),
                queryData = new QueryData() {
                    stateInfo = new QueryDataStateInfo() {
                        state = StyleState.Focus
                    }
                }
            });

            queryTables.Add(new QueryTable() {
                queryType = QueryType.State,
                queryId = new QueryId(3),
                queryData = new QueryData() {
                    stateInfo = new QueryDataStateInfo() {
                        state = StyleState.Active
                    }
                }
            });

            InitializeGenerated();
        }

        public int PropertyTypeCount => PropertyParsers.PropertyCount + customPropertyIdMap.Count;

        partial void InitializeGenerated();

        partial void DisposeGenerated();

        public void Dispose() {
            DisposeGenerated();
            attributeTagger.Dispose();
            variableTagger.Dispose();
            conditionTagger.Dispose();
            tagger.Dispose();
            gridTemplateTable.Dispose();
            gridCellTable.Dispose();
            propertyLocatorBuffer.Dispose();
            transitionLocatorBuffer.Dispose();
            transitionTable.Dispose();
            defaultValueIndices.Dispose();
            queries.Dispose();
            queryTables.Dispose();
            blockQueryRequirements.Dispose();
            styles.Dispose();
            styleBlocks.Dispose();
            stateHooks.Dispose();
            animationOptionsTable.Dispose();
            animationProperties.Dispose();
            animationKeyFrames.Dispose();

            for (int i = 0; i < attrTables.size; i++) {
                attrTables[i].elementIds.Dispose();
            }

            TypedUnsafe.Dispose(attrTables.array, Allocator.Persistent);
            attrTables = default;
            if (s_DebuggedStyleDatabase == this) {
                s_DebuggedStyleDatabase = null;
            }
        }

        ~StyleDatabase() {
            Dispose();
        }

        internal void Initialize() {

            int tagCount = attributeTagger.ActiveTagCount();

            attrTables = new CheckedArray<AttributeMemberTable>(TypedUnsafe.MallocCleared<AttributeMemberTable>(tagCount, Allocator.Persistent), tagCount);

        }

        internal void EnsureAdditionalStyleCount(int size) {
            styleBlocks.EnsureAdditionalCapacity(size);
        }

        internal void EnsureAdditionalPropertyCount(int count) {
            propertyLocatorBuffer.EnsureAdditionalCapacity(count);
        }

        // todo -- add overload that filters by module 
        public StyleId FindStyleFromPartialPath(string path, string styleName) {
            TryGetStyleFromPartialPath(path, styleName, out StyleId styleId);
            return styleId;
        }

        public bool TryGetStyleFromPartialPath(string filePath, string styleName, out StyleId style) {
            style = default;
            throw new NotImplementedException("replace string intern");
            int targetNameId = default; // internSystem.GetIndex(styleName);
            if (targetNameId == -1) {
                return false;
            }

            StructList<int> buffer = StructList<int>.Get();
            bool result = TryGetStyleFromPartialPathInternal(buffer, filePath, targetNameId, out style);
            buffer.Release();
            return result;
        }

        private bool TryGetStyleFromPartialPathInternal(StructList<int> buffer, string filePath, int targetNameId, out StyleId style) {

            throw new NotImplementedException();
            //
            // style = default;
            //
            // for (int i = 0; i < fileList.size; i++) {
            //     if (fileList.array[i].filePath.EndsWith(filePath, StringComparison.Ordinal)) {
            //         buffer.Add(i);
            //     }
            // }
            //
            // for (int i = 0; i < buffer.size; i++) {
            //     StyleNameMapping[] nameMappings = fileList.array[buffer[i]].nameMappings;
            //     int idx = BinarySearchNameMappings(nameMappings, targetNameId);
            //
            //     if (idx >= 0) {
            //         style = nameMappings[idx].styleId;
            //         return true;
            //     }
            // }
            //
            // return false;

        }

        private int BinarySearchNameMappings(StyleNameMapping[] mappings, int targetId) {
            int start = 0;
            int end = mappings.Length - 1;

            while (start <= end) {
                int index = start + (end - start >> 1);
                int num3 = mappings[index].styleNameId - targetId;

                if (num3 == 0) {
                    return index;
                }

                if (num3 < 0) {
                    start = index + 1;
                }
                else {
                    end = index - 1;
                }
            }

            return ~start;
        }

        internal bool TryGetStyle(string filePath, string styleName, out StyleDesc style) {

            int nameId = tagger.GetTagId(styleName);
            if (nameId < 0) {
                style = default;
                return false;
            }

            if (styleSheetMap.TryGetValue(filePath, out StyleSheetInfo sheetInfo)) {

                RangeInt mappingRange = sheetInfo.nameMappingRange;

                for (int i = mappingRange.start; i < mappingRange.end; i++) {
                    if (nameMappings[i].styleNameId == nameId) {
                        style = styles[(int) (nameMappings[i].styleId.id - numberRangeStart)];
                        return true;
                    }
                }
            }

            style = default;
            return false;

        }

        internal bool TryGetStyle(string filePath, CharSpan styleName, out StyleDesc style) {

            if (styleSheetMap.TryGetValue(filePath, out StyleSheetInfo sheetInfo)) {
                RangeInt mappingRange = sheetInfo.nameMappingRange;

                int tagId = tagger.GetTagId(styleName);

                for (int i = mappingRange.start; i < mappingRange.end; i++) {
                    if (nameMappings[i].styleNameId == tagId) {
                        style = styles[(int) (nameMappings[i].styleId.id - numberRangeStart)];
                        return true;
                    }
                }
            }

            style = default;
            return false;

        }

        internal struct StyleSheetInfo {

            public string fileName;
            public string moduleName;
            public RangeInt nameMappingRange;

        }

        internal struct StyleNameMapping : IComparable<StyleNameMapping> {

            public StyleId styleId;
            public int styleNameId;

            public int CompareTo(StyleNameMapping other) {
                return styleNameId - other.styleNameId;
            }

        }

        private int AddToPropertyTable<T>(ref DataList<T> table, T value) where T : unmanaged, IEquatable<T> {
            for (int i = 0; i < table.size; i++) {
                if (table[i].Equals(value)) {
                    return i;
                }
            }

            table.Add(value);
            return table.size - 1;
        }

        private int AddToPropertyTable<T>(ref DataList<T> table, byte* buffer, int offset) where T : unmanaged, IEquatable<T> {
            T value = *(T*) (buffer + offset);

            for (int i = 0; i < table.size; i++) {
                if (table[i].Equals(value)) {
                    return i;
                }
            }

            table.Add(value);
            return table.size - 1;
        }

        private void AddAnimationProperty(PropertyId propertyId, byte* valueBuffer, RangeInt offset, ref int location) {
            // todo -- maybe accept some other struct w/ more data instead of offset 
            if (offset.length == 0) {
                // if we're supposed to use the `current` value the offset wil be 0 length
                location = KeyFrameResult.k_Current;
            }
            else {
                AddProperty(propertyId.index, valueBuffer, offset.start, ref location);
            }
        }

        partial void AddProperty(PropertyId propertyId, byte* valueBuffer, int offset, ref int location);

        // todo -- support explicitly naming style sheets `export stylesheet xyz` in addition to implicitly treating files as style sheets 
        internal void AddStylesFromCompilation(StyleCompilationContext ctx) {

            // how do i reference selectors and stuff from other compiled styles?
            // i guess i need to look those up in the style db based on the file 

            CharStringBuilder stringBuilder = CharStringBuilder.Get();

            StyleFileShell shell = ctx.shell;
            StructList<CompiledStyle> compiledStyles = ctx.compiledStyles;
            StructList<CompiledProperty> propertyList = ctx.propertyBuffer;
            StructList<CompiledTransition> transitionList = ctx.transitionBuffer;

            EnsureAdditionalStyleCount(compiledStyles.size);
            EnsureAdditionalPropertyCount(propertyList.size);

            StructStack<BlockTraversal> stack = StructStack<BlockTraversal>.Get();

            RangeInt nameMappingRanges = new RangeInt(nameMappings.size, compiledStyles.size);
            nameMappings.EnsureAdditionalCapacity(compiledStyles.size);
            nameMappings.size += compiledStyles.size;
            CheckedArray<StyleNameMapping> localMappings = GetNameMappings(nameMappingRanges);
            DataList<QueryId> localQueries = new DataList<QueryId>(k_MaxBlockCountPerStyle, Allocator.Temp);
            DataList<ulong> localQueryRequirements = new DataList<ulong>(k_MaxBlockCountPerStyle, Allocator.Temp);
            DataList<CompiledProperty> scratchBuffer = new DataList<CompiledProperty>(PropertyTypeCount, Allocator.TempJob);

            int longsPerPropertyMap = LongBoolMap.GetMapSize(PropertyTypeCount);
            ulong* mapBuffer = stackalloc ulong[longsPerPropertyMap];
            PropertyMap map = new PropertyMap(mapBuffer, longsPerPropertyMap);
            
            fixed (byte* valueBuffer = ctx.propertyValueBuffer.array) {

                ProcessAnimations(ctx.compiledAnimations, valueBuffer, shell);

                for (int i = 0; i < compiledStyles.size; i++) {

                    localQueryRequirements.size = 0;
                    localQueries.size = 0;

                    ref CompiledStyle compiledStyle = ref compiledStyles.array[i];

                    string styleName = compiledStyle.styleName;

                    int nameId = tagger.TagString(styleName);

                    int firstDbBlock = styleBlocks.size;

                    stack.Push(new BlockTraversal() {
                        styleBlock = compiledStyle.rootBlock,
                        parentIdx = 0 // points to self, need to special case access so we always include the root block so this is fine anyway
                    });

                    ushort blockCount = 0;

                    // i might want to rethink what constitutes a block since a block without properties should contribute to condition requirements of children but not actually create a block

                    // todo -- collapse blocks with no properties, maybe this needs a pre-pass

                    while (stack.size != 0) {

                        BlockTraversal blockTraversal = stack.PopUnchecked();

                        // important that this is a copy! we're going to change some fields on it
                        CompiledStyleBlock compiledStyleBlock = blockTraversal.styleBlock; // blockBuffer[blockTraversal.blockIdx.id];

                        int propertyPtr = compiledStyleBlock.propertyStart;
                        int transitionPtr = compiledStyleBlock.transitionStart;

                        int propertyStart = propertyLocatorBuffer.size;
                        int transitionStart = transitionLocatorBuffer.size;
                        int stateHookStart = stateHooks.size;

                        scratchBuffer.size = 0;

                        map.Clear();

                        while (propertyPtr != 0) {
                            ref CompiledProperty property = ref propertyList.array[propertyPtr];
                            if (map.TrySetIndex(property.propertyKey.index)) {
                                scratchBuffer.Add(property);
                            }

                            propertyPtr = property.next;
                        }

                        map.Clear();

                        while (transitionPtr != 0) {
                            ref CompiledTransition transition = ref transitionList.array[transitionPtr];

                            if (map.TrySetIndex(transition.propertyId.index)) {
                                transitionLocatorBuffer.Add(new TransitionLocator(transition.propertyId, AddTransition(transition)));
                            }

                            transitionPtr = transition.next;
                        }

                        if (compiledStyleBlock.animationActionCount != 0) {

                            for (int a = 0; a < compiledStyleBlock.animationActionCount; a++) {

                                AnimationActionData action = ctx.animationActionBuffer[compiledStyleBlock.animationActionStart + a];

                                string moduleName = ctx.shell.GetString(action.moduleName) ?? ctx.shell.module.moduleName;
                                string animName = ctx.shell.GetString(action.animationName);

                                ModuleAndNameKey lookup = new ModuleAndNameKey(moduleName, animName);

                                if (!animationMap.TryGetValue(lookup, out AnimationReference animationReference)) {
                                    Debug.Log("Cannot find animation: " + lookup.name);
                                    continue;
                                }

                                // foreach override -> apply it
                                //if (animationList[animation].compiledAnimation.options) {
                                //    // override as appropriate
                                //}

                                // action.compiledAnimation.options;

                                // todo -- use default to resolve option overrides

                                stateHooks.Add(new StateHook() {
                                    hookEvent = action.hookEvent,
                                    hookType = action.hookType,
                                    animationReference = animationReference,
                                });

                            }
                        }

                        // have < 64 tables that we care about 
                        // each 'block' needs to tell the style which queries it needs
                        // the style needs to map those requirements into bit indices, not the block

                        for (int x = 0; x < scratchBuffer.size; x++) {

                            CompiledProperty property = scratchBuffer[x];

                            int indexInPropertyTable = -1;

                            PropertyId propertyId = property.propertyKey.index;
                            if (propertyId == PropertyId.GridLayoutColTemplate || propertyId == PropertyId.GridLayoutRowTemplate) {
                                DataList<GridCellDefinition> gridBuffer = new DataList<GridCellDefinition>(property.valueRange.length / sizeof(GridCellDefinition), Allocator.Temp);
                                ExtractGridTemplate(valueBuffer, property.valueRange, ref gridBuffer);
                                GridLayoutTemplate templateId = new GridLayoutTemplate(AddGridTemplateProperty(gridBuffer));
                                byte* buffer = (byte*) &templateId;
                                indexInPropertyTable = AddToPropertyTable(ref propertyTable_GridLayoutTemplate, buffer, 0);
                                gridBuffer.Dispose();

                            }
                            else {
                                AddProperty(property.propertyKey.index, valueBuffer, property.valueRange.start, ref indexInPropertyTable);
                            }

                            if (indexInPropertyTable == -1) {
                                // error probably
                                Debug.Log("Unable to add property:" + propertyId);
                                continue;
                            }

                            propertyLocatorBuffer.AddUnchecked(new PropertyLocator((PropertyId) property.propertyKey.index, indexInPropertyTable, property.propertyKey.variableNameId));

                        }

                        StyleStateByte stateRequirement = 0;
                        ParseBlockNode parseBlockNode = shell.blocks[compiledStyleBlock.parseBlockId.id];

                        bool invertQuery = parseBlockNode.invert;
                        QueryType queryType = default;
                        QueryData queryData = default;

                        switch (parseBlockNode.type) {

                            case BlockNodeType.Attribute:
                                // we need to give each attr key & value a unique tag id that is consistent across the whole db
                                // also need to keep the strings somewhere to test incoming strings against 

                                // need to lookup the key & value from the file shell 
                                CharSpan keySpan = shell.GetCharSpan(parseBlockNode.blockData.attributeData.attrKeyRange);
                                CharSpan valueSpan = shell.GetCharSpan(parseBlockNode.blockData.attributeData.attrValueRange);

                                int tagId;
                                if (valueSpan.HasValue) {
                                    stringBuilder.size = 0;
                                    stringBuilder.Append(keySpan);
                                    stringBuilder.Append('=');
                                    stringBuilder.Append(valueSpan);

                                    tagId = attributeTagger.TagString(stringBuilder.characters, stringBuilder.size);
                                }
                                else {
                                    tagId = attributeTagger.TagString(keySpan.data, keySpan.Length);
                                }

                                queryData.attrInfo = new QueryDataAttributeInfo() {
                                    tagId = tagId,
                                };

                                switch (parseBlockNode.blockData.attributeData.compareOp) {
                                    case AttributeOperator.Equal:
                                        queryType = QueryType.HasAttributeWithValue;
                                        break;

                                    case AttributeOperator.Exists:
                                        queryType = QueryType.HasAttribute;
                                        break;

                                    case AttributeOperator.Contains:
                                        throw new NotImplementedException("Attribute contains is not implemented");

                                    case AttributeOperator.StartsWith:
                                        throw new NotImplementedException("Attribute StartsWith is not implemented");

                                    case AttributeOperator.EndsWith:
                                        throw new NotImplementedException("Attribute EndsWith is not implemented");

                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }

                                break;

                            // todo - implement feature
                            case BlockNodeType.Root:
                                queryType = QueryType.Root;
                                break;

                            case BlockNodeType.FirstChild:
                                queryType = QueryType.FirstChild;
                                break;

                            case BlockNodeType.OnlyChild:
                                queryType = QueryType.OnlyChild;
                                break;

                            case BlockNodeType.LastChild:
                                queryType = QueryType.LastChild;
                                break;

                            case BlockNodeType.NthChild: {
                                queryType = QueryType.NthChild;
                                queryData.nthChild = new QueryDataNthChildInfo() {
                                    stepSize = parseBlockNode.blockData.nthChildData.stepSize,
                                    offset = parseBlockNode.blockData.nthChildData.offset,
                                };
                                break;
                            }

                            case BlockNodeType.ChildCount: {

                                queryData.childCountInfo = new QueryDataChildCountInfo() {
                                    op = parseBlockNode.blockData.childCountData.op,
                                    targetCount = parseBlockNode.blockData.childCountData.number
                                };
                                break;
                            }

                            case BlockNodeType.State:
                                queryType = QueryType.State;
                                queryData.stateInfo.state = parseBlockNode.stateRequirement;
                                stateRequirement = (StyleStateByte)parseBlockNode.stateRequirement;
                                break;

                            case BlockNodeType.Condition:
                                queryType = QueryType.Condition;
                                queryData.conditionInfo.tagId = conditionTagger.TagString(shell.GetString(parseBlockNode.blockData.conditionData.conditionRange));
                                break;
                        }

                        ulong requirements = 0; // parent requirements | own requirements

                        if (blockCount != 0) { // root block never has queries
                            QueryId queryId = GetOrConstructQueryTable(queryType, queryData, invertQuery);
                            int localQueryIndex = GetLocalQueryIndex(ref localQueries, queryId);
                            requirements |= localQueryRequirements[blockTraversal.parentIdx];
                            requirements |= (1ul << localQueryIndex);
                        }

                        localQueryRequirements.Add(requirements);

                        if (blockCount > 0) {
                            stateRequirement |= styleBlocks[blockTraversal.parentIdx].sortKey.stateRequirements;
                        }

                        if (blockTraversal.depth > k_MaxBlockCountPerStyle) {
                            throw new Exception($"Style depth is too large, max style depth is {k_MaxBlockCountPerStyle}! {shell.filePath} style: {styleName}");
                        }

                        if (blockCount > k_MaxBlockCountPerStyle) {
                            throw new Exception($"Style has too many blocks, max style block count is {k_MaxBlockCountPerStyle}! {shell.filePath} style: {styleName}");
                        }

                        BlockUsageSortKey sortKey = new BlockUsageSortKey() {
                            indexInSource = 0, // needs to be set before use  
                            sourceType = BlockSourceType.Style,
                            distanceToSource = 0,
                            // since we go backwards via stack traversal we invert the number here like that so the last block has the highest priority
                            localBlockPriority = (byte) (k_MaxBlockCountPerStyle - blockCount),
                            localBlockDepth = (byte) blockTraversal.depth,
                            stateRequirements = stateRequirement,
                            stateCount = (byte) math.countbits((uint) stateRequirement)
                        };

                        StyleBlock styleBlock = new StyleBlock() {
                            sortKey = sortKey,
                            propertyStart = propertyStart,
                            transitionStart = transitionStart,
                            stateHookStart = stateHookStart,
                            propertyCount = (ushort) (propertyLocatorBuffer.size - propertyStart),
                            transitionCount = (ushort) (transitionLocatorBuffer.size - transitionStart),
                            stateHookCount = (ushort) (stateHooks.size - stateHookStart),
                        };

                        styleBlocks.Add(styleBlock);

                        CompiledStyleBlock ptr = compiledStyleBlock.firstChild;

                        ushort depth = (ushort) (blockTraversal.depth + 1);

                        // reverse this? maybe doesn't matter since we already have an index on the blocks for sorting purposes
                        while (ptr != null) {

                            stack.Push(new BlockTraversal() {
                                styleBlock = ptr,
                                parentIdx = blockCount,
                                depth = depth
                            });

                            ptr = ptr.nextSibling;
                        }

                        blockCount++;

                    }

                    if (blockCount > k_MaxBlockCountPerStyle) {
                        // critical error!!!
                        throw new MissingDiagnosticException($"Critical style error: Styles can declare at most {k_MaxBlockCountPerStyle} blocks but " + styleName + " defined " + blockCount + " blocks.");
                    }

                    int queryOffset = queries.size;
                    int queryCount = localQueries.size;

                    for (int qIdx = 0; qIdx < localQueries.size; qIdx++) {

                        queries.Add(new QueryPair() {
                            bitIdx = qIdx,
                            queryId = localQueries[qIdx]
                        });
                    }

                    // 1 per block 
                    for (int x = 0; x < blockCount; x++) {
                        blockQueryRequirements.Add(new BitSet(localQueryRequirements[x]));
                    }

                    StyleId styleId = new StyleId((uint) styles.size + numberRangeStart, (uint) firstDbBlock, blockCount);
                    // styleId -> list<animationActions> (conditionMask, sortKey, keyframes....)

                    localMappings.array[i].styleId = styleId;
                    localMappings.array[i].styleNameId = nameId;

                    styleMetaMap[styleId.id - numberRangeStart] = new StyleMeta() {
                        filePath = shell.filePath,
                        styleName = styleName
                    };

                    styles.Add(new StyleDesc() {
                        styleId = styleId,
                        nameId = nameId,
                        queryLocationInfo = new QueryLocationInfo(queryOffset, queryCount)
                    });

                }

            }

            // sort by style name id
            NativeSortExtension.Sort(nameMappings.array + nameMappingRanges.start, nameMappingRanges.length);

            StructStack<BlockTraversal>.Release(ref stack);
            scratchBuffer.Dispose();
            localQueries.Dispose();
            localQueryRequirements.Dispose();

            CharStringBuilder.Release(stringBuilder);
            styleSheetMap[shell.filePath] = new StyleSheetInfo() {
                nameMappingRange = nameMappingRanges,
                fileName = shell.filePath,
                moduleName = default // todo 
            };

        }

        private void ProcessAnimations(StructList<CompiledAnimation> animationList, byte* valueBuffer, StyleFileShell shell) {

            StructList<CompiledPropertyKeyFrames> buffer = StructList<CompiledPropertyKeyFrames>.Get();

            for (int i = 0; i < animationList.size; i++) {

                ref CompiledAnimation animation = ref animationList.array[i];
                int animationPropertiesStart = animationProperties.size;

                // sorted by property and keyframe time

                animation.propertyKeyFrames.Sort((a, b) => a.key.index > b.key.index ? 1 : -1);

                for (int p = 0; p < animation.propertyKeyFrames.size; p++) {

                    int targetIndex = animation.propertyKeyFrames.array[p].key.index;

                    int end = p + 1;
                    for (; end < animation.propertyKeyFrames.size; end++) {
                        if (animation.propertyKeyFrames.array[end].key.index != targetIndex) {
                            break;
                        }
                    }

                    buffer.size = 0;

                    for (int x = p; x < end; x++) {
                        buffer.Add(animation.propertyKeyFrames.array[x]);
                    }

                    // todo -- this assumes all timings are in the same unit, we need to ensure that they are at parse time 

                    buffer.Sort((a, b) => a.time.value < b.time.value ? -1 : 1);

                    if (buffer[0].time.value != 0) {
                        // todo -- inject the 0 key frame w/ value == -1 to signify we use whatever was currently the value or use the [init] value
                    }

                    int propertyValueId = 0;
                    int previousPropertyValueId = 0;
                    ushort previousPropertyVariableId = 0;

                    int keyFramesStart = animationKeyFrames.size;

                    // if there is nothing the value buffer then we mark the property as 'current' for the purposes of animation

                    AddAnimationProperty(targetIndex, valueBuffer, buffer[0].valueRange, ref previousPropertyValueId);

                    for (int x = 1; x < buffer.size; x++) {

                        CompiledPropertyKeyFrames property = buffer.array[x];

                        AddAnimationProperty(targetIndex, valueBuffer, property.valueRange, ref propertyValueId);

                        animationKeyFrames.Add(new KeyFrameInfo() {
                            startTime = buffer[x - 1].time,
                            endTime = buffer[x].time,
                            startValueId = previousPropertyValueId,
                            endValueId = propertyValueId,
                            startVarId = previousPropertyVariableId,
                            endVarId = property.key.variableNameId
                        });

                        previousPropertyValueId = propertyValueId;
                        previousPropertyVariableId = property.key.variableNameId;
                    }

                    if (buffer[buffer.size - 1].time.value != 1f) {
                        animationKeyFrames.Add(new KeyFrameInfo() {
                            startTime = buffer[buffer.size - 1].time,
                            endTime = new UITimeMeasurement(1f, UITimeMeasurementUnit.Percentage), // todo -- time unit must match declaration type
                            startValueId = previousPropertyValueId,
                            endValueId = previousPropertyValueId, // todo -- use a setting to decide if we animate towards the style value or just carry the last value through
                            startVarId = previousPropertyVariableId,
                            endVarId = previousPropertyVariableId
                        });
                    }

                    // finally add the property to keyframe mapping
                    animationProperties.Add(new PropertyKeyFrames() {
                        propertyId = targetIndex,
                        keyFrames = new RangeInt(keyFramesStart, animationKeyFrames.size - keyFramesStart)
                    });

                    p = end - 1;
                }

                string moduleName = shell.module.moduleName;
                string animationName = shell.GetString(animation.animationName);

                ModuleAndNameKey key = new ModuleAndNameKey(moduleName, animationName);

                if (animationMap.ContainsKey(key)) {
                    Debug.Log($"Duplicate animation defined in module `{key.moduleName}`: `{key.name}`");
                    continue;
                }

                // todo -- maybe use keyframe ranges as the animation Id? can encode to long

                animationMap.Add(key, new AnimationReference() {
                    animationId = new AnimationId(animationKeyFrameRanges.size),
                    optionId = new OptionId(animationOptionsTable.size)
                });

                // todo -- generate an options id  instead of storing defaults here

                animationOptionsTable.Add(animation.options);
                animationKeyFrameRanges.Add(new AnimationKeyFrameRange(animationPropertiesStart, animationProperties.size - animationPropertiesStart));
            }

            buffer.Release();

        }

        private ushort AddGridTemplateProperty(DataList<GridCellDefinition> template) {

            int count = template.size;
            GridCellDefinition* ptr = template.GetArrayPointer();
            GridCellDefinition* gridBuffer = gridCellTable.GetArrayPointer();

            for (int i = 0; i < gridTemplateTable.size; i++) {
                GridTemplate item = gridTemplateTable[i];
                if (item.cellCount == count && TypedUnsafe.MemCmp(ptr, gridBuffer + item.offset)) {
                    return (ushort) i;
                }
            }

            gridTemplateTable.Add(new GridTemplate() {
                offset = gridCellTable.size,
                cellCount = template.size
            });

            gridCellTable.AddRange(template.GetArrayPointer(), template.size);

            return (ushort) (gridTemplateTable.size - 1);
        }

        private static void ExtractGridTemplate(byte* valueBuffer, RangeInt propertyValueRange, ref DataList<GridCellDefinition> trackList) {
            trackList.size = 0;

            int cellCount = propertyValueRange.length / sizeof(GridCellDefinition);

            GridCellDefinition* trackSizeBuffer = (GridCellDefinition*) (valueBuffer + propertyValueRange.start);

            for (int i = 0; i < cellCount; i++) {
                trackList.Add(*(trackSizeBuffer + i));
            }

        }

        private int AddTransition(CompiledTransition compiledTransition) {

            TransitionDefinition* array = transitionTable.GetArrayPointer();

            for (int i = transitionTable.size - 1; i >= 0; i--) {
                if (UnsafeUtility.MemCmp(&compiledTransition.definition, array + i, sizeof(TransitionDefinition)) == 0) {
                    return i;
                }
            }

            transitionTable.Add(compiledTransition.definition);
            return transitionTable.size - 1;

        }

        private static int GetLocalQueryIndex(ref DataList<QueryId> localQueries, QueryId queryId) {

            for (int i = 0; i < localQueries.size; i++) {
                if (localQueries[i].id == queryId.id) {
                    return i;
                }
            }

            localQueries.Add(queryId);
            return localQueries.size - 1;
        }

        private QueryId GetOrConstructQueryTable(QueryType queryType, in QueryData queryData, bool invertQuery) {

            // todo -- probably want to have a hashtable over this 
            for (int i = 0; i < queryTables.size; i++) {

                if (queryTables[i].invert == invertQuery && queryTables[i].queryType == queryType && queryTables[i].queryData == queryData) {
                    return queryTables[i].queryId;
                }

            }

            QueryId retn = new QueryId(queryTables.size);

            queryTables.Add(new QueryTable() {
                queryId = retn,
                queryData = queryData,
                queryType = queryType,
                invert = invertQuery
            });

            return retn;

        }

        public StyleId GetStyleId(string filePath, CharSpan styleName) {
            if (string.IsNullOrEmpty(filePath) || !styleName.HasValue) {
                return default;
            }

            int targetId = tagger.GetTagId(styleName);

            if (targetId < 0) {
                return default;
            }

            if (!styleSheetMap.TryGetValue(filePath, out StyleSheetInfo sheet)) {
                return default;
            }

            RangeInt mappingRange = sheet.nameMappingRange;

            for (int i = mappingRange.start; i < mappingRange.end; i++) {
                if (nameMappings[i].styleNameId == targetId) {
                    return nameMappings[i].styleId;
                }
            }

            return default;

        }

        public StyleId GetStyleId(string filePath, string styleName) {

            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(styleName)) {
                return default;
            }

            int targetId = tagger.GetTagId(styleName);

            if (targetId < 0) {
                return default;
            }

            if (!styleSheetMap.TryGetValue(filePath, out StyleSheetInfo sheet)) {
                return default;
            }

            RangeInt mappingRange = sheet.nameMappingRange;

            for (int i = mappingRange.start; i < mappingRange.end; i++) {
                if (nameMappings[i].styleNameId == targetId) {
                    return nameMappings[i].styleId;
                }
            }

            return default;

        }

        public int GetTotalPropertyCount(StyleId styleId) {
            if (!MathUtil.Between(styleId.id, numberRangeStart, numberRangeEnd)) {
                return -1;
            }

            int id = (int) (styleId.id - numberRangeStart);
            StyleDesc style = styles[id];
            int cnt = 0;
            for (int i = 0; i < styleId.blockCount; i++) {
                cnt += styleBlocks[style.rootBlockId + i].propertyCount;
            }

            return cnt;
        }

        internal bool TryGetStyleMetaData(StyleId styleId, out StyleMeta meta) {
            if (!MathUtil.Between(styleId.id, numberRangeStart, numberRangeEnd)) {
                meta = default;
                return false;
            }

            uint id = (uint) (styleId.id - numberRangeStart);
            return styleMetaMap.TryGetValue(id, out meta);
        }

        public bool TryGetStyleDesc(StyleId styleId, out StyleDesc desc) {
            if (!MathUtil.Between(styleId.id, numberRangeStart, numberRangeEnd)) {
                desc = default;
                return false;
            }

            desc = styles[(int) (styleId.id - numberRangeStart)];
            return true;

        }

        public struct Table<T> where T : unmanaged, IEquatable<T> {

            public int size;
            public T* array;

        }

        private struct BlockTraversal {

            public CompiledStyleBlock styleBlock;
            public ushort parentIdx;
            public ushort depth;

        }

        public struct StyleMeta {

            public string styleName;
            public string filePath;

        }

        public int GetQueryTableIndex(QueryType type, QueryData queryData) {
            for (int i = 0; i < queryTables.size; i++) {
                if (queryTables[i].queryType == type && queryTables[i].queryData == queryData) {
                    return i;
                }
            }

            return -1;
        }

        public int GetLocalQueryIndex(StyleId styleId, QueryId queryId) {
            if (!MathUtil.Between(styleId.id, numberRangeStart, numberRangeEnd)) {
                return 0;
            }

            if (!TryGetStyleDesc(styleId, out StyleDesc desc)) {
                return 0;
            }

            int queryCount = desc.queryLocationInfo.queryCount;
            int queryOffset = desc.queryLocationInfo.queryOffset;

            for (int i = 0; i < queryCount; i++) {

                if (queries[queryOffset + i].queryId.id == queryId.id) {
                    return i;
                }

            }

            return -1;

        }

        public int GetEnumTypeId(Type type) {
            if (!type.IsEnum) return 0;

            for (int i = 0; i < s_EnumTypeIds.Length; i++) {
                if (type == s_EnumTypeIds[i]) {
                    return i + 1; // +1 because 0 is the empty value
                }
            }

            return 0;

        }

        public bool TryGetStyleSheetInfo(string filePath, out StyleSheetInfo styleSheetInfo) {
            return styleSheetMap.TryGetValue(filePath, out styleSheetInfo);
        }

        public CheckedArray<StyleNameMapping> GetNameMappings(RangeInt nameMappingRange) {
            return new CheckedArray<StyleNameMapping>(nameMappings.array + nameMappingRange.start, nameMappingRange.length);
        }

        internal T GetPropertyForTesting<T>(StyleId styleId, int p1, PropertyId paddingLeft) {
            throw new NotImplementedException();
        }

        partial void SerializeDatabase(ref ManagedByteBuffer writer);

        partial void DeserializeDatabase(ref ManagedByteBuffer reader);

        partial void VerifySerializedDatabase(StyleDatabase b);

        public void Verify(StyleDatabase db) {
            VerifySerializedDatabase(db);
        }

        private bool StyleSheetMapsEqual(StyleDatabase other) {
            KeyValuePair<string, StyleSheetInfo>[] arrayA = styleSheetMap.ToArray();
            KeyValuePair<string, StyleSheetInfo>[] arrayB = other.styleSheetMap.ToArray();

            if (arrayA.Length != arrayB.Length) {
                return false;
            }

            Array.Sort(arrayA, (a, b) => String.Compare(a.Key, b.Key, StringComparison.Ordinal));
            Array.Sort(arrayB, (a, b) => String.Compare(a.Key, b.Key, StringComparison.Ordinal));

            for (int i = 0; i < arrayA.Length; i++) {
                if (arrayA[i].Value.fileName != arrayB[i].Value.fileName) {
                    return false;
                }

                if (arrayA[i].Value.moduleName != arrayB[i].Value.moduleName) {
                    return false;
                }

                if (arrayA[i].Value.nameMappingRange.start != arrayB[i].Value.nameMappingRange.start) {
                    return false;
                }

                if (arrayA[i].Value.nameMappingRange.length != arrayB[i].Value.nameMappingRange.length) {
                    return false;
                }
            }

            return true;

        }

        public bool TryGetAnimationIdByName(string module, string animationName, out AnimationReference reference) {
            return animationMap.TryGetValue(new ModuleAndNameKey(module, animationName), out reference);
        }

        public QueryTable GetQueryTable(QueryId query) {
            return queryTables[query.id];
        }

        public PropertyId GetCustomPropertyId(string moduleName, string propertyName) {
            customPropertyIdMap.TryGetValue(new ModuleAndNameKey(moduleName, propertyName), out PropertyId propertyId);
            return propertyId;
        }

        public bool TryRegisterCustomFloatProperty(ModuleAndNameKey key, float defaultValue) {

            if (customPropertyIdMap.TryGetValue(key, out PropertyId propertyId)) {
                return false;
            }

            propertyTypeById.Add(PropertyType.Single);

            defaultValueIndices.Add(AddToPropertyTable(ref propertyTable_Single, defaultValue));

            int propertyIndex = PropertyParsers.PropertyCount + customPropertyIdMap.Count;
            propertyId = (PropertyId) propertyIndex;
            customPropertyIdMap.Add(key, propertyId);
            customPropertyParsers.Add(s_FloatParser);

            return true;
        }

        private static readonly FloatParser s_FloatParser = new FloatParser();

        public IStylePropertyParser GetCustomPropertyParser(PropertyId propertyId) {

            if (propertyId.index < PropertyParsers.PropertyCount) {
                return null;
            }

            return customPropertyParsers[propertyId.index - PropertyParsers.PropertyCount];

        }

        private void SerializeCustomPropertyIdMap(ref ManagedByteBuffer writer) {
            writer.Write(customPropertyIdMap.Count);

            foreach (KeyValuePair<ModuleAndNameKey, PropertyId> kvp in customPropertyIdMap) {
                writer.Write(kvp.Key.moduleName);
                writer.Write(kvp.Key.name);
                writer.Write(kvp.Value.id);
            }
        }

        private void DeserializeCustomPropertyIdMap(ref ManagedByteBuffer buffer) {
            buffer.Read(out int count);
            if (customPropertyIdMap != null) {
                customPropertyIdMap.Clear();
            }
            else {
                customPropertyIdMap = new Dictionary<ModuleAndNameKey, PropertyId>(count);
            }

            for (int i = 0; i < count; i++) {
                buffer.Read(out string moduleName);
                buffer.Read(out string propertyName);
                buffer.Read(out int id);
                ModuleAndNameKey key = new ModuleAndNameKey(moduleName, propertyName);
                customPropertyIdMap[key] = (PropertyId) id;
            }
        }

        public bool TryGetCustomPropertyId(string moduleName, string propertyName, out PropertyId propertyId) {
            return customPropertyIdMap.TryGetValue(new ModuleAndNameKey(moduleName, propertyName), out propertyId);
        }

        public string GetCustomPropertyName(PropertyId propertyId) {
            foreach (KeyValuePair<ModuleAndNameKey, PropertyId> kvp in customPropertyIdMap) {
                if (kvp.Value == propertyId) {
                    return kvp.Key.moduleName + "::" + kvp.Key.name;
                }
            }

            return "Invalid / Unknown";
        }

        public int GetCustomPropertyRange(out PropertyId propertyId) {
            propertyId = (PropertyId) PropertyParsers.PropertyCount;
            return customPropertyIdMap.Count;
        }

        private static readonly char[] s_SplitChars = new char[] {':', ':'};

        [NonSerialized] private Dictionary<string, PropertyId> propertyIdNameCache = new Dictionary<string, PropertyId>();

        public PropertyId GetCustomPropertyId(string propertyName) {

            if (propertyIdNameCache.TryGetValue(propertyName, out PropertyId propertyId)) {
                return propertyId;
            }

            string[] str = propertyName.Split(s_SplitChars, StringSplitOptions.RemoveEmptyEntries);

            if (str.Length != 2) {
                propertyIdNameCache[propertyName] = PropertyId.Invalid;
                return PropertyId.Invalid;
            }

            ModuleAndNameKey moduleAndNameKey = new ModuleAndNameKey(str[0], str[1]);
            if (customPropertyIdMap.TryGetValue(moduleAndNameKey, out propertyId)) {
                propertyIdNameCache[propertyName] = propertyId;
                return propertyId;
            }

            propertyIdNameCache[propertyName] = PropertyId.Invalid;

            return PropertyId.Invalid;

        }

        internal static bool TryParseCustomPropertyTypeName(CharSpan typeName, out CustomPropertyType propertyType) {
            CustomPropertyType retn = default;
            bool valid = false;
            TryParseCustomPropertyTypeName_Generated(typeName, ref valid, ref retn);
            if (valid) {
                propertyType = retn;
                return true;
            }

            propertyType = default;
            return false;
        }

        internal bool TryRegisterCustomProperty(string moduleName, string propertyName, CustomPropertyType propertyType, ref PropertyParseContext context) {
            ModuleAndNameKey key = new ModuleAndNameKey(moduleName, propertyName);
            bool valid = false;

            RegisterCustomProperty_Generated(key, propertyType, ref context, ref valid);
            return valid;
        }

        static partial void TryParseCustomPropertyTypeName_Generated(CharSpan typeName, ref bool valid, ref CustomPropertyType propertyType);

        partial void RegisterCustomProperty_Generated(ModuleAndNameKey key, CustomPropertyType propertyType, ref PropertyParseContext context, ref bool valid);


        public bool TryRegisterCustomProperty_Enum(string moduleName, string propertyName, int enumValue, EnumParser enumParser) {
            ModuleAndNameKey key = new ModuleAndNameKey(moduleName, propertyName);
            
            if (customPropertyIdMap.TryGetValue(key, out PropertyId propertyId)) {
                return false;
            }

            propertyTypeById.Add(PropertyType.Enum);

            defaultValueIndices.Add(AddToPropertyTable(ref propertyTable_EnumValue, new EnumValue(enumValue)));

            int propertyIndex = PropertyParsers.PropertyCount + customPropertyIdMap.Count;
            propertyId = (PropertyId) propertyIndex;
            customPropertyIdMap.Add(key, propertyId);
            customPropertyParsers.Add(enumParser);
            return true;
        }

    }

}