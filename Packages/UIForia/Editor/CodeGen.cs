using System;
using System.Collections.Generic;
using System.IO;
using Shapes2D;
using TMPro;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.Util;
using UnityEditor;
using UnityEngine;

namespace UIForia.Editor {

    public enum InheritanceType {

        NotInherited,
        Inherited

    }

    public static class CodeGen {

        private static readonly PropertyGenerator[] properties = {
            // Overflow
            new PropertyGenerator<Overflow>(StylePropertyId.OverflowX, Overflow.None),
            new PropertyGenerator<Overflow>(StylePropertyId.OverflowY, Overflow.None),

            // Background
            new AnimatedPropertyGenerator<Color>(StylePropertyId.BorderColor, ColorUtil.UnsetValue),
            new AnimatedPropertyGenerator<Color>(StylePropertyId.BackgroundColor, ColorUtil.UnsetValue),
            new AnimatedPropertyGenerator<Color>(StylePropertyId.BackgroundColorSecondary, ColorUtil.UnsetValue),
            new PropertyGenerator<Texture2D>(StylePropertyId.BackgroundImage, null),
            new PropertyGenerator<Texture2D>(StylePropertyId.BackgroundImage1, null),
            new PropertyGenerator<Texture2D>(StylePropertyId.BackgroundImage2, null),
            new PropertyGenerator<GradientType>(StylePropertyId.BackgroundGradientType, GradientType.Linear),
            new PropertyGenerator<GradientAxis>(StylePropertyId.BackgroundGradientAxis, GradientAxis.Horizontal),
            new AnimatedPropertyGenerator<float>(StylePropertyId.BackgroundGradientStart, 0),
            new AnimatedPropertyGenerator<float>(StylePropertyId.BackgroundFillRotation, 0),
            new PropertyGenerator<BackgroundFillType>(StylePropertyId.BackgroundFillType, BackgroundFillType.Normal),
            new PropertyGenerator<BackgroundShapeType>(StylePropertyId.BackgroundShapeType, BackgroundShapeType.Rectangle),
//            new AnimatedPropertyGenerator<float>(StylePropertyId.BackgroundGridSize, 0),
//            new AnimatedPropertyGenerator<float>(StylePropertyId.BackgroundLineSize, 0),
            new AnimatedPropertyGenerator<float>(StylePropertyId.BackgroundFillOffsetX, 0),
            new AnimatedPropertyGenerator<float>(StylePropertyId.BackgroundFillOffsetY, 0),
            new AnimatedPropertyGenerator<float>(StylePropertyId.BackgroundFillScaleX, 1),
            new AnimatedPropertyGenerator<float>(StylePropertyId.BackgroundFillScaleY, 1),
            new AnimatedPropertyGenerator<float>(StylePropertyId.Opacity, 1),
            new PropertyGenerator<Texture2D>(StylePropertyId.Cursor, null, InheritanceType.Inherited),
            new PropertyGenerator<Visibility>(StylePropertyId.Visibility, Visibility.Visible),

            // Flex Item
            new AnimatedPropertyGenerator<int>(StylePropertyId.FlexItemOrder, 0),
            new AnimatedPropertyGenerator<int>(StylePropertyId.FlexItemGrow, 0),
            new AnimatedPropertyGenerator<int>(StylePropertyId.FlexItemShrink, 0),
            new PropertyGenerator<CrossAxisAlignment>(StylePropertyId.FlexItemSelfAlignment, CrossAxisAlignment.Unset),

            // Flex Layout
            new PropertyGenerator<LayoutDirection>(StylePropertyId.FlexLayoutDirection, LayoutDirection.Row),
            new PropertyGenerator<LayoutWrap>(StylePropertyId.FlexLayoutWrap, LayoutWrap.None),
            new PropertyGenerator<MainAxisAlignment>(StylePropertyId.FlexLayoutMainAxisAlignment, MainAxisAlignment.Start),
            new PropertyGenerator<CrossAxisAlignment>(StylePropertyId.FlexLayoutCrossAxisAlignment, CrossAxisAlignment.Start),

            // Grid Item
            new PropertyGenerator<int>(StylePropertyId.GridItemColStart, IntUtil.UnsetValue),
            new PropertyGenerator<int>(StylePropertyId.GridItemColSpan, 1),
            new PropertyGenerator<int>(StylePropertyId.GridItemRowStart, IntUtil.UnsetValue),
            new PropertyGenerator<int>(StylePropertyId.GridItemRowSpan, 1),
            new PropertyGenerator<CrossAxisAlignment>(StylePropertyId.GridItemColSelfAlignment, CrossAxisAlignment.Unset),
            new PropertyGenerator<CrossAxisAlignment>(StylePropertyId.GridItemRowSelfAlignment, CrossAxisAlignment.Unset),

            // Grid Layout
            new PropertyGenerator<LayoutDirection>(StylePropertyId.GridLayoutDirection, LayoutDirection.Row),
            new PropertyGenerator<GridLayoutDensity>(StylePropertyId.GridLayoutDensity, GridLayoutDensity.Sparse),
            new PropertyGenerator<IReadOnlyList<GridTrackSize>>(StylePropertyId.GridLayoutColTemplate, ListPool<GridTrackSize>.Empty, InheritanceType.NotInherited, "ListPool<GridTrackSize>.Empty"),
            new PropertyGenerator<IReadOnlyList<GridTrackSize>>(StylePropertyId.GridLayoutRowTemplate, ListPool<GridTrackSize>.Empty, InheritanceType.NotInherited, "ListPool<GridTrackSize>.Empty"),
            new PropertyGenerator<GridTrackSize>(StylePropertyId.GridLayoutColAutoSize, GridTrackSize.MaxContent),
            new PropertyGenerator<GridTrackSize>(StylePropertyId.GridLayoutRowAutoSize, GridTrackSize.MaxContent),
            new AnimatedPropertyGenerator<float>(StylePropertyId.GridLayoutColGap, 0),
            new AnimatedPropertyGenerator<float>(StylePropertyId.GridLayoutRowGap, 0),
            new PropertyGenerator<CrossAxisAlignment>(StylePropertyId.GridLayoutColAlignment, CrossAxisAlignment.Start),
            new PropertyGenerator<CrossAxisAlignment>(StylePropertyId.GridLayoutRowAlignment, CrossAxisAlignment.Start),

            // Size
            new AnimatedPropertyGenerator<UIMeasurement>(StylePropertyId.MinWidth, new UIMeasurement(0)),
            new AnimatedPropertyGenerator<UIMeasurement>(StylePropertyId.MaxWidth, new UIMeasurement(float.MaxValue)),
            new AnimatedPropertyGenerator<UIMeasurement>(StylePropertyId.PreferredWidth, UIMeasurement.Content100),
            new AnimatedPropertyGenerator<UIMeasurement>(StylePropertyId.MinHeight, new UIMeasurement(0)),
            new AnimatedPropertyGenerator<UIMeasurement>(StylePropertyId.MaxHeight, new UIMeasurement(float.MaxValue)),
            new AnimatedPropertyGenerator<UIMeasurement>(StylePropertyId.PreferredHeight, UIMeasurement.Content100),

            // Margin
            new AnimatedPropertyGenerator<UIMeasurement>(StylePropertyId.MarginTop, new UIMeasurement(0)),
            new AnimatedPropertyGenerator<UIMeasurement>(StylePropertyId.MarginRight, new UIMeasurement(0)),
            new AnimatedPropertyGenerator<UIMeasurement>(StylePropertyId.MarginBottom, new UIMeasurement(0)),
            new AnimatedPropertyGenerator<UIMeasurement>(StylePropertyId.MarginLeft, new UIMeasurement(0)),

            // Border
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BorderTop, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BorderRight, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BorderBottom, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BorderLeft, new UIFixedLength(0)),

            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BorderRadiusTopLeft, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BorderRadiusTopRight, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BorderRadiusBottomRight, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BorderRadiusBottomLeft, new UIFixedLength(0)),

            // Padding
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.PaddingTop, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.PaddingRight, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.PaddingBottom, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.PaddingLeft, new UIFixedLength(0)),

            // Text
            new AnimatedPropertyGenerator<Color>(StylePropertyId.TextColor, Color.black, InheritanceType.Inherited),
            new PropertyGenerator<TMP_FontAsset>(StylePropertyId.TextFontAsset, null, InheritanceType.Inherited, "TMP_FontAsset.defaultFontAsset"),
            new AnimatedPropertyGenerator<int>(StylePropertyId.TextFontSize, 18, InheritanceType.Inherited),
            new PropertyGenerator<Text.FontStyle>(StylePropertyId.TextFontStyle, Text.FontStyle.Normal, InheritanceType.Inherited),
            new PropertyGenerator<Text.TextAlignment>(StylePropertyId.TextAlignment, Text.TextAlignment.Left, InheritanceType.Inherited),
            new PropertyGenerator<TextTransform>(StylePropertyId.TextTransform, TextTransform.None, InheritanceType.Inherited),

            // Anchors
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.AnchorTop, new UIFixedLength(0f, UIFixedUnit.Percent)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.AnchorRight, new UIFixedLength(1f, UIFixedUnit.Percent)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.AnchorBottom, new UIFixedLength(1f, UIFixedUnit.Percent)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.AnchorLeft, new UIFixedLength(0f, UIFixedUnit.Percent)),
            new PropertyGenerator<AnchorTarget>(StylePropertyId.AnchorTarget, AnchorTarget.Parent),

            // Transform
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.TransformPositionX, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.TransformPositionY, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.TransformPivotX, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.TransformPivotY, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TransformScaleX, 1),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TransformScaleY, 1),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TransformRotation, 0),
            new PropertyGenerator<TransformBehavior>(StylePropertyId.TransformBehaviorX, TransformBehavior.Default),
            new PropertyGenerator<TransformBehavior>(StylePropertyId.TransformBehaviorY, TransformBehavior.Default),

            // Layout
            new PropertyGenerator<LayoutType>(StylePropertyId.LayoutType, LayoutType.Flex),
            new PropertyGenerator<LayoutBehavior>(StylePropertyId.LayoutBehavior, LayoutBehavior.Normal),
            new AnimatedPropertyGenerator<int>(StylePropertyId.ZIndex, 0),
            new AnimatedPropertyGenerator<int>(StylePropertyId.RenderLayerOffset, 0),
            new AnimatedPropertyGenerator<RenderLayer>(StylePropertyId.RenderLayer, RenderLayer.Default),

            // Scrollbar Vertical
            new PropertyGenerator<VerticalScrollbarAttachment>(StylePropertyId.ScrollbarVerticalAttachment, VerticalScrollbarAttachment.Right),
            new PropertyGenerator<ScrollbarButtonPlacement>(StylePropertyId.ScrollbarVerticalButtonPlacement, ScrollbarButtonPlacement.Hidden),

            new PropertyGenerator<float>(StylePropertyId.ScrollbarVerticalTrackSize, 10f),
            new PropertyGenerator<float>(StylePropertyId.ScrollbarVerticalTrackBorderRadius, 0),
            new PropertyGenerator<float>(StylePropertyId.ScrollbarVerticalTrackBorderSize, 0),
            new PropertyGenerator<Color>(StylePropertyId.ScrollbarVerticalTrackBorderColor, ColorUtil.UnsetValue),
            new PropertyGenerator<Texture2D>(StylePropertyId.ScrollbarVerticalTrackImage, null),
            new PropertyGenerator<Color>(StylePropertyId.ScrollbarVerticalTrackColor, Color.gray),

            new PropertyGenerator<float>(StylePropertyId.ScrollbarVerticalHandleSize, 10f),
            new PropertyGenerator<float>(StylePropertyId.ScrollbarVerticalHandleBorderRadius, 0),
            new PropertyGenerator<float>(StylePropertyId.ScrollbarVerticalHandleBorderSize, 0),
            new PropertyGenerator<Color>(StylePropertyId.ScrollbarVerticalHandleBorderColor, ColorUtil.UnsetValue),
            new PropertyGenerator<Texture2D>(StylePropertyId.ScrollbarVerticalHandleImage, null),
            new PropertyGenerator<Color>(StylePropertyId.ScrollbarVerticalHandleColor, Color.gray),

            new PropertyGenerator<float>(StylePropertyId.ScrollbarVerticalIncrementSize, 10f),
            new PropertyGenerator<float>(StylePropertyId.ScrollbarVerticalIncrementBorderRadius, 0),
            new PropertyGenerator<float>(StylePropertyId.ScrollbarVerticalIncrementBorderSize, 0),
            new PropertyGenerator<Color>(StylePropertyId.ScrollbarVerticalIncrementBorderColor, ColorUtil.UnsetValue),
            new PropertyGenerator<Texture2D>(StylePropertyId.ScrollbarVerticalIncrementImage, null),
            new PropertyGenerator<Color>(StylePropertyId.ScrollbarVerticalIncrementColor, Color.gray),

            new PropertyGenerator<float>(StylePropertyId.ScrollbarVerticalDecrementSize, 10f),
            new PropertyGenerator<float>(StylePropertyId.ScrollbarVerticalDecrementBorderRadius, 0),
            new PropertyGenerator<float>(StylePropertyId.ScrollbarVerticalDecrementBorderSize, 0),
            new PropertyGenerator<Color>(StylePropertyId.ScrollbarVerticalDecrementBorderColor, ColorUtil.UnsetValue),
            new PropertyGenerator<Texture2D>(StylePropertyId.ScrollbarVerticalDecrementImage, null),
            new PropertyGenerator<Color>(StylePropertyId.ScrollbarVerticalDecrementColor, Color.gray),

            // Scrollbar Horizontal
            new PropertyGenerator<HorizontalScrollbarAttachment>(StylePropertyId.ScrollbarHorizontalAttachment, HorizontalScrollbarAttachment.Bottom),
            new PropertyGenerator<ScrollbarButtonPlacement>(StylePropertyId.ScrollbarHorizontalButtonPlacement, ScrollbarButtonPlacement.Hidden),

            new PropertyGenerator<float>(StylePropertyId.ScrollbarHorizontalTrackSize, 10f),
            new PropertyGenerator<float>(StylePropertyId.ScrollbarHorizontalTrackBorderRadius, 0),
            new PropertyGenerator<float>(StylePropertyId.ScrollbarHorizontalTrackBorderSize, 0),
            new PropertyGenerator<Color>(StylePropertyId.ScrollbarHorizontalTrackBorderColor, ColorUtil.UnsetValue),
            new PropertyGenerator<Texture2D>(StylePropertyId.ScrollbarHorizontalTrackImage, null),
            new PropertyGenerator<Color>(StylePropertyId.ScrollbarHorizontalTrackColor, Color.gray),

            new PropertyGenerator<float>(StylePropertyId.ScrollbarHorizontalHandleSize, 10f),
            new PropertyGenerator<float>(StylePropertyId.ScrollbarHorizontalHandleBorderRadius, 0),
            new PropertyGenerator<float>(StylePropertyId.ScrollbarHorizontalHandleBorderSize, 0),
            new PropertyGenerator<Color>(StylePropertyId.ScrollbarHorizontalHandleBorderColor, ColorUtil.UnsetValue),
            new PropertyGenerator<Texture2D>(StylePropertyId.ScrollbarHorizontalHandleImage, null),
            new PropertyGenerator<Color>(StylePropertyId.ScrollbarHorizontalHandleColor, Color.gray),

            new PropertyGenerator<float>(StylePropertyId.ScrollbarHorizontalIncrementSize, 10f),
            new PropertyGenerator<float>(StylePropertyId.ScrollbarHorizontalIncrementBorderRadius, 0),
            new PropertyGenerator<float>(StylePropertyId.ScrollbarHorizontalIncrementBorderSize, 0),
            new PropertyGenerator<Color>(StylePropertyId.ScrollbarHorizontalIncrementBorderColor, ColorUtil.UnsetValue),
            new PropertyGenerator<Texture2D>(StylePropertyId.ScrollbarHorizontalIncrementImage, null),
            new PropertyGenerator<Color>(StylePropertyId.ScrollbarHorizontalIncrementColor, Color.gray),

            new PropertyGenerator<float>(StylePropertyId.ScrollbarHorizontalDecrementSize, 10f),
            new PropertyGenerator<float>(StylePropertyId.ScrollbarHorizontalDecrementBorderRadius, 0),
            new PropertyGenerator<float>(StylePropertyId.ScrollbarHorizontalDecrementBorderSize, 0),
            new PropertyGenerator<Color>(StylePropertyId.ScrollbarHorizontalDecrementBorderColor, ColorUtil.UnsetValue),
            new PropertyGenerator<Texture2D>(StylePropertyId.ScrollbarHorizontalDecrementImage, null),
            new PropertyGenerator<Color>(StylePropertyId.ScrollbarHorizontalDecrementColor, Color.gray),
        };

        [MenuItem("UIForia/Regenerate Style Stuff")]
        public static void GenerateStyleProxies() {
            string generatedPath = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../Packages/UIForia/Src/_Generated1.cs"));
            string generatedPath2 = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../Packages/UIForia/Src/_Generated2.cs"));
            string generatedPath3 = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../Packages/UIForia/Src/_Generated3.cs"));

            string template = @"
namespace UIForia.Rendering {
    
    public partial struct UIStyleSetStateProxy {
        __REPLACE__UIStyleSetStateProxy__
    }

    public partial struct StyleProperty {

        public bool IsUnset {
            get { 
                switch(propertyId) {
                    __REPLACE_StyleProperty__IsUnset
                }
                return true;
            }
        }

    }

    public partial class UIStyle {
    
        __REPLACE__UIStyle
        
    }

    public partial class UIStyleSet {
    
        __REPLACE__UIStyleSet_Properties

        __REPLACE__UIStyleSet_Methods

        public StyleProperty GetComputedStyleProperty(StylePropertyId propertyId) {
        __REPLACE_UIStyleSet_GetComputed  
        }

    }

    public static partial class StyleUtil {
        
      public static bool CanAnimate(StylePropertyId propertyId) {
                switch (propertyId) {
    
__REPLACE_StyleUtil__CanAnimate
                }
    
                return false;
            }

        public static bool IsInherited(StylePropertyId propertyId) {
            switch (propertyId) {

__REPLACE_StyleUtil__IsInherited
            }

            return false;
        }

    }

}";

            const string StyleBindingTemplate = @"using UIForia.Compilers.AliasSource;
using UIForia.Rendering;    

namespace UIForia.StyleBindings {
    __REPLACE_StyleBindingClasses
}

namespace UIForia.Compilers {

    public partial class StyleBindingCompiler {

__REPLACE_StyleBindingCompiler_EnumSources

        private StyleBindings.StyleBinding DoCompile(string key, string value, Target targetState) {
            switch(targetState.property.ToLower()) {

__REPLACE_StyleBindingCompiler_DoCompile

            }
            return null;
        }

    }

}";

            string retn = "";
            for (int i = 0; i < properties.Length; i++) {
                retn += InflatePropertyTemplate(properties[i]);
            }

            template = template.Replace("__REPLACE__UIStyleSetStateProxy__", retn);
            retn = "";

            for (int i = 0; i < properties.Length; i++) {
                string statement = $"                    case StylePropertyId.{properties[i].propertyIdName}: return ";
                retn += statement + InflateStylePropertyUnset(properties[i]) + ";\n";
            }

            template = template.Replace("__REPLACE_StyleProperty__IsUnset", retn);
            retn = "";

            for (int i = 0; i < properties.Length; i++) {
                retn += InflateUIStyleSetProperties(properties[i]);
            }

            template = template.Replace("__REPLACE__UIStyleSet_Properties", retn);
            retn = "";

            for (int i = 0; i < properties.Length; i++) {
                retn += InflateStyleSetMethods(properties[i]);
            }

            template = template.Replace("__REPLACE__UIStyleSet_Methods", retn);
            retn = GetComputedStyle();
            
            template = template.Replace("__REPLACE_UIStyleSet_GetComputed", retn);
            retn = "";
            
            for (int i = 0; i < properties.Length; i++) {
                if (properties[i].inheritanceType == InheritanceType.Inherited) {
                    retn += $"                    case StylePropertyId.{properties[i].propertyIdName}: return true;\n";
                }
            }

            template = template.Replace("__REPLACE_StyleUtil__IsInherited", retn);
            retn = "";

            for (int i = 0; i < properties.Length; i++) {
                if (properties[i] is AnimatedPropertyGenerator) {
                    retn += $"                    case StylePropertyId.{properties[i].propertyIdName}: return true;\n";
                }
            }

            template = template.Replace("__REPLACE_StyleUtil__CanAnimate", retn);
            retn = "";

            for (int i = 0; i < properties.Length; i++) {
                retn += UIStyle_Property(properties[i]);
            }

            template = template.Replace("__REPLACE__UIStyle", retn);

            File.WriteAllText(generatedPath, template);

            template = StyleBindingTemplate;
            template = template.Replace("__REPLACE_StyleBindingClasses", CreateStyleBindingClasses());
            retn = InflateStyleBindingCompilerDoCompile();
            template = template.Replace("__REPLACE_StyleBindingCompiler_DoCompile", retn);
            retn = CreateEnumAliasSources();
            template = template.Replace("__REPLACE_StyleBindingCompiler_EnumSources", retn);

            File.WriteAllText(generatedPath2, template);
            
            string code = @"using Shapes2D;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UIForia.Util;
using UIForia.Text;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

// Do not edit this file. See CodeGen.cs instead.

namespace UIForia.Rendering {

    public static class DefaultStyleValues_Generated {

";

            for (int i = 0; i < properties.Length; i++) {
                code += "\t\t" + DefaultValue(properties[i]);
            }

            code += DefaultGetValue();

            code += " \n}\n}";

            File.WriteAllText(generatedPath3, code);
        }

        private const string BaseStyleBindingTemplate = @"        
    public class StyleBinding___NAME__ : StyleBinding {

        public readonly Expression<__TYPE__> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding___NAME__(string propertyName, StylePropertyId propertyId, StyleState state, Expression<__TYPE__> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.m_PropertyMap[(int)propertyId].As__CAST_TYPE__;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(__STYLE_PROPERTY_CONSTRUCTOR__, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(__STYLE_PROPERTY_CONSTRUCTOR__);
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(__STYLE_PROPERTY_CONSTRUCTOR__, state);
        }

    }
";

        private static string CreateStyleBindingClasses() {
            HashSet<string> templates = new HashSet<string>();
            string retn = "";
            for (int i = 0; i < properties.Length; i++) {
                PropertyGenerator generator = properties[i];
                string key = generator.GetFullTypeName();


                if (!templates.Contains(key)) {
                    retn += BaseStyleBindingTemplate
                        .Replace("__NAME__", generator.GetPrintableTypeName())
                        .Replace("__TYPE__", generator.GetFullTypeName())
                        .Replace("__CAST_TYPE__", generator.GetCastAccessor())
                        .Replace("__STYLE_PROPERTY_CONSTRUCTOR__", generator.StylePropertyConstructorParameterized("propertyId"));
                }

                templates.Add(key);
            }

            return retn;
        }

        private static string CreateEnumAliasSources() {
            string retn = "";
            HashSet<Type> templates = new HashSet<Type>();
            for (int i = 0; i < properties.Length; i++) {
                PropertyGenerator generator = properties[i];
                if (generator.type.IsEnum) {
                    if (!templates.Contains(generator.type)) {
                        templates.Add(generator.type);
                        retn += $"        private static readonly EnumAliasSource<{generator.GetFullTypeName()}> s_EnumSource_{generator.GetPrintableTypeName()} = new EnumAliasSource<{generator.GetFullTypeName()}>();\n";
                    }
                }
            }

            return retn;
        }

        private static string InflateStyleBindingCompilerDoCompile() {
            string retn = "";
            for (int i = 0; i < properties.Length; i++) {
                PropertyGenerator generator = properties[i];

                string name = "UIForia.Rendering.StylePropertyId." + generator.propertyIdName;
                string bindingName = generator.GetPrintableTypeName();
                string type = generator.GetFullTypeName();
                retn += $@"case ""{generator.propertyIdName.ToLower()}"":
                    return new UIForia.StyleBindings.StyleBinding_{bindingName}(""{generator.propertyIdName}"", {name}, targetState.state, Compile<{type}>(value, {generator.GetAliasSources()}));                
                ";
            }

            return retn;
        }

        private static string GetComputedStyle() {
            string code = "\t\t\tswitch(propertyId) {\n";

            for (int i = 0; i < properties.Length; i++) {
                code += $"\t\t\t\tcase {nameof(StylePropertyId)}.{properties[i].propertyIdName}:\n";
                code += $"\t\t\t\t\t return {properties[i].StyleSetGetComputed};\n";
            }

            code += "\t\t\t\tdefault: throw new System.ArgumentOutOfRangeException(nameof(propertyId), propertyId, null);\n";
            code += "\t\t\t\t}";
            return code;
        }

        private static string InflateStyleSetMethods(PropertyGenerator propertyGenerator) {
            return $@"
        public void Set{propertyGenerator.propertyIdName}({propertyGenerator.GetFullTypeName()} value, {nameof(StyleState)} state) {{
            {propertyGenerator.GetStyleSetSetter()};
        }}

        public {propertyGenerator.GetFullTypeName()} Get{propertyGenerator.propertyIdName}({nameof(StyleState)} state) {{
            return {propertyGenerator.GetStyleSetGetter()};
        }}
        ";
        }

        private static string InflateStylePropertyUnset(PropertyGenerator propertyGenerator) {
            return propertyGenerator.GetIsUnset();
        }

        private static string InflatePropertyTemplate(PropertyGenerator propertyGenerator) {
            const string propertyTemplate = @"
        public __TYPE__ __NAME__ {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.__NAME__, state).As__CAST_TYPE__; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(__CONSTRUCTOR__, state); }
        }
        ";

            return propertyTemplate
                .Replace("__TYPE__", propertyGenerator.GetFullTypeName())
                .Replace("__NAME__", propertyGenerator.propertyIdName)
                .Replace("__CAST_TYPE__", propertyGenerator.GetCastAccessor())
                .Replace("__CONSTRUCTOR__", propertyGenerator.StylePropertyConstructor);
        }

        private static string InflateUIStyleSetProperties(PropertyGenerator propertyGenerator) {
            string propertyTemplate;

            if (propertyGenerator.inheritanceType == InheritanceType.Inherited) {
                propertyTemplate = @"

            public __TYPE__ __NAME__ { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.__NAME__, out property)) return property.As__CAST_TYPE__;
                    if (m_PropertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.__NAME__), out property)) return property.As__CAST_TYPE__;
                    return DefaultStyleValues_Generated.__NAME__;
                }
            }";
            }
            else {
                propertyTemplate = @"

            public __TYPE__ __NAME__ { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.__NAME__, out property)) return property.As__CAST_TYPE__;
                    return DefaultStyleValues_Generated.__NAME__;
                }
            }";
            }

            return propertyTemplate
                .Replace("__TYPE__", propertyGenerator.GetFullTypeName())
                .Replace("__NAME__", propertyGenerator.propertyIdName)
                .Replace("__CAST_TYPE__", propertyGenerator.GetCastAccessor())
                .Replace("__CONSTRUCTOR__", propertyGenerator.StylePropertyConstructor);
        }

        private static string DefaultGetValue() {
            string code = "\t\tpublic static StyleProperty GetPropertyValue(StylePropertyId propertyId) {\n\n";

            code += "\t\t\tswitch(propertyId) {\n";

            for (int i = 0; i < properties.Length; i++) {
                code += $"\t\t\t\tcase {nameof(StylePropertyId)}.{properties[i].propertyIdName}:\n";
                code += $"\t\t\t\t\t return {properties[i].AsStyleProperty};\n";
            }

            code += "\t\t\t\tdefault: throw new System.ArgumentOutOfRangeException(nameof(propertyId), propertyId, null);\n";
            code += "\t\t\t\t}\n}";
            return code;
        }       

        private static string UIStyle_Property(PropertyGenerator propertyGenerator) {
            string template = $@"
        public __TYPE__ __NAME__ {{
            [System.Diagnostics.DebuggerStepThrough]
            get {{ return {UIStyle_GetReaderType(propertyGenerator)} }}
            [System.Diagnostics.DebuggerStepThrough]
            set {{ SetProperty({propertyGenerator.StylePropertyConstructor}); }}
        }}
            ";
            return template
                .Replace("__TYPE__", propertyGenerator.GetFullTypeName())
                .Replace("__NAME__", propertyGenerator.propertyIdName);
        }

        private static string DefaultValue(PropertyGenerator propertyGenerator) {
            if (propertyGenerator.type.IsEnum || typeof(int) == propertyGenerator.type || typeof(float) == propertyGenerator.type) {
                return $"public const {propertyGenerator.GetTypeName()} {propertyGenerator.propertyIdName} = {propertyGenerator.GetDefaultValue()};\n";
            }
            else {
                return $"public static readonly {propertyGenerator.GetTypeName()} {propertyGenerator.propertyIdName} = {propertyGenerator.GetDefaultValue()};\n";
            }
        }

        public static string UIStyle_GetReaderType(PropertyGenerator propertyGenerator) {
            if (propertyGenerator.type.IsEnum) {
                return $"({propertyGenerator.GetFullTypeName()})FindEnumProperty(StylePropertyId.{propertyGenerator.propertyIdName});";
            }
            else if (typeof(int) == propertyGenerator.type) {
                return $"FindIntProperty(StylePropertyId.{propertyGenerator.propertyIdName});";
            }
            else if (typeof(float) == propertyGenerator.type) {
                return $"FindFloatProperty(StylePropertyId.{propertyGenerator.propertyIdName});";
            }
            else if (typeof(Color) == propertyGenerator.type) {
                return $"FindColorProperty(StylePropertyId.{propertyGenerator.propertyIdName});";
            }
            else if (typeof(UIFixedLength) == propertyGenerator.type) {
                return $"FindUIFixedLengthProperty(StylePropertyId.{propertyGenerator.propertyIdName});";
            }
            else if (typeof(UIMeasurement) == propertyGenerator.type) {
                return $"FindUIMeasurementProperty(StylePropertyId.{propertyGenerator.propertyIdName});";
            }
            else if (typeof(GridTrackSize) == propertyGenerator.type) {
                return $"FindGridTrackSizeProperty(StylePropertyId.{propertyGenerator.propertyIdName});";
            }
            else if (typeof(TMP_FontAsset) == propertyGenerator.type) {
                return $"GetProperty(StylePropertyId.{propertyGenerator.propertyIdName}).AsFont;";
            }
            else if (typeof(Texture2D) == propertyGenerator.type) {
                return $"GetProperty(StylePropertyId.{propertyGenerator.propertyIdName}).AsTexture2D;";
            }
            else if (typeof(IReadOnlyList<GridTrackSize>) == propertyGenerator.type) {
                return $"GetProperty(StylePropertyId.{propertyGenerator.propertyIdName}).AsGridTemplate;";
            }

            throw new ArgumentOutOfRangeException();
        }

    }

}