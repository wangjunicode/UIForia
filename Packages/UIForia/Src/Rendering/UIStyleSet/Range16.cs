using System.Runtime.InteropServices;

namespace UIForia.Util {

    [StructLayout(LayoutKind.Explicit)]
    public struct Range16 {

        /// <summary>
        ///   <para>The starting index of the range, where 0 is the first position, 1 is the second, 2 is the third, and so on.</para>
        /// </summary>
        [FieldOffset(0)] public ushort start;

        /// <summary>
        ///   <para>The length of the range.</para>
        /// </summary>
        [FieldOffset(2)] public ushort length;

        [FieldOffset(0)] private uint asUint;

        /// <summary>
        ///   <para>Constructs a new Range16 with given start, length values.</para>
        /// </summary>
        /// <param name="start">The starting index of the range.</param>
        /// <param name="length">The length of the range.</param>
        public Range16(int start, int length) : this() {
            this.start = (ushort) start;
            this.length = (ushort) length;
        }

        public Range16(uint start, uint length) : this() {
            this.start = (ushort) start;
            this.length = (ushort) length;
        }

        /// <summary>
        ///   <para>Constructs a new Range16 with given start, length values.</para>
        /// </summary>
        /// <param name="start">The starting index of the range.</param>
        /// <param name="length">The length of the range.</param>
        public Range16(ushort start, ushort length) : this() {
            this.start = start;
            this.length = length;
        }

        public Range16(uint input) : this() {
            this.start = (ushort) ((input & 0xffff) - start);
            this.length = (ushort) ((input >> 16) & (1 << 16) - 1);
        }

        public static Range16 FromStartEnd(ushort start, ushort end) {
            return new Range16(start, end - start);    
        }
        
        /// <summary>
        ///   <para>The end index of the range (not inclusive).</para>
        /// </summary>
        public int end => start + length;

        public static implicit operator uint(Range16 range16) {
            return range16.asUint;
        }

    }

}