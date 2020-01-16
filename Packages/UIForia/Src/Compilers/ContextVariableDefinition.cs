using System;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Systems;
using UIForia.UIInput;
using UIForia.Util;

namespace UIForia.Compilers {

    public class ContextVariableDefinition {

        public int id;
        public Type type;
        public string name;
        public bool isExposed;
        public LightList<string> nameList;
        public AliasResolverType variableType;

        public void PushAlias(string alias) {
            if (nameList == null) {
                nameList = new LightList<string>(4);
            }
            nameList.Add(alias);
        }
        
        public string GetName() {
            if (nameList != null && nameList.size > 0) {
                return nameList.Last;
            }

            return name;
        }

        public Expression Resolve(LinqCompiler compiler) {
            switch (variableType) {
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
                    ParameterExpression variable = compiler.AddVariable(type, "ctxvar_" + GetName());

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

                    ParameterExpression variable = compiler.AddVariable(type, "repeat_item_" + GetName());

                    compiler.Assign(variable, Expression.MakeMemberAccess(call, contextVarType.GetField(nameof(ContextVariable<int>.value))));
                    return variable;
                }
                case AliasResolverType.RepeatIndex: {
                    ParameterExpression el = compiler.GetVariable(TemplateCompiler.k_CastElement);
                    Expression access = Expression.MakeMemberAccess(el, TemplateCompiler.s_Element_BindingNode);
                    Expression call = ExpressionFactory.CallInstanceUnchecked(access, TemplateCompiler.s_LinqBindingNode_GetContextVariable, Expression.Constant(id));

                    UnaryExpression convert = Expression.Convert(call, typeof(ContextVariable<int>));
                    ParameterExpression variable = compiler.AddVariable(type, "ctxvar_" + GetName());

                    compiler.Assign(variable, Expression.MakeMemberAccess(convert, typeof(ContextVariable<int>).GetField(nameof(ContextVariable<int>.value))));
                    return variable;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
    }

}