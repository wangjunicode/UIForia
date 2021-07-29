using UIForia.Style;
using UnityEngine;

namespace UIForia.Compilers {

    internal struct CompiledProperty {

        public PropertyKeyInfo propertyKey;

        /// <summary>
        /// This range is an index into <see cref="wd.propertyValueBuffer"/>
        /// </summary>
        public RangeInt valueRange;
        public int next;

        public CompiledProperty(PropertyId propertyId, RangeInt valueRange, ushort variableNameId = ushort.MaxValue) {
            this.next = 0;
            this.propertyKey = new PropertyKeyInfo(propertyId, variableNameId);
            this.valueRange = valueRange;
        }

    }

}