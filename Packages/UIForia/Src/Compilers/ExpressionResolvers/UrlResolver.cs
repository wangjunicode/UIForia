using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using TMPro;
using UIForia.Parsing;
using UnityEngine;

namespace UIForia.Compilers {

    public class LengthResolver : ExpressionAliasResolver {

        private static readonly MethodInfo s_LengthVector1;
        private static readonly MethodInfo s_LengthVector2;

        static LengthResolver() {
            s_LengthVector1 = ReflectionUtil.GetMethodInfo(typeof(LengthResolver), nameof(FixedVec1));
            s_LengthVector2 = ReflectionUtil.GetMethodInfo(typeof(LengthResolver), nameof(FixedVec2));
        }

        public LengthResolver(string aliasName) : base(aliasName) { }

        public override Expression CompileAsMethodExpression(CompilerContext context, List<ASTNode> parameters) {
            
            if (context.targetType == typeof(FixedLengthVector)) {
                if (parameters.Count == 1) {
                    Expression expr = context.Visit(typeof(UIFixedLength), parameters[0]);
                    if (expr == null) {
                        throw new CompileException($"Invalid arguments for {aliasName}");
                    }
                    return new MethodCallExpression_Static<UIFixedLength, FixedLengthVector>(s_LengthVector1, new [] { expr });
                }

                if (parameters.Count == 2) {
                    Expression expr0 = context.Visit(typeof(UIFixedLength), parameters[0]);
                    Expression expr1 = context.Visit(typeof(UIFixedLength), parameters[1]);
                    if (expr0 == null || expr1 == null) {
                        throw new CompileException($"Invalid arguments for {aliasName}");
                    }
                    return new MethodCallExpression_Static<UIFixedLength, UIFixedLength, FixedLengthVector>(s_LengthVector2, new [] { expr0, expr1 });
                }
            }

            if (context.targetType == typeof(UIFixedLength)) {
                
            }

            return null;

        }

        [Pure]
        public static FixedLengthVector FixedVec1(UIFixedLength x) {
            return new FixedLengthVector(x, x);
        }
        
        [Pure]
        public static FixedLengthVector FixedVec2(UIFixedLength x, UIFixedLength y) {
            return new FixedLengthVector(x, y);
        }

    }

    public class SizeResolver : ExpressionAliasResolver {

        private static readonly MethodInfo s_SizeInfo1;
        private static readonly MethodInfo s_SizeInfo2;

        static SizeResolver() {
            s_SizeInfo1 = ReflectionUtil.GetMethodInfo(typeof(SizeResolver), nameof(Size1));
            s_SizeInfo2 = ReflectionUtil.GetMethodInfo(typeof(SizeResolver), nameof(Size2));
        }

        public SizeResolver(string aliasName) : base(aliasName) { }

        public override Expression CompileAsMethodExpression(CompilerContext context, List<ASTNode> parameters) {
            if (context.targetType != typeof(MeasurementPair)) {
                return null;
            }

            if (parameters.Count == 1) {
                Expression expr0 = context.Visit(typeof(UIMeasurement), parameters[0]);
                if (expr0 == null) {
                    throw new CompileException($"Invalid arguments for {aliasName}");
                }

                return new MethodCallExpression_Static<UIMeasurement, MeasurementPair>(s_SizeInfo1, new[] {expr0});
            }
            else if (parameters.Count == 2) {
                Expression expr0 = context.Visit(typeof(UIMeasurement), parameters[0]);
                Expression expr1 = context.Visit(typeof(UIMeasurement), parameters[1]);
                if (expr0 == null || expr1 == null) {
                    throw new CompileException($"Invalid arguments for {aliasName}");
                }

                return new MethodCallExpression_Static<UIMeasurement, UIMeasurement, MeasurementPair>(s_SizeInfo2, new[] {expr0, expr1});
            }

            return null;
        }

        [Pure]
        public static MeasurementPair Size1(UIMeasurement x) {
            return new MeasurementPair(x, x);
        }

        [Pure]
        public static MeasurementPair Size2(UIMeasurement x, UIMeasurement y) {
            return new MeasurementPair(x, y);
        }

    }

    public class UrlResolver : ExpressionAliasResolver {

        private static readonly MethodInfo s_TextureUrl;
        private static readonly MethodInfo s_FontUrl;
        private static readonly MethodInfo s_AudioClipUrl;
        private static readonly Expression[] s_Expressions;

        static UrlResolver() {
            s_TextureUrl = ReflectionUtil.GetMethodInfo(typeof(UrlResolver), nameof(TextureUrl));
            s_FontUrl = ReflectionUtil.GetMethodInfo(typeof(UrlResolver), nameof(FontUrl));
            s_AudioClipUrl = ReflectionUtil.GetMethodInfo(typeof(UrlResolver), nameof(AudioUrl));

            s_Expressions = new Expression[1];
        }

        public UrlResolver(string aliasName) : base(aliasName) { }

        public override Expression CompileAsMethodExpression(CompilerContext context, List<ASTNode> parameters) {
            if (parameters.Count == 1) {
                Expression expression = context.Visit(typeof(string), parameters[0]);
                s_Expressions[0] = expression;

                if (expression == null) {
                    return null;
                }

                if (typeof(Texture2D) == context.targetType) {
                    return new MethodCallExpression_Static<string, Texture2D>(s_TextureUrl, s_Expressions);
                }

                if (typeof(TMP_FontAsset) == context.targetType) {
                    return new MethodCallExpression_Static<string, TMP_FontAsset>(s_FontUrl, s_Expressions);
                }

                if (typeof(AudioClip) == context.targetType) {
                    return new MethodCallExpression_Static<string, AudioClip>(s_AudioClipUrl, s_Expressions);
                }
            }

            return null;
        }

        [Pure]
        private static Texture2D TextureUrl(string path) {
            return ResourceManager.GetTexture(path);
        }

        [Pure]
        private static TMP_FontAsset FontUrl(string path) {
            return ResourceManager.GetFont(path);
        }

        [Pure]
        private static AudioClip AudioUrl(string path) {
            return ResourceManager.GetAudioClip(path);
        }

    }

}