using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Mono.Linq.Expressions;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using Debug = UnityEngine.Debug;

namespace UIForia.Parsing {

    [DebuggerDisplay("{rawType.Name}")]
    public class ProcessedType {

        public const int k_PoolThreshold = 5;
        public readonly Type rawType;
        public readonly TemplateAttribute templateAttr;
        public readonly bool requiresTemplateExpansion;
        public readonly bool requiresUpdateFn;
        private object rawCtorFn;
        private Action<object> clearFn;
        private StructList<PropertyChangeHandlerDesc> methods;
        internal int totalReleased;
        internal LightList<UIElement> poolList;
        private Func<ProcessedType, UIElement, UIElement> constructionFn;
        public readonly string tagName;
        private static readonly LinqCompiler s_Compiler;
        private static readonly FieldInfo s_rawCtorFnRef;
        public int id;
        public int references;
        public bool requiresBeforePropertyUpdates;
        public bool requiresAfterPropertyUpdates;

        static ProcessedType() {
            s_Compiler = new LinqCompiler();
            s_rawCtorFnRef = typeof(ProcessedType).GetField(nameof(rawCtorFn), BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public ProcessedType(Type rawType, TemplateAttribute templateAttr, string tagName = null) {
            this.id = -1; // set by TypeProcessor
            this.rawType = rawType;
            this.templateAttr = templateAttr;
            this.tagName = tagName;
            this.requiresUpdateFn = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnUpdate)));
            this.requiresBeforePropertyUpdates = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnBeforePropertyBindings)));
            this.requiresAfterPropertyUpdates = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnAfterPropertyBindings)));

            // CompileClear(rawType);
            this.requiresTemplateExpansion = (
                !typeof(UIContainerElement).IsAssignableFrom(rawType) &&
                !typeof(UITextElement).IsAssignableFrom(rawType) &&
                !typeof(UISlotDefinition).IsAssignableFrom(rawType) &&
                !typeof(UISlotOverride).IsAssignableFrom(rawType)
            );
        }

        public struct PropertyChangeHandlerDesc {

            public MethodInfo methodInfo;
            public string memberName;

        }

        public bool IsUnresolvedGeneric { get; set; }

        public void GetChangeHandlers(string memberName, StructList<PropertyChangeHandlerDesc> retn) {
            if (methods == null) {
                MethodInfo[] candidates = ReflectionUtil.GetInstanceMethods(rawType);
                for (int i = 0; i < candidates.Length; i++) {
                    IEnumerable<OnPropertyChanged> attrs = candidates[i].GetCustomAttributes<OnPropertyChanged>();
                    methods = methods ?? new StructList<PropertyChangeHandlerDesc>();
                    foreach (OnPropertyChanged a in attrs) {
                        methods.Add(new PropertyChangeHandlerDesc() {
                            methodInfo = candidates[i],
                            memberName = a.propertyName
                        });
                    }
                }

            }

            if (methods == null) {
                return;
            }

            for (int i = 0; i < methods.size; i++) {
                if (methods.array[i].memberName == memberName) {
                    retn.Add(methods[i]);
                }
            }
        }

        public void CreateCtor() {
            ReflectionUtil.TypeArray1[0] = rawType;
            Type genericType = ReflectionUtil.CreateGenericType(typeof(Action<>), ReflectionUtil.TypeArray1);
            try {
                ConstructorInfo constructor = rawType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (constructor == null) {
                    throw new Exception($"{rawType} must define a default constructor for UIForia to function properly");
                }

                DynamicMethod helperMethod = new DynamicMethod(string.Empty, typeof(void), ReflectionUtil.TypeArray1, rawType.Module, true);
                ILGenerator ilGenerator = helperMethod.GetILGenerator();
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Call, constructor);
                ilGenerator.Emit(OpCodes.Ret);
                rawCtorFn = helperMethod.CreateDelegate(genericType);
            }
            catch (Exception e) {
                Debug.Log(e.Message);
            }

            s_Compiler.SetSignature(
                new Parameter<ProcessedType>("processedType", ParameterFlags.NeverNull),
                new Parameter<UIElement>("instance", ParameterFlags.NeverNull),
                typeof(UIElement)
            );

            ParameterExpression typeParam = s_Compiler.GetParameter("processedType");
            ParameterExpression retnParam = s_Compiler.GetParameter("instance");
            ParameterExpression castVal = s_Compiler.AddVariable(genericType, "castVal");
            MemberExpression typeConstructorFn = Expression.MakeMemberAccess(typeParam, s_rawCtorFnRef);

            UnaryExpression converted = Expression.Convert(typeConstructorFn, genericType);
            s_Compiler.Assign(castVal, converted);
            s_Compiler.RawExpression(Expression.Invoke(
                    castVal,
                    Expression.Convert(retnParam, rawType)
                )
            );
            s_Compiler.RawExpression(retnParam);
            LambdaExpression lambda = s_Compiler.BuildLambda();
//            Debug.Log(lambda.ToCSharpCode());
            constructionFn = (Func<ProcessedType, UIElement, UIElement>) lambda.Compile();
            s_Compiler.Reset();
        }

        public UIElement CreateInstance() {
            UIElement instance = null;

            if (constructionFn == null) {
                CreateCtor(); // todo move this, don't create if we don't use it in dynamic case but needs to be pre-compiled and available for AOT case
            }

            if (poolList != null && poolList.size > 0) {
                return constructionFn(this, poolList.RemoveLast());
            }

            instance = (UIElement) FormatterServices.GetUninitializedObject(rawType);
            return constructionFn(this, instance);
        }

        public void ReleaseElement(UIElement element) {
            totalReleased++;
            if (totalReleased >= k_PoolThreshold) {
                if (poolList == null) {
                    poolList = new LightList<UIElement>();
                }

                clearFn(element);
                poolList.Add(element);
            }
        }

        private void CompileClear(Type type) {
            s_Compiler.SetSignature(new Parameter<object>("element", ParameterFlags.NeverNull));

            ParameterExpression parameterExpression = s_Compiler.GetParameter("element");

            UnaryExpression converted = Expression.Convert(parameterExpression, type);

            ParameterExpression cast = s_Compiler.AddVariable(type, "cast");
            s_Compiler.Assign(cast, converted);
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int i = 0; i < fields.Length; i++) {
                Type fieldType = fields[i].FieldType;
                if (fields[i].IsInitOnly) {
                    throw new Exception("UIForia elements cannot have readonly fields when building for AOT platforms or production");
                }

                s_Compiler.Assign(Expression.Field(cast, fields[i]), Expression.Default(fieldType));
            }

            Debug.Log(s_Compiler.BuildLambda().ToCSharpCode());
            clearFn = (Action<object>) s_Compiler.BuildLambda().Compile();
            s_Compiler.Reset();
        }

        public void ValidateAttributes(StructList<AttributeDefinition> attributes) {
            
        }

    }

}