using UIForia.Util.Unsafe;

namespace UIForia {

    public struct ListFilter : IListFilter<StylePairUpdate> {

        public readonly StylePairUpdateType updateType;

        public ListFilter(StylePairUpdateType updateType) {
            this.updateType = updateType;
        }
        
        public bool Filter(in StylePairUpdate item) {
            return item.updateType == updateType;
        }

    }

}