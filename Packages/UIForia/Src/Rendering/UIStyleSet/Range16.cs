namespace UIForia {

    public struct Range16 {

        /// <summary>
        ///   <para>The starting index of the range, where 0 is the first position, 1 is the second, 2 is the third, and so on.</para>
        /// </summary>
        public ushort start;

        /// <summary>
        ///   <para>The length of the range.</para>
        /// </summary>
        public ushort length;

        /// <summary>
        ///   <para>Constructs a new Range16 with given start, length values.</para>
        /// </summary>
        /// <param name="start">The starting index of the range.</param>
        /// <param name="length">The length of the range.</param>
        public Range16(int start, int length) {
            this.start = (ushort) start;
            this.length = (ushort) length;
        }
        
        /// <summary>
        ///   <para>Constructs a new Range16 with given start, length values.</para>
        /// </summary>
        /// <param name="start">The starting index of the range.</param>
        /// <param name="length">The length of the range.</param>
        public Range16(ushort start, ushort length) {
            this.start = start;
            this.length = length;
        }

        /// <summary>
        ///   <para>The end index of the range (not inclusive).</para>
        /// </summary>
        public int end => start + length;

    }

}