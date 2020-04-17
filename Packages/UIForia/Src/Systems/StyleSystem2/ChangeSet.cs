using UIForia.Rendering;
using UIForia.Style;
using UIForia.Util;

namespace UIForia {

    internal struct ChangeSet {

        internal StyleState state;

        internal StructList<StylePropertyUpdate> instanceUpdates;
        internal StructList<StyleGroup2> sharedStyles;
        public StyleSet styleSet;

        public void SetInstanceProperty(in StyleProperty2 property, StyleState state) {
            instanceUpdates = instanceUpdates ?? new StructList<StylePropertyUpdate>();
            for (int i = 0; i < instanceUpdates.size; i++) {
                ref StylePropertyUpdate update = ref instanceUpdates.array[i];
                if (update.property.propertyId == property.propertyId && update.state == state) {
                    update.property = property;
                    return;
                }
            }

            instanceUpdates.Add(new StylePropertyUpdate() {
                property = property,
                state = state
            });
        }

        public void SetSharedStyleGroups(StructList<StyleGroup2> updateGroups) {
            sharedStyles = sharedStyles ?? new StructList<StyleGroup2>(updateGroups.size);
            sharedStyles.EnsureCapacity(updateGroups.size);
            updateGroups.CopyToArrayUnchecked(sharedStyles.array);
            sharedStyles.size = updateGroups.size;
        }
        
    }

}