using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Mono.Linq.Expressions;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Util;
using Debug = UnityEngine.Debug;

namespace UIForia.Parsing {

    public struct TemplateDefinition {

        public string contents;
        public TemplateLanguage language;
        public string filePath;

    }

    [DebuggerDisplay("{rawType.Name}")]
    public class ProcessedType {

        public const int k_PoolThreshold = 5;
        public readonly Type rawType;
        public readonly TemplateAttribute templateAttr;
        public readonly bool requiresTemplateExpansion;
        public readonly bool requiresUpdateFn;
        public readonly bool isPoolable;
        private object rawCtorFn;
        internal int totalReleased;
        internal LightList<UIElement> poolList;
        private static readonly MethodInfo s_GetPooledInstanceMethodRef;
        private static readonly LinqCompiler s_Compiler;
        private static readonly FieldInfo s_constructorFnAccess;
        private static readonly MethodInfo s_CreateCtorMethodRef;
        private Func<ProcessedType, UIElement, UIElement> constructionFn;

        public ProcessedType(Type rawType, TemplateAttribute templateAttr) {
            this.rawType = rawType;
            this.templateAttr = templateAttr;
            this.requiresUpdateFn = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnUpdate)));
            this.isPoolable = rawType.GetCustomAttribute<PoolableElementAttribute>() != null;
            CreateCtor();
            CompileClear(rawType);
            this.requiresTemplateExpansion = (
                !typeof(UIContainerElement).IsAssignableFrom(rawType) &&
                !typeof(UITextElement).IsAssignableFrom(rawType) &&
                !typeof(UISlotDefinition).IsAssignableFrom(rawType) &&
                !typeof(UISlotContent).IsAssignableFrom(rawType)
            );
        }

        public UIElement GetPooledInstance() {
            UIElement retn = (UIElement) FormatterServices.GetUninitializedObject(rawType);
            return retn;
        }

        public void CreateCtor() {
            ReflectionUtil.TypeArray1[0] = rawType;
            Type genericType = ReflectionUtil.CreateGenericType(typeof(Action<>), ReflectionUtil.TypeArray1);
            ConstructorInfo constructor = rawType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            DynamicMethod helperMethod = new DynamicMethod(string.Empty, typeof(void), ReflectionUtil.TypeArray1, rawType.Module, true);
            ILGenerator ilGenerator = helperMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Call, constructor);
            ilGenerator.Emit(OpCodes.Ret);
            rawCtorFn = helperMethod.CreateDelegate(genericType);

            s_Compiler.SetSignature(
                new Parameter<ProcessedType>("processedType", ParameterFlags.NeverNull),
                new Parameter<UIElement>("instance", ParameterFlags.NeverNull),
                typeof(UIElement)
            );

            ParameterExpression typeParam = s_Compiler.GetParameter("processedType");
            ParameterExpression retnParam = s_Compiler.GetParameter("instance");
            ParameterExpression castVal = s_Compiler.AddVariable(genericType, "castVal");
            MemberExpression typeConstructorFn = System.Linq.Expressions.Expression.MakeMemberAccess(typeParam, typeof(ProcessedType).GetField(nameof(rawCtorFn), BindingFlags.Instance | BindingFlags.NonPublic));

            UnaryExpression converted = System.Linq.Expressions.Expression.Convert(typeConstructorFn, genericType);
            s_Compiler.Assign(castVal, converted);
            s_Compiler.RawExpression(System.Linq.Expressions.Expression.Invoke(
                    castVal,
                    System.Linq.Expressions.Expression.Convert(retnParam, rawType)
                )
            );
            s_Compiler.RawExpression(retnParam);
            LambdaExpression lambda = s_Compiler.BuildLambda();
            Debug.Log(lambda.ToCSharpCode());
            constructionFn = (Func<ProcessedType, UIElement, UIElement>) lambda.Compile();
            s_Compiler.Reset();
        }

        static ProcessedType() {
            s_Compiler = new LinqCompiler();
            s_GetPooledInstanceMethodRef = typeof(ProcessedType).GetMethod("GetPooledInstance");
            s_constructorFnAccess = typeof(ProcessedType).GetField("constructorFn");
            s_CreateCtorMethodRef = typeof(ProcessedType).GetMethod("CreateCtor");
        }

        public void MakeConstructor() {
            s_Compiler.SetSignature(new Parameter<ProcessedType>("processedType", ParameterFlags.NeverNull));
            ParameterExpression typeParam = s_Compiler.GetParameter("processedType");
            MethodCallExpression getElement = System.Linq.Expressions.Expression.Call(typeParam, s_GetPooledInstanceMethodRef);
            MemberExpression constructorFunctionAccess = System.Linq.Expressions.Expression.MakeMemberAccess(typeParam, s_constructorFnAccess);

            MethodCallExpression createFn = System.Linq.Expressions.Expression.Call(typeParam, s_CreateCtorMethodRef);

            ParameterExpression retnParam = s_Compiler.AddVariable(typeof(UIElement), "retn");
            ParameterExpression p = s_Compiler.AddVariable(typeof(object), "x");
            s_Compiler.Assign(p, constructorFunctionAccess);
            s_Compiler.Assign(retnParam, getElement);

            s_Compiler.IfEqual(p, System.Linq.Expressions.Expression.Default(typeof(object)), () => {
                s_Compiler.RawExpression(createFn);
                s_Compiler.Assign(p, constructorFunctionAccess);
            });

            Type genericType = ReflectionUtil.CreateGenericType(typeof(Action<>), rawType);
            ParameterExpression castVal = s_Compiler.AddVariable(genericType, "z");

            UnaryExpression converted = System.Linq.Expressions.Expression.Convert(p, genericType);

            s_Compiler.Assign(castVal, converted);
            s_Compiler.RawExpression(System.Linq.Expressions.Expression.Invoke(
                    castVal,
                    System.Linq.Expressions.Expression.Convert(retnParam, rawType)
                )
            );
            s_Compiler.RawExpression(retnParam);

//            constructionFn = (Action<ProcessedType, UIElement>) s_Compiler.BuildLambda().Compile();
        }

        public TemplateDefinition GetTemplate(string templateRoot) {
            if (templateAttr == null) {
                throw new Exception($"Template not defined for {rawType.Name}");
            }

            switch (templateAttr.templateType) {
                case TemplateType.Internal: {
                    string templatePath = Application.Settings.GetInternalTemplatePath(templateAttr.template);
                    string file = TryReadFile(templatePath);
                    if (file == null) {
                        throw new TemplateParseException(templateRoot, $"Cannot find template in (internal) path {templatePath}.");
                    }

                    TemplateLanguage language = TemplateLanguage.XML;
//                    if (Path.GetExtension(templatePath) == "xml") {
//                    }

                    return new TemplateDefinition() {
                        contents = file,
                        language = language
                    };
                }

                case TemplateType.File: {
                    string templatePath = Application.Settings.GetTemplatePath(templateRoot, templateAttr.template);
                    string file = TryReadFile(templatePath);
                    if (file == null) {
                        throw new TemplateParseException(templateRoot, $"Cannot find template in path {templatePath}.");
                    }

                    TemplateLanguage language = TemplateLanguage.XML;

                    return new TemplateDefinition() {
                        contents = file,
                        language = language
                    };
                }

                default:
                    return new TemplateDefinition() {
                        contents = templateAttr.template,
                        language = TemplateLanguage.XML
                    };
            }
        }

        public TemplateDefinition GetTemplateFromApplication(Application application) {
            if (templateAttr == null) {
                throw new Exception($"Template not defined for {rawType.Name}");
            }

            switch (templateAttr.templateType) {
                case TemplateType.Internal: {
                    string templatePath = application.settings.GetInternalTemplatePath(templateAttr.template);
                    string file = TryReadFile(templatePath);
                    if (file == null) {
                        throw new TemplateParseException(application.TemplateRootPath, $"Cannot find template in (internal) path {templatePath}.");
                    }

                    return new TemplateDefinition() {
                        contents = file,
                        filePath = GetTemplatePath(),
                        language = TemplateLanguage.XML
                    };
                }

                case TemplateType.File: {
                    string templatePath = application.settings.GetInternalTemplatePath(templateAttr.template);
                    string file = TryReadFile(templatePath);
                    if (file == null) {
                        throw new TemplateParseException(application.TemplateRootPath, $"Cannot find template in path {templatePath}.");
                    }

                    return new TemplateDefinition() {
                        contents = file,
                        language = TemplateLanguage.XML
                    };
                }

                default:
                    return new TemplateDefinition() {
                        contents = templateAttr.template,
                        language = TemplateLanguage.XML
                    };
            }
        }

        private static string TryReadFile(string path) {
            if (!path.EndsWith(".xml")) {
                path += ".xml";
            }

            // todo should probably be cached, but be careful about reloading

            try {
                return File.ReadAllText(path);
            }
            catch (FileNotFoundException e) {
                Debug.LogWarning(e.Message);
                throw;
            }
            catch (Exception) {
                return null;
            }
        }

        public bool HasTemplatePath() {
            return templateAttr.templateType == TemplateType.File;
        }

        // path from Assets directory
        public string GetTemplatePath() {
            return !HasTemplatePath() ? rawType.AssemblyQualifiedName : templateAttr.template;
        }

        public UIElement CreateInstance() {
            UIElement instance = null;
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

                // clear(element);
                poolList.Add(element);
            }
        }


        public static Action<object> CompileClear(Type type) {
            s_Compiler.SetSignature(new Parameter<object>("element", ParameterFlags.NeverNull));

            ParameterExpression parameterExpression = s_Compiler.GetParameter("element");

            UnaryExpression converted = Expression.Convert(parameterExpression, type);

            ParameterExpression cast = s_Compiler.AddVariable(type, "cast");
            s_Compiler.Assign(cast, converted);
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int i = 0; i < fields.Length; i++) {
                Type fieldType = fields[i].FieldType;
                s_Compiler.Assign(Expression.Field(cast, fields[i]), Expression.Default(fieldType));
            }

            Debug.Log(s_Compiler.BuildLambda().ToCSharpCode());
            Action<object> retn = (Action<object>) s_Compiler.BuildLambda().Compile();
            s_Compiler.Reset();
            return retn;
        }

    }

}