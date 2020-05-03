using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Style;

namespace UIForia {

    public unsafe struct StackLongBuffer8 {

        public int size;
        public fixed long array[8];

    }

    public unsafe struct StackIntBuffer7 {

        public int size;
        public fixed int array[7];

    }

    // todo shouldn't be class. struct wrapping element with an id lookup should be enough
    public unsafe class StyleSet {

        // could denormalize `state` to be always up to do date here and just handle style application separately
        internal UIElement element;
        public const int k_MaxSharedStyles = 7;

        internal int styleDataId; // if i convert stylesystem to use paged list then this can be a pointer because pages won't ever reallocate. for now it must be an index because the backing list can resize, invalidating a pointer
        internal readonly VertigoStyleSystem styleSystem;

        internal StyleSet(UIElement element, VertigoStyleSystem styleSystem) {
            this.element = element;
            this.styleSystem = styleSystem;
            this.styleDataId = styleSystem.CreatesStyleData();
        }

        internal void EnterState(StyleState2 state) {
            styleSystem.EnterState(this, state);
        }

        internal void ExitState(StyleState2 newState) {
            styleSystem.ExitState(this, state);
        }

        public StyleState2 state {
            get => styleSystem.GetState(styleDataId);
        }

        public void SetSharedStyles(IList<StyleId> styles) {
            if (styles.Count >= k_MaxSharedStyles) {
                // todo -- diagnostic
                return;
            }

            // todo -- int buffer now
            StackIntBuffer7 buffer = new StackIntBuffer7();

            for (int i = 0; i < styles.Count; i++) {
                buffer.array[buffer.size++] = styles[i];
            }

            styleSystem.SetSharedStyles(this, ref buffer);

        }

        internal unsafe void SetSharedStyles(ref StackIntBuffer7 styles) {
            // I'm not sure what the right way to handle duplicates is
            // honestly its probably best to just apply them twice and assume user error
            // might play animations twice or something but should be ok in most use cases
            // inspector would show the duplicate and maybe highlight it as a possible problem

            styleSystem.SetSharedStyles(this, ref styles);
        }

    }

}