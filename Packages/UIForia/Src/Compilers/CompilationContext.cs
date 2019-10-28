using System;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Systems;
using UIForia.Util;

namespace UIForia.Compilers {

    public struct StoredTemplate {

        public string templateName;
        public int templateId;
        public UIElement closureRoot;

        public StoredTemplate(string templateName, int templateId, UIElement closureRoot) {
            this.templateName = templateName;
            this.templateId = templateId;
            this.closureRoot = closureRoot;
        }

    }

    public struct CompilationData {

        public Type elementType;

    }

    public class CompilationContext {

        public bool outputComments;
        public ProcessedType rootType;
        public ProcessedType elementType;
        public CompiledTemplate compiledTemplate;

        public LightList<ParameterExpression> variables;
        public LightStack<LightList<Expression>> statementStacks;
        
        public Expression rootParam;
        public Expression templateScope;
        public Expression applicationExpr;
        private Expression slotUsage;
        private ParameterExpression slotUsageListVar;

        private int currentDepth;
        private int maxDepth;
        private int bindingNodeCount;

        private readonly LightStack<ParameterExpression> hierarchyStack;

        public CompilationContext() {
            this.outputComments = true;
            this.variables = new LightList<ParameterExpression>();
            this.statementStacks = new LightStack<LightList<Expression>>();
            this.hierarchyStack = new LightStack<ParameterExpression>();
        }

        public ParameterExpression ParentExpr => hierarchyStack.PeekAtUnchecked(hierarchyStack.Count - 2);
        public ParameterExpression ElementExpr => hierarchyStack.PeekUnchecked();

        public void Initialize(ParameterExpression parent) {
            hierarchyStack.Push(parent);
            PushBlock();
        }

        private static readonly MethodInfo s_StructList_SlotUsage_GetMinSize = typeof(StructList<SlotUsage>).GetMethod(nameof(StructList<SlotUsage>.GetMinSize), new[] {typeof(int)});
        private static readonly MethodInfo s_StructList_SlotUsage_Release = typeof(StructList<SlotUsage>).GetMethod(nameof(StructList<SlotUsage>.Release), BindingFlags.Static);
        private static readonly MethodInfo s_StructList_SlotUsage_ReleaseSelf = typeof(StructList<SlotUsage>).GetMethod(nameof(StructList<SlotUsage>.Release), BindingFlags.Instance | BindingFlags.Public);
        private static readonly FieldInfo s_StructList_SlotUsage_Size = typeof(StructList<SlotUsage>).GetField(nameof(StructList<SlotUsage>.size));

        private const string k_SlotUsageListName = "slotUsageList";

        public Expression GetSlotUsageArray(int slotCount) {
            if (slotUsageListVar == null) {
                slotUsageListVar = Expression.Parameter(typeof(StructList<SlotUsage>), k_SlotUsageListName);
                variables.Add(slotUsageListVar);
            }

            Expression getSlotList = Expression.Call(null, s_StructList_SlotUsage_GetMinSize, Expression.Constant(slotCount));

            AddStatement(
                Expression.Assign(slotUsageListVar, getSlotList)
            );

            AddStatement(
                Expression.Assign(
                    Expression.Field(slotUsageListVar, s_StructList_SlotUsage_Size), Expression.Constant(slotCount)
                )
            );

            return slotUsageListVar;
        }

        public void ReleaseSlotUsage() {
            AddStatement(
                Expression.Call(slotUsageListVar, s_StructList_SlotUsage_ReleaseSelf)
            );
        }

        public void PushScope() {
            currentDepth++;

            if (currentDepth > maxDepth) {
                maxDepth = currentDepth;
                ParameterExpression variable = Expression.Parameter(typeof(UIElement), "targetElement_" + currentDepth);
                variables.Add(variable);
                hierarchyStack.Push(variable);
            }
            else {
                string targetName = "targetElement_" + currentDepth;
                for (int i = 0; i < variables.size; i++) {
                    if (variables[i].Type == typeof(UIElement) && variables[i].Name == targetName) {
                        hierarchyStack.Push(variables[i]);
                        return;
                    }
                }

                throw new ArgumentOutOfRangeException();
            }
        }

        public ParameterExpression GetVariable(Type type, string name) {
            for (int i = 0; i < variables.size; i++) {
                if (variables[i].Name == name) {
                    if (variables[i].Type != type) {
                        throw new CompileException("Variable already taken: " + name);
                    }

                    return variables[i];
                }
            }

            ParameterExpression param = Expression.Parameter(type, name);
            variables.Add(param);
            return param;
        }
        
        public void PopScope() {
            currentDepth--;
            hierarchyStack.Pop();
        }

        public void PushBlock() {
            statementStacks.Push(LightList<Expression>.Get());
        }

        public BlockExpression PopBlock() {
            LightList<Expression> statements = statementStacks.Pop();
            Expression[] array = statements.ToArray();
            LightList<Expression>.Release(ref statements);
            return Expression.Block(typeof(void), array);
        }

        public BlockExpression Finalize(Type type) {
            LightList<Expression> statements = statementStacks.Pop();
            Expression[] array = statements.ToArray();
            LightList<Expression>.Release(ref statements);
            return Expression.Block(type, variables, array);
        }

        public void AddStatement(Expression expression) {
            this.statementStacks.PeekUnchecked().Add(expression);
        }

        public void Assign(Expression target, Expression value) {
            AddStatement(Expression.Assign(target, value));
        }

        public void Return(Expression arg) {
            AddStatement(arg);
        }

        public void IfEqualsNull(Expression target, BlockExpression block) {
            AddStatement(Expression.IfThen(Expression.Equal(target, Expression.Constant(null)), block));
        }

        private static MethodInfo s_Comment = typeof(ExpressionUtil).GetMethod(nameof(ExpressionUtil.Comment), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        private static MethodInfo s_CommentNewLineBefore = typeof(ExpressionUtil).GetMethod(nameof(ExpressionUtil.CommentNewLineBefore), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        private static MethodInfo s_CommentNewLineAfter = typeof(ExpressionUtil).GetMethod(nameof(ExpressionUtil.CommentNewLineAfter), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        public void Comment(string comment) {
            if (outputComments) {
                AddStatement(Expression.Call(s_Comment, Expression.Constant(comment)));
            }
        }

        public void CommentNewLineBefore(string comment) {
            if (outputComments) {
                AddStatement(Expression.Call(s_CommentNewLineBefore, Expression.Constant(comment)));
            }
        }
        
        public void CommentNewLineAfter(string comment) {
            if (outputComments) {
                AddStatement(Expression.Call(s_CommentNewLineAfter, Expression.Constant(comment)));
            }
        }

        public void PushContextVariable(string aliasName) {
            
        }

    }

}