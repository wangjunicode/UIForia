using System;
using UIForia.Util;

namespace UIForia.Style {

    public class FixedLengthParser : IStylePropertyParser {

        public bool TryParse(CharStream stream, PropertyId propertyId, Diagnostics diagnostics, out StyleProperty2 property) {
            throw new NotImplementedException();
        }

        public bool TryParseFromBinding(CharStream stream, PropertyId propertyId, Diagnostics diagnostics, out StyleProperty2 property) {
            throw new NotImplementedException();
        }

    }

}