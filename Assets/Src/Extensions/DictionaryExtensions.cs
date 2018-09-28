using System.Collections.Generic;

namespace Src.Extensions {

    public static class DictionaryExtensions {

        public static U GetOrDefault<T, U>(this Dictionary<T, U> self, T key) {
            U retn;
            if (self.TryGetValue(key, out retn)) {
                return retn;
            }

            return default(U);
        }

    }

}