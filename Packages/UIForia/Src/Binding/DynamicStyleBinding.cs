using UIForia.Compilers.Style;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia {
    
    public class DynamicStyleBinding : Binding {

        private readonly string[] styleNames;
        private readonly ParsedTemplate template;
        public readonly ArrayLiteralExpression<string> bindingList;

        public DynamicStyleBinding(ParsedTemplate template, ArrayLiteralExpression<string> bindingList) : base("style") {
            this.template = template;
            this.bindingList = bindingList.Clone();
            this.styleNames = new string[bindingList.list.Length];
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            string[] styles = bindingList.Evaluate(context);
            bool shouldSet = false;

            for (int i = 0; i < styles.Length; i++) {
                if (styleNames[i] != styles[i]) {
                    shouldSet = true;
                    styleNames[i] = styles[i];
                }
            }

            if (shouldSet) {
                LightList<UIStyleGroupContainer> groups = LightListPool<UIStyleGroupContainer>.Get();
                string tagName = element.GetDisplayName();
                UIStyleGroupContainer groupContainer = template.ResolveElementStyle(tagName);
                if (groupContainer != null) {
                    groups.Add(groupContainer);    
                }

                for (int i = 0; i < styles.Length; i++) {
                    if (!string.IsNullOrEmpty(styles[i])) {
                        if (template.TryResolveStyleGroup(styles[i], out UIStyleGroupContainer group)) {
                            group.styleType = StyleType.Shared;
                            groups.Add(group);
                        }
                    }
                }
                
                element.style.SetStyleGroups(groups);
                LightListPool<UIStyleGroupContainer>.Release(ref groups);
            }
        }

        public override bool IsConstant() {
            return bindingList.IsConstant();
        }

    }

}