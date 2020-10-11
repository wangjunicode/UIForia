using System.Runtime.InteropServices;
using UIForia.ListTypes;

namespace UIForia.Style {

    internal enum StyleInfoFlags { }

    internal unsafe struct StyleData {
        
        public int propertyCount;

        public int bufferCapacity;
        public void* buffer;
        
        public PropertyId* propertyKeys => (PropertyId*) buffer;
        public PropertyData* propertyValues => (PropertyData*) (propertyKeys + propertyCount);
        public PropertyId* propertyFlags => (PropertyId*) buffer;

    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct StyleList {

        [FieldOffset(0)] public fixed uint styleIdsInline[4];
        [FieldOffset(0)] public List_StyleId styleIdList;

    }

    internal struct InstanceStyle {

        public List_StyleProperty properties;
        public StyleSystem2.AnimationInfo* animationInfo;
        public StyleJobs.StyleTransition transitions;

    }

    internal unsafe struct StyleInfo {

        public bool requiresAttributeCheck;
        public bool requiresStateUpdate;

        public InstanceStyle* instanceStyle;
        public StyleInfoFlags flags;
        public StyleList styleList;

        // todo -- make this a block-allocated list
        // some of these lesser used ones like transitions can probably be stored differently
        // previous properties can probably also be held elsewhere 
        // maybe we dont need to store parts? can be implicit in contributors
        // can diff by scanning contributors and checking for type == style part

        public AllocatedList_StylePart styleParts;
        public AllocatedList_StylePropertyInfo styleProperties;
        public AllocatedList_StylePropertyInfo previousProperties; // can be reduced to a range into paged buffer
        public AllocatedList_Contributor styleContributors;
        public AllocatedList_Transition transitionList;

        public bool needsStyleRebuild;
        public StyleSystem2.StyleInfoUpdate updateFlags;
        public int changeSetId;
        public StyleState2 state;

        public bool HasAnimations { get; }
        public bool HasTransitions { get; set; }
        public bool HasPropertyChanges { get; set; }
        public bool HasOnlyValueChanges { get; set; }

    }

    internal unsafe struct AllocatedList_Transition {

        public int size;
        public int capacity;
        public StyleJobs.StyleTransition* array;

    }

    public unsafe struct AllocatedList_StylePropertyInfo {

        public int size;
        public int capacity;
        public StylePropertyInfo* array;

    }

    public unsafe struct AllocatedList_Contributor {

        public int size;
        public int capacity;
        public Contributor* array;

    }

    public unsafe struct AllocatedList_StylePart {

        public int size;
        public int capacity;
        public StylePart* array;

    }

}