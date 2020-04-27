using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia {

    public unsafe struct StyleRebuildJob : IJob {

        public UnmanagedPagedList<StyleProperty2> propertyOutput;
        public UnmanagedPagedList<StyleRebuildResult> rebuildList;

        public void Execute() {

            UnsafeSpan<StyleProperty2> instanceStyles = default;
            UnsafeSpan<StyleProperty2> selectorStyles = default;
            UnsafeSpan<StyleProperty2> sharedStyles = default;

            UnmanagedList<StyleProperty2> propertyBuffer = new UnmanagedList<StyleProperty2>(256, Allocator.TempJob);

            BitBuffer256 buffer = new BitBuffer256();
            IntBoolMap map = new IntBoolMap(buffer.ptr, buffer.bitCount);

            // animated styles need to be accounted for
            // transitions are low priority animations
            // animations always run, they just have differing priority and output of highest priority is taken.
            
            // i think we need all of these to build before the animations are combined into here 
            // because animations win (i think, might have to check against instance properties but should be simple since we already have a list of those)
            
            for (int i = 0; i < instanceStyles.size; i++) {
                map[instanceStyles.array[i].propertyId.index] = true;
                propertyBuffer.AddUnchecked(instanceStyles.array[i]); // todo -- can be memcpy    
            }

            for (int i = 0; i < selectorStyles.size; i++) {
                if (map.TrySetIndex(selectorStyles.array[i].propertyId.index)) {
                    propertyBuffer.AddUnchecked(selectorStyles.array[i]);
                }
            }

            for (int i = 0; i < sharedStyles.size; i++) {
                if (map.TrySetIndex(sharedStyles.array[i].propertyId.index)) {
                    propertyBuffer.AddUnchecked(sharedStyles.array[i]);
                }
            }

            propertyBuffer.Sort(); // maybe linear search is better for small lists since we'll have almost no cache misses. if this is true then we want to split keys + data into different sections

            // need to put this output somewhere
            propertyOutput.AddRange(propertyBuffer.GetRawPointer(), propertyBuffer.size);

            propertyBuffer.Dispose();

        }

    }

}