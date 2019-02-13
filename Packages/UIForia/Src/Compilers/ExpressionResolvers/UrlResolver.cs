using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using TMPro;
using UIForia.Parsing;
using UnityEngine;

namespace UIForia.Compilers {

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