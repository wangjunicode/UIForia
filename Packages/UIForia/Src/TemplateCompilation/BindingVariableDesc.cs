using System;

namespace UIForia.Compilers {

    public struct BindingVariableDesc {

        public Type variableType;
        public Type originTemplateType;
        public string variableName;
        public int index;
        public BindingVariableKind kind;

    }

}