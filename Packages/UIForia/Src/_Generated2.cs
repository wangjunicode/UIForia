using UIForia.Compilers.AliasSource;
using UIForia.Rendering;    

namespace UIForia.StyleBindings {
            
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsOverflow;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsColor;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsTexture2D;
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
        
    public class StyleBinding_GradientType : StyleBinding {

        public readonly Expression<Shapes2D.GradientType> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_GradientType(string propertyName, StylePropertyId propertyId, StyleState state, Expression<Shapes2D.GradientType> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsGradientType;
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
        
    public class StyleBinding_GradientAxis : StyleBinding {

        public readonly Expression<Shapes2D.GradientAxis> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_GradientAxis(string propertyName, StylePropertyId propertyId, StyleState state, Expression<Shapes2D.GradientAxis> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsGradientAxis;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsFloat;
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
        
    public class StyleBinding_BackgroundFillType : StyleBinding {

        public readonly Expression<UIForia.Rendering.BackgroundFillType> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_BackgroundFillType(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.BackgroundFillType> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsBackgroundFillType;
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
        
    public class StyleBinding_BackgroundShapeType : StyleBinding {

        public readonly Expression<UIForia.Rendering.BackgroundShapeType> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_BackgroundShapeType(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.BackgroundShapeType> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsBackgroundShapeType;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsCursorStyle;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsVisibility;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsInt;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsCrossAxisAlignment;
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

        public readonly Expression<UIForia.Rendering.LayoutDirection> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_LayoutDirection(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.LayoutDirection> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsLayoutDirection;
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

        public readonly Expression<UIForia.Rendering.LayoutWrap> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_LayoutWrap(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.LayoutWrap> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsLayoutWrap;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsMainAxisAlignment;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsGridAxisAlignment;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsGridLayoutDensity;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsGridTemplate;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsGridTrackSize;
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

        public readonly Expression<UIForia.UIMeasurement> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_UIMeasurement(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.UIMeasurement> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsUIMeasurement;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsUIFixedLength;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsFont;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsFontStyle;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsTextAlignment;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsTextTransform;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsAnchorTarget;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsTransformOffset;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsTransformBehavior;
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

        public readonly Expression<UIForia.Rendering.LayoutType> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_LayoutType(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.LayoutType> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsLayoutType;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsLayoutBehavior;
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

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsRenderLayer;
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
        
    public class StyleBinding_VerticalScrollbarAttachment : StyleBinding {

        public readonly Expression<UIForia.Rendering.VerticalScrollbarAttachment> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_VerticalScrollbarAttachment(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.VerticalScrollbarAttachment> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsVerticalScrollbarAttachment;
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
        
    public class StyleBinding_ScrollbarButtonPlacement : StyleBinding {

        public readonly Expression<UIForia.Rendering.ScrollbarButtonPlacement> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_ScrollbarButtonPlacement(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.ScrollbarButtonPlacement> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsScrollbarButtonPlacement;
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
        
    public class StyleBinding_HorizontalScrollbarAttachment : StyleBinding {

        public readonly Expression<UIForia.Rendering.HorizontalScrollbarAttachment> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_HorizontalScrollbarAttachment(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.HorizontalScrollbarAttachment> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.m_PropertyMap[(int)propertyId].AsHorizontalScrollbarAttachment;
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
        private static readonly EnumAliasSource<Shapes2D.GradientType> s_EnumSource_GradientType = new EnumAliasSource<Shapes2D.GradientType>();
        private static readonly EnumAliasSource<Shapes2D.GradientAxis> s_EnumSource_GradientAxis = new EnumAliasSource<Shapes2D.GradientAxis>();
        private static readonly EnumAliasSource<UIForia.Rendering.BackgroundFillType> s_EnumSource_BackgroundFillType = new EnumAliasSource<UIForia.Rendering.BackgroundFillType>();
        private static readonly EnumAliasSource<UIForia.Rendering.BackgroundShapeType> s_EnumSource_BackgroundShapeType = new EnumAliasSource<UIForia.Rendering.BackgroundShapeType>();
        private static readonly EnumAliasSource<UIForia.Rendering.Visibility> s_EnumSource_Visibility = new EnumAliasSource<UIForia.Rendering.Visibility>();
        private static readonly EnumAliasSource<UIForia.Layout.CrossAxisAlignment> s_EnumSource_CrossAxisAlignment = new EnumAliasSource<UIForia.Layout.CrossAxisAlignment>();
        private static readonly EnumAliasSource<UIForia.Rendering.LayoutDirection> s_EnumSource_LayoutDirection = new EnumAliasSource<UIForia.Rendering.LayoutDirection>();
        private static readonly EnumAliasSource<UIForia.Rendering.LayoutWrap> s_EnumSource_LayoutWrap = new EnumAliasSource<UIForia.Rendering.LayoutWrap>();
        private static readonly EnumAliasSource<UIForia.Layout.MainAxisAlignment> s_EnumSource_MainAxisAlignment = new EnumAliasSource<UIForia.Layout.MainAxisAlignment>();
        private static readonly EnumAliasSource<UIForia.Layout.GridAxisAlignment> s_EnumSource_GridAxisAlignment = new EnumAliasSource<UIForia.Layout.GridAxisAlignment>();
        private static readonly EnumAliasSource<UIForia.Layout.GridLayoutDensity> s_EnumSource_GridLayoutDensity = new EnumAliasSource<UIForia.Layout.GridLayoutDensity>();
        private static readonly EnumAliasSource<UIForia.Text.FontStyle> s_EnumSource_FontStyle = new EnumAliasSource<UIForia.Text.FontStyle>();
        private static readonly EnumAliasSource<UIForia.Text.TextAlignment> s_EnumSource_TextAlignment = new EnumAliasSource<UIForia.Text.TextAlignment>();
        private static readonly EnumAliasSource<UIForia.Text.TextTransform> s_EnumSource_TextTransform = new EnumAliasSource<UIForia.Text.TextTransform>();
        private static readonly EnumAliasSource<UIForia.Rendering.AnchorTarget> s_EnumSource_AnchorTarget = new EnumAliasSource<UIForia.Rendering.AnchorTarget>();
        private static readonly EnumAliasSource<UIForia.Rendering.TransformBehavior> s_EnumSource_TransformBehavior = new EnumAliasSource<UIForia.Rendering.TransformBehavior>();
        private static readonly EnumAliasSource<UIForia.Rendering.LayoutType> s_EnumSource_LayoutType = new EnumAliasSource<UIForia.Rendering.LayoutType>();
        private static readonly EnumAliasSource<UIForia.Layout.LayoutBehavior> s_EnumSource_LayoutBehavior = new EnumAliasSource<UIForia.Layout.LayoutBehavior>();
        private static readonly EnumAliasSource<UIForia.Rendering.RenderLayer> s_EnumSource_RenderLayer = new EnumAliasSource<UIForia.Rendering.RenderLayer>();
        private static readonly EnumAliasSource<UIForia.Rendering.VerticalScrollbarAttachment> s_EnumSource_VerticalScrollbarAttachment = new EnumAliasSource<UIForia.Rendering.VerticalScrollbarAttachment>();
        private static readonly EnumAliasSource<UIForia.Rendering.ScrollbarButtonPlacement> s_EnumSource_ScrollbarButtonPlacement = new EnumAliasSource<UIForia.Rendering.ScrollbarButtonPlacement>();
        private static readonly EnumAliasSource<UIForia.Rendering.HorizontalScrollbarAttachment> s_EnumSource_HorizontalScrollbarAttachment = new EnumAliasSource<UIForia.Rendering.HorizontalScrollbarAttachment>();


        private StyleBindings.StyleBinding DoCompile(string key, string value, Target targetState) {
            switch(targetState.property.ToLower()) {

case "overflowx":
                    return new UIForia.StyleBindings.StyleBinding_Overflow("OverflowX", UIForia.Rendering.StylePropertyId.OverflowX, targetState.state, Compile<UIForia.Rendering.Overflow>(value, s_EnumSource_Overflow));                
                case "overflowy":
                    return new UIForia.StyleBindings.StyleBinding_Overflow("OverflowY", UIForia.Rendering.StylePropertyId.OverflowY, targetState.state, Compile<UIForia.Rendering.Overflow>(value, s_EnumSource_Overflow));                
                case "bordercolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("BorderColor", UIForia.Rendering.StylePropertyId.BorderColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "backgroundcolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("BackgroundColor", UIForia.Rendering.StylePropertyId.BackgroundColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "backgroundcolorsecondary":
                    return new UIForia.StyleBindings.StyleBinding_Color("BackgroundColorSecondary", UIForia.Rendering.StylePropertyId.BackgroundColorSecondary, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "backgroundimage":
                    return new UIForia.StyleBindings.StyleBinding_Texture2D("BackgroundImage", UIForia.Rendering.StylePropertyId.BackgroundImage, targetState.state, Compile<UnityEngine.Texture2D>(value, textureUrlSource));                
                case "backgroundimage1":
                    return new UIForia.StyleBindings.StyleBinding_Texture2D("BackgroundImage1", UIForia.Rendering.StylePropertyId.BackgroundImage1, targetState.state, Compile<UnityEngine.Texture2D>(value, textureUrlSource));                
                case "backgroundimage2":
                    return new UIForia.StyleBindings.StyleBinding_Texture2D("BackgroundImage2", UIForia.Rendering.StylePropertyId.BackgroundImage2, targetState.state, Compile<UnityEngine.Texture2D>(value, textureUrlSource));                
                case "backgroundgradienttype":
                    return new UIForia.StyleBindings.StyleBinding_GradientType("BackgroundGradientType", UIForia.Rendering.StylePropertyId.BackgroundGradientType, targetState.state, Compile<Shapes2D.GradientType>(value, s_EnumSource_GradientType));                
                case "backgroundgradientaxis":
                    return new UIForia.StyleBindings.StyleBinding_GradientAxis("BackgroundGradientAxis", UIForia.Rendering.StylePropertyId.BackgroundGradientAxis, targetState.state, Compile<Shapes2D.GradientAxis>(value, s_EnumSource_GradientAxis));                
                case "backgroundgradientstart":
                    return new UIForia.StyleBindings.StyleBinding_float("BackgroundGradientStart", UIForia.Rendering.StylePropertyId.BackgroundGradientStart, targetState.state, Compile<float>(value, null));                
                case "backgroundfillrotation":
                    return new UIForia.StyleBindings.StyleBinding_float("BackgroundFillRotation", UIForia.Rendering.StylePropertyId.BackgroundFillRotation, targetState.state, Compile<float>(value, null));                
                case "backgroundfilltype":
                    return new UIForia.StyleBindings.StyleBinding_BackgroundFillType("BackgroundFillType", UIForia.Rendering.StylePropertyId.BackgroundFillType, targetState.state, Compile<UIForia.Rendering.BackgroundFillType>(value, s_EnumSource_BackgroundFillType));                
                case "backgroundshapetype":
                    return new UIForia.StyleBindings.StyleBinding_BackgroundShapeType("BackgroundShapeType", UIForia.Rendering.StylePropertyId.BackgroundShapeType, targetState.state, Compile<UIForia.Rendering.BackgroundShapeType>(value, s_EnumSource_BackgroundShapeType));                
                case "backgroundfilloffsetx":
                    return new UIForia.StyleBindings.StyleBinding_float("BackgroundFillOffsetX", UIForia.Rendering.StylePropertyId.BackgroundFillOffsetX, targetState.state, Compile<float>(value, null));                
                case "backgroundfilloffsety":
                    return new UIForia.StyleBindings.StyleBinding_float("BackgroundFillOffsetY", UIForia.Rendering.StylePropertyId.BackgroundFillOffsetY, targetState.state, Compile<float>(value, null));                
                case "backgroundfillscalex":
                    return new UIForia.StyleBindings.StyleBinding_float("BackgroundFillScaleX", UIForia.Rendering.StylePropertyId.BackgroundFillScaleX, targetState.state, Compile<float>(value, null));                
                case "backgroundfillscaley":
                    return new UIForia.StyleBindings.StyleBinding_float("BackgroundFillScaleY", UIForia.Rendering.StylePropertyId.BackgroundFillScaleY, targetState.state, Compile<float>(value, null));                
                case "opacity":
                    return new UIForia.StyleBindings.StyleBinding_float("Opacity", UIForia.Rendering.StylePropertyId.Opacity, targetState.state, Compile<float>(value, null));                
                case "cursor":
                    return new UIForia.StyleBindings.StyleBinding_CursorStyle("Cursor", UIForia.Rendering.StylePropertyId.Cursor, targetState.state, Compile<UIForia.Rendering.CursorStyle>(value, null));                
                case "visibility":
                    return new UIForia.StyleBindings.StyleBinding_Visibility("Visibility", UIForia.Rendering.StylePropertyId.Visibility, targetState.state, Compile<UIForia.Rendering.Visibility>(value, s_EnumSource_Visibility));                
                case "flexitemorder":
                    return new UIForia.StyleBindings.StyleBinding_int("FlexItemOrder", UIForia.Rendering.StylePropertyId.FlexItemOrder, targetState.state, Compile<int>(value, null));                
                case "flexitemgrow":
                    return new UIForia.StyleBindings.StyleBinding_int("FlexItemGrow", UIForia.Rendering.StylePropertyId.FlexItemGrow, targetState.state, Compile<int>(value, null));                
                case "flexitemshrink":
                    return new UIForia.StyleBindings.StyleBinding_int("FlexItemShrink", UIForia.Rendering.StylePropertyId.FlexItemShrink, targetState.state, Compile<int>(value, null));                
                case "flexitemselfalignment":
                    return new UIForia.StyleBindings.StyleBinding_CrossAxisAlignment("FlexItemSelfAlignment", UIForia.Rendering.StylePropertyId.FlexItemSelfAlignment, targetState.state, Compile<UIForia.Layout.CrossAxisAlignment>(value, s_EnumSource_CrossAxisAlignment));                
                case "flexlayoutdirection":
                    return new UIForia.StyleBindings.StyleBinding_LayoutDirection("FlexLayoutDirection", UIForia.Rendering.StylePropertyId.FlexLayoutDirection, targetState.state, Compile<UIForia.Rendering.LayoutDirection>(value, s_EnumSource_LayoutDirection));                
                case "flexlayoutwrap":
                    return new UIForia.StyleBindings.StyleBinding_LayoutWrap("FlexLayoutWrap", UIForia.Rendering.StylePropertyId.FlexLayoutWrap, targetState.state, Compile<UIForia.Rendering.LayoutWrap>(value, s_EnumSource_LayoutWrap));                
                case "flexlayoutmainaxisalignment":
                    return new UIForia.StyleBindings.StyleBinding_MainAxisAlignment("FlexLayoutMainAxisAlignment", UIForia.Rendering.StylePropertyId.FlexLayoutMainAxisAlignment, targetState.state, Compile<UIForia.Layout.MainAxisAlignment>(value, s_EnumSource_MainAxisAlignment));                
                case "flexlayoutcrossaxisalignment":
                    return new UIForia.StyleBindings.StyleBinding_CrossAxisAlignment("FlexLayoutCrossAxisAlignment", UIForia.Rendering.StylePropertyId.FlexLayoutCrossAxisAlignment, targetState.state, Compile<UIForia.Layout.CrossAxisAlignment>(value, s_EnumSource_CrossAxisAlignment));                
                case "griditemcolstart":
                    return new UIForia.StyleBindings.StyleBinding_int("GridItemColStart", UIForia.Rendering.StylePropertyId.GridItemColStart, targetState.state, Compile<int>(value, null));                
                case "griditemcolspan":
                    return new UIForia.StyleBindings.StyleBinding_int("GridItemColSpan", UIForia.Rendering.StylePropertyId.GridItemColSpan, targetState.state, Compile<int>(value, null));                
                case "griditemrowstart":
                    return new UIForia.StyleBindings.StyleBinding_int("GridItemRowStart", UIForia.Rendering.StylePropertyId.GridItemRowStart, targetState.state, Compile<int>(value, null));                
                case "griditemrowspan":
                    return new UIForia.StyleBindings.StyleBinding_int("GridItemRowSpan", UIForia.Rendering.StylePropertyId.GridItemRowSpan, targetState.state, Compile<int>(value, null));                
                case "griditemcolselfalignment":
                    return new UIForia.StyleBindings.StyleBinding_GridAxisAlignment("GridItemColSelfAlignment", UIForia.Rendering.StylePropertyId.GridItemColSelfAlignment, targetState.state, Compile<UIForia.Layout.GridAxisAlignment>(value, s_EnumSource_GridAxisAlignment));                
                case "griditemrowselfalignment":
                    return new UIForia.StyleBindings.StyleBinding_GridAxisAlignment("GridItemRowSelfAlignment", UIForia.Rendering.StylePropertyId.GridItemRowSelfAlignment, targetState.state, Compile<UIForia.Layout.GridAxisAlignment>(value, s_EnumSource_GridAxisAlignment));                
                case "gridlayoutdirection":
                    return new UIForia.StyleBindings.StyleBinding_LayoutDirection("GridLayoutDirection", UIForia.Rendering.StylePropertyId.GridLayoutDirection, targetState.state, Compile<UIForia.Rendering.LayoutDirection>(value, s_EnumSource_LayoutDirection));                
                case "gridlayoutdensity":
                    return new UIForia.StyleBindings.StyleBinding_GridLayoutDensity("GridLayoutDensity", UIForia.Rendering.StylePropertyId.GridLayoutDensity, targetState.state, Compile<UIForia.Layout.GridLayoutDensity>(value, s_EnumSource_GridLayoutDensity));                
                case "gridlayoutcoltemplate":
                    return new UIForia.StyleBindings.StyleBinding_GridTrackTemplate("GridLayoutColTemplate", UIForia.Rendering.StylePropertyId.GridLayoutColTemplate, targetState.state, Compile<System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize>>(value, null));                
                case "gridlayoutrowtemplate":
                    return new UIForia.StyleBindings.StyleBinding_GridTrackTemplate("GridLayoutRowTemplate", UIForia.Rendering.StylePropertyId.GridLayoutRowTemplate, targetState.state, Compile<System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize>>(value, null));                
                case "gridlayoutmainaxisautosize":
                    return new UIForia.StyleBindings.StyleBinding_GridTrackSize("GridLayoutMainAxisAutoSize", UIForia.Rendering.StylePropertyId.GridLayoutMainAxisAutoSize, targetState.state, Compile<UIForia.Layout.LayoutTypes.GridTrackSize>(value, null));                
                case "gridlayoutcrossaxisautosize":
                    return new UIForia.StyleBindings.StyleBinding_GridTrackSize("GridLayoutCrossAxisAutoSize", UIForia.Rendering.StylePropertyId.GridLayoutCrossAxisAutoSize, targetState.state, Compile<UIForia.Layout.LayoutTypes.GridTrackSize>(value, null));                
                case "gridlayoutcolgap":
                    return new UIForia.StyleBindings.StyleBinding_float("GridLayoutColGap", UIForia.Rendering.StylePropertyId.GridLayoutColGap, targetState.state, Compile<float>(value, null));                
                case "gridlayoutrowgap":
                    return new UIForia.StyleBindings.StyleBinding_float("GridLayoutRowGap", UIForia.Rendering.StylePropertyId.GridLayoutRowGap, targetState.state, Compile<float>(value, null));                
                case "gridlayoutcolalignment":
                    return new UIForia.StyleBindings.StyleBinding_GridAxisAlignment("GridLayoutColAlignment", UIForia.Rendering.StylePropertyId.GridLayoutColAlignment, targetState.state, Compile<UIForia.Layout.GridAxisAlignment>(value, s_EnumSource_GridAxisAlignment));                
                case "gridlayoutrowalignment":
                    return new UIForia.StyleBindings.StyleBinding_GridAxisAlignment("GridLayoutRowAlignment", UIForia.Rendering.StylePropertyId.GridLayoutRowAlignment, targetState.state, Compile<UIForia.Layout.GridAxisAlignment>(value, s_EnumSource_GridAxisAlignment));                
                case "minwidth":
                    return new UIForia.StyleBindings.StyleBinding_UIMeasurement("MinWidth", UIForia.Rendering.StylePropertyId.MinWidth, targetState.state, Compile<UIForia.UIMeasurement>(value, measurementSources));                
                case "maxwidth":
                    return new UIForia.StyleBindings.StyleBinding_UIMeasurement("MaxWidth", UIForia.Rendering.StylePropertyId.MaxWidth, targetState.state, Compile<UIForia.UIMeasurement>(value, measurementSources));                
                case "preferredwidth":
                    return new UIForia.StyleBindings.StyleBinding_UIMeasurement("PreferredWidth", UIForia.Rendering.StylePropertyId.PreferredWidth, targetState.state, Compile<UIForia.UIMeasurement>(value, measurementSources));                
                case "minheight":
                    return new UIForia.StyleBindings.StyleBinding_UIMeasurement("MinHeight", UIForia.Rendering.StylePropertyId.MinHeight, targetState.state, Compile<UIForia.UIMeasurement>(value, measurementSources));                
                case "maxheight":
                    return new UIForia.StyleBindings.StyleBinding_UIMeasurement("MaxHeight", UIForia.Rendering.StylePropertyId.MaxHeight, targetState.state, Compile<UIForia.UIMeasurement>(value, measurementSources));                
                case "preferredheight":
                    return new UIForia.StyleBindings.StyleBinding_UIMeasurement("PreferredHeight", UIForia.Rendering.StylePropertyId.PreferredHeight, targetState.state, Compile<UIForia.UIMeasurement>(value, measurementSources));                
                case "margintop":
                    return new UIForia.StyleBindings.StyleBinding_UIMeasurement("MarginTop", UIForia.Rendering.StylePropertyId.MarginTop, targetState.state, Compile<UIForia.UIMeasurement>(value, measurementSources));                
                case "marginright":
                    return new UIForia.StyleBindings.StyleBinding_UIMeasurement("MarginRight", UIForia.Rendering.StylePropertyId.MarginRight, targetState.state, Compile<UIForia.UIMeasurement>(value, measurementSources));                
                case "marginbottom":
                    return new UIForia.StyleBindings.StyleBinding_UIMeasurement("MarginBottom", UIForia.Rendering.StylePropertyId.MarginBottom, targetState.state, Compile<UIForia.UIMeasurement>(value, measurementSources));                
                case "marginleft":
                    return new UIForia.StyleBindings.StyleBinding_UIMeasurement("MarginLeft", UIForia.Rendering.StylePropertyId.MarginLeft, targetState.state, Compile<UIForia.UIMeasurement>(value, measurementSources));                
                case "bordertop":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("BorderTop", UIForia.Rendering.StylePropertyId.BorderTop, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "borderright":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("BorderRight", UIForia.Rendering.StylePropertyId.BorderRight, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "borderbottom":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("BorderBottom", UIForia.Rendering.StylePropertyId.BorderBottom, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "borderleft":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("BorderLeft", UIForia.Rendering.StylePropertyId.BorderLeft, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "borderradiustopleft":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("BorderRadiusTopLeft", UIForia.Rendering.StylePropertyId.BorderRadiusTopLeft, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "borderradiustopright":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("BorderRadiusTopRight", UIForia.Rendering.StylePropertyId.BorderRadiusTopRight, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "borderradiusbottomright":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("BorderRadiusBottomRight", UIForia.Rendering.StylePropertyId.BorderRadiusBottomRight, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "borderradiusbottomleft":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("BorderRadiusBottomLeft", UIForia.Rendering.StylePropertyId.BorderRadiusBottomLeft, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "paddingtop":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("PaddingTop", UIForia.Rendering.StylePropertyId.PaddingTop, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "paddingright":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("PaddingRight", UIForia.Rendering.StylePropertyId.PaddingRight, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "paddingbottom":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("PaddingBottom", UIForia.Rendering.StylePropertyId.PaddingBottom, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "paddingleft":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("PaddingLeft", UIForia.Rendering.StylePropertyId.PaddingLeft, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "textcolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("TextColor", UIForia.Rendering.StylePropertyId.TextColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "textfontasset":
                    return new UIForia.StyleBindings.StyleBinding_TMP_FontAsset("TextFontAsset", UIForia.Rendering.StylePropertyId.TextFontAsset, targetState.state, Compile<TMPro.TMP_FontAsset>(value, fontUrlSource));                
                case "textfontsize":
                    return new UIForia.StyleBindings.StyleBinding_int("TextFontSize", UIForia.Rendering.StylePropertyId.TextFontSize, targetState.state, Compile<int>(value, null));                
                case "textfontstyle":
                    return new UIForia.StyleBindings.StyleBinding_FontStyle("TextFontStyle", UIForia.Rendering.StylePropertyId.TextFontStyle, targetState.state, Compile<UIForia.Text.FontStyle>(value, s_EnumSource_FontStyle));                
                case "textalignment":
                    return new UIForia.StyleBindings.StyleBinding_TextAlignment("TextAlignment", UIForia.Rendering.StylePropertyId.TextAlignment, targetState.state, Compile<UIForia.Text.TextAlignment>(value, s_EnumSource_TextAlignment));                
                case "texttransform":
                    return new UIForia.StyleBindings.StyleBinding_TextTransform("TextTransform", UIForia.Rendering.StylePropertyId.TextTransform, targetState.state, Compile<UIForia.Text.TextTransform>(value, s_EnumSource_TextTransform));                
                case "anchortop":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("AnchorTop", UIForia.Rendering.StylePropertyId.AnchorTop, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "anchorright":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("AnchorRight", UIForia.Rendering.StylePropertyId.AnchorRight, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "anchorbottom":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("AnchorBottom", UIForia.Rendering.StylePropertyId.AnchorBottom, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "anchorleft":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("AnchorLeft", UIForia.Rendering.StylePropertyId.AnchorLeft, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "anchortarget":
                    return new UIForia.StyleBindings.StyleBinding_AnchorTarget("AnchorTarget", UIForia.Rendering.StylePropertyId.AnchorTarget, targetState.state, Compile<UIForia.Rendering.AnchorTarget>(value, s_EnumSource_AnchorTarget));                
                case "transformpositionx":
                    return new UIForia.StyleBindings.StyleBinding_TransformOffset("TransformPositionX", UIForia.Rendering.StylePropertyId.TransformPositionX, targetState.state, Compile<UIForia.TransformOffset>(value, null));                
                case "transformpositiony":
                    return new UIForia.StyleBindings.StyleBinding_TransformOffset("TransformPositionY", UIForia.Rendering.StylePropertyId.TransformPositionY, targetState.state, Compile<UIForia.TransformOffset>(value, null));                
                case "transformpivotx":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("TransformPivotX", UIForia.Rendering.StylePropertyId.TransformPivotX, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "transformpivoty":
                    return new UIForia.StyleBindings.StyleBinding_UIFixedLength("TransformPivotY", UIForia.Rendering.StylePropertyId.TransformPivotY, targetState.state, Compile<UIForia.UIFixedLength>(value, fixedSources));                
                case "transformscalex":
                    return new UIForia.StyleBindings.StyleBinding_float("TransformScaleX", UIForia.Rendering.StylePropertyId.TransformScaleX, targetState.state, Compile<float>(value, null));                
                case "transformscaley":
                    return new UIForia.StyleBindings.StyleBinding_float("TransformScaleY", UIForia.Rendering.StylePropertyId.TransformScaleY, targetState.state, Compile<float>(value, null));                
                case "transformrotation":
                    return new UIForia.StyleBindings.StyleBinding_float("TransformRotation", UIForia.Rendering.StylePropertyId.TransformRotation, targetState.state, Compile<float>(value, null));                
                case "transformbehaviorx":
                    return new UIForia.StyleBindings.StyleBinding_TransformBehavior("TransformBehaviorX", UIForia.Rendering.StylePropertyId.TransformBehaviorX, targetState.state, Compile<UIForia.Rendering.TransformBehavior>(value, s_EnumSource_TransformBehavior));                
                case "transformbehaviory":
                    return new UIForia.StyleBindings.StyleBinding_TransformBehavior("TransformBehaviorY", UIForia.Rendering.StylePropertyId.TransformBehaviorY, targetState.state, Compile<UIForia.Rendering.TransformBehavior>(value, s_EnumSource_TransformBehavior));                
                case "layouttype":
                    return new UIForia.StyleBindings.StyleBinding_LayoutType("LayoutType", UIForia.Rendering.StylePropertyId.LayoutType, targetState.state, Compile<UIForia.Rendering.LayoutType>(value, s_EnumSource_LayoutType));                
                case "layoutbehavior":
                    return new UIForia.StyleBindings.StyleBinding_LayoutBehavior("LayoutBehavior", UIForia.Rendering.StylePropertyId.LayoutBehavior, targetState.state, Compile<UIForia.Layout.LayoutBehavior>(value, s_EnumSource_LayoutBehavior));                
                case "zindex":
                    return new UIForia.StyleBindings.StyleBinding_int("ZIndex", UIForia.Rendering.StylePropertyId.ZIndex, targetState.state, Compile<int>(value, null));                
                case "renderlayeroffset":
                    return new UIForia.StyleBindings.StyleBinding_int("RenderLayerOffset", UIForia.Rendering.StylePropertyId.RenderLayerOffset, targetState.state, Compile<int>(value, null));                
                case "renderlayer":
                    return new UIForia.StyleBindings.StyleBinding_RenderLayer("RenderLayer", UIForia.Rendering.StylePropertyId.RenderLayer, targetState.state, Compile<UIForia.Rendering.RenderLayer>(value, s_EnumSource_RenderLayer));                
                case "scrollbarverticalattachment":
                    return new UIForia.StyleBindings.StyleBinding_VerticalScrollbarAttachment("ScrollbarVerticalAttachment", UIForia.Rendering.StylePropertyId.ScrollbarVerticalAttachment, targetState.state, Compile<UIForia.Rendering.VerticalScrollbarAttachment>(value, s_EnumSource_VerticalScrollbarAttachment));                
                case "scrollbarverticalbuttonplacement":
                    return new UIForia.StyleBindings.StyleBinding_ScrollbarButtonPlacement("ScrollbarVerticalButtonPlacement", UIForia.Rendering.StylePropertyId.ScrollbarVerticalButtonPlacement, targetState.state, Compile<UIForia.Rendering.ScrollbarButtonPlacement>(value, s_EnumSource_ScrollbarButtonPlacement));                
                case "scrollbarverticaltracksize":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarVerticalTrackSize", UIForia.Rendering.StylePropertyId.ScrollbarVerticalTrackSize, targetState.state, Compile<float>(value, null));                
                case "scrollbarverticaltrackborderradius":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarVerticalTrackBorderRadius", UIForia.Rendering.StylePropertyId.ScrollbarVerticalTrackBorderRadius, targetState.state, Compile<float>(value, null));                
                case "scrollbarverticaltrackbordersize":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarVerticalTrackBorderSize", UIForia.Rendering.StylePropertyId.ScrollbarVerticalTrackBorderSize, targetState.state, Compile<float>(value, null));                
                case "scrollbarverticaltrackbordercolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("ScrollbarVerticalTrackBorderColor", UIForia.Rendering.StylePropertyId.ScrollbarVerticalTrackBorderColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "scrollbarverticaltrackimage":
                    return new UIForia.StyleBindings.StyleBinding_Texture2D("ScrollbarVerticalTrackImage", UIForia.Rendering.StylePropertyId.ScrollbarVerticalTrackImage, targetState.state, Compile<UnityEngine.Texture2D>(value, textureUrlSource));                
                case "scrollbarverticaltrackcolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("ScrollbarVerticalTrackColor", UIForia.Rendering.StylePropertyId.ScrollbarVerticalTrackColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "scrollbarverticalhandlesize":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarVerticalHandleSize", UIForia.Rendering.StylePropertyId.ScrollbarVerticalHandleSize, targetState.state, Compile<float>(value, null));                
                case "scrollbarverticalhandleborderradius":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarVerticalHandleBorderRadius", UIForia.Rendering.StylePropertyId.ScrollbarVerticalHandleBorderRadius, targetState.state, Compile<float>(value, null));                
                case "scrollbarverticalhandlebordersize":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarVerticalHandleBorderSize", UIForia.Rendering.StylePropertyId.ScrollbarVerticalHandleBorderSize, targetState.state, Compile<float>(value, null));                
                case "scrollbarverticalhandlebordercolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("ScrollbarVerticalHandleBorderColor", UIForia.Rendering.StylePropertyId.ScrollbarVerticalHandleBorderColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "scrollbarverticalhandleimage":
                    return new UIForia.StyleBindings.StyleBinding_Texture2D("ScrollbarVerticalHandleImage", UIForia.Rendering.StylePropertyId.ScrollbarVerticalHandleImage, targetState.state, Compile<UnityEngine.Texture2D>(value, textureUrlSource));                
                case "scrollbarverticalhandlecolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("ScrollbarVerticalHandleColor", UIForia.Rendering.StylePropertyId.ScrollbarVerticalHandleColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "scrollbarverticalincrementsize":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarVerticalIncrementSize", UIForia.Rendering.StylePropertyId.ScrollbarVerticalIncrementSize, targetState.state, Compile<float>(value, null));                
                case "scrollbarverticalincrementborderradius":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarVerticalIncrementBorderRadius", UIForia.Rendering.StylePropertyId.ScrollbarVerticalIncrementBorderRadius, targetState.state, Compile<float>(value, null));                
                case "scrollbarverticalincrementbordersize":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarVerticalIncrementBorderSize", UIForia.Rendering.StylePropertyId.ScrollbarVerticalIncrementBorderSize, targetState.state, Compile<float>(value, null));                
                case "scrollbarverticalincrementbordercolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("ScrollbarVerticalIncrementBorderColor", UIForia.Rendering.StylePropertyId.ScrollbarVerticalIncrementBorderColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "scrollbarverticalincrementimage":
                    return new UIForia.StyleBindings.StyleBinding_Texture2D("ScrollbarVerticalIncrementImage", UIForia.Rendering.StylePropertyId.ScrollbarVerticalIncrementImage, targetState.state, Compile<UnityEngine.Texture2D>(value, textureUrlSource));                
                case "scrollbarverticalincrementcolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("ScrollbarVerticalIncrementColor", UIForia.Rendering.StylePropertyId.ScrollbarVerticalIncrementColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "scrollbarverticaldecrementsize":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarVerticalDecrementSize", UIForia.Rendering.StylePropertyId.ScrollbarVerticalDecrementSize, targetState.state, Compile<float>(value, null));                
                case "scrollbarverticaldecrementborderradius":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarVerticalDecrementBorderRadius", UIForia.Rendering.StylePropertyId.ScrollbarVerticalDecrementBorderRadius, targetState.state, Compile<float>(value, null));                
                case "scrollbarverticaldecrementbordersize":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarVerticalDecrementBorderSize", UIForia.Rendering.StylePropertyId.ScrollbarVerticalDecrementBorderSize, targetState.state, Compile<float>(value, null));                
                case "scrollbarverticaldecrementbordercolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("ScrollbarVerticalDecrementBorderColor", UIForia.Rendering.StylePropertyId.ScrollbarVerticalDecrementBorderColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "scrollbarverticaldecrementimage":
                    return new UIForia.StyleBindings.StyleBinding_Texture2D("ScrollbarVerticalDecrementImage", UIForia.Rendering.StylePropertyId.ScrollbarVerticalDecrementImage, targetState.state, Compile<UnityEngine.Texture2D>(value, textureUrlSource));                
                case "scrollbarverticaldecrementcolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("ScrollbarVerticalDecrementColor", UIForia.Rendering.StylePropertyId.ScrollbarVerticalDecrementColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "scrollbarhorizontalattachment":
                    return new UIForia.StyleBindings.StyleBinding_HorizontalScrollbarAttachment("ScrollbarHorizontalAttachment", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalAttachment, targetState.state, Compile<UIForia.Rendering.HorizontalScrollbarAttachment>(value, s_EnumSource_HorizontalScrollbarAttachment));                
                case "scrollbarhorizontalbuttonplacement":
                    return new UIForia.StyleBindings.StyleBinding_ScrollbarButtonPlacement("ScrollbarHorizontalButtonPlacement", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalButtonPlacement, targetState.state, Compile<UIForia.Rendering.ScrollbarButtonPlacement>(value, s_EnumSource_ScrollbarButtonPlacement));                
                case "scrollbarhorizontaltracksize":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarHorizontalTrackSize", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalTrackSize, targetState.state, Compile<float>(value, null));                
                case "scrollbarhorizontaltrackborderradius":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarHorizontalTrackBorderRadius", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalTrackBorderRadius, targetState.state, Compile<float>(value, null));                
                case "scrollbarhorizontaltrackbordersize":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarHorizontalTrackBorderSize", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalTrackBorderSize, targetState.state, Compile<float>(value, null));                
                case "scrollbarhorizontaltrackbordercolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("ScrollbarHorizontalTrackBorderColor", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalTrackBorderColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "scrollbarhorizontaltrackimage":
                    return new UIForia.StyleBindings.StyleBinding_Texture2D("ScrollbarHorizontalTrackImage", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalTrackImage, targetState.state, Compile<UnityEngine.Texture2D>(value, textureUrlSource));                
                case "scrollbarhorizontaltrackcolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("ScrollbarHorizontalTrackColor", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalTrackColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "scrollbarhorizontalhandlesize":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarHorizontalHandleSize", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalHandleSize, targetState.state, Compile<float>(value, null));                
                case "scrollbarhorizontalhandleborderradius":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarHorizontalHandleBorderRadius", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalHandleBorderRadius, targetState.state, Compile<float>(value, null));                
                case "scrollbarhorizontalhandlebordersize":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarHorizontalHandleBorderSize", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalHandleBorderSize, targetState.state, Compile<float>(value, null));                
                case "scrollbarhorizontalhandlebordercolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("ScrollbarHorizontalHandleBorderColor", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalHandleBorderColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "scrollbarhorizontalhandleimage":
                    return new UIForia.StyleBindings.StyleBinding_Texture2D("ScrollbarHorizontalHandleImage", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalHandleImage, targetState.state, Compile<UnityEngine.Texture2D>(value, textureUrlSource));                
                case "scrollbarhorizontalhandlecolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("ScrollbarHorizontalHandleColor", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalHandleColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "scrollbarhorizontalincrementsize":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarHorizontalIncrementSize", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalIncrementSize, targetState.state, Compile<float>(value, null));                
                case "scrollbarhorizontalincrementborderradius":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarHorizontalIncrementBorderRadius", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalIncrementBorderRadius, targetState.state, Compile<float>(value, null));                
                case "scrollbarhorizontalincrementbordersize":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarHorizontalIncrementBorderSize", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalIncrementBorderSize, targetState.state, Compile<float>(value, null));                
                case "scrollbarhorizontalincrementbordercolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("ScrollbarHorizontalIncrementBorderColor", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalIncrementBorderColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "scrollbarhorizontalincrementimage":
                    return new UIForia.StyleBindings.StyleBinding_Texture2D("ScrollbarHorizontalIncrementImage", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalIncrementImage, targetState.state, Compile<UnityEngine.Texture2D>(value, textureUrlSource));                
                case "scrollbarhorizontalincrementcolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("ScrollbarHorizontalIncrementColor", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalIncrementColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "scrollbarhorizontaldecrementsize":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarHorizontalDecrementSize", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalDecrementSize, targetState.state, Compile<float>(value, null));                
                case "scrollbarhorizontaldecrementborderradius":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarHorizontalDecrementBorderRadius", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalDecrementBorderRadius, targetState.state, Compile<float>(value, null));                
                case "scrollbarhorizontaldecrementbordersize":
                    return new UIForia.StyleBindings.StyleBinding_float("ScrollbarHorizontalDecrementBorderSize", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalDecrementBorderSize, targetState.state, Compile<float>(value, null));                
                case "scrollbarhorizontaldecrementbordercolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("ScrollbarHorizontalDecrementBorderColor", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalDecrementBorderColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                case "scrollbarhorizontaldecrementimage":
                    return new UIForia.StyleBindings.StyleBinding_Texture2D("ScrollbarHorizontalDecrementImage", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalDecrementImage, targetState.state, Compile<UnityEngine.Texture2D>(value, textureUrlSource));                
                case "scrollbarhorizontaldecrementcolor":
                    return new UIForia.StyleBindings.StyleBinding_Color("ScrollbarHorizontalDecrementColor", UIForia.Rendering.StylePropertyId.ScrollbarHorizontalDecrementColor, targetState.state, Compile<UnityEngine.Color>(value, colorSources));                
                

            }
            return null;
        }

    }

}