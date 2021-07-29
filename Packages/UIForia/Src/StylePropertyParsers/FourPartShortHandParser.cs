using UIForia.Compilers;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {
    internal abstract class FourPartShortHandParser<T> : ValueShortHandParser<T>, IStyleShorthandParser where T : unmanaged {

        private PropertyId top;
        private PropertyId bottom;
        private PropertyId left;
        private PropertyId right;

        protected FourPartShortHandParser(string shortHandName, PropertyId top, PropertyId bottom, PropertyId left, PropertyId right) : base(shortHandName) {
            this.top = top;
            this.bottom = bottom;
            this.left = left;
            this.right = right;
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
                        shorthandPropertyBuffer.Add(new CompiledProperty(top, valueRange, variableId));
                        shorthandPropertyBuffer.Add(new CompiledProperty(right, valueRange, variableId));
                        shorthandPropertyBuffer.Add(new CompiledProperty(bottom, valueRange, variableId));
                        shorthandPropertyBuffer.Add(new CompiledProperty(left, valueRange, variableId));
                    }
                    break;
                }
                case 2: {

                    if (TryParseValueOrVariable(ref context, ref slotBuffer.array[0], out length, out variableId)) {
                        RangeInt valueRange = valueBuffer.WriteWithRange(length);
                        shorthandPropertyBuffer.Add(new CompiledProperty(top, valueRange, variableId));
                        shorthandPropertyBuffer.Add(new CompiledProperty(bottom, valueRange, variableId));
                    }

                    if (TryParseValueOrVariable(ref context, ref slotBuffer.array[1], out length, out variableId)) {
                        RangeInt valueRange = valueBuffer.WriteWithRange(length);
                        shorthandPropertyBuffer.Add(new CompiledProperty(right, valueRange, variableId));
                        shorthandPropertyBuffer.Add(new CompiledProperty(left, valueRange, variableId));
                    }

                    break;
                }
                case 3:

                    if (TryParseValueOrVariable(ref context, ref slotBuffer.array[0], out length, out variableId)) {
                        RangeInt valueRange = valueBuffer.WriteWithRange(length);
                        shorthandPropertyBuffer.Add(new CompiledProperty(top, valueRange, variableId));
                    }

                    if (TryParseValueOrVariable(ref context, ref slotBuffer.array[1], out length, out variableId)) {
                        RangeInt valueRange = valueBuffer.WriteWithRange(length);
                        shorthandPropertyBuffer.Add(new CompiledProperty(right, valueRange, variableId));
                        shorthandPropertyBuffer.Add(new CompiledProperty(left, valueRange, variableId));
                    }

                    if (TryParseValueOrVariable(ref context, ref slotBuffer.array[2], out length, out variableId)) {
                        RangeInt valueRange = valueBuffer.WriteWithRange(length);
                        shorthandPropertyBuffer.Add(new CompiledProperty(bottom, valueRange, variableId));
                    }
                    break;
                case 4:

                    if (TryParseValueOrVariable(ref context, ref slotBuffer.array[0], out length, out variableId)) {
                        RangeInt valueRange = valueBuffer.WriteWithRange(length);
                        shorthandPropertyBuffer.Add(new CompiledProperty(top, valueRange, variableId));
                    }

                    if (TryParseValueOrVariable(ref context, ref slotBuffer.array[1], out length, out variableId)) {
                        RangeInt valueRange = valueBuffer.WriteWithRange(length);
                        shorthandPropertyBuffer.Add(new CompiledProperty(right, valueRange, variableId));
                    }

                    if (TryParseValueOrVariable(ref context, ref slotBuffer.array[2], out length, out variableId)) {
                        RangeInt valueRange = valueBuffer.WriteWithRange(length);
                        shorthandPropertyBuffer.Add(new CompiledProperty(bottom, valueRange, variableId));
                    }

                    if (TryParseValueOrVariable(ref context, ref slotBuffer.array[3], out length, out variableId)) {
                        RangeInt valueRange = valueBuffer.WriteWithRange(length);
                        shorthandPropertyBuffer.Add(new CompiledProperty(left, valueRange, variableId));
                    }
                    break;
                default:
                    context.diagnostics.LogError($"{shortHandName} only supports 1-4 values.");
                    return false;
            }
            
            return true;
        }
    }
}