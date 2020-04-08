using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UIForia.Util {

    public class NotNullObjectEqualityComparer<T> : IEqualityComparer<T> where T : class {

        public static readonly NotNullObjectEqualityComparer<T> Instance = new NotNullObjectEqualityComparer<T>();
        
        public bool Equals(T x, T y) {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj) {
            return RuntimeHelpers.GetHashCode(obj);
        }

    }

}