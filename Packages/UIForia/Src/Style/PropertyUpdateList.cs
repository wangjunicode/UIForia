namespace UIForia.Style {

    internal unsafe struct PropertyUpdateList {

        public int size;
        public PropertyUpdate* array;

    }
    
    internal unsafe struct InstancePropertyUpdateList {

        public int size;
        public PropertyContainer* array;

    }

}
