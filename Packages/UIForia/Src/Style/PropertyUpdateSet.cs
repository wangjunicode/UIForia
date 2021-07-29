namespace UIForia.Style {

    internal unsafe struct PropertyUpdateSet {

        public PropertyUpdate* propertyUpdates;
        public CheckedArray<PropertyUpdate>* updateLists;

    }

    internal unsafe struct InstancePropertyUpdateSet {

        public PropertyContainer* buffer;
        public InstancePropertyUpdateList* updateLists;

    }

    internal unsafe struct  TransitionUpdateList {

        public int size;
        public PendingTransition* array;

    }
    
    internal unsafe struct TransitionUpdateSet {

        public PendingTransition* pendingTransitionBuffer;
        public TransitionUpdateList* updateLists;

    }

}