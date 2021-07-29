using System.Runtime.InteropServices;
using UIForia.Util.Unsafe;

namespace UIForia.Style {

    /// <summary>
    /// A chunk of data representing a style. Contains offsets into arrays of the <see cref="StyleDatabase"/>
    /// </summary>
    [AssertSize(20)]
    [StructLayout(LayoutKind.Sequential)]
    internal struct StyleBlock {
        
        // Note -- Fields are laid out by byte width for maximum compression! Be wary of changing the order or types of these fields
        
        /// <summary>
        /// How deep in the style's block hierarchy this block is
        /// </summary>
        // public ushort depth;
        
        public BlockUsageSortKey sortKey;
        
        /// <summary>
        /// Index into property locator buffer, <see cref="PropertyLocator"/>
        /// </summary>
        public int propertyStart;
        public int transitionStart;
        public int stateHookStart;

        public ushort propertyCount;
        public ushort transitionCount;
        public ushort stateHookCount;

        internal CheckedArray<PropertyLocator> GetProperties(ref DataList<PropertyLocator> locators) {
            return locators.Slice(propertyStart, propertyCount);
        }
        
        // /// <summary>
        // /// Index in the style database's selector list at which this style block's transitions start. Not a local index!
        // /// </summary>
        // public ushort transitionStart;
        //
        //
        // /// <summary>
        // /// How many transitions are defined by this block
        // /// </summary>
        // public ushort transitionCount;

        // /// <summary>
        // /// <see cref="data"/> is a discriminated union of data. This field is the discrimator
        // /// </summary>
        // public CompiledBlockType type;
        //
        // /// <summary>
        // /// A discriminated union of all block data
        // /// </summary>
        // public CompiledBlockData data;

        // public ulong stateRequirements;

        // /// <summary>
        // /// Local index of the block relative to the first block in the style.
        // /// Used to tie break precedence between style blocks originating from the same style at the same depth
        // /// </summary>
        // public ushort index;

        // /// <summary>
        // /// Index in the style database's selector list at which this style block's selectors start. Not a local index!
        // /// </summary>
        // public ushort selectorStart;
        //
        // /// <summary>
        // /// How many selectors are defined by this block
        // /// </summary>
        // public ushort selectorCount;

    }

}