using UIForia.Attributes;
using UIForia.Elements;

namespace SeedLib {
    [Template("SeedLib/Tree/TreeNode.xml")]
    public class TreeNode : ListItem {
        public string title;

        public override void OnEnable() {
            // TODO: Refactor at some point. Repeats ListItem logic, but with a different parent type.
            UIImageElement image = FindFirstByType<UIImageElement>();
            var itemList = FindParent<TreeFoldout>();
            if (itemList == null) {
                return;
            }

            bool isDense = itemList.TryGetAttribute("variant", out string attribute) && attribute.Contains("dense");
            SetAttribute("dense", isDense ? "true" : null);
            image.SetAttribute("dense", isDense ? "true" : null);
        }
    }
}