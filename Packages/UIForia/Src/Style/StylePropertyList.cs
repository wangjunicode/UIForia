using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Style {

    public unsafe partial struct StylePropertyList {

        private readonly ElementId elementId;
        private readonly UIApplication application;

        internal StylePropertyList(ElementId elementId, UIApplication application) {
            this.elementId = elementId;
            this.application = application;
        }

        public int PropertyCount {
            get {
                if (!application.IsElementAlive(elementId)) return 0;
                ref InstancePropertyInfo instanceInfo = ref application.instancePropertyTable[elementId.index];
                return instanceInfo.listSlice.length;
            }
        }

        internal ushort ResolveVariableId(string variableName) {
            return application.ResolveVariableName(variableName).value;
        }

        internal void SetProperty(ushort propertyIndex, ushort variableNameId, void* value, int size) {

            if (!application.IsElementAlive(elementId)) return;

            PropertyContainer[] properties = application.instancePropertyAllocator.memory;
            ref InstancePropertyInfo instanceInfo = ref application.instancePropertyTable[elementId.index];
            int start = instanceInfo.listSlice.start;
            int end = start + instanceInfo.listSlice.length;

            int index = -1;

            for (int i = start; i < end; i++) {

                if (properties[i].propertyIndex != propertyIndex) {
                    continue;
                }

                index = i;
                break;

            }

            if (index != -1) {
                fixed (void* ptr = properties[index].bytes) {
                    if (variableNameId == properties[index].variableNameId && UnsafeUtility.MemCmp(value, ptr, size) == 0) {
                        return;
                    }
                }
            }

            PropertyContainer propertyContainer = new PropertyContainer() {
                elementId = elementId,
                propertyIndex = propertyIndex,
                variableNameId = variableNameId,
            };

            UnsafeUtility.MemCpy(propertyContainer.bytes, value, size);

            if (index != -1) {
                properties[index] = propertyContainer;
            }
            else {
                application.instancePropertyAllocator.Add(ref instanceInfo.listSlice, propertyContainer);
            }


        }

    }

}