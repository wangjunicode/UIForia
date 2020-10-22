using UIForia.Attributes;
using UIForia.Elements;

namespace SeedLib {
    [Template("SeedLib/Tree/TreeFoldout.xml")]
    public class TreeFoldout : Foldout {
        public ImageLocator icon;

        public UIElement titleRow;
        public UIElement foldoutChildren;
        
        public override void OnCreate() {
            titleRow = FindById("title-row");
            foldoutChildren = FindById("foldout-children");
        }

        public override void OnEnable() {
            base.OnEnable();

            bool isDense = TryGetAttribute("variant", out string attribute) && attribute.Contains("dense");
            SetAttribute("dense", isDense ? "true" : null);

            UIElement icon = FindById<UIElement>("title-icon");
            icon.SetAttribute("dense", isDense ? "true" : null);
        }
    }
}