using System;
using UIForia.Parsing.Expressions;

namespace UIForia.Compilers {

    public class ExposedVariableData {

        public Type rootType;
        public ScopedContextVariable[] scopedVariables;
        public AttributeDefinition[] exposedAttrs;

    }

}