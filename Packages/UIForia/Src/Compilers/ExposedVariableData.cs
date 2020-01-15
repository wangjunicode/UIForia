using System;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Compilers {

    public class ExposedVariableData {

        public Type rootType;
        public ScopedContextVariable[] scopedVariables;
        public AttributeDefinition2[] exposedAttrs;

    }

}