using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public interface IToStringBuffer {

        public void ToStringBuffer(CharStringBuilder builder);

    }

    internal struct StringKey {

        public string output;
        public CharStringBuilder input;
        public int lastTouchedTime; // todo -- eviction strategy
        public int hash;

        public override int GetHashCode() {
            return hash;
        }

    }

    internal class StringKeyComparer : IEqualityComparer<StringKey> {

        public bool Equals(StringKey x, StringKey y) {
            if (x.input != null) {
                return x.input.EqualsString(y.output);
            }

            if (y.input != null) {
                return y.input.EqualsString(x.output);
            }

            // resizing or rehashing   
            return x.output == y.output;
        }

        public int GetHashCode(StringKey obj) {
            return obj.hash;
        }

    }

    public unsafe partial class StringMemoizer {

        internal CharStringBuilder buffer = new CharStringBuilder();
        
        private Dictionary<StringKey, string> map = new Dictionary<StringKey, string>(new StringKeyComparer());

        private StructList<StringKey> toRemoveList = new StructList<StringKey>();
        
        public void GarbageCollect(int frameCount, int threshold) {
            int timeout = frameCount - threshold;
            
            foreach (KeyValuePair<StringKey, string> kvp in map) {
                if (kvp.Key.lastTouchedTime < timeout) {
                    toRemoveList.Add(kvp.Key);
                }
            }

            for (int i = 0; i < toRemoveList.size; i++) {
                map.Remove(toRemoveList[i]);
            }

            toRemoveList.Clear();
            
        }
        
        private string Memoize() {

            fixed (char* c = buffer.characters) {

                StringKey key = new StringKey() {
                    input = buffer,
                    hash = Hash(c, buffer.size * 2),
                    lastTouchedTime = 0,
                    output = null
                };

                if (!map.TryGetValue(key, out string result)) {
                    result = buffer.ToString();
                    key.output = result;
                    key.input = null;
                    key.lastTouchedTime = Time.frameCount;

                    map[key] = result;
                }

                return result;
            }
        }

        private static int Hash(void* ptr, int bytes) {
            // djb2 - Dan Bernstein hash function
            // http://web.archive.org/web/20190508211657/http://www.cse.yorku.ca/~oz/hash.html
            byte* str = (byte*) ptr;
            ulong hash = 5381;
            while (bytes > 0) {
                ulong c = str[--bytes];
                hash = ((hash << 5) + hash) + c;
            }

            return (int) hash;
        }

    }

}