using System;
using UIForia.Style;
using UIForia.Util;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.NewStyleParsing {

    [Serializable]
    public struct CompiledSharedStyle {

        public string styleName; // might be worth giving this an id that we resolve later or just keep a style names array.
        // keeping it unmanaged is better for reading / writing

        public ushort activeOffset;
        public ushort activeCount;
        public ushort focusOffset;
        public ushort focusCount;
        public ushort hoverOffset;
        public ushort hoverCount;
        public ushort normalOffset;
        public ushort normalCount;
        public ushort totalPropertyCount;

        public void Serialize(ref ManagedByteBuffer buffer) {
            buffer.Write(styleName);

            buffer.Write(activeOffset);
            buffer.Write(activeCount);

            buffer.Write(focusOffset);
            buffer.Write(focusCount);

            buffer.Write(hoverOffset);
            buffer.Write(hoverCount);

            buffer.Write(normalOffset);
            buffer.Write(normalCount);

            buffer.Write(totalPropertyCount);
        }

        public void Deserialize(ref ManagedByteBuffer buffer) {
            buffer.Read(out styleName);

            buffer.Read(out activeOffset);
            buffer.Read(out activeCount);

            buffer.Read(out focusOffset);
            buffer.Read(out focusCount);

            buffer.Read(out hoverOffset);
            buffer.Read(out hoverCount);

            buffer.Read(out normalOffset);
            buffer.Read(out normalCount);

            buffer.Read(out totalPropertyCount);
        }

    }

    [Serializable]
    public struct CompiledStyleFile {

        public CompiledSharedStyle[] styles;
        public byte[] properties;
        public int propertyCount;

        public void Serialize(ref ManagedByteBuffer buffer) {
            buffer.Write(styles.Length);
            for (int i = 0; i < styles.Length; i++) {
                styles[i].Serialize(ref buffer);
            }

            buffer.Write(propertyCount);
            buffer.Write(properties);
        }

        public static CompiledStyleFile Deserialize(ManagedByteBuffer buffer) {
            CompiledStyleFile retn = new CompiledStyleFile();
            buffer.Read(out int styleCount);
            retn.styles = new CompiledSharedStyle[styleCount];
            for (int i = 0; i < styleCount; i++) {
                retn.styles[i].Deserialize(ref buffer);
            }

            buffer.Read(out retn.propertyCount);
            buffer.Read(retn.properties);

            return retn;
        }

    }

    public unsafe class StyleFileCompiler : IDisposable {

        private StructList<StaticPropertyId> activeKeys;
        private StructList<StaticPropertyId> focusKeys;
        private StructList<StaticPropertyId> hoverKeys;
        private StructList<StaticPropertyId> normalKeys;
        private StructList<StaticPropertyId> selectorKeys;

        private StructList<PropertyData> activeValues;
        private StructList<PropertyData> focusValues;
        private StructList<PropertyData> hoverValues;
        private StructList<PropertyData> normalValues;
        private StructList<PropertyData> selectorValues;

        private State state;

        private LightList<string> permutedPropertyValues;
        private StructList<StyleProperty2> shorthandBuffer;
        private Diagnostics diagnostics;

        // this wants to be a byte buffer
        // makes serialization forced to be binary though
        // i think thats fine
        private ByteBuffer outputProperties;
        private StructList<CompiledSharedStyle> outputStyles;

        public StyleFileCompiler() {

            this.activeKeys = new StructList<StaticPropertyId>(32);
            this.focusKeys = new StructList<StaticPropertyId>(32);
            this.hoverKeys = new StructList<StaticPropertyId>(32);
            this.normalKeys = new StructList<StaticPropertyId>(32);
            this.selectorKeys = new StructList<StaticPropertyId>(16);

            this.activeValues = new StructList<PropertyData>(32);
            this.focusValues = new StructList<PropertyData>(32);
            this.hoverValues = new StructList<PropertyData>(32);
            this.normalValues = new StructList<PropertyData>(32);
            this.selectorValues = new StructList<PropertyData>(16);

            this.shorthandBuffer = new StructList<StyleProperty2>();
            this.permutedPropertyValues = new LightList<string>();

            this.outputProperties = new ByteBuffer(16 * 1024, Allocator.Persistent);
            this.outputStyles = new StructList<CompiledSharedStyle>(32);

        }

        public bool TryCompile(ParsedStyleFile styleFile, Diagnostics diagnostics, out CompiledStyleFile result) {
            this.diagnostics = diagnostics;
            outputProperties.ResetSize();
            outputStyles.size = 0;

            Array.Sort(styleFile.propertyDefinitions);

            // find all dependencies first
            // collect / copy data from all dependencies up front, then we can compile in parallel
           // for (int i = 0; i < styleFile.dependencies.Length; i++) { }

            for (int i = 0; i < styleFile.styleNodeList.Length; i++) {

                CompileStyle(styleFile, styleFile.styleNodeList[i]);

            }

            // module id = moduleTypeNamesSorted.IndexOf(module);
            // moving a module or re-ordering its conditions should not affect cache
            // adding / removing a module should invalidate caches
            // adding / removing a module condition should invalidate caches

            byte[] p = new byte[outputProperties.byteSize];

            fixed (byte* byteptr = p) {
                UnsafeUtility.MemCpy(byteptr, outputProperties.data, outputProperties.byteSize);
            }

            result = new CompiledStyleFile() {
                styles = outputStyles.ToArray(),
                properties = p
            };

            return true;
        }

        private void ClearPropertyBuffers() {
            activeKeys.size = 0;
            focusKeys.size = 0;
            hoverKeys.size = 0;
            normalKeys.size = 0;
            activeValues.size = 0;
            focusValues.size = 0;
            hoverValues.size = 0;
            normalValues.size = 0;
        }

        private void CompileStyle(ParsedStyleFile styleFile, StyleNode styleNode) {

            ClearPropertyBuffers();
            state = default;

            CompileStylePartChildren(styleFile, styleFile.nodeList[styleNode.index]);

            ushort activeCount = (ushort) activeKeys.size;
            ushort focusCount = (ushort) focusKeys.size;
            ushort hoverCount = (ushort) hoverKeys.size;
            ushort normalCount = (ushort) normalKeys.size;

            ushort totalCount = (ushort) (activeCount + focusCount + hoverCount + normalCount);

            // store keys + values separately like they are in the style db
            int baseOffset = outputProperties.byteSize;

            ushort activeOffset = (ushort) (AppendToOutputBuffer(activeKeys, activeValues) - baseOffset);
            ushort focusOffset = (ushort) (AppendToOutputBuffer(focusKeys, focusValues) - baseOffset);
            ushort hoverOffset = (ushort) (AppendToOutputBuffer(hoverKeys, hoverValues) - baseOffset);
            ushort normalOffset = (ushort) (AppendToOutputBuffer(normalKeys, normalValues) - baseOffset);

            outputStyles.Add(new CompiledSharedStyle() {
                styleName = styleNode.styleName,
                activeCount = activeCount,
                activeOffset = activeOffset,
                focusCount = focusCount,
                focusOffset = focusOffset,
                hoverCount = hoverCount,
                hoverOffset = hoverOffset,
                normalCount = normalCount,
                normalOffset = normalOffset,
                totalPropertyCount = totalCount
            });

        }

        private int AppendToOutputBuffer(StructList<StaticPropertyId> keyBuffer, StructList<PropertyData> valueBuffer) {
            int start = outputProperties.byteSize;

            if (keyBuffer.size == 0) return start;

            fixed (StaticPropertyId* keyptr = keyBuffer.array) {
                outputProperties.WriteRange(keyptr, keyBuffer.size);
            }

            fixed (PropertyData* dataptr = valueBuffer.array) {
                outputProperties.WriteRange(dataptr, valueBuffer.size);
            }

            return start;
        }

        private void CompileStylePart(ParsedStyleFile styleFile, in StyleASTNode node) {
            switch (node.nodeType) {

                case StyleNodeType.Root:
                case StyleNodeType.Style:
                case StyleNodeType.Mixin:
                case StyleNodeType.Animation:
                case StyleNodeType.Package:
                case StyleNodeType.ConstVariable:
                    throw new Exception("invalid");

                // todo -- I think it makes more sense to handle condition id + depth in the parser
                // case StyleNodeType.ConditionBlock:
                //     state.conditionDepth++;
                //
                //     // FindConditionData();
                //     // state.conditionId = ModuleSystem.GetConditionId();
                //     
                //     CompileStylePartChildren(styleFile, styleFile.nodeList[node.index]);
                //
                //     state.conditionDepth--;
                //     break;

                case StyleNodeType.Selector:
                    // todo -- handle this & make sure not nested (might have been done in parser already)
                    break;

                case StyleNodeType.Property: {

                    PropertyDefinition propertyData = FindPropertyData(styleFile.propertyDefinitions, node.index);

                    IStylePropertyParser parser = PropertyParsers.s_ParserTable[propertyData.parserIndex];

                    PermuteConstants(styleFile, propertyData.propertyValue, permutedPropertyValues);

                    for (int i = 0; i < permutedPropertyValues.size; i++) {

                        fixed (char* charptr = permutedPropertyValues.array[i]) {

                            CharStream stream = new CharStream(charptr, 0, (uint) permutedPropertyValues.array[i].Length);

                            if (parser.TryParse(stream, propertyData.propertyId, diagnostics, out StyleProperty2 result)) {
                                WriteProperty(result, propertyData.conditionDepth, propertyData.conditionId);
                            }
                            else {
                                // todo -- diagnostics
                            }
                        }

                    }

                    break;
                }

                case StyleNodeType.ShorthandProperty: {
                    PropertyDefinition propertyData = FindPropertyData(styleFile.propertyDefinitions, node.index);

                    IStyleShorthandParser parser = PropertyParsers.s_ShorthandParserTable[propertyData.parserIndex];

                    PermuteConstants(styleFile, propertyData.propertyValue, permutedPropertyValues);

                    for (int i = 0; i < permutedPropertyValues.size; i++) {

                        fixed (char* charptr = permutedPropertyValues.array[i]) {

                            CharStream stream = new CharStream(charptr, 0, (uint) permutedPropertyValues.array[i].Length);
                            shorthandBuffer.size = 0;
                            if (parser.TryParse(stream, diagnostics, shorthandBuffer)) {
                                for (int j = 0; j < shorthandBuffer.size; j++) {
                                    WriteProperty(shorthandBuffer[i], propertyData.conditionDepth, propertyData.conditionId);
                                }
                            }
                            else {
                                // todo -- diagnostics
                            }
                        }

                    }

                    break;
                }

                case StyleNodeType.State_Active: {
                    BufferTarget old = state.bufferTarget;
                    state.bufferTarget = BufferTarget.Active;
                    CompileStylePartChildren(styleFile, node);
                    state.bufferTarget = old;
                    break;
                }

                case StyleNodeType.State_Focused: {
                    BufferTarget old = state.bufferTarget;
                    state.bufferTarget = BufferTarget.Focus;
                    CompileStylePartChildren(styleFile, node);
                    state.bufferTarget = old;
                    break;
                }

                case StyleNodeType.State_Hover: {
                    BufferTarget old = state.bufferTarget;
                    state.bufferTarget = BufferTarget.Hover;
                    CompileStylePartChildren(styleFile, node);
                    state.bufferTarget = old;
                    break;
                }

                case StyleNodeType.State_Normal: {
                    BufferTarget old = state.bufferTarget;
                    state.bufferTarget = BufferTarget.Normal;
                    CompileStylePartChildren(styleFile, node);
                    state.bufferTarget = old;
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int GetPropertyBufferSize(BufferTarget stateBufferTarget) {
            switch (stateBufferTarget) {

                case BufferTarget.Active:
                    return activeKeys.size;

                case BufferTarget.Focus:
                    return focusKeys.size;

                case BufferTarget.Hover:
                    return hoverKeys.size;

                case BufferTarget.Normal:
                    return normalKeys.size;

                case BufferTarget.Selector:
                    return selectorKeys.size;

                default:
                    throw new ArgumentOutOfRangeException(nameof(stateBufferTarget), stateBufferTarget, null);
            }
        }

        // todo -- this should take in a property value string, find all the @contantNames and replace the value.
        // we'll do this for every possible combination of constants which will include all conditional constant cases.
        private void PermuteConstants(ParsedStyleFile styleFile, string propertyValue, LightList<string> permutationList) {
            permutationList.size = 0;
            permutationList.Add(propertyValue);
        }

        private void WriteProperty(StyleProperty2 property, ushort conditionDepth, ushort conditionId) {
            StructList<StaticPropertyId> keyBuffer;
            StructList<PropertyData> dataBuffer;

            switch (state.bufferTarget) {

                case BufferTarget.Active:
                    keyBuffer = activeKeys;
                    dataBuffer = activeValues;
                    break;

                case BufferTarget.Focus:
                    keyBuffer = focusKeys;
                    dataBuffer = focusValues;
                    break;

                case BufferTarget.Hover:
                    keyBuffer = hoverKeys;
                    dataBuffer = hoverValues;
                    break;

                case BufferTarget.Normal:
                    keyBuffer = normalKeys;
                    dataBuffer = normalValues;
                    break;

                case BufferTarget.Selector:
                    keyBuffer = selectorKeys;
                    dataBuffer = selectorValues;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            dataBuffer.Add(new PropertyData() {value = property.longVal});
            keyBuffer.Add(new StaticPropertyId(property.propertyId, conditionDepth, conditionId));
        }

        private void CompileStylePartChildren(ParsedStyleFile styleFile, in StyleASTNode node) {
            int ptrIdx = styleFile.nodeList[node.index].firstChildIndex;

            while (ptrIdx != -1) {
                ref StyleASTNode child = ref styleFile.nodeList[ptrIdx];
                ptrIdx = child.nextSiblingIndex;

                CompileStylePart(styleFile, child);

            }
        }

        private static PropertyDefinition FindPropertyData(PropertyDefinition[] propertyDefinitions, int targetIndex) {
            int num1 = 0;
            int num2 = propertyDefinitions.Length - 1;
            while (num1 <= num2) {
                int index1 = num1 + (num2 - num1 >> 1);

                int compare = propertyDefinitions[index1].nodeIndex.CompareTo(targetIndex);

                if (compare == 0) {
                    return propertyDefinitions[index1];
                }

                if (compare < 0) {
                    num1 = index1 + 1;
                }
                else {
                    num2 = index1 - 1;
                }
            }

            return default;
        }

        private enum BufferTarget {

            Active,
            Focus,
            Hover,
            Normal,
            Selector

        }

        private struct State {

            public BufferTarget bufferTarget;
            public ushort conditionDepth;
            public ushort conditionId;

        }

        public void Dispose() {
            outputProperties.Dispose();
        }

    }

}