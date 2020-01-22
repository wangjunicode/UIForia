using System;
using System.Linq.Expressions;
using UIForia.Util;

namespace UIForia.Compilers {

    public class UIForiaLinqCompiler : LinqCompiler {

        private static readonly LightList<UIForiaLinqCompiler> s_CompilerPool = new LightList<UIForiaLinqCompiler>();
        
        private ParameterExpression elementParameter;
        private ParameterExpression rootParameter;
        private ParameterExpression castElementParameter;
        private ParameterExpression castRootParameter;

        private Type elementType;
        private Type rootElementType;

        private const string k_CastElement = "__castElement";
        private const string k_CastRoot = "__castRoot";

        public void Setup(Type rootElementType, Type elementType) {
            this.rootElementType = rootElementType;
            this.elementType = elementType;
            elementParameter = null;
            rootParameter = null;
            castElementParameter = null;
            castRootParameter = null;
        }

        public ParameterExpression GetElement() {
            if (elementParameter == null) {
                elementParameter = GetParameter("__element");
            }

            return elementParameter;
        }

        public ParameterExpression GetRoot() {
            if (rootParameter == null) {
                rootParameter = GetParameter("__root");
            }

            return rootParameter;
        }

        public ParameterExpression GetCastElement() {
            if (castElementParameter == null) {
                Parameter p = new Parameter(elementType, k_CastElement, ParameterFlags.NeverNull);
                castElementParameter = AddVariableUnchecked(p, ExpressionFactory.Convert(GetElement(), elementType));
            }

            return castElementParameter;
        }

        public ParameterExpression GetCastRoot() {
            if (castRootParameter == null) {
                Parameter p = new Parameter(rootElementType, k_CastRoot, ParameterFlags.NeverNull);
                castRootParameter = AddVariableUnchecked(p, ExpressionFactory.Convert(GetRoot(), rootElementType));
            }

            return castRootParameter;
        }

        protected override LinqCompiler CreateNested() {
            if (s_CompilerPool.size == 0) {
                return new UIForiaLinqCompiler();
            }

            return s_CompilerPool.RemoveLast();
        }

        public override void Release() {
            Reset();
            s_CompilerPool.Add(this);
        }

    }

}