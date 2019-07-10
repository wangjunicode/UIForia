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

}