using System;
using System.Linq.Expressions;
using UIForia.Exceptions;
using UIForia.Util;

namespace UIForia.Compilers {

    public class BlockDefinition2 {

        public BlockDefinition2 parent;
        private LightList<ParameterExpression> variables;
        private LightList<Expression> statements;
        public LinqCompiler compiler;
        public int blockId;
        
        public BlockDefinition2() {
            this.variables = LightList<ParameterExpression>.Get();
            this.statements = LightList<Expression>.Get();
        }

        public ParameterExpression AddInternalVariable(Type type, string name = null) {
            
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name)) {
                name = "_var$";
            }

            variables = variables ?? new LightList<ParameterExpression>();
            
            ParameterExpression retn = Expression.Parameter(type, compiler.GetUniqueVariableName(name));
            
            variables.Add(retn);
            return retn;
        }

        public LightList<Expression> GetStatements() {
            return statements;
        }
        
        public LightList<ParameterExpression> GetVariables() {
            return variables;
        }

        public Expression AddStatement(Expression statement) {
            statements.Add(statement);
            return statement;
        }

        public ParameterExpression ResolveVariable(string variableName) {
            if (variables == null) return null;
            
            for (int i = 0; i < variables.Count; i++) {
                if (variables[i].Name == variableName) {
                    return variables[i];
                }
            }

            // does this hide closures?
            return parent?.ResolveVariable(variableName);
        }

        public void Spawn() {
            this.variables = LightList<ParameterExpression>.Get();
            this.statements = LightList<Expression>.Get();
        }

        public void Release() {
            parent = null;
            LightList<ParameterExpression>.Release(ref variables);
            LightList<Expression>.Release(ref statements);
        }

        public BlockExpression ToBlockExpression() {
            if (variables != null && variables.Count > 0) {
                return Expression.Block(typeof(void), variables, statements);
            }
            else {
                return Expression.Block(typeof(void), statements);
            }
        }

    }

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

        // compiler.Variable("root", type);
        // compiler.Variable("root", "expression");
        // compiler.Assign("root.value.x", "value");
        // compiler.Variable("x", "''");
        // compiler.Return("(val, val2) => val1);
        // compiler.IfThen("expression", () => { compiler.Assign("x", "y") })
        // compiler.Call();

        public Expression ToExpressionBlock(Type retnType = null) {
            if (blockExpression != null) {
                return blockExpression;
            }

            retnType = retnType ?? typeof(void);

            if (statements == null || statements.Count == 0) {
                throw CompileException.NoStatementsRootBlock();
            }

            // write last expression
            // if has null check 
            // lastexpression = variable
            // variable
            // last expression = assign(var, lastExpression); 

            // if null check is required, last expression needs to be the return value
            // if not last expression needs to be typed parameter value (ie a variable)
            // this can be 'inlined' by replacing the assignment with just the lefthand expression

            // lastExpression = statement;
            if (requireNullCheck) {
                statements.RemoveLast();

                Expression lastExpression = statements[statements.Count - 1];
                Expression output = AddVariable(lastExpression.Type, "rhsOutput");
                PrependStatement(Expression.Assign(output, Expression.Default(output.Type)));

                ReplaceLastAssignment(output);
                statements.Add(Expression.Label(returnTarget));
                statements.Add(output);
                statements[statements.Count - 1] = output;
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