using UIForia.Elements;
using UIForia.Layout.LayoutTypes;
using UIForia.Util;

namespace UIForia.Layout {

    internal abstract class LayoutBoxPool {

        protected readonly LightList<LayoutBox> pool = new LightList<LayoutBox>();

        public abstract LayoutBox Get(UIElement element);

        public void Release(LayoutBox box) {
            if (box.isInPool) {
                return;
            }

            box.isInPool = false;
            pool.Add(box);
        }
    }

    internal class LayoutBoxPool<T> : LayoutBoxPool where T: LayoutBox, new() {

        public override LayoutBox Get(UIElement element) {
            LayoutBox retn = null;
            if (pool.Count > 0) {
                retn = pool.RemoveLast();
            }
            else {
                retn = new T();
            }
            retn.isInPool = false;
            retn.OnSpawn(element);
            return retn;
        }

    }

}