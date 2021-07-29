using System;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {

    internal class ByteParser : IStylePropertyParser {

        private static ByteParser instance;

        public static ByteParser Instance {
            get {
                if (instance == null) instance = new ByteParser();
                return instance;
            }
        }

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {
            throw new NotImplementedException();
        }

        public bool TryParse(ref PropertyParseContext context, out byte b) {
            throw new NotImplementedException();
        }

    }

}