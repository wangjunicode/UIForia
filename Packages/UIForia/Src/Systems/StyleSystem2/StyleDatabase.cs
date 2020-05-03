using System;
using System.Collections.Generic;
using Packages.UIForia.Util.Unsafe;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEngine.Assertions;

namespace UIForia {

    public struct StyleSheetInfo {

        public readonly int moduleIndex;
        public readonly NameKey nameKey;

        public StyleSheetInfo(int moduleIndex, NameKey nameKey) {
            this.nameKey = nameKey;
            this.moduleIndex = moduleIndex;
        }

    }

    public struct ModuleInfo {

        public readonly NameKey nameKey;

        public ModuleInfo(NameKey nameKey) {
            this.nameKey = nameKey;
        }

    }

    public class StyleDatabase : IDisposable {

        // probably needed for user resolution. would be great to use a better structure though
        private Dictionary<string, int> join_StyleName__StyleIndex;

        public ByteBuffer buffer_staticStyleProperties;

        private readonly int dbIndex;

        // maybe introduce a new Table type that grows at a non doubling rate after initializing
        public UnmanagedList<ModuleInfo> table_ModuleInfo;
        public UnmanagedList<StaticStyleInfo> table_StyleInfo;
        public UnmanagedList<StyleSheetInfo> table_StyleSheetInfo;

        public UnmanagedList<int> join_StyleIndex__ModuleIndex;
        public UnmanagedList<int> join_StyleIndex__StyleSheetIndex;

        public UnmanagedLongMap<int> table_StyleSheetMap;
        public UnmanagedLongMap<StyleId> table_StyleIdMap;

        public LightList<string> join_StyleIndex__StyleName;
        public LightList<string> join_StyleSheetIndex__Name;

        public const int ImplicitStyleSheetIndex = 0;
        public const int ImplicitModuleIndex = 0;

        public StyleDatabase() : this(64, 512) { }

        internal static readonly List<StyleDatabase> s_DebugDatabases = new List<StyleDatabase>();

        private const string k_ImplicitModuleName = "_IMPLICIT_MODULE_";
        private const string k_ImplicitStyleSheetName = "_IMPLICIT_STYLE_SHEET_";

        public StyleDatabase(int initialStyleCount, int initialPropertyCount) {
            // for compiled output we know this already
            buffer_staticStyleProperties = new ByteBuffer(TypedUnsafe.SizeOf<StaticPropertyId, PropertyData>(initialPropertyCount), Allocator.Persistent);

            join_StyleName__StyleIndex = new Dictionary<string, int>(); // could be more than index since we already pay the search cost maybe encode other data also
            join_StyleIndex__StyleName = new LightList<string>();

            join_StyleSheetIndex__Name = new LightList<string>();

            table_ModuleInfo = new UnmanagedList<ModuleInfo>(8, Allocator.Persistent);
            table_StyleSheetMap = new UnmanagedLongMap<int>(16, Allocator.Persistent);
            table_StyleIdMap = new UnmanagedLongMap<StyleId>(128, Allocator.Persistent);

            // these could all live in a single buffer since they grow identically.
            table_StyleInfo = new UnmanagedList<StaticStyleInfo>(initialStyleCount, Allocator.Persistent);
            join_StyleIndex__ModuleIndex = new UnmanagedList<int>(initialStyleCount, Allocator.Persistent);
            join_StyleIndex__StyleSheetIndex = new UnmanagedList<int>(initialStyleCount, Allocator.Persistent);

            table_StyleSheetInfo = new UnmanagedList<StyleSheetInfo>(8, Allocator.Persistent);

            NameKey moduleNameHash = MurmurHash3.Hash(k_ImplicitModuleName);
            NameKey styleSheetHash = MurmurHash3.Hash(k_ImplicitStyleSheetName);

            // implicit module
            table_ModuleInfo.Add(new ModuleInfo(moduleNameHash));

            // implicit style sheet
            table_StyleSheetInfo.Add(new StyleSheetInfo(ImplicitModuleIndex, moduleNameHash));

            join_StyleSheetIndex__Name.Add(k_ImplicitStyleSheetName);

            table_StyleSheetMap.Add(MakeStyleSheetKey(moduleNameHash, styleSheetHash), 0);

            AddStyle("__INVALID__", ImplicitStyleSheetIndex, ImplicitModuleIndex, default(StaticStyleInfo)); // 0 index should be considered invalid

            this.dbIndex = s_DebugDatabases.Count;
            s_DebugDatabases.Add(this);
        }

        public void AddModule(string moduleName, Action<ModuleBuilder> action) {
            if (action == null) return;

            NameKey moduleNameHash = MurmurHash3.Hash(moduleName);
            // todo -- verify no duplicates

            table_ModuleInfo.Add(new ModuleInfo(moduleNameHash));
            // table_ModuleCondition.Add(default);

            action.Invoke(new ModuleBuilder(this, moduleNameHash, table_ModuleInfo.size - 1));
        }

        public void AddStyleSheet(string name, Action<StyleSheetBuilder> action) {
            AddStyleSheet(name, table_ModuleInfo[ImplicitModuleIndex].nameKey, ImplicitModuleIndex, action);
        }

        internal int AddStyleSheet(string styleSheetName, NameKey moduleNameHash, int moduleIndex, Action<StyleSheetBuilder> action) {
            if (action == null) return -1;

            int styleSheetId = table_StyleSheetInfo.size;

            NameKey styleSheetNameKey = MurmurHash3.Hash(styleSheetName);
            long styleSheetKey = MakeStyleSheetKey(moduleNameHash, styleSheetNameKey);

            if (!table_StyleSheetMap.TryAddValue(styleSheetKey, styleSheetId)) {
                // todo -- diagnostic
                throw new Exception("Duplicate style in module: " + styleSheetName);
            }

            join_StyleSheetIndex__Name.Add(styleSheetName);

            StyleSheetBuilder builder = new StyleSheetBuilder(this, moduleIndex, styleSheetId);

            action.Invoke(builder);

            table_StyleSheetInfo.Add(new StyleSheetInfo(moduleIndex, styleSheetNameKey));

            return styleSheetId;
        }

        public int AddStyle(string name, Action<StyleBuilder> action) {
            return AddStyle(name, ImplicitStyleSheetIndex, ImplicitModuleIndex, action);
        }

        internal static long MakeStyleSheetKey(NameKey moduleName, NameKey styleSheetName) {
            return BitUtil.IntsToLong(moduleName, styleSheetName);
        }

        internal static long MakeStyleKey(int styleSheetIndex, string styleName) {
            return BitUtil.IntsToLong(styleSheetIndex, MurmurHash3.Hash(styleName));
        }

        // Assumes style data is already in the buffer
        internal int AddStyle(string name, int styleSheetIndex, int moduleIndex, StaticStyleInfo styleInfo) {
            long styleKey = MakeStyleKey(styleSheetIndex, name);
            int styleIndex = table_StyleInfo.size;

            StyleState2 states = default;

            if (styleInfo.activeCount > 0) states |= StyleState2.Active;
            if (styleInfo.focusCount > 0) states |= StyleState2.Focused;
            if (styleInfo.hoverCount > 0) states |= StyleState2.Hover;
            if (styleInfo.normalCount > 0) states |= StyleState2.Normal;

            StyleId styleId = new StyleId((ushort) styleIndex, states, default, dbIndex); // todo -- selectors

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
            join_StyleName__StyleIndex.Add(name, table_StyleInfo.size - 1);
            return styleIndex;

        }

        internal int AddStyle(string name, int moduleIndex, int styleSheetIndex, Action<StyleBuilder> action) {

            StyleBuilder builder = new StyleBuilder(); // todo optimize if i use this for non test code

            action?.Invoke(builder);

            int totalPropertyCount = builder.GetSharedStylePropertyCount();

            buffer_staticStyleProperties.EnsureAdditionalCapacity<StaticPropertyId, PropertyData>(totalPropertyCount);

            int baseOffset = buffer_staticStyleProperties.GetWritePosition();

            PropertyRange activeRange = WriteSharedProperties(builder.activeGroup?.properties, ref buffer_staticStyleProperties, baseOffset);
            PropertyRange focusRange = WriteSharedProperties(builder.focusGroup?.properties, ref buffer_staticStyleProperties, baseOffset);
            PropertyRange hoverRange = WriteSharedProperties(builder.hoverGroup?.properties, ref buffer_staticStyleProperties, baseOffset);
            PropertyRange normalRange = WriteSharedProperties(builder.normalGroup?.properties, ref buffer_staticStyleProperties, baseOffset);

            return AddStyle(name, styleSheetIndex, moduleIndex, new StaticStyleInfo() {
                conditionMask = 0, // maybe init this to module mask already?
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

        }

        private struct PropertyRange {

            public ushort count;
            public ushort offset;

            public PropertyRange(int offset, int count) {
                this.count = (ushort) count;
                this.offset = (ushort) offset;
            }

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

        public void Dispose() {
            buffer_staticStyleProperties.Dispose();

            join_StyleIndex__ModuleIndex.Dispose();
            join_StyleIndex__StyleSheetIndex.Dispose();

            table_StyleInfo.Dispose();
            table_StyleSheetMap.Dispose();
            table_StyleIdMap.Dispose();

            s_DebugDatabases.Remove(this);
        }

        internal static StyleDatabase GetDatabase(int i) {
            return s_DebugDatabases[i];
        }

        internal StyleId GetStyleId(long styleKey) {
            table_StyleIdMap.TryGetValue(styleKey, out StyleId styleId);
            return styleId;
        }

        public StyleSheetInterface GetStyleSheet(string moduleName, string sheetName) {

            long key = BitUtil.IntsToLong(MurmurHash3.Hash(moduleName), MurmurHash3.Hash(sheetName));

            if (table_StyleSheetMap.TryGetValue(key, out int idx)) {
                return new StyleSheetInterface(this, idx);
            }

            return default;
        }

        internal string ResolveStyleName(ushort styleIndex) {

            if (styleIndex > join_StyleIndex__StyleName.size) {
                return "unresolved style";
            }

            int sheetIdx = join_StyleIndex__StyleSheetIndex[styleIndex];
            string sheetName = join_StyleSheetIndex__Name[sheetIdx];
            string styleName = join_StyleIndex__StyleName[styleIndex];
            return sheetName + " / " + styleName;
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

                if (keys[k].propertyId == propertyId) {
                    return values[k];
                }

            }

            return default;

        }

    }

    public struct StyleSheetInterface {

        private readonly StyleDatabase db;
        private readonly int sheetIndex;

        internal StyleSheetInterface(StyleDatabase db, int sheetIndex) {
            this.db = db;
            this.sheetIndex = sheetIndex;
        }

        public StyleId GetStyle(string styleName) {
            return db.GetStyleId(StyleDatabase.MakeStyleKey(sheetIndex, styleName));
        }

    }

}