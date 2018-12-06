using System;
using System.Reflection;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Compilers {

    public abstract class ExpressionAliasResolver {

        public string aliasName;

        public ExpressionAliasResolver(string aliasName) {
            if (aliasName[0] != '$') {
                aliasName = "$" + aliasName;
            }

            this.aliasName = aliasName;
        }

        public virtual Expression CompileAsValueExpression2(IdentifierNode node, Func<Type, ASTNode, Expression> visit) {
            return null;
        }
        
        public virtual Expression CompileAsValueExpression(ASTNode node, Func<Type, ASTNode, Expression> visit) {
            return null;
        }

        public virtual Expression CompileAsAccessExpression(ASTNode node, Func<Type, ASTNode, Expression> visit) {
            return null;
        }

        public virtual Expression CompileAsMethodExpression(CompilerContext context, InvokeNode node) {
            return null;
        }

    }

    public class ValueResolver<T> : ExpressionAliasResolver {

        public readonly T value;
        
        public ValueResolver(string aliasName, T value) : base(aliasName) {
            this.value = value;
        }

//        public override Expression CompileAsValueExpression(ASTNode node, Func<Type, ASTNode, Expression> visit) {
//            return new ConstantExpression<T>(value);
//        }

    }

    public class MethodResolver : ExpressionAliasResolver {

        public readonly LightList<MethodInfo> infos;

        public MethodResolver(string aliasName, LightList<MethodInfo> infos) : base(aliasName) {
            this.infos = infos;
        }

        public MethodResolver(string aliasName, MethodInfo info) : base(aliasName) {
            this.infos = new LightList<MethodInfo>(1);
            this.infos.Add(info);
        }
      
//        public override Expression CompileAsMethodExpression(CompilerContext context, InvokeNode invokeNode) {
//            
//            // need to figure out type signatures
//            // for each method
//            // if argument count matches
//            // for each argument
//            // compile expression w/ target type equal to parameter type
//            
//            // for each method info
//            
//            // Compile(parameters[i].ParameterType, node.parameters[i]);
//            ParameterInfo[] parameterInfos = infos[0].GetParameters();
//            
//            Expression[] expressionParameters = new Expression[parameterInfos.Length];
//            
//            for (int i = 0; i < invokeNode.parameters.Count; i++) {
//
//                Expression arg = context.Visit(parameterInfos[i].ParameterType, invokeNode.parameters[i]);
//                if (arg != null) {
//                    expressionParameters[i] = arg;
//                }
//                
//            }
//
//            if (infos[0].IsStatic) {
//                
//            }
//
//            if (infos[0].ReturnType == typeof(void)) {
//                    
//            }
//            
//            return MethodExpressionFactory.CreateMethodExpression(infos[0], expressionParameters);
//        }

    }

    
    public class EnumResolver<T> : ExpressionAliasResolver {

        public EnumResolver(string aliasName) : base(aliasName) { }

//        public override Expression CompileAsAccessExpression(ASTNode node, Func<Type, ASTNode, Expression> visit) {
//            return new ConstantExpression<T>(default);
//        }
//
//        public class EnumValueExpression : Expression<T> {
//
//            public override Type YieldedType => typeof(T);
//            
//            public override T Evaluate(ExpressionContext context) {
//                throw new NotImplementedException();
//            }
//
//            public override bool IsConstant() {
//                return true;
//            }
//
//        }

    }
}