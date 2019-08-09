using UIForia.Elements;
using UIForia.Util;

namespace UIForia.Layout {

    internal abstract class FastLayoutBoxPool {

        protected readonly LightList<FastLayoutBox> pool = new LightList<FastLayoutBox>();

        public abstract FastLayoutBox Get(LayoutOwner owner, UIElement element);

        public void Release(FastLayoutBox box) {
            box.firstChild = null;
            box.nextSibling = null;
            box.parent = null;
            box.OnDestroy();
            box.element = null;
            box.flags = 0;
            pool.Add(box);
        }

    }
    
    internal class FastLayoutBoxPool<T> : FastLayoutBoxPool where T : FastLayoutBox, new() {

        public override FastLayoutBox Get(LayoutOwner owner, UIElement element) {
            FastLayoutBox retn = null;
            if (pool.Count > 0) {
                retn = pool.RemoveLast();
            }
            else {
                retn = new T();
            }

            retn.owner = owner;
            retn.element = element;
            retn.OnInitialize();
            retn.UpdateStyleData();
            return retn;
        }

    }

}