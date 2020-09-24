using System;
using System.Collections.Generic;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia {

    public unsafe struct StringHandle {

        public int index;
        public readonly int length;
        public readonly AllocatorId capacity;
        public readonly ushort* data;

        public StringHandle(int index, AllocatorId allocatorId, int length, ushort* data) {
            this.index = index;
            this.capacity = allocatorId;
            this.length = length;
            this.data = data;
        }

    }

    /// <summary>
    /// This is a system that handles uniquely identifying strings. This is needed for 2 reasons. Firstly for UIForia to work in
    /// burst we cannot use strings in any of our burst data types. Secondly, style keys are stored using a combination of
    /// stringId from style sheet and stringId from the style itself. These Ids must be unique. We could have just used a
    /// dictionary(string, styleId) for this lookup but concatenation strings causes an allocation that I want to avoid.
    /// Its also just faster to use int keys.
    ///
    /// The main reason for this however is element attributes and how they get used in selectors. Selectors need to be really
    /// fast at matching/rejecting candidate elements. In the attribute case this means comparing strings, which we can't do
    /// in burst. So the string intern system is the solution. Instead of comparing strings we just pre-cache all the
    /// strings that will be used in the frame and assign them all an id that is unique to that string. Then we have
    /// an equality check that is just comparing integers.
    ///
    /// The kink here is that we also need to support AttributeStartsWith/Contains/EndsWith. The solution is to
    /// keep an indexed list of 'burstable' string values (a StringHandle) which is a pointer to the string content represented 
    /// as ushort (which is the size of a character in C#, 2 bytes)
    ///
    /// So now we can hold a string table with values, but of course we always add to the table and never remove then
    /// we grow memory infinitely and its easy to imagine pathological cases like element.SetAttribute("frameId", runtime.frameId.ToString())
    /// To fix this we ref count our strings which are not constant. Constant string come from the style and template compilers, we
    /// know certain strings will always be present for the lifetime of the application. Others are not, such as the "frameid" example
    /// above. For this case we remove a string entry when its ref count goes to 0.
    ///
    /// Internally refcount will be equal to int.MinValue if a string is constant, otherwise some positive integer. When refcount is 0,
    /// we remove the string and release its memory, then recycle the index.
    /// </summary>
    public unsafe class StringInternSystem : IDisposable {

        public Pow2AllocatorSet allocatorSet;
        private Dictionary<string, StringInfo> internMap;
        public DataList<StringHandle>.Shared burstValues;
        public LightList<string> stringValues;
        public StructList<int> freeIndices;
        private int idGenerator;

        public StringInternSystem() {
            FixedAllocatorDesc* fixedAllocatorDesc = stackalloc FixedAllocatorDesc[5];
            fixedAllocatorDesc[0] = new FixedAllocatorDesc(16, 256, 1);
            fixedAllocatorDesc[1] = new FixedAllocatorDesc(32, 128, 1);
            fixedAllocatorDesc[2] = new FixedAllocatorDesc(64, 32, 1);
            fixedAllocatorDesc[3] = new FixedAllocatorDesc(128, 4, 0);
            fixedAllocatorDesc[4] = new FixedAllocatorDesc(256, 4, 0);
            this.allocatorSet = new Pow2AllocatorSet(fixedAllocatorDesc, 5);
            this.internMap = new Dictionary<string, StringInfo>();
            this.freeIndices = new StructList<int>(32);
            this.burstValues = new DataList<StringHandle>.Shared(128, Allocator.Persistent);
            this.stringValues = new LightList<string>(128);
            Debug = this;
        }

        public int AddConstant(string value) {
            return Add(value, true);
        }

        public int Add(string value) {
            return Add(value, false);
        }

        public int AddWithoutBurst(string value) {
            return AddWithoutBurst(value, false);
        }

        public int AddConstWithoutBurst(string value) {
            return AddWithoutBurst(value, true);
        }

        private int AddWithoutBurst(string value, bool isConstant) {
            if (internMap.TryGetValue(value, out StringInfo info)) {
                // reference count will be negative if string is constant
                // I really wish dictionary could return a ref here
                if (info.referenceCount > 0) {
                    info.referenceCount++;
                    internMap[value] = info;
                }

                return info.idx;
            }

            int idx;
            if (freeIndices.size > 0) {
                idx = freeIndices.array[0];
                freeIndices.SwapRemoveAt(0);
            }
            else {
                // if we need a new id then make sure we can support it
                idx = idGenerator++;
                stringValues.EnsureCapacity(idGenerator);
                burstValues.EnsureCapacity(idGenerator);
            }

            internMap.Add(value, new StringInfo() {
                idx = idx,
                referenceCount = isConstant ? int.MinValue : 1
            });

            // still writes to burst index but wont allocate
            stringValues.array[idx] = value;
            burstValues[idx] = default;

            return idx;
        }

        private void AssertHasBurstValue(ref StringInfo info, string value) {
            if (info.hasBurst == false) {
                AllocatorId allocatorId = allocatorSet.AllocateBlock(value.Length * 2, out void* ptr);

                // update our lookup values
                burstValues[info.idx] = new StringHandle(info.idx, allocatorId, value.Length, (ushort*) ptr);

                // copy the string content into the string handle pointer
                fixed (char* charptr = value) {
                    TypedUnsafe.MemCpy((ushort*) ptr, (ushort*) charptr, value.Length);
                }
            }
        }

        private int Add(string value, bool isConstant) {
            if (internMap.TryGetValue(value, out StringInfo info)) {
                // reference count will be negative if string is constant
                // I really wish dictionary could return a ref here
                if (info.referenceCount > 0) {
                    info.referenceCount++;
                    AssertHasBurstValue(ref info, value);
                    internMap[value] = info;
                }
                else if (!info.hasBurst) {
                    AssertHasBurstValue(ref info, value);
                    internMap[value] = info;
                }

                return info.idx;
            }

            int idx;
            if (freeIndices.size > 0) {
                idx = freeIndices.array[0];
                freeIndices.SwapRemoveAt(0);
            }
            else {
                // if we need a new id then make sure we can support it
                idx = idGenerator++;
                stringValues.EnsureCapacity(idGenerator);
                burstValues.EnsureCapacity(idGenerator);
            }

            internMap.Add(value, new StringInfo() {
                idx = idx,
                referenceCount = isConstant ? int.MinValue : 1
            });

            // allocate a byte block for the string content
            AllocatorId allocatorId = allocatorSet.AllocateBlock(value.Length * 2, out void* ptr);

            // update our lookup values
            stringValues.array[idx] = value;
            burstValues[idx] = new StringHandle(idx, allocatorId, value.Length, (ushort*) ptr);

            // copy the string content into the string handle pointer
            fixed (char* charptr = value) {
                TypedUnsafe.MemCpy((ushort*) ptr, (ushort*) charptr, value.Length);
            }

            return idx;
        }

        public void Remove(int idx) {
            string toRemove = stringValues.array[idx];
            Remove(toRemove);
        }

        public void Remove(string value) {
            // if value not found or the ref count is negative (ie string was constant) then early out
            if (value == null || !internMap.TryGetValue(value, out StringInfo info) || info.referenceCount < 0) {
                return;
            }

            info.referenceCount--;

            // if ref count hits 0 we need to nuke the string and free up that memory
            if (info.referenceCount == 0) {
                freeIndices.Add(info.idx);
                internMap.Remove(value);

                if (burstValues[info.idx].data != null) {
                    allocatorSet.Free(burstValues[info.idx].data, burstValues[info.idx].capacity);
                }

                burstValues[info.idx] = default;
                stringValues[info.idx] = default;
            }
        }

        public string GetString(int valueIndex) {
            return stringValues.array[valueIndex];
        }

        public void Dispose() {
            allocatorSet.Dispose();
            burstValues.Dispose();
        }

        public int GetRefCount(string value) {
            if (internMap.TryGetValue(value, out StringInfo info)) {
                return info.referenceCount;
            }

            return 0;
        }

        public StringHandle GetStringHandle(int idx) {
            return burstValues[idx];
        }

        private struct StringInfo {

            public int idx;
            public int referenceCount;
            public bool hasBurst;

        }

        public int GetIndex(string value) {
            if (internMap.TryGetValue(value, out StringInfo info)) {
                return info.idx;
            }

            return -1;
        }

        internal static StringInternSystem Debug;

    }

}