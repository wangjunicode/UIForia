using UIForia.Elements;

namespace UIForia.Compilers {

    public readonly struct CompileTimeSlotUsage {

        public readonly string slotName;
        public readonly string variableName;
        public readonly int slotId;

        public CompileTimeSlotUsage(string slotName, int slotId, string variableName) {
            this.slotName = slotName;
            this.slotId = slotId;
            this.variableName = variableName;
        }


    }

    public readonly struct SlotUsage {

        public readonly string slotName;
        public readonly int slotId;
        public readonly UIElement outerContext;
        
        public SlotUsage(string slotName, int slotId, UIElement outerContext) {
            this.slotName = slotName;
            this.slotId = slotId;
            this.outerContext = outerContext;
        }

    }

}