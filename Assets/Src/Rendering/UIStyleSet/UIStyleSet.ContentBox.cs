namespace Rendering {

    public partial class UIStyleSet {

        public ContentBoxRect margin {
            get { return new ContentBoxRect(marginTop, marginRight, marginBottom, marginLeft); }
            set { SetMargin(value); }
        }

        public float marginLeft {
            get { return FindActiveStyle((s) => s.margin.left != UIStyle.UnsetFloatValue).margin.left; }
            set { SetMarginLeft(value); }
        }

        public float marginRight {
            get { return FindActiveStyle((s) => s.margin.right != UIStyle.UnsetFloatValue).margin.right; }
            set { SetMarginRight(value); }
        }

        public float marginTop {
            get { return FindActiveStyle((s) => s.margin.top != UIStyle.UnsetFloatValue).margin.top; }
            set { SetMarginTop(value); }
        }

        public float marginBottom {
            get { return FindActiveStyle((s) => s.margin.bottom != UIStyle.UnsetFloatValue).margin.bottom; }
            set { SetMarginBottom(value); }
        }

        public float GetMarginLeft(StyleState state) {
            return GetStyle(state).margin.left;
        }

        public float GetMarginRight(StyleState state) {
            return GetStyle(state).margin.right;
        }

        public float GetMarginTop(StyleState state) {
            return GetStyle(state).margin.top;
        }

        public float GetMarginBottom(StyleState state) {
            return GetStyle(state).margin.bottom;
        }

        public void SetMargin(ContentBoxRect margin, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).margin = margin;
            if (this.margin != margin) {
                view.layoutSystem.SetMargin(element, margin);
            }
        }

        public ContentBoxRect GetMargin(StyleState state = StyleState.Normal) {
            return GetStyle(state).margin;
        }

        public void SetMarginLeft(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).margin.left = value;
            ContentBoxRect rect = margin;
            if (rect.left == value) {
                view.layoutSystem.SetMargin(element, rect);
            }
        }

        public void SetMarginRight(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).margin.right = value;
            ContentBoxRect rect = margin;
            if (rect.right == value) {
                view.layoutSystem.SetMargin(element, rect);
            }
        }

        public void SetMarginTop(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).margin.top = value;
            ContentBoxRect rect = margin;
            if (rect.top == value) {
                view.layoutSystem.SetMargin(element, rect);
            }
        }

        public void SetMarginBottom(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).margin.bottom = value;
            ContentBoxRect rect = margin;
            if (rect.bottom == value) {
                view.layoutSystem.SetMargin(element, rect);
            }
        }

        public ContentBoxRect padding {
            get { return new ContentBoxRect(paddingTop, paddingRight, paddingBottom, paddingLeft); }
            set { SetPadding(value); }
        }

        public float paddingLeft {
            get { return FindActiveStyle((s) => s.padding.left != UIStyle.UnsetFloatValue).padding.left; }
            set { SetPaddingLeft(value); }
        }

        public float paddingRight {
            get { return FindActiveStyle((s) => s.padding.right != UIStyle.UnsetFloatValue).padding.right; }
            set { SetPaddingRight(value); }
        }

        public float paddingTop {
            get { return FindActiveStyle((s) => s.padding.top != UIStyle.UnsetFloatValue).padding.top; }
            set { SetPaddingTop(value); }
        }

        public float paddingBottom {
            get { return FindActiveStyle((s) => s.padding.bottom != UIStyle.UnsetFloatValue).padding.bottom; }
            set { SetPaddingBottom(value); }
        }

        public ContentBoxRect GetPadding(StyleState state = StyleState.Normal) {
            return GetStyle(state).padding;
        }

        public float GetPaddingLeft(StyleState state) {
            return GetStyle(state).padding.left;
        }

        public float GetPaddingRight(StyleState state) {
            return GetStyle(state).padding.right;
        }

        public float GetPaddingTop(StyleState state) {
            return GetStyle(state).padding.top;
        }

        public float GetPaddingBottom(StyleState state) {
            return GetStyle(state).padding.bottom;
        }

        public void SetPadding(ContentBoxRect padding, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).padding = padding;
            if (this.padding == padding) {
                view.layoutSystem.SetPadding(element, padding);
            }
        }

        public void SetPaddingLeft(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).padding.left = value;
            ContentBoxRect rect = padding;
            if (rect.left == value) {
                view.layoutSystem.SetPadding(element, rect);
            }
        }

        public void SetPaddingRight(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).padding.right = value;
            ContentBoxRect rect = padding;
            if (rect.right == value) {
                view.layoutSystem.SetPadding(element, rect);
            }
        }

        public void SetPaddingTop(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).padding.top = value;
            ContentBoxRect rect = padding;
            if (rect.top == value) {
                view.layoutSystem.SetPadding(element, rect);
            }
        }

        public void SetPaddingBottom(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).padding.bottom = value;
            ContentBoxRect rect = padding;
            if (rect.bottom == value) {
                view.layoutSystem.SetPadding(element, rect);
            }
        }

        public ContentBoxRect border {
            get { return new ContentBoxRect(borderTop, borderRight, borderBottom, borderLeft); }
            set { SetBorder(value); }
        }

        public float borderLeft {
            get { return FindActiveStyle((s) => s.border.left != UIStyle.UnsetFloatValue).border.left; }
            set { SetBorderLeft(value); }
        }

        public float borderRight {
            get { return FindActiveStyle((s) => s.border.right != UIStyle.UnsetFloatValue).border.right; }
            set { SetBorderRight(value); }
        }

        public float borderTop {
            get { return FindActiveStyle((s) => s.border.top != UIStyle.UnsetFloatValue).border.top; }
            set { SetBorderTop(value); }
        }

        public float borderBottom {
            get { return FindActiveStyle((s) => s.border.bottom != UIStyle.UnsetFloatValue).border.bottom; }
            set { SetBorderBottom(value); }
        }

        public void SetBorder(ContentBoxRect border, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).border = border;
            if (this.border == border) {
                view.layoutSystem.SetBorder(element, border);
                view.renderSystem.OnElementStyleChanged(element);
            }
        }

        public ContentBoxRect GetBorder(StyleState state) {
            return GetStyle(state).border;
        }

        public float GetBorderLeft(StyleState state) {
            return GetStyle(state).border.left;
        }

        public float GetBorderRight(StyleState state) {
            return GetStyle(state).border.right;
        }

        public float GetBorderTop(StyleState state) {
            return GetStyle(state).border.top;
        }

        public float GetBorderBottom(StyleState state) {
            return GetStyle(state).border.bottom;
        }

        public void SetBorderLeft(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).border.left = value;
            ContentBoxRect rect = border;
            if (rect.left == value) {
                view.layoutSystem.SetBorder(element, rect);
            }
        }

        public void SetBorderRight(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).border.right = value;
            ContentBoxRect rect = border;
            if (rect.right == value) {
                view.layoutSystem.SetBorder(element, rect);
            }
        }

        public void SetBorderTop(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).border.top = value;
            ContentBoxRect rect = border;
            if (rect.top == value) {
                view.layoutSystem.SetBorder(element, rect);
            }
        }

        public void SetBorderBottom(float value, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).border.bottom = value;
            ContentBoxRect rect = border;
            if (rect.bottom == value) {
                view.layoutSystem.SetBorder(element, rect);
            }
        }

    }

}