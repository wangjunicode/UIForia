using UIForia.Compilers;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {
    internal abstract class TwoPartShortHandParser<T> : ValueShortHandParser<T>, IStyleShorthandParser where T : unmanaged {

        private PropertyId first;
        private PropertyId second;

        protected TwoPartShortHandParser(string shortHandName, PropertyId first, PropertyId second) : base(shortHandName) {
            this.first = first;
            this.second = second;
        }

        public bool TryParseFromStyleSheet(ref PropertyParseContext context,
                                           StructList<CharStream> slotBuffer,
                                           ref ManagedByteBuffer valueBuffer,
                                           StructList<CompiledProperty> shorthandPropertyBuffer) {

            int slotCount = slotBuffer.size;

            T length;
            ushort variableId;

            switch (slotCount) {
                case 1: {

                    if (TryParseValueOrVariable(ref context, ref slotBuffer.array[0], out length, out variableId)) {
                        RangeInt valueRange = valueBuffer.WriteWithRange(length);
                        shorthandPropertyBuffer.Add(new CompiledProperty(first, valueRange, variableId));
                        shorthandPropertyBuffer.Add(new CompiledProperty(second, valueRange, variableId));
                    }
                    break;
                }
                case 2: {

                    if (TryParseValueOrVariable(ref context, ref slotBuffer.array[0], out length, out variableId)) {
                        RangeInt valueRange = valueBuffer.WriteWithRange(length);
                        shorthandPropertyBuffer.Add(new CompiledProperty(first, valueRange, variableId));
                    }

                    if (TryParseValueOrVariable(ref context, ref slotBuffer.array[1], out length, out variableId)) {
                        RangeInt valueRange = valueBuffer.WriteWithRange(length);
                        shorthandPropertyBuffer.Add(new CompiledProperty(second, valueRange, variableId));
                    }

                    break;
                }
                default:
                    context.diagnostics.LogError($"{shortHandName} only supports 1-2 values.");
                    return false;
            }
            
            return true;
        }

    }
}