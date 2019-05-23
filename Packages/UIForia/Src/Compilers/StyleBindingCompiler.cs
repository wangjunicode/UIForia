using System;
using JetBrains.Annotations;
using TMPro;
using UIForia.Bindings.StyleBindings;
using UIForia.Compilers.AliasSource;
using UIForia.Expressions;
using UIForia.Layout;
using UIForia.Parsing.Expression;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public partial class StyleBindingCompiler {

        private ExpressionCompiler compiler;

        internal static readonly MethodAliasSource parentMeasurementSource;
        internal static readonly MethodAliasSource contentMeasurementSource;
        internal static readonly MethodAliasSource viewportWidthMeasurementSource;
        internal static readonly MethodAliasSource viewportHeightMeasurementSource;
        internal static readonly MethodAliasSource pixelMeasurementSource;

        internal static readonly MethodAliasSource textureUrlSource;
        internal static readonly MethodAliasSource fontUrlSource;

        internal static readonly ValueAliasSource<int> siblingIndexSource;
        internal static readonly ValueAliasSource<int> activeSiblingIndexSource;
        internal static readonly ValueAliasSource<int> childCountSource;
        internal static readonly ValueAliasSource<int> activeChildCountSource;
        internal static readonly ValueAliasSource<int> inactiveChildCountSource;
        internal static readonly ValueAliasSource<int> templateChildChildCountSource;

        internal static readonly IAliasSource[] fixedSources;
        internal static readonly IAliasSource[] measurementSources;
        internal static readonly IAliasSource[] colorSources;


        // todo implement these globally in bindings
        private static readonly string[] k_ElementProperties = {
            "$childIndex",
            "$childCount",
            "$activeChildIndex",
            "$inactiveChildCount",
            "$activeChildCount",
            "$hasActiveChildren",
            "$hasInactiveChildren",
            "$hasChildren"
        };

        private Type rootType;
        private Type elementType;

        public StyleBindingCompiler() {
            this.compiler = new ExpressionCompiler();
        }

        public void SetCompiler(ExpressionCompiler compiler) {
            this.compiler = compiler;
        }
        
        public StyleBinding Compile(Type rootType, Type elementType, AttributeDefinition attributeDefinition) {
            return Compile(rootType, elementType, attributeDefinition.key, attributeDefinition.value);
        }

        public StyleBinding Compile(Type rootType, Type elementType, string key, string value) {
            
            if (key == "style") {
                    
            }
            
            if (!key.StartsWith("style.")) return null;
            this.rootType = rootType;
            this.elementType = elementType;

            Target targetState = GetTargetState(key);

            StyleBinding retn = DoCompile(key, value, targetState);
            return retn;
            
        }

        private static Target GetTargetState(string key) {
            if (key.StartsWith("style.hover.")) {
                return new Target(key.Substring("style.hover.".Length), StyleState.Hover);
            }

            if (key.StartsWith("style.focused.")) {
                return new Target(key.Substring("style.focused.".Length), StyleState.Focused);
            }

            if (key.StartsWith("style.active.")) {
                return new Target(key.Substring("style.active.".Length), StyleState.Active);
            }

            if (key.StartsWith("style.")) {
                return new Target(key.Substring("style.".Length), StyleState.Normal);
            }

            throw new Exception("invalid target state");
        }

        private Expression<T> Compile<T>(string value, params IAliasSource[] sources) {

            Expression<T> expression = compiler.Compile<T>(rootType, elementType, value);
            
            return expression;
        }

      
        private struct Target {

            public readonly string property;
            public readonly StyleState state;

            public Target(string property, StyleState state) {
                this.property = property;
                this.state = state;
            }

        }

    }

}