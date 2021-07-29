using System;
using UIForia.Compilers;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {

    internal class TextureInfoParser : IStylePropertyParser {
        
        private static TextureInfoParser instance;

        public static TextureInfoParser Instance => instance ??= new TextureInfoParser();
        
        public bool TryParse(ref PropertyParseContext context, out TextureInfo value) {
            //
            // if (!context.charStream.TryParseInt(out value)) {
            //     context.diagnostics.LogError($"Unable to parse an int value from {context.charStream}");
            //     return false;
            // }
            throw new NotImplementedException();

        }
        
        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {
            valueRange = default;

            if (TryParse(ref context, out TextureInfo value)) {
                valueRange = valueBuffer.WriteWithRange(value);
                return true;
            }

            return false;
        }

    }

}