using System;

namespace UIForia {

    [Serializable]
    public struct PropertyData {

        public long value;

        public PropertyData(long longVal) {
            this.value = longVal;
        }

    }

}