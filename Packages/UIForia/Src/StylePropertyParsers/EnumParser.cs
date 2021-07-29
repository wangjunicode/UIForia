using System;
using System.Collections.Generic;
using UIForia.Extensions;
using UIForia.Compilers;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {

    internal class EnumParser : IStylePropertyParser {

        private static Dictionary<Type, EnumParser> parserMap;

        private Type enumType;

        private EnumParser(Type type) {
            this.enumType = type;
        }

        public static bool TryParse(ref PropertyParseContext context, Type type, out int i, out EnumParser enumParser) {

            parserMap ??= new Dictionary<Type, EnumParser>();

            if (parserMap.TryGetValue(type, out EnumParser parser)) {
                enumParser = parser;
                return parser.TryParse(ref context, out i);
            }

            if (!type.IsEnum) {
                context.diagnostics.LogError(type.GetTypeName() + " is not an enum but is trying to be treated like one.");
                i = default;
                enumParser = default;
                return false;
            }

            enumParser = new EnumParser(type);
            parserMap[type] = enumParser;
            return enumParser.TryParse(ref context, out i);
        }

        public bool TryParse(ref PropertyParseContext context, out int enumValue) {

            if (!context.charStream.TryParseEnum(enumType, out enumValue)) {
                context.diagnostics.LogError($"Unable to parse {context.charStream} into {enumType.GetTypeName()}");
                return false;
            }

            return true;
        }

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {
            valueRange = default;
            if (TryParse(ref context, out int val)) {
                valueRange = valueBuffer.WriteWithRange(val);
                return true;
            }

            return false;
        }

    }

    internal class EnumParser<T> : IStylePropertyParser where T : Enum {

        private static EnumParser<T> instance;

        public static EnumParser<T> Instance => instance ??= new EnumParser<T>();

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {

            valueRange = default;

            if (!context.charStream.TryParseEnum<T>(out int val)) {
                context.diagnostics.LogError($"Unable to parse {context.charStream} into {typeof(T).GetTypeName()}");
                return false;
            }

            valueRange = valueBuffer.WriteWithRange(val);

            return true;
        }

    }

    internal class EnumFlagParser<T> : IStylePropertyParser where T : Enum {

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {

            valueRange = default;

            int enumVal = default;

            do {

                if (!context.charStream.TryParseEnum<T>(out int val)) {
                    context.diagnostics.LogError($"Unable to parse {context.charStream} into {typeof(T).GetTypeName()}");
                    return false;
                }

                enumVal |= val;

            } while (context.charStream.TryParseCharacter('|'));

            valueRange = valueBuffer.WriteWithRange(enumVal);

            return true;
        }

    }

}