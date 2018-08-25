using JetBrains.Annotations;

namespace Rendering {

    public partial class UIStyleSet {

        [PublicAPI]
        public ContentBoxRect margin {
            // return activeStyle.margin;
            get { return new ContentBoxRect(marginTop, marginRight, marginBottom, marginLeft); }
            set { SetMargin(value); }
        }

        [PublicAPI]
        public float marginLeft {
            get { return FindActiveStyle((s) => s.margin.left != FloatUtil.UnsetFloatValue).margin.left; }
            set { SetMarginLeft(value); }
        }

        [PublicAPI]
        public float marginRight {
            get { return FindActiveStyle((s) => s.margin.right != FloatUtil.UnsetFloatValue).margin.right; }
            set { SetMarginRight(value); }
        }

        [PublicAPI]
        public float marginTop {
            get { return FindActiveStyle((s) => s.margin.top != FloatUtil.UnsetFloatValue).margin.top; }
            set { SetMarginTop(value); }
        }

        [PublicAPI]
        public float marginBottom {
            get { return FindActiveStyle((s) => s.margin.bottom != FloatUtil.UnsetFloatValue).margin.bottom; }
            set { SetMarginBottom(value); }
        }

        [PublicAPI]
        public float GetMarginLeft(StyleState state) {
            return GetStyle(state).margin.left;
        }

        [PublicAPI]
        public float GetMarginRight(StyleState state) {
            return GetStyle(state).margin.right;
        }

        [PublicAPI]
        public float GetMarginTop(StyleState state) {
            return GetStyle(state).margin.top;
        }

        [PublicAPI]
        public float GetMarginBottom(StyleState state) {
            return GetStyle(state).margin.bottom;
        }

        [PublicAPI]
        public void SetMargin(ContentBoxRect value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).margin = value;
            activeStyles.margin = margin;
            changeHandler.SetMargin(elementId, margin);
        }

        [PublicAPI]
        public ContentBoxRect GetMargin(StyleState state = StyleState.Normal) {
            return GetStyle(state).margin;
        }

        [PublicAPI]
        public void SetMarginLeft(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).margin.left = value;
            // activeStyle.marginLeft = FindActiveStyle() => .marginLeft;
            if (marginLeft == value) {
                changeHandler.SetMargin(elementId, margin);
            }
        }

        [PublicAPI]
        public void SetMarginRight(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).margin.right = value;
            if (marginRight == value) {
                changeHandler.SetMargin(elementId, margin);
            }
        }

        [PublicAPI]
        public void SetMarginTop(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).margin.top = value;
            if (marginTop == value) {
                changeHandler.SetMargin(elementId, margin);
            }
        }

        [PublicAPI]
        public void SetMarginBottom(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).margin.bottom = value;
            if (marginBottom == value) {
                changeHandler.SetMargin(elementId, margin);
            }
        }

        [PublicAPI]
        public ContentBoxRect padding {
            get { return new ContentBoxRect(paddingTop, paddingRight, paddingBottom, paddingLeft); }
            set { SetPadding(value); }
        }

        [PublicAPI]
        public float paddingLeft {
            get { return FindActiveStyle((s) => s.padding.left != FloatUtil.UnsetFloatValue).padding.left; }
            set { SetPaddingLeft(value); }
        }

        [PublicAPI]
        public float paddingRight {
            get { return FindActiveStyle((s) => s.padding.right != FloatUtil.UnsetFloatValue).padding.right; }
            set { SetPaddingRight(value); }
        }

        [PublicAPI]
        public float paddingTop {
            get { return FindActiveStyle((s) => s.padding.top != FloatUtil.UnsetFloatValue).padding.top; }
            set { SetPaddingTop(value); }
        }

        [PublicAPI]
        public float paddingBottom {
            get { return FindActiveStyle((s) => s.padding.bottom != FloatUtil.UnsetFloatValue).padding.bottom; }
            set { SetPaddingBottom(value); }
        }

        [PublicAPI]
        public ContentBoxRect GetPadding(StyleState state = StyleState.Normal) {
            return GetStyle(state).padding;
        }

        [PublicAPI]
        public float GetPaddingLeft(StyleState state) {
            return GetStyle(state).padding.left;
        }

        [PublicAPI]
        public float GetPaddingRight(StyleState state) {
            return GetStyle(state).padding.right;
        }

        [PublicAPI]
        public float GetPaddingTop(StyleState state) {
            return GetStyle(state).padding.top;
        }

        [PublicAPI]
        public float GetPaddingBottom(StyleState state) {
            return GetStyle(state).padding.bottom;
        }

        [PublicAPI]
        public void SetPadding(ContentBoxRect value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).padding = value;
            changeHandler.SetPadding(elementId, padding);
        }

        [PublicAPI]
        public void SetPaddingLeft(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).padding.left = value;
            if (paddingLeft == value) {
                changeHandler.SetPadding(elementId, padding);
            }
        }

        [PublicAPI]
        public void SetPaddingRight(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).padding.right = value;
            if (paddingRight == value) {
                changeHandler.SetPadding(elementId, padding);
            }
        }

        [PublicAPI]
        public void SetPaddingTop(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).padding.top = value;
            if (paddingTop == value) {
                changeHandler.SetPadding(elementId, padding);
            }
        }

        [PublicAPI]
        public void SetPaddingBottom(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).padding.bottom = value;
            if (paddingBottom == value) {
                changeHandler.SetPadding(elementId, padding);
            }
        }

        [PublicAPI]
        public ContentBoxRect border {
            get { return new ContentBoxRect(borderTop, borderRight, borderBottom, borderLeft); }
            set { SetBorder(value); }
        }

        [PublicAPI]
        public float borderLeft {
            get { return FindActiveStyle((s) => s.border.left != FloatUtil.UnsetFloatValue).border.left; }
            set { SetBorderLeft(value); }
        }

        [PublicAPI]
        public float borderRight {
            get { return FindActiveStyle((s) => s.border.right != FloatUtil.UnsetFloatValue).border.right; }
            set { SetBorderRight(value); }
        }

        [PublicAPI]
        public float borderTop {
            get { return FindActiveStyle((s) => s.border.top != FloatUtil.UnsetFloatValue).border.top; }
            set { SetBorderTop(value); }
        }

        [PublicAPI]
        public float borderBottom {
            get { return FindActiveStyle((s) => s.border.bottom != FloatUtil.UnsetFloatValue).border.bottom; }
            set { SetBorderBottom(value); }
        }

        // in these cases we just set the whole value because we might be in a style state
        // where only part of the content box is affected by a set call
        [PublicAPI]
        public void SetBorder(ContentBoxRect value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).border = value;
            changeHandler.SetBorder(elementId, border);
        }

        [PublicAPI]
        public ContentBoxRect GetBorder(StyleState state) {
            return GetStyle(state).border;
        }

        [PublicAPI]
        public float GetBorderLeft(StyleState state) {
            return GetStyle(state).border.left;
        }

        [PublicAPI]
        public float GetBorderRight(StyleState state) {
            return GetStyle(state).border.right;
        }

        [PublicAPI]
        public float GetBorderTop(StyleState state) {
            return GetStyle(state).border.top;
        }

        [PublicAPI]
        public float GetBorderBottom(StyleState state) {
            return GetStyle(state).border.bottom;
        }

        [PublicAPI]
        public void SetBorderLeft(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).border.left = value;
            if (borderLeft == value) {
                changeHandler.SetBorder(elementId, border);
            }
        }

        [PublicAPI]
        public void SetBorderRight(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).border.right = value;
            if (borderRight == value) {
                changeHandler.SetBorder(elementId, border);
            }
        }

        [PublicAPI]
        public void SetBorderTop(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).border.top = value;
            if (borderRight == value) {
                changeHandler.SetBorder(elementId, border);
            }
        }

        [PublicAPI]
        public void SetBorderBottom(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).border.bottom = value;
            if (borderBottom == value) {
                changeHandler.SetBorder(elementId, border);
            }
        }

    }

}