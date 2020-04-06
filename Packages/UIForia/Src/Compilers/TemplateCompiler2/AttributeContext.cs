using System;
using System.Collections.Generic;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Compilers {

    public struct AttributeContext {

        public int size;

        public Type rootType;
        public Type[] referencedTypes;

        public ReadOnlySizedArray<AttributeDefinition> attributes;

        public IList<string> GetNamespaces(int depth) {
            throw new NotImplementedException();
        }

    }

}