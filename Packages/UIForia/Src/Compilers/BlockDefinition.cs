using System;
using System.Linq.Expressions;
using UIForia.Exceptions;
using UIForia.Util;

namespace UIForia.Compilers {

    public class BlockDefinition {

        public LightList<ParameterExpression> variables;
        public LightList<Expression> statements;

        public BlockDefinition() {
            this.variables = new LightList<ParameterExpression>();
            this.statements = new LightList<Expression>();
        }

        public bool LastStatementWasAssignment {
            get { return statements.Count > 0 && statements[statements.Count - 1].NodeType == ExpressionType.Assign; }
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
            retnType = retnType ?? typeof(void);
            if (statements == null || statements.Count == 0) {
                throw CompileException.NoStatementsRootBlock();
            }

            if (variables != null && variables.Count > 0) {
                return Expression.Block(retnType, variables, statements);
            }

            return Expression.Block(retnType, statements);
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

    }

}