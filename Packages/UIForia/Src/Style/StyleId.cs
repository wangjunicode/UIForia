using System.Diagnostics;
using UIForia.Util.Unsafe;

namespace UIForia.Style {

    [AssertSize(8)]
    [DebuggerDisplay("{GetDebugDisplay()}")]
    public struct StyleId {

        /// <summary>
        /// Sequential unique id
        /// </summary>
        internal uint id;
        
        /// <summary>
        /// Encodes block offset, count and source depth via bit magic 
        /// </summary>
        private uint dataBits;

        // todo -- some issue with setting full value in mask here, value 128 with 7 bits gets masked to 0
        // i think the masks need a + 1 or something 
        // https://stackoverflow.com/a/29326393/1738765 
        private const int sz0 = 18, loc0 = 0, mask0 = ((1 << sz0) - 1) << loc0;
        private const int sz1 = 7, loc1 = loc0 + sz0, mask1 = ((1 << sz1) - 1) << loc1;

        private const int sz2 = 7, loc2 = loc1 + sz1, mask2 = ((1 << sz2) - 1) << loc2;
        // const int sz3 = 4, loc3 = loc2 + sz2, mask3 = ((1 << sz3) - 1) << loc3;

        internal StyleId(ulong encoded) {
            id = (uint) (encoded & uint.MaxValue);
            dataBits = (uint) (encoded >> 32);
        }

        internal StyleId(uint id, uint blockOffset, uint blockCount, uint sourceDepthDiff = 0) {
            this.id = id;
            this.dataBits = 0;
            this.dataBits = (uint) (dataBits & ~mask0 | (uint) (blockOffset << loc0) & mask0);
            this.dataBits = (uint) (dataBits & ~mask1 | (uint) (blockCount << loc1) & mask1);
            this.dataBits = (uint) (dataBits & ~mask2 | (uint) (sourceDepthDiff << loc2) & mask2);
        }

        public bool isSelectorStyle => sourceDepthDiff != 0;

        /// <summary>
        /// Points into <see cref="StyleDatabase.styleBlocks"/>
        /// </summary>
        public int blockOffset {
            get => (int) ((dataBits & mask0) >> loc0);
        }

        public int blockCount {
            get => (int) (dataBits & mask1) >> loc1;
        }

        /// <summary>
        /// Level of nestedness
        /// </summary>
        public int sourceDepthDiff {
            get => (int) (dataBits & mask2) >> loc2;
        }

        internal CheckedArray<StyleBlock> GetBlocks(ref DataList<StyleBlock> blocks) {
            return blocks.Slice(blockOffset, blockCount);
        }
        
        public bool IsValid => id != 0;

        public override string ToString() {
            return "StyleId(" + id + ")";
        }

        public int CompareTo(StyleId other) {
            if (id < other.id) {
                return -1;
            }

            return id > other.id ? 1 : 0;
        }

        public bool Equals(StyleId other) {
            return id == other.id;
        }

        public static bool operator ==(in StyleId a, in StyleId b) {
            return a.id == b.id;
        }

        public static bool operator !=(StyleId a, StyleId b) {
            return a.id != b.id;
        }

        public override bool Equals(object obj) {
            return obj is StyleId other && Equals(other);
        }

        public override int GetHashCode() {
            return (int) id;
        }

        public string GetDebugDisplay() {
#if DEBUG
            if (id == 0) {
                return "Invalid";
            }

            // todo -- get active app somehow 
            // for (int i = 0; i < UIRuntime.s_ApplicationList.Count; i++) {
            //     if (UIRuntime.s_ApplicationList[i].TryGetTarget(out UIApplication app)) {
            //         StyleDatabase styleDatabase = app.styleDatabase;
            //         if (!MathUtil.Between(id, styleDatabase.numberRangeStart, styleDatabase.numberRangeEnd)) {
            //             continue;
            //         }
            //
            //         if (styleDatabase.TryGetStyleMetaData(this, out StyleDatabase.StyleMeta meta)) {
            //             return meta.styleName + " [id=" + id + "] (file: " + PathUtil.ProjectRelativePath(meta.filePath) + ")";
            //         }
            //
            //         return "Unknown / Invalid";
            //     }
            // }

            return "Unknown";
#else
            return "";
#endif

        }

    }

}