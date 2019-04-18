using UIForia.Attributes;
using UIForia.Templates;

namespace UIForia.Elements {

    public class RenderBlockElement : UIElement {

        public string blockId;

        private UITemplate renderedTemplate;
        internal TemplateScope scope;

        [OnPropertyChanged(nameof(blockId))]
        private void OnIdChanged() {
         
        }
        
        public override string GetDisplayName() {
            return "RenderBlock";
        }

    }

}