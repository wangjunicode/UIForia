using System.Collections.Generic;

namespace UIForia.Util {

    public static class LightListPool<T> {

        private static readonly Stack<LightList<T>> s_LightListPool = new Stack<LightList<T>>();
        private static readonly HashSet<LightList<T>> s_Contained = new HashSet<LightList<T>>();
        
        public static LightList<T> Get() {
            if (s_LightListPool.Count > 0) {
                s_Contained.Remove(s_LightListPool.Peek());
                return s_LightListPool.Pop();
            }

            return new LightList<T>();
        }

        public static void Release(ref LightList<T> toRelease) {
            
            if (toRelease == null || s_Contained.Contains(toRelease)) {
                return;
            }

            s_Contained.Add(toRelease);
            s_LightListPool.Push(toRelease);
            toRelease.Clear();
            toRelease = null;
        }

    }

}