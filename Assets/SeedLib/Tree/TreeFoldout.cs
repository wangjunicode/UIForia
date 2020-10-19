using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;

namespace SeedLib {
    [Template("SeedLib/Tree/TreeFoldout.xml")]
    public class TreeFoldout : Foldout {
        public ImageLocator icon;

        public override void OnEnable() {
            base.OnEnable();
            
            UIElement icon = FindById<UIElement>("title-icon");

            bool isDense = TryGetAttribute("variant", out string attribute) && attribute.Contains("dense");
            icon.SetAttribute("dense", isDense ? "true" : null);
        }
    }
}