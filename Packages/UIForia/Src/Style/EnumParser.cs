using System;
using UIForia.Util;

namespace UIForia.Style {

    public class EnumParser<T> : IStylePropertyParser where T : Enum {

        public bool TryParse(CharStream stream, PropertyId propertyId, out StyleProperty2 property) {
            throw new NotImplementedException();
        }

    }

}