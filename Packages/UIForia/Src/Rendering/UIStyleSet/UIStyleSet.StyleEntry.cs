
namespace UIForia.Rendering {

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
                    retn = 10000;
                }
                else if ((type & StyleType.Implicit) != 0) {
                    retn = 100000;
                }
                else if((type & StyleType.Shared) != 0) {
                    retn = styleNumber;
                }

                int bonus = 0;
                if (state != StyleState.Normal) {
                    bonus += 500;

                    if ((state & StyleState.Focused) != 0) {
                        bonus += 60;
                    }
                    
                    else if ((state & StyleState.Hover) != 0) {
                        bonus += 50;
                    }

                    else if ((state & StyleState.Active) != 0) {
                        bonus += 40;
                    }

                    else if ((state & StyleState.Inactive) != 0) {
                        bonus += 30;
                    }
                   
                }

                return retn + bonus;
            }

        }

    }

}