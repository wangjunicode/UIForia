using System;
using System.Collections.Generic;
using System.Threading;
using UIForia.Exceptions;
using UIForia.Rendering;
using UIForia.Style;
using UIForia.Util;

namespace UIForia.Style2 {

    public struct Style {

        private int styleId;
        private StyleSheet2 styleSheet;

        internal Style(StyleSheet2 sheet, int styleId) {
            this.styleSheet = sheet;
            this.styleId = styleId;
        }

        public int GetPropertyCount(StyleState state = 0) {
            return styleSheet.GetPropertyRange(styleId, state).length;
        }

        public bool TryGetProperty(PropertyId propertyId, out StyleProperty2 property, StyleState state = StyleState.Normal) {
            if (styleSheet == null) {
                property = default;
                return false;
            }

            Range16 range = styleSheet.GetPropertyRange(styleId, state);

            for (int i = range.start; i < range.end; i++) {
                if (styleSheet.properties.array[i].propertyId.id == propertyId.id) {
                    property = styleSheet.properties.array[i];
                    return true;
                }
            }

            property = default;
            return false;
        }

    }

    internal unsafe struct InternalStyle {

        public int id;
        public CharSpan name;
        public fixed uint ranges[4];

        public InternalStyle(CharSpan name, int id) : this() {
            this.id = id;
            this.name = name;
        }

    }

    internal class TopSortNode {

        public int mark;
        public ParsedStyle style;
        public LightList<TopSortNode> dependencies;

    }

    internal class DependencySorter {

        private LightList<TopSortNode> temp;
        private StructList<ParsedStyle> sorted;

        public void Sort(IList<ParsedStyle> styles) {
            LightList<TopSortNode> source = new LightList<TopSortNode>(styles.Count);

            for (int i = 0; i < styles.Count; i++) {
                // need to resolve dependencies and map to top sort nodes
                TopSortNode n = new TopSortNode();
                n.style = styles[i];
                n.mark = 0;
                n.dependencies = new LightList<TopSortNode>();
                source.Add(n);
            }

            while (source.size != 0) {
                Visit(source[0]);
            }
        }

        internal void Visit(TopSortNode node) {
            if (node.mark == 1) {
                // error
            }

            if (node.mark == 0) {
                node.mark = 1;
                // get dependencies
                for (int i = 0; i < node.dependencies.size; i++) {
                    Visit(node.dependencies[i]);
                }

                node.mark = 2;
                sorted.Add(node.style);
            }
        }

    }

    // parser can check xml for for </Style> and create file for it if needed without actually parsing the content

    // parsing != building
    // parsing totally done in parallel because no building is needed
    // building totally done in parallel because all data needed for building is present
    // building probably able to run in jobs
    // if path not found should be reported at parse time
    // if variable or style not found should reported after parse before first build?

    // crunching style sheet -> new implicit sheet = more memory usage (probably configurable)

    // xml style files? xml parser probably needs to either call parser in a 1 off fashion or we parse all xml files, add pseudo style files, parse those too

    // 2nd pass to build style parse dependencies? or encounter as needed?

    public class StyleSheet2 {

        public readonly Module module;
        public readonly string filePath;
        internal readonly char[] source;

        internal ParsedMixin[] mixins;
        internal PendingConstant[] constants;
        internal ParsedStyle[] rawStyles;

        internal StructList<InternalStyle> styles;
        internal StructList<StyleProperty2> properties;
        private LightStack<CharSpan> mixinTracer;

        // animations
        // spritesheets
        // sounds
        // cursors

        internal StyleBodyPart[] parts;
        public readonly ushort id;

        private static int idGenerator;
        [ThreadStatic] private static StructList<char> s_CharBuffer;

        internal StyleSheet2(Module module, string filePath, char[] source) {
            this.id = (ushort) Interlocked.Add(ref idGenerator, 1);
            this.module = module;
            this.filePath = filePath;
            this.source = source;
            this.properties = new StructList<StyleProperty2>();
            this.mixinTracer = new LightStack<CharSpan>(); // dont like this
        }

        public bool TryGetStyle(string styleName, out Style style) {
            for (int i = 0; i < styles.size; i++) {
                if (styles[i].name == styleName) {
                    style = new Style(this, styles[i].id);
                    return true;
                }
            }

            style = default;
            return false;
        }

        private unsafe void BuildExternalStyle(ref InternalStyle style) { }

        // todo -- this needs to be the last of the build steps
        private unsafe void BuildStyle(ref ParsedStyle parsedStyle) {
            int NextMultiple = BitUtil.NextMultipleOf32(PropertyParsers.PropertyCount) >> 5; // divide by 32

            StyleProperty2* normal = stackalloc StyleProperty2[PropertyParsers.PropertyCount];
            StyleProperty2* active = stackalloc StyleProperty2[PropertyParsers.PropertyCount];
            StyleProperty2* hover = stackalloc StyleProperty2[PropertyParsers.PropertyCount];
            StyleProperty2* focus = stackalloc StyleProperty2[PropertyParsers.PropertyCount];
            StyleProperty2** states = stackalloc StyleProperty2*[4];
            IntBoolMap* maps = stackalloc IntBoolMap[4];

            uint* map0 = stackalloc uint[NextMultiple];
            uint* map1 = stackalloc uint[NextMultiple];
            uint* map2 = stackalloc uint[NextMultiple];
            uint* map3 = stackalloc uint[NextMultiple];

            maps[StyleStateIndex.Normal] = new IntBoolMap(map0, NextMultiple);
            maps[StyleStateIndex.Active] = new IntBoolMap(map1, NextMultiple);
            maps[StyleStateIndex.Hover] = new IntBoolMap(map2, NextMultiple);
            maps[StyleStateIndex.Focus] = new IntBoolMap(map3, NextMultiple);

            states[StyleStateIndex.Normal] = normal;
            states[StyleStateIndex.Active] = active;
            states[StyleStateIndex.Hover] = hover;
            states[StyleStateIndex.Focus] = focus;

            StyleBuildData buildData = new StyleBuildData() {
                states = states,
                targetSheet = this,
                targetStyleName = parsedStyle.name,
                maps = maps
            };

            InternalStyle style = new InternalStyle();

            // note: we run through the style properties BACKWARDS so that we can do a quick test to see if a property was already set.
            // if it was, we dont need to do more work, we can just continue to the next instruction. 
            int index = parsedStyle.rangeEnd - 1;

            BuildPartList(ref buildData, ref index, parsedStyle.rangeStart);

            style.name = parsedStyle.name;

            // this could easily be converted to a native list if we need that

            int normalCount = buildData.maps[StyleStateIndex.Normal].Occupancy;
            int hoverCount = buildData.maps[StyleStateIndex.Hover].Occupancy;
            int activeCount = buildData.maps[StyleStateIndex.Active].Occupancy;
            int focusCount = buildData.maps[StyleStateIndex.Focus].Occupancy;

            style.ranges[StyleStateIndex.Normal] = new Range16(properties.size, normalCount);

            for (int i = 0; i < normalCount; i++) {
                properties.Add(buildData.states[StyleStateIndex.Normal][i]);
            }

            style.ranges[StyleStateIndex.Hover] = new Range16(properties.size, hoverCount);

            for (int i = 0; i < hoverCount; i++) {
                properties.Add(buildData.states[StyleStateIndex.Hover][i]);
            }

            style.ranges[StyleStateIndex.Active] = new Range16(properties.size, activeCount);

            for (int i = 0; i < activeCount; i++) {
                properties.Add(buildData.states[StyleStateIndex.Active][i]);
            }

            style.ranges[StyleStateIndex.Focus] = new Range16(properties.size, focusCount);

            for (int i = 0; i < focusCount; i++) {
                properties.Add(buildData.states[StyleStateIndex.Focus][i]);
            }

            styles.Add(style);
        }

        private void BuildApplyMixin(Part_ApplyMixin mixin, ref StyleBuildData buildData) {
            if (mixin.isVariable) {
                throw new NotImplementedException();
            }

            for (int i = 0; i < mixinTracer.size; i++) {
                
                if (mixinTracer.array[i] != mixin.mixinName) {
                    continue;
                }
                
                string error = mixinTracer.array[0].ToString();
                
                for (int j = 1; j <= i; j++) {
                    error += " -> ";
                    error += mixinTracer.array[j].ToString();
                }

                throw new ParseException("Found recursion in building mixin:" + error);
            }

            // find mixin, probably only works locally for now
            for (int i = 0; i < mixins.Length; i++) {

                if (mixins[i].name == mixin.mixinName) {
                    mixinTracer.Push(mixin.mixinName);
                    int idx = (int) mixins[i].rangeEnd - 1;
                    BuildPartList(ref buildData, ref idx, (int) mixins[i].rangeStart);
                    mixinTracer.Pop();
                    return;
                }

            }

            // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
            ParseException ex = new ParseException("Unable to find local mixin with name " + mixin.mixinName + " on line " + mixin.mixinName.GetLineNumber());
            ex.SetFileName(filePath);
            throw ex;
        }

        private static unsafe void BuildProperty(Part_Property part, ref StyleBuildData buildData) {
            int propertyIdIndex = part.value.propertyId.index;
            ref IntBoolMap map = ref buildData.maps[buildData.stateIndex];
            StyleProperty2* state = buildData.states[buildData.stateIndex];

            if (map.TrySetIndex(propertyIdIndex)) {
                state[map.Occupancy - 1] = part.value;
            }
        }

        private unsafe void BuildVariableProperty(Part_VariableProperty variableProperty, ref StyleBuildData buildData) {
            int propertyIdIndex = variableProperty.propertyId.index;
            ref IntBoolMap map = ref buildData.maps[buildData.stateIndex];
            StyleProperty2* state = buildData.states[buildData.stateIndex];

            if (map.TrySetIndex(propertyIdIndex)) {
                s_CharBuffer = s_CharBuffer ?? new StructList<char>(128);
                s_CharBuffer.size = 0;

                StyleProperty2 property = default;

                CharStream stream = new CharStream(variableProperty.declaration);

                int idx = stream.NextIndexOf('@');

                // todo this needs a while loop around it, could have multiple constants

                if (idx != -1) {
                    s_CharBuffer.AddRange(source, stream.IntPtr, idx - stream.IntPtr);

                    stream.AdvanceTo(idx + 1);

                    if (!stream.TryParseIdentifier(out CharSpan identifier)) { }

                    if (stream.TryParseCharacter('.')) {
                        if (!stream.TryParseIdentifier(out CharSpan dotIdentifier)) {
                            throw new NotImplementedException();
                        }

                        // todo -- figure out how to handle referenced constants
                        // probably want to localize them at parse time and resolve in the build step. match against whole name

                        throw new NotImplementedException();
                    }
                    else {
                        if (TryResolveLocalConstant(identifier, out CharSpan span)) {
                            s_CharBuffer.EnsureAdditionalCapacity(span.Length);
                            for (int j = span.rangeStart; j < span.rangeEnd; j++) {
                                s_CharBuffer.array[s_CharBuffer.size++] = span.data[j];
                            }
                        }
                    }

                    CharStream parseStream = new CharStream(s_CharBuffer.array, 0, (uint) s_CharBuffer.size);

                    if (!PropertyParsers.s_parseEntries[propertyIdIndex].parser.TryParse(parseStream, variableProperty.propertyId, out property)) {
                        // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                        ParseException ex = new ParseException($"Error parsing property {variableProperty.propertyId} on line {variableProperty.declaration.GetLineNumber()}");
                        ex.SetFileName(filePath);
                        throw ex;
                    }
                }

                state[map.Occupancy - 1] = property;
            }
        }

        private unsafe void ReplaceConstants(StructList<char> buffer, CharStream propertyStream) {
            CharStream duplicate = new CharStream(propertyStream);

            while (duplicate.HasMoreTokens) {
                int idx = duplicate.NextIndexOf('@');

                if (idx == -1) {
                    AppendRangeToCharBuffer(buffer, duplicate.Data, duplicate.IntPtr, (int) duplicate.End);
                    break;
                }

                AppendRangeToCharBuffer(buffer, duplicate.Data, duplicate.IntPtr, idx);

                duplicate.AdvanceTo(idx + 1);

                // leave whitespace after identifier since we'll scoop that into the char buffer
                if (!duplicate.TryParseIdentifier(out CharSpan identifier, true, WhitespaceHandling.ConsumeBefore)) {
                    throw new ParseException($"Expected to find a valid identifier after the @ sign on line {duplicate.GetLineNumber()}");
                }

                if (!TryResolveLocalConstant(identifier, out CharSpan constant)) {
                    throw new ParseException($"Unresolved constant reference in property {propertyStream} on line {propertyStream.GetLineNumber()}");
                }

                AppendRangeToCharBuffer(buffer, constant);
            }
        }

        private static unsafe void AppendRangeToCharBuffer(StructList<char> buffer, CharSpan charSpan) {
            AppendRangeToCharBuffer(buffer, charSpan.data, charSpan.rangeStart, charSpan.rangeEnd);
        }

        private static unsafe void AppendRangeToCharBuffer(StructList<char> buffer, char* ptr, int start, int end) {
            buffer.EnsureAdditionalCapacity(end - start);
            for (int i = start; i < end; i++) {
                buffer.array[buffer.size++] = ptr[i];
            }
        }

        private unsafe void BuildVariableShorthand(Part_VariablePropertyShorthand shorthand, ref StyleBuildData buildData) {
            CharStream stream = new CharStream(shorthand.declaration);

            s_CharBuffer = s_CharBuffer ?? new StructList<char>(64);
            s_CharBuffer.size = 0;

            ReplaceConstants(s_CharBuffer, stream);

            //note! line numbers will be wrong in parser probably. we can fix this by storing a line number, or by injecting n new lines into s_CharBuffer.
            CharStream parseStream = new CharStream(s_CharBuffer.array, 0, (ushort) s_CharBuffer.size);
            ShorthandEntry entry = PropertyParsers.s_ShorthandEntries[shorthand.shorthandIndex];

            // need a temp buffer to store output from parser
            // using our properties list, will remove immediately afterwards
            int propertyCount = properties.size;

            try {

                if (!entry.parser.TryParse(parseStream, properties)) {
                    ParseException ex = new ParseException($"Failed to parse shorthand '{PropertyParsers.s_ShorthandNames[shorthand.shorthandIndex]}' with value '{parseStream}' on line {stream.GetLineNumber()}. Original value was '{shorthand.declaration}'.");
                    ex.SetFileName(filePath);
                    throw ex;
                }

                for (int i = propertyCount; i < properties.size; i++) {
                    int propertyIdIndex = properties.array[i].propertyId.index;
                    ref IntBoolMap map = ref buildData.maps[buildData.stateIndex];
                    StyleProperty2* state = buildData.states[buildData.stateIndex];
                    if (map.TrySetIndex(propertyIdIndex)) {
                        state[map.Occupancy - 1] = properties.array[i];
                    }
                }
            }
            finally {
                // remove the temporary properties
                while (properties.size != propertyCount) {
                    properties.size--;
                }
            }
        }

        private void BuildPartList(ref StyleBuildData buildData, ref int index, int rangeStart) {

            while (index >= rangeStart) {

                ref StyleBodyPart part = ref parts[index];

                switch (part.type) {

                    case BodyPartType.Style: {
                        throw new InvalidArgumentException("Should never hit a Style part while building. Styles cannot be nested");
                    }

                    case BodyPartType.EnterState: {
                        index--;
                        BuildStyleState(ref buildData, ref index, part.enterState);
                        break;
                    }

                    case BodyPartType.Property: {
                        index--;
                        BuildProperty(part.property, ref buildData);
                        break;
                    }

                    case BodyPartType.VariableProperty: {
                        index--;
                        BuildVariableProperty(part.variableProperty, ref buildData);
                        break;
                    }

                    case BodyPartType.VariablePropertyShorthand: {
                        index--;
                        BuildVariableShorthand(part.variableShorthand, ref buildData);
                        break;
                    }

                    case BodyPartType.ApplyMixin: {
                        index--;
                        BuildApplyMixin(part.applyMixin, ref buildData);
                        break;
                    }

                    case BodyPartType.ConditionBlock: {
                        Part_ConditionBlock conditionBlock = part.conditionBlock;

                        if (!module.GetDisplayConditions()[conditionBlock.conditionId]) {
                            index = conditionBlock.rangeStart;
                        }

                        index--; // need to -1 even if we hit the jump condition above

                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

        }

        private void BuildStyleState(ref StyleBuildData buildData, ref int index, in Part_EnterState data) {
            int oldIndex = buildData.stateIndex;
            buildData.stateIndex = data.stateIndex;

            BuildPartList(ref buildData, ref index, data.rangeStart);

            buildData.stateIndex = oldIndex;
        }

        private void BuildLocalConstants() {
            if (constants == null) return;

            List<bool> moduleConditions = module.GetDisplayConditions();

            for (int i = 0; i < constants.Length; i++) {
                ref PendingConstant constant = ref constants[i];

                constant.resolvedValue = default;

                if (!constant.HasDefinition) {
                    throw new ParseException($"Cannot find a definition for constant {constant}");
                }

                if (moduleConditions != null) {
                    for (int j = constant.partRange.start; j < constant.partRange.end; j++) {
                        ref StyleBodyPart part = ref parts[j];
                        Part_ConstantBranch branch = part.constantBranch;
                        if (moduleConditions[branch.conditionId]) {
                            constant.resolvedValue = branch.value;
                            break;
                        }
                    }
                }

                if (constant.resolvedValue != null) {
                    constant.resolvedValue = constant.defaultValue;
                }
            }
        }

        private bool TryResolveLocalConstant(CharSpan identifier, out CharSpan charSpan) {
            if (identifier.Length == 0) {
                charSpan = default;
                return false;
            }

            for (int i = 0; i < constants.Length; i++) {
                ref PendingConstant constant = ref constants[i];

                if (constant.name != identifier) {
                    continue;
                }

                charSpan = constant.resolvedValue;
                return true;
            }

            charSpan = default;
            return false;
        }

        public void Build() {
            BuildLocalConstants();

            if (rawStyles != null) {
                // todo -- can probably be an array
                if (styles == null) {
                    styles = new StructList<InternalStyle>(rawStyles.Length);
                }

                styles.Clear();
                properties.Clear();
                for (int i = 0; i < rawStyles.Length; i++) {
                    BuildStyle(ref rawStyles[i]);
                }
            }
        }

        public string GetConstant(string s) {
            CharSpan span = new CharSpan(s);
            if (TryResolveLocalConstant(span, out CharSpan value)) {
                return value.ToString();
            }

            return null;
        }

        internal void SetParts(StyleBodyPart[] partArray) {
            this.parts = partArray;
        }

        internal void SetStyles(ParsedStyle[] rawStyleArray) {
            this.rawStyles = rawStyleArray;
        }

        public void SetMixins(ParsedMixin[] rawMixins) {
            this.mixins = rawMixins;
        }

        internal void SetConstants(PendingConstant[] constants) {
            this.constants = constants;
        }

        public Range16 GetPropertyRange(int styleId, StyleState state) {
            unsafe {
                ref InternalStyle ptr = ref styles.array[styleId];
                switch (state) {
                    case StyleState.Normal:
                        return new Range16(ptr.ranges[StyleStateIndex.Normal]);

                    case StyleState.Hover:
                        return new Range16(ptr.ranges[StyleStateIndex.Hover]);

                    case StyleState.Active:
                        return new Range16(ptr.ranges[StyleStateIndex.Active]);

                    case StyleState.Focused:
                        return new Range16(ptr.ranges[StyleStateIndex.Focus]);

                    default:
                        return new Range16(
                            new Range16(ptr.ranges[StyleStateIndex.Normal]).start,
                            new Range16(ptr.ranges[StyleStateIndex.Focus]).end
                        );
                }
            }
        }

    }

}