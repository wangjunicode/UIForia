using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UIForia.Util;

namespace UIForia.Compilers {

    public class TemplateLinqCompilerContext {

        public int depth;
        public Type elementType;
        public Type parentType;
        public ElementBindingType elementBindingType;
        public IList<string> namespaces;

        public Expression currentEvent;
        public Expression changeHandlerCurrentValue;
        public Expression changeHandlerPreviousValue;

        public AttrInfo currentAttribute;

        public ReadOnlySizedArray<TemplateContextReference> rootVariables;
        public LightStack<StructList<BindingVariableDesc>> variableStack;
        public StructList<BindingVariableDesc> localVariables;

        public void Init(ElementBindingType elementBindingType, Type elementType, Type parentType, ReadOnlySizedArray<TemplateContextReference> contexts, StructList<BindingVariableDesc> localVariables, LightStack<StructList<BindingVariableDesc>> variableStack) {
            this.rootVariables = contexts;
            this.variableStack = variableStack;
            this.elementBindingType = elementBindingType;
            this.elementType = elementType;
            this.parentType = parentType;
            this.localVariables = localVariables;
        }

        public void Setup(in AttrInfo attrInfo) {
            currentAttribute = attrInfo;
            depth = attrInfo.depth;
            namespaces = rootVariables.array[depth].templateNode.root.templateShell.referencedNamespaces;
            currentEvent = null;
            changeHandlerCurrentValue = null;
            changeHandlerPreviousValue = null;
        }

        public bool TryGetBindingVariable(string aliasName, out BindingVariableDesc variable) {
            for (int i = 0; i < localVariables.size; i++) {
                if (localVariables.array[i].variableName == aliasName) {
                    variable = localVariables.array[i];
                    return true;
                }
            }

            for (int i = variableStack.size - 1; i >= 0; i--) {
                StructList<BindingVariableDesc> stack = variableStack.array[i];
                for (int j = stack.size - 1; j >= 0; j--) {
                    ref BindingVariableDesc variableDesc = ref stack.array[i];

                    // todo -- let this also handle aliases
                    if (variableDesc.variableName != aliasName) {
                        continue;
                    }

                    variable = new BindingVariableDesc() {
                        index = localVariables.size,
                        variableName = aliasName,
                        originTemplateType = variableDesc.originTemplateType,
                        variableType = variableDesc.variableType,
                        kind = BindingVariableKind.Reference
                    };

                    localVariables.Add(variable);

                    return true;

                }
            }

            variable = default;
            return false;
        }

        public bool TryAddLocalVariable(string variableName, Type templateOriginType, Type variableType, out BindingVariableDesc variable) {
            // todo -- assert name is unique
            variable = new BindingVariableDesc() {
                index = localVariables.size,
                variableName = variableName,
                originTemplateType = templateOriginType,
                variableType = variableType,
                kind = BindingVariableKind.Local
            };

            for (int i = 0; i < localVariables.size; i++) {
                if (localVariables.array[i].variableName == variableName) {
                    return false;
                }
            }
            
            localVariables.Add(variable);

            return true;

        }

    }

}