using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Compilers {

    public class AttributeCompilerContext {

        public int depth;
        public ProcessedType elementType;
        public ProcessedType parentType;
        public TemplateNodeType templateNodeType;
        public IList<string> namespaces;

        public Expression currentEvent;
        public Expression changeHandlerCurrentValue;
        public Expression changeHandlerPreviousValue;
        
        public StructList<TemplateContextReference> rootVariables;
        public LightStack<StructList<BindingVariableDesc>> variableStack;
        
        public TemplateNode templateNode;
        
        public BindingResult bindingResult;
        public AttrInfo currentAttribute;

        public AttributeCompilerContext() {
            bindingResult = new BindingResult();
            rootVariables = new StructList<TemplateContextReference>();
        }
        
        public void Init(TemplateNode templateNode, TemplateNodeType templateNodeType, ProcessedType elementType, ProcessedType parentType, LightStack<StructList<BindingVariableDesc>> variableStack) {
            this.templateNode = templateNode;
            this.variableStack = variableStack;
            this.templateNodeType = templateNodeType;
            this.elementType = elementType;
            this.parentType = parentType;
            this.bindingResult.Clear();
            this.rootVariables.Clear();
        }

        public void Setup(in AttrInfo attrInfo) {
            currentAttribute = attrInfo;
            depth = attrInfo.depth;
            namespaces = rootVariables.array[depth].templateNode.root.templateShell.referencedNamespaces;
            currentEvent = null;
            changeHandlerCurrentValue = null;
            changeHandlerPreviousValue = null;
            if ((attrInfo.isInjected)) {
                
            }
        }
        
        public bool TryAddLocalVariable(string variableName, Type templateOriginType, Type variableType, out BindingVariableDesc variable) {
            variable = new BindingVariableDesc() {
                index = bindingResult.localVariables.size,
                variableName = variableName,
                originTemplateType = templateOriginType,
                variableType = variableType,
                kind = BindingVariableKind.Local
            };

            for (int i = 0; i < bindingResult.localVariables.size; i++) {
                if (bindingResult.localVariables.array[i].variableName == variableName) {
                    return false;
                }
            }
            
            bindingResult.localVariables.Add(variable);

            return true;

        }

        public bool TryGetBindingVariable(string aliasName, out BindingVariableDesc variable) {
            for (int i = 0; i < bindingResult.localVariables.size; i++) {
                if (bindingResult.localVariables.array[i].variableName == aliasName) {
                    variable = bindingResult.localVariables.array[i];
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
                        index = bindingResult.localVariables.size,
                        variableName = aliasName,
                        originTemplateType = variableDesc.originTemplateType,
                        variableType = variableDesc.variableType,
                        kind = BindingVariableKind.Reference
                    };

                    bindingResult.localVariables.Add(variable);

                    return true;

                }
            }

            variable = default;
            return false;
        }

        public void AddContextReference(ProcessedType processedType, TemplateNode templateNode) {
            rootVariables.Add(new TemplateContextReference(processedType, templateNode));
        }

        public void AddInputBinding(InputEventClass eventClass, InputHandlerDescriptor descriptor, LambdaExpression lambda) {
            bindingResult.inputHandlers.Add(new InputHandlerResult() {
                eventClass = eventClass,
                descriptor = descriptor,
                lambdaExpression = lambda
            });
        }

    }

}