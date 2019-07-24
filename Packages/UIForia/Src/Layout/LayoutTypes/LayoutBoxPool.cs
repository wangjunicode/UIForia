using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Layout.LayoutTypes;
using UIForia.Systems;
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

    internal class LayoutBoxPool<T> : LayoutBoxPool where T : LayoutBox, new() {

        public override LayoutBox Get(UIElement element) {
            LayoutBox retn = null;
            if (pool.Count > 0) {
                retn = pool.RemoveLast();
            }
            else {
                retn = new T();
            }

            retn.isInPool = false;
            retn.element = element;
            retn.style = element.style;
            retn.children = retn.children ?? new List<LayoutBox>(4);
            retn.cachedPreferredWidth = -1;
            retn.view = element.View;
            retn.markedForLayout = true;
            retn.UpdateFromStyle();
            retn.OnSpawn(element);
            return retn;
        }

    }
    
    internal abstract class FastLayoutBoxPool {

        protected readonly LightList<FastLayoutBox> pool = new LightList<FastLayoutBox>();

        public abstract FastLayoutBox Get(FastLayoutSystem.LayoutOwner owner, UIElement element);

        public void Release(FastLayoutBox box) {
            box.firstChild = null;
            box.nextSibling = null;
            box.parent = null;
            box.element = null;
            box.relayoutBoundary = null;
            box.flags = 0;
            pool.Add(box);
        }

    }

    internal class FastLayoutBoxPool<T> : FastLayoutBoxPool where T : FastLayoutBox, new() {

        public override FastLayoutBox Get(FastLayoutSystem.LayoutOwner owner, UIElement element) {
            FastLayoutBox retn = null;
            if (pool.Count > 0) {
                retn = pool.RemoveLast();
            }
            else {
                retn = new T();
            }

            retn.owner = owner;
            retn.element = element;
            retn.UpdateStyleData();
            return retn;
        }

    }

}