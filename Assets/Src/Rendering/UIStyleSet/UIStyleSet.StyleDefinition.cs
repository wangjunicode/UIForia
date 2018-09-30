using System.Collections.Generic;

namespace Rendering {

    public partial class UIStyleSet {

        private struct StyleEntry {

            public readonly UIStyle style;
            public readonly StyleState state;
            public readonly StyleType type;
            public readonly int priority;

            //style number is used to prioritize shared styles, higher numbers are less important
            public StyleEntry(UIStyle style, StyleType type, StyleState state, int styleNumber = -1) {
                this.style = style;
                this.type = type;
                this.state = state;
                this.priority = GetSortPriority(type, state, styleNumber);
            }

            private static int GetSortPriority(StyleType type, StyleState state, int styleNumber) {
                int retn = 0;

                if ((type & StyleType.Default) != 0) {
                    return -10000;
                }

                if ((type & StyleType.Instance) != 0) {
                    retn += 10000;
                }
                else {
                    retn += styleNumber;
                }

                if (state != StyleState.Normal) {
                    retn += 500;

                    if ((state & StyleState.Hover) != 0) {
                        retn += 50;
                    }

                    else if ((state & StyleState.Active) != 0) {
                        retn += 40;
                    }

                    else if ((state & StyleState.Disabled) != 0) {
                        retn += 30;
                    }

                    else if ((state & StyleState.Focused) != 0) {
                        retn += 20;
                    }
                }

                return retn;
            }

        }

    }

}