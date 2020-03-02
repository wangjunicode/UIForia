using System;
using UIForia.Parsing;

namespace UIForia.Compilers {

    public class ExposedVariableData {

        public Type rootType;
        public ContextVariableDefinition[] scopedVariables;
        public AttributeDefinition[] exposedAttrs;

    }

}