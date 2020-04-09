using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UIForia.Util;

namespace UIForia.Compilers {

    public class TemplateLinqCompilerContext {

        public int depth;
        public Type elementType;
        public Type parentType;
        public BindingType bindingType;
        public IList<string> namespaces;
        
        public Expression currentEvent;
        public Expression changeHandlerCurrentValue;
        public Expression changeHandlerPreviousValue;

        public AttrInfo currentAttribute;

        public ReadOnlySizedArray<TemplateContextReference> rootVariables;
        private LightStack<StructList<BindingVariableDesc>> variableStack;

        public void Init(BindingType bindingType, Type elementType, Type parentType, ReadOnlySizedArray<TemplateContextReference> contexts, LightStack<StructList<BindingVariableDesc>> variableStack) {
            this.rootVariables = contexts;
            this.variableStack = variableStack;
            this.bindingType = bindingType;
            this.elementType = elementType;
            this.parentType = parentType;
        }
        
        public void Setup(in AttrInfo attrInfo) {
            currentAttribute = attrInfo;
            depth = attrInfo.depth;
            namespaces = rootVariables.array[depth].templateNode.root.templateShell.referencedNamespaces;
            currentEvent = null;
            changeHandlerCurrentValue = null;
            changeHandlerPreviousValue = null;
        }

        public bool TryGetLocalBindingVariable(string aliasName, out BindingVariableDesc variable) {
            variable = default;
            return false;
        }

        public bool TryGetParentBindingVariable(string aliasName, out BindingVariableDesc variable) {
            for (int i = variableStack.size - 1; i >= 0; i--) {
                StructList<BindingVariableDesc> list = variableStack.array[i];
                for (int j = 0; j < list.size; j++) {
                    if (list.array[j].variableName == aliasName) {
                        variable = list.array[j];
                        return true;
                    }
                }
            }
            variable = default;
            return false;
        }

    }

}