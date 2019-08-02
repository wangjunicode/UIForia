using System;
using System.Collections.Generic;
using TMPro;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Parsing.Expression.AstNodes;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers.ExpressionResolvers {

    public class UrlResolver : ExpressionAliasResolver {

        public UrlResolver(string aliasName) : base(aliasName) { }

        public override Expression CompileAsMethodExpression(CompilerContext context, LightList<ASTNode> parameters) {
            if (parameters.Count == 1) {
                Expression expression = context.Visit(typeof(string), parameters[0]);

                if (!(expression is Expression<string> stringExpression)) {
                    return null;
                }
                
                if (typeof(Texture2D) == context.targetType) {
                    return new TextureResolver(stringExpression);
                }

                if (typeof(FontAsset) == context.targetType) {
                    return new FontResolver(stringExpression);
                }

                if (typeof(AudioClip) == context.targetType) {
                    return new AudioResolver(stringExpression);
                }
            }

            return null;
        }

        public class TextureResolver : Expression<Texture2D> {

            private readonly bool isConstant;
            private readonly Expression<string> argument0;

            public TextureResolver(Expression<string> argument0) {
                this.argument0 = argument0;
                this.isConstant = argument0.IsConstant();
            }

            public override Type YieldedType => typeof(Texture2D);

            public override Texture2D Evaluate(ExpressionContext context) {
                UIElement element = (UIElement) context.rootObject;
                return element.Application.ResourceManager.GetTexture(argument0.Evaluate(context));
            }

            public override bool IsConstant() {
                return isConstant; // todo -- maybe not constant if resource manager is dynamic
            }

        }
        
        public class FontResolver : Expression<FontAsset> {

            private readonly bool isConstant;
            private readonly Expression<string> argument0;

            public FontResolver(Expression<string> argument0) {
                this.argument0 = argument0;
                this.isConstant = argument0.IsConstant();
            }

            public override Type YieldedType => typeof(FontAsset);

            public override FontAsset Evaluate(ExpressionContext context) {
                UIElement element = (UIElement) context.rootObject;
                return element.Application.ResourceManager.GetFont(argument0.Evaluate(context));
            }

            public override bool IsConstant() {
                return isConstant; // todo -- maybe not constant if resource manager is dynamic
            }

        }
        
        public class AudioResolver : Expression<AudioClip> {

            private readonly bool isConstant;
            private readonly Expression<string> argument0;

            public AudioResolver(Expression<string> argument0) {
                this.argument0 = argument0;
                this.isConstant = argument0.IsConstant();
            }

            public override Type YieldedType => typeof(AudioClip);

            public override AudioClip Evaluate(ExpressionContext context) {
                UIElement element = (UIElement) context.rootObject;
                return element.Application.ResourceManager.GetAudioClip(argument0.Evaluate(context));
            }

            public override bool IsConstant() {
                return isConstant; // todo -- maybe not constant if resource manager is dynamic
            }

        }


    }

}