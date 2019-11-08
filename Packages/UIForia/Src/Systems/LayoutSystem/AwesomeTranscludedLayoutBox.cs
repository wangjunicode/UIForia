using System;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Systems {

    public class AwesomeTranscludedLayoutBox : AwesomeLayoutBox {

        protected override float ComputeContentWidth() {
            throw new NotImplementedException();
        }

        protected override float ComputeContentHeight() {
            throw new NotImplementedException();
        }

        public override void OnChildrenChanged(LightList<AwesomeLayoutBox> childList) {
            throw new NotImplementedException();
        }

        public override void RunLayoutHorizontal(int frameId) {
            throw new NotImplementedException();
        }

        public override void RunLayoutVertical() {
            throw new NotImplementedException();
        }

        public override void OnStyleChanged(StructList<StyleProperty> propertyList) {}

        public LightList<AwesomeLayoutBox> GetChildren() {
            throw new NotImplementedException();
        }

    }

}