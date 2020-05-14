using System;
using Packages.UIForia.Util.Unsafe;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia {

    public class StringHashCollisionException : Exception {

        public StringHashCollisionException(string value, string collision)
            : base($"'{value}' has a hash collision with '{collision}'. This is considered a compile error, you must rename all occurrences of one of these to a value that does not collide.") { }

    }
    
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

    // can grow but never shrink
    // returns integer ids, can lookup value
    // static string repository for BurstChar strings
    public unsafe struct StaticStringTable : IDisposable {

        private static int idGenerator;

        private int id;
        private PagedList<ushort> buffer; // paged list means any pointers we give out are always valid
        private IntMap<RangeInt> stringOffsets;

        public static StaticStringTable Create(int initialBufferSize = 8192, Allocator allocator = Allocator.Persistent) {
            return new StaticStringTable {
                id = ++idGenerator,
                buffer = new PagedList<ushort>((uint) initialBufferSize, allocator),
                stringOffsets = new IntMap<RangeInt>(256, allocator)
            };
        }

        // todo -- probably want to switch to a StringId scheme
        // where StringId stringId = new StringId((byte)originTable, stringIndex);
        // string index = index into a pointer table. hashmap returns index into array
        // this way i can be sure the wrong table doesn't get asked for a key

        public BurstableString Get(int key) {
            if (stringOffsets.TryGetValue(key, out RangeInt index)) {
                // length shouldn't include null terminator
                return new BurstableString(id, index.length - 1, buffer.GetPointer(index.start));
            }

            return default;
        }

        public int GetOrCreateReference(string value) {

            if (string.IsNullOrEmpty(value)) {
                return -1;
            }

            int hash = MurmurHash3.Hash(value);

            if (stringOffsets.TryGetValue(hash, out RangeInt index)) {
                // assert value is equal or we fail with compile error
                // this will happen if two strings have the same hash but different values (ie collision)

                // first ushort is the length
                ushort* content = buffer.GetPointer(index.start);

                fixed (char* charptr = value) {
                    // + 1 for null terminator
                    if (index.length != value.Length + 1 || UnsafeUtility.MemCmp(charptr, content, value.Length) != 0) {
                        throw new StringHashCollisionException(value, new string((char*) content));
                    }

                }

            }
            else {

                fixed (char* charptr = value) {
                    // + 1 for null terminator
                    index = buffer.AddRange((ushort*) charptr, value.Length + 1);
                }

                stringOffsets.Add(hash, index);
            }

            return hash;

        }

        public void Dispose() {
            buffer.Dispose();
            stringOffsets.Dispose();
        }

    }

}