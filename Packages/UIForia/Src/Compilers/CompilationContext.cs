using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Systems;
using UIForia.Templates;
using UIForia.Util;

namespace UIForia.Compilers {

    public class CompilationContext {

        public IReadOnlyList<string> namespaces;
        public Dictionary<string, ParsedTemplate.AliasedUIStyleGroupContainer> sharedStyleMap;
        public Dictionary<string, UIStyleGroupContainer> implicitStyleMap;
        public UIStyleGroupContainer implicitRootStyle;
        public LightList<ParameterExpression> variables;
        public Expression applicationExpr;
        public Type rootType;
        public Type elementType;
        public TemplateContextTreeDefinition contextTree;
        public ParameterExpression rootParam;
        public ParameterExpression templateParam;
        public ParameterExpression scopeParam;
        public Expression sharedBindingsExpr;
        public LightStack<Expression> bindingNodeStack;
        public LightStack<CompiledTemplate> expansionStack;

        public StructList<VariableGroup> variableGroups = new StructList<VariableGroup>();

        private int currentDepth;
        private int maxDepth;

        public LightStack<LightList<Expression>> statementStacks;
        
        public CompilationContext() {
            
       //     this.statements = LightList<Expression>.Get();
            this.variables = LightList<ParameterExpression>.Get();
            this.rootParam = Expression.Parameter(typeof(UIElement), "root");
            this.scopeParam = Expression.Parameter(typeof(TemplateScope2), "scope");
            this.templateParam = Expression.Parameter(typeof(CompiledTemplate), "template");
            this.bindingNodeStack = new LightStack<Expression>();
            this.expansionStack = new LightStack<CompiledTemplate>();
            this.statementStacks = new LightStack<LightList<Expression>>();
            this.statementStacks.Push(new LightList<Expression>());
            
            AddVariableGroup();
        }

        private void AddVariableGroup() {
            ParameterExpression targetElement;
            ParameterExpression childArray;

            if (currentDepth != 0) {
                targetElement = Expression.Parameter(typeof(UIElement), "targetElement_" + currentDepth);
                childArray = Expression.Parameter(typeof(UIElement[]), "childArray_" + currentDepth);
            }
            else {
                targetElement = rootParam;
                childArray = null;
            }

            ParameterExpression bindingNode = Expression.Parameter(typeof(LinqBindingNode), "bindingNode_" + currentDepth);

            if (currentDepth != 0) {
                variables.Add(targetElement);
                variables.Add(childArray);
            }

            variables.Add(bindingNode);

            variableGroups.Add(new VariableGroup() {
                targetElement = targetElement,
                childArray = childArray,
                bindingNode = bindingNode,
            });
        }

        public void PushScope() {
            currentDepth++;

            if (currentDepth > maxDepth) {
                maxDepth = currentDepth;
                AddVariableGroup();
            }
        }

        public ParameterExpression GetParentTargetElementVariable() {
            return variableGroups[currentDepth - 1].targetElement;
        }

        public ParameterExpression GetTargetElementVariable() {
            return variableGroups[currentDepth].targetElement;
        }

        public ParameterExpression GetBindingNodeVariable() {
            return variableGroups[currentDepth].bindingNode;
        }

        public ParameterExpression GetChildArrayVariable() {
            return variableGroups[currentDepth].childArray;
        }

        public void PopScope() {
            currentDepth--;
        }

        public ParameterExpression GetVariable(Type type, string name) {
            for (int i = 0; i < variables.Count; i++) {
                if (variables[i].Type == type && variables[i].Name == name) {
                    return variables[i];
                }
            }

            ParameterExpression variable = Expression.Parameter(type, name);
            variables.Add(variable);
            return variable;
        }

        public void PushBindingNode(Expression bindingNode) {
            bindingNodeStack.Push(bindingNode);
        }

        public void PushBlock() {
            this.statementStacks.Push(LightList<Expression>.Get());    
        }

        public BlockExpression PopBlock() {
            return Expression.Block(typeof(void), this.statementStacks.Pop());
        }
        
        public void AddStatement(Expression expression) {
            this.statementStacks.PeekUnchecked().Add(expression);
        }

    }

}