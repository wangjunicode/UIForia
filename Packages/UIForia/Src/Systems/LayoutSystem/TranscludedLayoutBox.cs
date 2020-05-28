using System;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Systems {

    public class TranscludedLayoutBox : LayoutBox {

        private LightList<LayoutBox> childList;

        protected override float ComputeContentWidth() {
            throw new NotImplementedException();
        }

        protected override float ComputeContentHeight() {
            throw new NotImplementedException();
        }

        public override void OnChildrenChanged() {
            // throw new NotImplementedException();
            // this.childList = this.childList ?? new LightList<LayoutBox>(childList.size);
            // this.childList.AddRange(childList);
        }

        public override void RunLayoutHorizontal(int frameId) {
            throw new NotImplementedException();
        }

        public override void RunLayoutVertical(int frameId) {
            throw new NotImplementedException();
        }

        public override void OnStyleChanged(StyleProperty[] propertyList, int propertyCount) { }

    }

}