using System;
using UIForia.Util;

namespace UIForia.Style {

    public class EnumParser<T> : IStylePropertyParser where T : Enum {

        public bool TryParse(CharStream stream, PropertyId propertyId, Diagnostics diagnostics, out StyleProperty2 property) {
            throw new NotImplementedException();
        }

        public bool TryParseFromBinding(CharStream stream, PropertyId propertyId, Diagnostics diagnostics, out StyleProperty2 property) {
            throw new NotImplementedException();
        }

    }

}