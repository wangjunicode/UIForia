using System;
using System.Linq.Expressions;
using UIForia.Exceptions;
using UIForia.Util;

namespace UIForia.Compilers {

    public class BlockDefinition {

        private LightList<ParameterExpression> variables;
        private LightList<Expression> statements;
        public bool requireNullCheck;
        public LabelTarget returnTarget;

        private BlockExpression blockExpression;

        public BlockDefinition parent;

        public BlockDefinition() {
            this.variables = new LightList<ParameterExpression>();
            this.statements = new LightList<Expression>();
        }

        public bool LastStatementWasAssignment {
            get { return statements.Count > 0 && statements[statements.Count - 1].NodeType == ExpressionType.Assign; }
        }

        public LabelTarget ReturnTarget {
            get {
                if (returnTarget == null) {
                    // todo -- handle multiple of these
                    returnTarget = Expression.Label("retn");
                }

                return returnTarget;
            }
        }

        public void AddStatement(Expression statement) {
            statements.Add(statement);
        }

        public void PrependStatement(Expression statement) {
            statements.Insert(0, statement);
        }

        public ParameterExpression AddVariable(Type type, string name = null) {
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name)) {
                name = "_var$";
            }

            variables = variables ?? new LightList<ParameterExpression>();

            for (int i = 0; i < variables.Count; i++) {
                if (variables[i].Name == name) {
                    name += i;
                    break;
                }
            }

            ParameterExpression retn = Expression.Parameter(type, name);
            variables.Add(retn);
            return retn;
        }

        public Expression AddAssignment(Expression target, Expression value) {
            Expression assign = Expression.Assign(target, value);
            statements.Add(assign);
            return assign;
        }

        public ParameterExpression ResolveVariable(string variableName) {
            if (variables == null) return null;
            for (int i = 0; i < variables.Count; i++) {
                if (variables[i].Name == variableName) {
                    return variables[i];
                }
            }

            return null;
        }

        public Expression ToExpressionBlock(Type retnType = null) {
            if (blockExpression != null) {
                return blockExpression;
            }

            retnType = retnType ?? typeof(void);

            if (statements == null || statements.Count == 0) {
                throw CompileException.NoStatementsRootBlock();
            }

            if (requireNullCheck) {
                Expression lastExpression = GetLastAssignment();
                Expression output = AddVariable(lastExpression.Type, "rhsOutput");
                PrependStatement(Expression.Assign(output, Expression.Default(output.Type)));

                statements.Add(Expression.Label(returnTarget));
                statements.Add(output); 
                ReplaceLastAssignment(output);
            }

            if (variables != null && variables.Count > 0) {
                blockExpression = Expression.Block(retnType, variables, statements);
            }
            else {
                blockExpression = Expression.Block(retnType, statements);
            }

            return blockExpression;
        }

        public static implicit operator Expression(BlockDefinition block) {
            return block.ToExpressionBlock();
        }

        public void ReplaceLastAssignment(Expression output) {
            for (int i = statements.Count - 1; i >= 0; i--) {
                if (statements[i].NodeType == ExpressionType.Assign) {
                    BinaryExpression assignment = (BinaryExpression) statements[i];
                    statements[i] = Expression.Assign(output, assignment.Right);
                    variables.Remove(assignment.Left as ParameterExpression);
                    return;
                }
            }
        }

        public Type GetLastAssignmentType() {
            for (int i = statements.Count - 1; i >= 0; i--) {
                if (statements[i].NodeType == ExpressionType.Assign) {
                    BinaryExpression assignment = (BinaryExpression) statements[i];
                    return assignment.Left.Type;
                }
            }

            return null;
        }

        public Expression GetLastAssignment() {
            for (int i = statements.Count - 1; i >= 0; i--) {
                if (statements[i].NodeType == ExpressionType.Assign) {
                    BinaryExpression assignment = (BinaryExpression) statements[i];
                    return assignment.Left;
                }
            }

            return null;
        }

    }

}