using UIForia.Expressions;
using UIForia.Elements;
using UIForia.Rendering;

namespace UIForia.Bindings.StyleBindings {
            
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
        
    public class StyleBinding_ClipBehavior : StyleBinding {

        public readonly Expression<UIForia.Layout.ClipBehavior> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_ClipBehavior(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Layout.ClipBehavior> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsClipBehavior;
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
        
    public class StyleBinding_ClipBounds : StyleBinding {

        public readonly Expression<UIForia.Rendering.ClipBounds> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_ClipBounds(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.ClipBounds> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsClipBounds;
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
        
    public class StyleBinding_BackgroundFit : StyleBinding {

        public readonly Expression<UIForia.Rendering.BackgroundFit> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_BackgroundFit(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.BackgroundFit> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsBackgroundFit;
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
        
    public class StyleBinding_GridItemPlacement : StyleBinding {

        public readonly Expression<UIForia.Layout.LayoutTypes.GridItemPlacement> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_GridItemPlacement(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Layout.LayoutTypes.GridItemPlacement> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsGridItemPlacement;
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
        
    public class StyleBinding_LayoutFit : StyleBinding {

        public readonly Expression<UIForia.Layout.LayoutFit> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_LayoutFit(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Layout.LayoutFit> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsLayoutFit;
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
        
    public class StyleBinding_SpaceDistribution : StyleBinding {

        public readonly Expression<UIForia.Layout.SpaceDistribution> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_SpaceDistribution(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Layout.SpaceDistribution> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsSpaceDistribution;
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
        
    public class StyleBinding_AlignmentDirection : StyleBinding {

        public readonly Expression<UIForia.Layout.AlignmentDirection> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_AlignmentDirection(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Layout.AlignmentDirection> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsAlignmentDirection;
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
        
    public class StyleBinding_AlignmentTarget : StyleBinding {

        public readonly Expression<UIForia.Layout.AlignmentTarget> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_AlignmentTarget(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Layout.AlignmentTarget> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsAlignmentTarget;
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
        
    public class StyleBinding_OffsetMeasurement : StyleBinding {

        public readonly Expression<UIForia.OffsetMeasurement> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_OffsetMeasurement(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.OffsetMeasurement> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsOffsetMeasurement;
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
        
    public class StyleBinding_FontAsset : StyleBinding {

        public readonly Expression<UIForia.FontAsset> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_FontAsset(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.FontAsset> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsFont;
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
        
    public class StyleBinding_UnderlayType : StyleBinding {

        public readonly Expression<UIForia.Rendering.UnderlayType> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_UnderlayType(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Rendering.UnderlayType> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsUnderlayType;
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
        
    public class StyleBinding_WhitespaceMode : StyleBinding {

        public readonly Expression<UIForia.Text.WhitespaceMode> expression;
        public readonly StylePropertyId propertyId;
        
        public StyleBinding_WhitespaceMode(string propertyName, StylePropertyId propertyId, StyleState state, Expression<UIForia.Text.WhitespaceMode> expression)
            : base(propertyName, state) {
            this.propertyId = propertyId;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            if (!element.style.IsInState(state)) return;

            var oldValue = element.style.propertyMap[(int)propertyId].AsWhitespaceMode;
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
