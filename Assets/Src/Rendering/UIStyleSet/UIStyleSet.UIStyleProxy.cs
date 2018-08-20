using UnityEngine;

namespace Rendering {

    public partial class UIStyleSet {

        public struct UIStyleProxy {

            private readonly UIStyleSet styleSet;
            private readonly StyleState state;

            internal static UIStyle hack;

            public UIStyleProxy(UIStyleSet styleSet, StyleState state) {
                this.styleSet = styleSet;
                this.state = state;
            }

            public Color backgroundColor {
                get { return styleSet.GetBackgroundColor(state); }
                set { styleSet.SetBackgroundColor(value, state); }
            }

            public Texture2D backgroundImage {
                get { return styleSet.GetBackgroundImage(state); }
                set { styleSet.SetBackgroundImage(value, state); }
            }

            public Color borderColor {
                get { return styleSet.GetBorderColor(state); }
                set { styleSet.SetBorderColor(value, state); }
            }

            public ContentBoxRect margin {
                get { return styleSet.GetMargin(state); }
                set { styleSet.SetMargin(value, state); }
            }

            public ContentBoxRect padding {
                get { return styleSet.GetPadding(state); }
                set { styleSet.SetPadding(value, state); }
            }

            public ContentBoxRect border {
                get { return styleSet.GetBorder(state); }
                set { styleSet.SetBorder(value, state); }
            }

            public static implicit operator UIStyleProxy(UIStyle style) {
                UIStyleProxy proxy = new UIStyleProxy(null, StyleState.Normal);
                hack = style;
                return proxy;
            }

        }

    }

}