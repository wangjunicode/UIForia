using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine;

namespace UIForia.LinqExpressions {

    public class Gen<T> { }

    public class Ex : Gen<Vector3> { }

    public class ElementPool {

        public T MK<T, U>(U data) {
            return default;
        }

        private Dictionary<Type, LightList<object>> pools;
        private Dictionary<Type, Action<object>> resetFunctions;

        public T Get<T>() where T : UIElement {
            
//            pools.TryGetValue(typeof(T), out Type type);
            return null;
        }

        public void Release<T>(T element) where T : UIElement {
            if (!resetFunctions.TryGetValue(typeof(T), out Action<object> reset)) {
                reset = LinqExpressions.CompileClear(typeof(T));
                resetFunctions[typeof(T)] = reset;
            }

            reset(element);
        }

    }

    public static class LinqExpressions {

        private static readonly List<Expression> s_ExpressionList = new List<Expression>(24);

        public static bool IsAutoProperty(PropertyInfo prop, FieldInfo[] fieldInfos) {
            if (!prop.CanWrite || !prop.CanRead) {
                return false;
            }

            string target = "<" + prop.Name + ">";

            for (int i = 0; i < fieldInfos.Length; i++) {
                if (fieldInfos[i].Name.StartsWith(target)) {
                    return true;
                }
            }

            return false;
        }

        public static void ClearEventInvocations(object obj, string eventName) {
            FieldInfo fi = GetEventField(obj.GetType(), eventName);
            if (fi == null) return;
            fi.SetValue(obj, null);
        }

        private static FieldInfo GetEventField(Type type, string eventName) {
            FieldInfo field = null;
            while (type != null) {
                /* Find events defined as field */
                field = type.GetField(eventName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null && (field.FieldType == typeof(MulticastDelegate) || field.FieldType.IsSubclassOf(typeof(MulticastDelegate))))
                    break;

                /* Find events defined as property { add; remove; } */
                field = type.GetField("EVENT_" + eventName.ToUpper(), BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                    break;
                type = type.BaseType;
            }

            return field;
        }

        private static readonly MethodInfo s_FieldSetValue = typeof(FieldInfo).GetMethod(nameof(FieldInfo.SetValue), new[] {typeof(object), typeof(object)});

        // todo -- we need to know if this being compiled for dev or not.
        // In production mode we generate valid C# code and thus need to know how to assign
        // to auto properties (direct assign if public, reflection if not) & clear events (reflection)
        public static Action<object> CompileClear(Type type) {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "element");

            UnaryExpression converted = Expression.Convert(parameterExpression, type);

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int i = 0; i < fields.Length; i++) {
                Type fieldType = fields[i].FieldType;
                if (fields[i].IsInitOnly) {
                    object defaultValue = fieldType.IsValueType ? Activator.CreateInstance(fieldType) : null;
                    // use reflection to set the value since the compiler will complain about setting a read only field
                    s_ExpressionList.Add(Expression.Call(
                        Expression.Constant(fields[i]),
                        s_FieldSetValue,
                        parameterExpression,
                        Expression.Convert(Expression.Constant(defaultValue), typeof(object)))
                    );
                }
                else {
                    s_ExpressionList.Add(Expression.Assign(Expression.Field(converted, fields[i]), Expression.Default(fieldType)));
                }
            }

            Expression blockExpr = Expression.Block(s_ExpressionList);
            s_ExpressionList.Clear();

            return Expression.Lambda<Action<object>>(blockExpr, parameterExpression).Compile();
        }

    }

}