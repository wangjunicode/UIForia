
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
                
                // instance        0000 0000 1111 1111 
                
                // shared + index  0000 0000 0000 0010
                
                // implicit styles 0000 0000 0000 0001
                
                // base            0000 0000 0000 0000 

                int baseBits = 0;
            
                // type = Selector
                
                
                if ((type & StyleType.Instance) != 0) {
                    baseBits = ushort.MaxValue;
                }
                else if ((type & StyleType.Implicit) != 0) {
                    baseBits = 1;
                }
                else if((type & StyleType.Shared) != 0) {
                    baseBits = 2 + styleNumber;
                }
                
                return BitUtil.SetHighLowBits((int) state, baseBits);
            }

        }

    }

}