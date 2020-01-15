using System;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Systems;
using UIForia.UIInput;
using UIForia.Util;

namespace UIForia.Compilers {

    public enum AliasResolverType {

        MouseEvent,
        KeyEvent,
        TouchEvent,
        Element,
        Parent,
        ContextVariable,
        ControllerEvent,
        Root,
        RepeatItem,
        RepeatIndex

    }

    internal struct ContextVarAliasResolver {

        public string name;
        public string strippedName;
        public Type type;
        public int id;
        public AliasResolverType resolverType;

        public ContextVarAliasResolver(string name, Type type, int id, AliasResolverType resolverType) {
            this.name = name[0] == '$' ? name : '$' + name;
            this.strippedName = name.Substring(0);
            this.type = type;
            this.id = id;
            this.resolverType = resolverType;
        }

        public Expression Resolve(LinqCompiler compiler) {
            switch (resolverType) {
                case AliasResolverType.MouseEvent:
                    return compiler.Value(TemplateCompiler.k_InputEventParameterName + "." + nameof(GenericInputEvent.AsMouseInputEvent));

                case AliasResolverType.KeyEvent:
                    return compiler.Value(TemplateCompiler.k_InputEventParameterName + "." + nameof(GenericInputEvent.AsKeyInputEvent));

                case AliasResolverType.TouchEvent:
                case AliasResolverType.ControllerEvent:
                    throw new NotImplementedException();

                case AliasResolverType.Element:
                    return compiler.GetVariable(TemplateCompiler.k_CastElement);

                case AliasResolverType.Root:
                    return compiler.GetVariable(TemplateCompiler.k_CastRoot);

                case AliasResolverType.Parent: // todo -- use expressions
                    return compiler.Value(TemplateCompiler.k_CastElement + ".parent");

                case AliasResolverType.ContextVariable: {
                    ParameterExpression el = compiler.GetVariable(TemplateCompiler.k_CastElement);
                    Expression access = Expression.MakeMemberAccess(el, TemplateCompiler.s_Element_BindingNode);
                    Expression call = ExpressionFactory.CallInstanceUnchecked(access, TemplateCompiler.s_LinqBindingNode_GetContextVariable, Expression.Constant(id));
                    Type contextVarType = ReflectionUtil.CreateGenericType(typeof(ContextVariable<>), type);

                    UnaryExpression convert = Expression.Convert(call, contextVarType);
                    ParameterExpression variable = compiler.AddVariable(type, "ctxvar_" + strippedName);

                    compiler.Assign(variable, Expression.MakeMemberAccess(convert, contextVarType.GetField("value")));
                    return variable;
                }
                case AliasResolverType.RepeatItem: {
                    compiler.Comment(name);
                    //var repeat_item_name = element.bindingNode.GetRepeatItem<T>(id).value;
                    ParameterExpression el = compiler.GetVariable(TemplateCompiler.k_CastElement);
                    Expression access = Expression.MakeMemberAccess(el, TemplateCompiler.s_Element_BindingNode);

                    ReflectionUtil.TypeArray1[0] = type;
                    MethodInfo getItem = TemplateCompiler.s_LinqBindingNode_GetRepeatItem.MakeGenericMethod(ReflectionUtil.TypeArray1);
                    Expression call = ExpressionFactory.CallInstanceUnchecked(access, getItem, Expression.Constant(id));
                    Type contextVarType = ReflectionUtil.CreateGenericType(typeof(ContextVariable<>), type);

                    UnaryExpression convert = Expression.Convert(call, contextVarType);
                    ParameterExpression variable = compiler.AddVariable(type, "repeat_item_" + strippedName);

                    compiler.Assign(variable, Expression.MakeMemberAccess(convert, contextVarType.GetField(nameof(ContextVariable<int>.value))));
                    return variable;
                }
                case AliasResolverType.RepeatIndex: {
                    ParameterExpression el = compiler.GetVariable(TemplateCompiler.k_CastElement);
                    Expression access = Expression.MakeMemberAccess(el, TemplateCompiler.s_Element_BindingNode);
                    Expression call = ExpressionFactory.CallInstanceUnchecked(access, TemplateCompiler.s_LinqBindingNode_GetContextVariable, Expression.Constant(id));

                    UnaryExpression convert = Expression.Convert(call, typeof(ContextVariable<int>));
                    ParameterExpression variable = compiler.AddVariable(type, "ctxvar_" + strippedName);

                    compiler.Assign(variable, Expression.MakeMemberAccess(convert, typeof(ContextVariable<int>).GetField(nameof(ContextVariable<int>.value))));
                    return variable;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}