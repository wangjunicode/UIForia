using System;

namespace UIForia.Style {

    internal enum StyleVariableState {

        Default,
        Added,
        Changed,
        Removed

    }

    internal struct TVARIABLETYPE {

        public ElementId elementId;
        public ushort variableNameId;
        public ushort enumTypeId;

        public StyleVariableState state;
        public TraversalInfo traversalInfo;

        public bool TryConvertToTTEMPLATE(out TTEMPLATE ttemplate) {
            throw new NotImplementedException();
        }

    }

    public struct TTEMPLATE : IEquatable<TTEMPLATE> {

        public bool Equals(TTEMPLATE other) {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj) {
            return obj is TTEMPLATE other && Equals(other);
        }

        public override int GetHashCode() {
            throw new NotImplementedException();
        }

    }

}