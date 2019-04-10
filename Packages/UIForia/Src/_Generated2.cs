using UIForia.Compilers.AliasSource;
using UIForia.Expressions;
using UIForia.Elements;
using UIForia.Rendering;

namespace UIForia.Bindings.StyleBindings {
            
    public class StyleBinding_Overflow : StyleBinding {

        public readonly Expression<UIForia.Rendering.Overflow> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_Overflow(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.Overflow> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsOverflow;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }
        
    public class StyleBinding_Color : StyleBinding {

        public readonly Expression<UnityEngine.Color> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_Color(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UnityEngine.Color> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsColor;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, value), state);
        }

    }
        
    public class StyleBinding_UIFixedLength : StyleBinding {

        public readonly Expression<UIForia.UIFixedLength> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_UIFixedLength(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.UIFixedLength> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsUIFixedLength;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, value), state);
        }

    }
        
    public class StyleBinding_Texture2D : StyleBinding {

        public readonly Expression<UnityEngine.Texture2D> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_Texture2D(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UnityEngine.Texture2D> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsTexture2D;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, 0, 0, value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, 0, 0, value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, 0, 0, value), state);
        }

    }
        
    public class StyleBinding_string : StyleBinding {

        public readonly Expression<string> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_string(string propertyName, StylePropertyId propertyId, StyleState state, Expression<string> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsString;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, 0, 0, value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, 0, 0, value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, 0, 0, value), state);
        }

    }
        
    public class StyleBinding_float : StyleBinding {

        public readonly Expression<float> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_float(string propertyName, StylePropertyId propertyId, StyleState state, Expression<float> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsFloat;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, value), state);
        }

    }
        
    public class StyleBinding_CursorStyle : StyleBinding {

        public readonly Expression<UIForia.Rendering.CursorStyle> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_CursorStyle(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.CursorStyle> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsCursorStyle;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, 0, 0, value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, 0, 0, value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, 0, 0, value), state);
        }

    }
        
    public class StyleBinding_Visibility : StyleBinding {

        public readonly Expression<UIForia.Rendering.Visibility> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_Visibility(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.Visibility> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsVisibility;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }
        
    public class StyleBinding_int : StyleBinding {

        public readonly Expression<int> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_int(string propertyName, StylePropertyId propertyId, StyleState state, Expression<int> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsInt;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, value), state);
        }

    }
        
    public class StyleBinding_CrossAxisAlignment : StyleBinding {

        public readonly Expression<UIForia.Layout.CrossAxisAlignment> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_CrossAxisAlignment(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Layout.CrossAxisAlignment> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsCrossAxisAlignment;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }
        
    public class StyleBinding_LayoutDirection : StyleBinding {

        public readonly Expression<UIForia.Layout.LayoutDirection> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_LayoutDirection(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Layout.LayoutDirection> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsLayoutDirection;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }
        
    public class StyleBinding_LayoutWrap : StyleBinding {

        public readonly Expression<UIForia.Layout.LayoutWrap> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_LayoutWrap(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Layout.LayoutWrap> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsLayoutWrap;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }
        
    public class StyleBinding_MainAxisAlignment : StyleBinding {

        public readonly Expression<UIForia.Layout.MainAxisAlignment> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_MainAxisAlignment(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Layout.MainAxisAlignment> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsMainAxisAlignment;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }
        
    public class StyleBinding_GridAxisAlignment : StyleBinding {

        public readonly Expression<UIForia.Layout.GridAxisAlignment> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_GridAxisAlignment(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Layout.GridAxisAlignment> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsGridAxisAlignment;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }
        
    public class StyleBinding_GridLayoutDensity : StyleBinding {

        public readonly Expression<UIForia.Layout.GridLayoutDensity> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_GridLayoutDensity(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Layout.GridLayoutDensity> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsGridLayoutDensity;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }
        
    public class StyleBinding_GridTrackTemplate : StyleBinding {

        public readonly Expression<System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize>> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_GridTrackTemplate(string propertyName, StylePropertyId propertyId, StyleState state, Expression<System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize>> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsGridTemplate;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, 0, 0, value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, 0, 0, value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, 0, 0, value), state);
        }

    }
        
    public class StyleBinding_GridTrackSize : StyleBinding {

        public readonly Expression<UIForia.Layout.LayoutTypes.GridTrackSize> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_GridTrackSize(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Layout.LayoutTypes.GridTrackSize> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsGridTrackSize;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, value), state);
        }

    }
        
    public class StyleBinding_UIMeasurement : StyleBinding {

        public readonly Expression<UIForia.Rendering.UIMeasurement> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_UIMeasurement(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.UIMeasurement> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsUIMeasurement;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, value), state);
        }

    }
        
    public class StyleBinding_TMP_FontAsset : StyleBinding {

        public readonly Expression<TMPro.TMP_FontAsset> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_TMP_FontAsset(string propertyName, StylePropertyId propertyId, StyleState state, Expression<TMPro.TMP_FontAsset> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsFont;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, 0, 0, value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, 0, 0, value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, 0, 0, value), state);
        }

    }
        
    public class StyleBinding_FontStyle : StyleBinding {

        public readonly Expression<UIForia.Text.FontStyle> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_FontStyle(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Text.FontStyle> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsFontStyle;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }
        
    public class StyleBinding_TextAlignment : StyleBinding {

        public readonly Expression<UIForia.Text.TextAlignment> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_TextAlignment(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Text.TextAlignment> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsTextAlignment;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }
        
    public class StyleBinding_ShadowType : StyleBinding {

        public readonly Expression<UIForia.Rendering.ShadowType> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_ShadowType(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.ShadowType> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsShadowType;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }
        
    public class StyleBinding_TextTransform : StyleBinding {

        public readonly Expression<UIForia.Text.TextTransform> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_TextTransform(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Text.TextTransform> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsTextTransform;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }
        
    public class StyleBinding_AnchorTarget : StyleBinding {

        public readonly Expression<UIForia.Rendering.AnchorTarget> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_AnchorTarget(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.AnchorTarget> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsAnchorTarget;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }
        
    public class StyleBinding_TransformOffset : StyleBinding {

        public readonly Expression<UIForia.TransformOffset> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_TransformOffset(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.TransformOffset> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsTransformOffset;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, value), state);
        }

    }
        
    public class StyleBinding_TransformBehavior : StyleBinding {

        public readonly Expression<UIForia.Rendering.TransformBehavior> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_TransformBehavior(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.TransformBehavior> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsTransformBehavior;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }
        
    public class StyleBinding_LayoutType : StyleBinding {

        public readonly Expression<UIForia.Layout.LayoutType> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_LayoutType(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Layout.LayoutType> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsLayoutType;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }
        
    public class StyleBinding_LayoutBehavior : StyleBinding {

        public readonly Expression<UIForia.Layout.LayoutBehavior> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_LayoutBehavior(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Layout.LayoutBehavior> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsLayoutBehavior;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }
        
    public class StyleBinding_RenderLayer : StyleBinding {

        public readonly Expression<UIForia.Rendering.RenderLayer> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_RenderLayer(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.RenderLayer> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsRenderLayer;
            var value = expression.Evaluate(context);
            if (value != oldValue) {
                element.style.SetProperty(new StyleProperty(propertyId, (int)value), state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, ExpressionContext context) {
            var value = expression.Evaluate(context);
            style.SetProperty(new StyleProperty(propertyId, (int)value));
        }

        public override void Apply(UIStyleSet styleSet, ExpressionContext context) {
            var value = expression.Evaluate(context);
            styleSet.SetProperty(new StyleProperty(propertyId, (int)value), state);
        }

    }

}

namespace UIForia.Compilers {

    public partial class StyleBindingCompiler {

        private static readonly EnumAliasSource<UIForia.Rendering.Overflow> s_EnumSource_Overflow = new EnumAliasSource<UIForia.Rendering.Overflow>();
        private static readonly EnumAliasSource<UIForia.Rendering.Visibility> s_EnumSource_Visibility = new EnumAliasSource<UIForia.Rendering.Visibility>();
        private static readonly EnumAliasSource<UIForia.Layout.CrossAxisAlignment> s_EnumSource_CrossAxisAlignment = new EnumAliasSource<UIForia.Layout.CrossAxisAlignment>();
        private static readonly EnumAliasSource<UIForia.Layout.LayoutDirection> s_EnumSource_LayoutDirection = new EnumAliasSource<UIForia.Layout.LayoutDirection>();
        private static readonly EnumAliasSource<UIForia.Layout.LayoutWrap> s_EnumSource_LayoutWrap = new EnumAliasSource<UIForia.Layout.LayoutWrap>();
        private static readonly EnumAliasSource<UIForia.Layout.MainAxisAlignment> s_EnumSource_MainAxisAlignment = new EnumAliasSource<UIForia.Layout.MainAxisAlignment>();
        private static readonly EnumAliasSource<UIForia.Layout.GridAxisAlignment> s_EnumSource_GridAxisAlignment = new EnumAliasSource<UIForia.Layout.GridAxisAlignment>();
        private static readonly EnumAliasSource<UIForia.Layout.GridLayoutDensity> s_EnumSource_GridLayoutDensity = new EnumAliasSource<UIForia.Layout.GridLayoutDensity>();
        private static readonly EnumAliasSource<UIForia.Text.FontStyle> s_EnumSource_FontStyle = new EnumAliasSource<UIForia.Text.FontStyle>();
        private static readonly EnumAliasSource<UIForia.Text.TextAlignment> s_EnumSource_TextAlignment = new EnumAliasSource<UIForia.Text.TextAlignment>();
        private static readonly EnumAliasSource<UIForia.Rendering.ShadowType> s_EnumSource_ShadowType = new EnumAliasSource<UIForia.Rendering.ShadowType>();
        private static readonly EnumAliasSource<UIForia.Text.TextTransform> s_EnumSource_TextTransform = new EnumAliasSource<UIForia.Text.TextTransform>();
        private static readonly EnumAliasSource<UIForia.Rendering.AnchorTarget> s_EnumSource_AnchorTarget = new EnumAliasSource<UIForia.Rendering.AnchorTarget>();
        private static readonly EnumAliasSource<UIForia.Rendering.TransformBehavior> s_EnumSource_TransformBehavior = new EnumAliasSource<UIForia.Rendering.TransformBehavior>();
        private static readonly EnumAliasSource<UIForia.Layout.LayoutType> s_EnumSource_LayoutType = new EnumAliasSource<UIForia.Layout.LayoutType>();
        private static readonly EnumAliasSource<UIForia.Layout.LayoutBehavior> s_EnumSource_LayoutBehavior = new EnumAliasSource<UIForia.Layout.LayoutBehavior>();
        private static readonly EnumAliasSource<UIForia.Rendering.RenderLayer> s_EnumSource_RenderLayer = new EnumAliasSource<UIForia.Rendering.RenderLayer>();


        private UIForia.Bindings.StyleBindings.StyleBinding DoCompile(string key, string value, Target targetState) {
            switch(targetState.property.ToLower()) {

case "overflowx":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_Overflow("OverflowX", UIForia.Rendering.StylePropertyId.OverflowX, targetState.state, Compile<UIForia.Rendering.Overflow>(value, s_EnumSource_Overflow));                
                case "overflowy":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_Overflow("OverflowY", UIForia.Rendering.StylePropertyId.OverflowY, targetState.state, Compile<UIForia.Rendering.Overflow>(value, s_EnumSource_Overflow));                
                case "backgroundcolor":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_Color("BackgroundColor", UIForia.Rendering.StylePropertyId.BackgroundColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "backgroundimageoffsetx":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("BackgroundImageOffsetX", UIForia.Rendering.StylePropertyId.BackgroundImageOffsetX, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "backgroundimageoffsety":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("BackgroundImageOffsetY", UIForia.Rendering.StylePropertyId.BackgroundImageOffsetY, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "backgroundimagescalex":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("BackgroundImageScaleX", UIForia.Rendering.StylePropertyId.BackgroundImageScaleX, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "backgroundimagescaley":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("BackgroundImageScaleY", UIForia.Rendering.StylePropertyId.BackgroundImageScaleY, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "backgroundimagetilex":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("BackgroundImageTileX", UIForia.Rendering.StylePropertyId.BackgroundImageTileX, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "backgroundimagetiley":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("BackgroundImageTileY", UIForia.Rendering.StylePropertyId.BackgroundImageTileY, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "backgroundimagerotation":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("BackgroundImageRotation", UIForia.Rendering.StylePropertyId.BackgroundImageRotation, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "backgroundimage":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_Texture2D("BackgroundImage", UIForia.Rendering.StylePropertyId.BackgroundImage, targetState.state, Compile<UnityEngine.Texture2D>(value, textureUrlSource));                
                case "painter":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_string("Painter", UIForia.Rendering.StylePropertyId.Painter, targetState.state, Compile<string>(value, null));                
                case "opacity":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("Opacity", UIForia.Rendering.StylePropertyId.Opacity, targetState.state, Compile<float>(value, null));                
                case "cursor":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_CursorStyle("Cursor", UIForia.Rendering.StylePropertyId.Cursor, targetState.state, Compile<UIForia.Rendering.CursorStyle>(value, null));                
                case "visibility":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_Visibility("Visibility", UIForia.Rendering.StylePropertyId.Visibility, targetState.state, Compile<UIForia.Rendering.Visibility>(value, s_EnumSource_Visibility));                
                case "flexitemorder":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_int("FlexItemOrder", UIForia.Rendering.StylePropertyId.FlexItemOrder, targetState.state, Compile<int>(value, null));                
                case "flexitemgrow":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_int("FlexItemGrow", UIForia.Rendering.StylePropertyId.FlexItemGrow, targetState.state, Compile<int>(value, null));                
                case "flexitemshrink":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_int("FlexItemShrink", UIForia.Rendering.StylePropertyId.FlexItemShrink, targetState.state, Compile<int>(value, null));                
                case "flexitemselfalignment":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_CrossAxisAlignment("FlexItemSelfAlignment", UIForia.Rendering.StylePropertyId.FlexItemSelfAlignment, targetState.state, Compile<UIForia.Layout.CrossAxisAlignment>(value, s_EnumSource_CrossAxisAlignment));                
                case "flexlayoutdirection":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_LayoutDirection("FlexLayoutDirection", UIForia.Rendering.StylePropertyId.FlexLayoutDirection, targetState.state, Compile<UIForia.Layout.LayoutDirection>(value, s_EnumSource_LayoutDirection));                
                case "flexlayoutwrap":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_LayoutWrap("FlexLayoutWrap", UIForia.Rendering.StylePropertyId.FlexLayoutWrap, targetState.state, Compile<UIForia.Layout.LayoutWrap>(value, s_EnumSource_LayoutWrap));                
                case "flexlayoutmainaxisalignment":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_MainAxisAlignment("FlexLayoutMainAxisAlignment", UIForia.Rendering.StylePropertyId.FlexLayoutMainAxisAlignment, targetState.state, Compile<UIForia.Layout.MainAxisAlignment>(value, s_EnumSource_MainAxisAlignment));                
                case "flexlayoutcrossaxisalignment":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_CrossAxisAlignment("FlexLayoutCrossAxisAlignment", UIForia.Rendering.StylePropertyId.FlexLayoutCrossAxisAlignment, targetState.state, Compile<UIForia.Layout.CrossAxisAlignment>(value, s_EnumSource_CrossAxisAlignment));                
                case "griditemcolstart":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_int("GridItemColStart", UIForia.Rendering.StylePropertyId.GridItemColStart, targetState.state, Compile<int>(value, null));                
                case "griditemcolspan":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_int("GridItemColSpan", UIForia.Rendering.StylePropertyId.GridItemColSpan, targetState.state, Compile<int>(value, null));                
                case "griditemrowstart":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_int("GridItemRowStart", UIForia.Rendering.StylePropertyId.GridItemRowStart, targetState.state, Compile<int>(value, null));                
                case "griditemrowspan":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_int("GridItemRowSpan", UIForia.Rendering.StylePropertyId.GridItemRowSpan, targetState.state, Compile<int>(value, null));                
                case "griditemcolselfalignment":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_GridAxisAlignment("GridItemColSelfAlignment", UIForia.Rendering.StylePropertyId.GridItemColSelfAlignment, targetState.state, Compile<UIForia.Layout.GridAxisAlignment>(value, s_EnumSource_GridAxisAlignment));                
                case "griditemrowselfalignment":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_GridAxisAlignment("GridItemRowSelfAlignment", UIForia.Rendering.StylePropertyId.GridItemRowSelfAlignment, targetState.state, Compile<UIForia.Layout.GridAxisAlignment>(value, s_EnumSource_GridAxisAlignment));                
                case "gridlayoutdirection":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_LayoutDirection("GridLayoutDirection", UIForia.Rendering.StylePropertyId.GridLayoutDirection, targetState.state, Compile<UIForia.Layout.LayoutDirection>(value, s_EnumSource_LayoutDirection));                
                case "gridlayoutdensity":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_GridLayoutDensity("GridLayoutDensity", UIForia.Rendering.StylePropertyId.GridLayoutDensity, targetState.state, Compile<UIForia.Layout.GridLayoutDensity>(value, s_EnumSource_GridLayoutDensity));                
                case "gridlayoutcoltemplate":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_GridTrackTemplate("GridLayoutColTemplate", UIForia.Rendering.StylePropertyId.GridLayoutColTemplate, targetState.state, Compile<System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize>>(value, null));                
                case "gridlayoutrowtemplate":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_GridTrackTemplate("GridLayoutRowTemplate", UIForia.Rendering.StylePropertyId.GridLayoutRowTemplate, targetState.state, Compile<System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize>>(value, null));                
                case "gridlayoutmainaxisautosize":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_GridTrackSize("GridLayoutMainAxisAutoSize", UIForia.Rendering.StylePropertyId.GridLayoutMainAxisAutoSize, targetState.state, Compile<UIForia.Layout.LayoutTypes.GridTrackSize>(value, null));                
                case "gridlayoutcrossaxisautosize":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_GridTrackSize("GridLayoutCrossAxisAutoSize", UIForia.Rendering.StylePropertyId.GridLayoutCrossAxisAutoSize, targetState.state, Compile<UIForia.Layout.LayoutTypes.GridTrackSize>(value, null));                
                case "gridlayoutcolgap":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("GridLayoutColGap", UIForia.Rendering.StylePropertyId.GridLayoutColGap, targetState.state, Compile<float>(value, null));                
                case "gridlayoutrowgap":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("GridLayoutRowGap", UIForia.Rendering.StylePropertyId.GridLayoutRowGap, targetState.state, Compile<float>(value, null));                
                case "gridlayoutcolalignment":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_GridAxisAlignment("GridLayoutColAlignment", UIForia.Rendering.StylePropertyId.GridLayoutColAlignment, targetState.state, Compile<UIForia.Layout.GridAxisAlignment>(value, s_EnumSource_GridAxisAlignment));                
                case "gridlayoutrowalignment":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_GridAxisAlignment("GridLayoutRowAlignment", UIForia.Rendering.StylePropertyId.GridLayoutRowAlignment, targetState.state, Compile<UIForia.Layout.GridAxisAlignment>(value, s_EnumSource_GridAxisAlignment));                
                case "minwidth":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIMeasurement("MinWidth", UIForia.Rendering.StylePropertyId.MinWidth, targetState.state, Compile<UIForia.Rendering.UIMeasurement>(value, measurementSources));                
                case "maxwidth":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIMeasurement("MaxWidth", UIForia.Rendering.StylePropertyId.MaxWidth, targetState.state, Compile<UIForia.Rendering.UIMeasurement>(value, measurementSources));                
                case "preferredwidth":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIMeasurement("PreferredWidth", UIForia.Rendering.StylePropertyId.PreferredWidth, targetState.state, Compile<UIForia.Rendering.UIMeasurement>(value, measurementSources));                
                case "minheight":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIMeasurement("MinHeight", UIForia.Rendering.StylePropertyId.MinHeight, targetState.state, Compile<UIForia.Rendering.UIMeasurement>(value, measurementSources));                
                case "maxheight":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIMeasurement("MaxHeight", UIForia.Rendering.StylePropertyId.MaxHeight, targetState.state, Compile<UIForia.Rendering.UIMeasurement>(value, measurementSources));                
                case "preferredheight":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIMeasurement("PreferredHeight", UIForia.Rendering.StylePropertyId.PreferredHeight, targetState.state, Compile<UIForia.Rendering.UIMeasurement>(value, measurementSources));                
                case "margintop":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIMeasurement("MarginTop", UIForia.Rendering.StylePropertyId.MarginTop, targetState.state, Compile<UIForia.Rendering.UIMeasurement>(value, measurementSources));                
                case "marginright":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIMeasurement("MarginRight", UIForia.Rendering.StylePropertyId.MarginRight, targetState.state, Compile<UIForia.Rendering.UIMeasurement>(value, measurementSources));                
                case "marginbottom":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIMeasurement("MarginBottom", UIForia.Rendering.StylePropertyId.MarginBottom, targetState.state, Compile<UIForia.Rendering.UIMeasurement>(value, measurementSources));                
                case "marginleft":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIMeasurement("MarginLeft", UIForia.Rendering.StylePropertyId.MarginLeft, targetState.state, Compile<UIForia.Rendering.UIMeasurement>(value, measurementSources));                
                case "bordercolor":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_Color("BorderColor", UIForia.Rendering.StylePropertyId.BorderColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "bordertop":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("BorderTop", UIForia.Rendering.StylePropertyId.BorderTop, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "borderright":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("BorderRight", UIForia.Rendering.StylePropertyId.BorderRight, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "borderbottom":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("BorderBottom", UIForia.Rendering.StylePropertyId.BorderBottom, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "borderleft":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("BorderLeft", UIForia.Rendering.StylePropertyId.BorderLeft, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "borderradiustopleft":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("BorderRadiusTopLeft", UIForia.Rendering.StylePropertyId.BorderRadiusTopLeft, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "borderradiustopright":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("BorderRadiusTopRight", UIForia.Rendering.StylePropertyId.BorderRadiusTopRight, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "borderradiusbottomright":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("BorderRadiusBottomRight", UIForia.Rendering.StylePropertyId.BorderRadiusBottomRight, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "borderradiusbottomleft":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("BorderRadiusBottomLeft", UIForia.Rendering.StylePropertyId.BorderRadiusBottomLeft, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "paddingtop":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("PaddingTop", UIForia.Rendering.StylePropertyId.PaddingTop, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "paddingright":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("PaddingRight", UIForia.Rendering.StylePropertyId.PaddingRight, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "paddingbottom":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("PaddingBottom", UIForia.Rendering.StylePropertyId.PaddingBottom, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "paddingleft":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("PaddingLeft", UIForia.Rendering.StylePropertyId.PaddingLeft, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "textcolor":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_Color("TextColor", UIForia.Rendering.StylePropertyId.TextColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "textfontasset":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_TMP_FontAsset("TextFontAsset", UIForia.Rendering.StylePropertyId.TextFontAsset, targetState.state, Compile<TMPro.TMP_FontAsset>(value, fontUrlSource));                
                case "textfontsize":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("TextFontSize", UIForia.Rendering.StylePropertyId.TextFontSize, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "textfontstyle":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_FontStyle("TextFontStyle", UIForia.Rendering.StylePropertyId.TextFontStyle, targetState.state, Compile<UIForia.Text.FontStyle>(value, s_EnumSource_FontStyle));                
                case "textalignment":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_TextAlignment("TextAlignment", UIForia.Rendering.StylePropertyId.TextAlignment, targetState.state, Compile<UIForia.Text.TextAlignment>(value, s_EnumSource_TextAlignment));                
                case "textoutlinewidth":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("TextOutlineWidth", UIForia.Rendering.StylePropertyId.TextOutlineWidth, targetState.state, Compile<float>(value, null));                
                case "textoutlinecolor":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_Color("TextOutlineColor", UIForia.Rendering.StylePropertyId.TextOutlineColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "textglowcolor":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_Color("TextGlowColor", UIForia.Rendering.StylePropertyId.TextGlowColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "textglowoffset":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("TextGlowOffset", UIForia.Rendering.StylePropertyId.TextGlowOffset, targetState.state, Compile<float>(value, null));                
                case "textglowinner":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("TextGlowInner", UIForia.Rendering.StylePropertyId.TextGlowInner, targetState.state, Compile<float>(value, null));                
                case "textglowouter":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("TextGlowOuter", UIForia.Rendering.StylePropertyId.TextGlowOuter, targetState.state, Compile<float>(value, null));                
                case "textglowpower":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("TextGlowPower", UIForia.Rendering.StylePropertyId.TextGlowPower, targetState.state, Compile<float>(value, null));                
                case "textshadowcolor":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_Color("TextShadowColor", UIForia.Rendering.StylePropertyId.TextShadowColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "textshadowoffsetx":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("TextShadowOffsetX", UIForia.Rendering.StylePropertyId.TextShadowOffsetX, targetState.state, Compile<float>(value, null));                
                case "textshadowoffsety":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("TextShadowOffsetY", UIForia.Rendering.StylePropertyId.TextShadowOffsetY, targetState.state, Compile<float>(value, null));                
                case "textshadowintensity":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("TextShadowIntensity", UIForia.Rendering.StylePropertyId.TextShadowIntensity, targetState.state, Compile<float>(value, null));                
                case "textshadowsoftness":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("TextShadowSoftness", UIForia.Rendering.StylePropertyId.TextShadowSoftness, targetState.state, Compile<float>(value, null));                
                case "textshadowtype":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_ShadowType("TextShadowType", UIForia.Rendering.StylePropertyId.TextShadowType, targetState.state, Compile<UIForia.Rendering.ShadowType>(value, s_EnumSource_ShadowType));                
                case "texttransform":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_TextTransform("TextTransform", UIForia.Rendering.StylePropertyId.TextTransform, targetState.state, Compile<UIForia.Text.TextTransform>(value, s_EnumSource_TextTransform));                
                case "anchortop":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("AnchorTop", UIForia.Rendering.StylePropertyId.AnchorTop, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "anchorright":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("AnchorRight", UIForia.Rendering.StylePropertyId.AnchorRight, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "anchorbottom":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("AnchorBottom", UIForia.Rendering.StylePropertyId.AnchorBottom, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "anchorleft":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("AnchorLeft", UIForia.Rendering.StylePropertyId.AnchorLeft, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "anchortarget":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_AnchorTarget("AnchorTarget", UIForia.Rendering.StylePropertyId.AnchorTarget, targetState.state, Compile<UIForia.Rendering.AnchorTarget>(value, s_EnumSource_AnchorTarget));                
                case "transformpositionx":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_TransformOffset("TransformPositionX", UIForia.Rendering.StylePropertyId.TransformPositionX, targetState.state, Compile<UIForia.TransformOffset>(value, null));                
                case "transformpositiony":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_TransformOffset("TransformPositionY", UIForia.Rendering.StylePropertyId.TransformPositionY, targetState.state, Compile<UIForia.TransformOffset>(value, null));                
                case "transformpivotx":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("TransformPivotX", UIForia.Rendering.StylePropertyId.TransformPivotX, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "transformpivoty":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIFixedLength("TransformPivotY", UIForia.Rendering.StylePropertyId.TransformPivotY, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "transformscalex":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("TransformScaleX", UIForia.Rendering.StylePropertyId.TransformScaleX, targetState.state, Compile<float>(value, null));                
                case "transformscaley":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("TransformScaleY", UIForia.Rendering.StylePropertyId.TransformScaleY, targetState.state, Compile<float>(value, null));                
                case "transformrotation":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("TransformRotation", UIForia.Rendering.StylePropertyId.TransformRotation, targetState.state, Compile<float>(value, null));                
                case "transformbehaviorx":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_TransformBehavior("TransformBehaviorX", UIForia.Rendering.StylePropertyId.TransformBehaviorX, targetState.state, Compile<UIForia.Rendering.TransformBehavior>(value, s_EnumSource_TransformBehavior));                
                case "transformbehaviory":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_TransformBehavior("TransformBehaviorY", UIForia.Rendering.StylePropertyId.TransformBehaviorY, targetState.state, Compile<UIForia.Rendering.TransformBehavior>(value, s_EnumSource_TransformBehavior));                
                case "layouttype":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_LayoutType("LayoutType", UIForia.Rendering.StylePropertyId.LayoutType, targetState.state, Compile<UIForia.Layout.LayoutType>(value, s_EnumSource_LayoutType));                
                case "layoutbehavior":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_LayoutBehavior("LayoutBehavior", UIForia.Rendering.StylePropertyId.LayoutBehavior, targetState.state, Compile<UIForia.Layout.LayoutBehavior>(value, s_EnumSource_LayoutBehavior));                
                case "zindex":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_int("ZIndex", UIForia.Rendering.StylePropertyId.ZIndex, targetState.state, Compile<int>(value, null));                
                case "renderlayeroffset":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_int("RenderLayerOffset", UIForia.Rendering.StylePropertyId.RenderLayerOffset, targetState.state, Compile<int>(value, null));                
                case "renderlayer":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_RenderLayer("RenderLayer", UIForia.Rendering.StylePropertyId.RenderLayer, targetState.state, Compile<UIForia.Rendering.RenderLayer>(value, s_EnumSource_RenderLayer));                
                case "scrollbar":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_string("Scrollbar", UIForia.Rendering.StylePropertyId.Scrollbar, targetState.state, Compile<string>(value, null));                
                case "scrollbarsize":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_UIMeasurement("ScrollbarSize", UIForia.Rendering.StylePropertyId.ScrollbarSize, targetState.state, Compile<UIForia.Rendering.UIMeasurement>(value, measurementSources));                
                case "scrollbarcolor":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_Color("ScrollbarColor", UIForia.Rendering.StylePropertyId.ScrollbarColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "shadowtype":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_ShadowType("ShadowType", UIForia.Rendering.StylePropertyId.ShadowType, targetState.state, Compile<UIForia.Rendering.ShadowType>(value, s_EnumSource_ShadowType));                
                case "shadowoffsetx":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("ShadowOffsetX", UIForia.Rendering.StylePropertyId.ShadowOffsetX, targetState.state, Compile<float>(value, null));                
                case "shadowoffsety":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("ShadowOffsetY", UIForia.Rendering.StylePropertyId.ShadowOffsetY, targetState.state, Compile<float>(value, null));                
                case "shadowsoftnessx":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("ShadowSoftnessX", UIForia.Rendering.StylePropertyId.ShadowSoftnessX, targetState.state, Compile<float>(value, null));                
                case "shadowsoftnessy":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("ShadowSoftnessY", UIForia.Rendering.StylePropertyId.ShadowSoftnessY, targetState.state, Compile<float>(value, null));                
                case "shadowintensity":
                    return new UIForia.Bindings.StyleBindings.StyleBinding_float("ShadowIntensity", UIForia.Rendering.StylePropertyId.ShadowIntensity, targetState.state, Compile<float>(value, null));                
                

            }
            return null;
        }

    }

}