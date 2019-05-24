using UIForia.Expressions;
using UIForia.Util;

namespace UIForia.Compilers.ExpressionResolvers {

    public class SelectOptionAliasResolver<T> : ExpressionAliasResolver {

        private bool isInternal;
        
        public SelectOptionAliasResolver(string itemAlias, bool isInternal) : base(itemAlias) { }

        public override Expression CompileAsValueExpression(CompilerContext context) {
            ReflectionUtil.ObjectArray2[0] = aliasName;
            ReflectionUtil.ObjectArray2[1] = isInternal;
            return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(SelectOptionExpression<>),
                new GenericArguments(typeof(T)),
                ReflectionUtil.ObjectArray2
            );
        }

    }

}