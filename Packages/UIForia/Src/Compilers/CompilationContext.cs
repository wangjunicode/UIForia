using System;
using System.Linq.Expressions;
using UIForia.Elements;
using UIForia.Systems;
using UIForia.Util;

namespace UIForia.Compilers {

    public class CompilationContext {

//        public IReadOnlyList<string> namespaces;
//        public Dictionary<string, ParsedTemplate.AliasedUIStyleGroupContainer> sharedStyleMap;
//        public Dictionary<string, UIStyleGroupContainer> implicitStyleMap;
//        public UIStyleGroupContainer implicitRootStyle;
        public LightList<ParameterExpression> variables;
        public Expression applicationExpr;
        public Type rootType;
        public Type elementType;
        public TemplateContextTreeDefinition contextTree;
        public ParameterExpression rootParam;
        public ParameterExpression templateParam;
        public ParameterExpression scopeParam;
        public Expression sharedBindingsExpr; // probably lives on the template itself since CompilationContext goes away
        public LightStack<Expression> bindingNodeStack;
        public LightStack<CompiledTemplate> expansionStack;
        public LightStack<LightList<Expression>> statementStacks;

        public StructList<VariableGroup> variableGroups;

        private int currentDepth;
        private int maxDepth;

        public CompilationContext() {
            
            this.variables = LightList<ParameterExpression>.Get();
            this.rootParam = Expression.Parameter(typeof(UIElement), "root");
            this.scopeParam = Expression.Parameter(typeof(TemplateScope2), "scope");
            this.templateParam = Expression.Parameter(typeof(CompiledTemplate), "template");
            this.bindingNodeStack = new LightStack<Expression>();
            this.expansionStack = new LightStack<CompiledTemplate>();
            this.statementStacks = new LightStack<LightList<Expression>>();
            this.statementStacks.Push(new LightList<Expression>());
            this.variableGroups = StructList<VariableGroup>.Get();
            
            AddVariableGroup();
        }

        public CompilationContext Clone() {
            CompilationContext retn = new CompilationContext();
            retn.sharedBindingsExpr = sharedBindingsExpr;
            retn.rootType = rootType;
            return retn;
        }

        private void AddVariableGroup() {
            ParameterExpression targetElement;

            if (currentDepth != 0) {
                targetElement = Expression.Parameter(typeof(UIElement), "targetElement_" + currentDepth);
            }
            else {
                targetElement = rootParam;
            }

            ParameterExpression bindingNode = Expression.Parameter(typeof(LinqBindingNode), "bindingNode_" + currentDepth);
            ParameterExpression slotUsage = Expression.Parameter(typeof(LightList<UIElement>), "slotInput" + currentDepth);

            if (currentDepth != 0) {
                variables.Add(targetElement);
            }

            variables.Add(bindingNode);
            variables.Add(slotUsage);

            variableGroups.Add(new VariableGroup() {
                targetElement = targetElement,
                bindingNode = bindingNode,
                slotUsage = slotUsage
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

        public ParameterExpression GetSlotUsageVariable() {
            return variableGroups[currentDepth].slotUsage;
        }
        
        public ParameterExpression GetParentSlotUsageVariable() {
            return variableGroups[currentDepth - 1].slotUsage;
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