using UIForia.Rendering;
using UIForia.Util;

namespace UIForia {

    public class DynamicStyleBinding : Binding {

        private ParsedTemplate template;
        private LightList<string> styleNames;
        public readonly ArrayLiteralExpression<string> bindingList;

        // todo if style count is 1 don't use the array just use a string
        public DynamicStyleBinding(ParsedTemplate template, ArrayLiteralExpression<string> bindingList) : base("style") {
            this.template = template;
            this.bindingList = bindingList.Clone();
            this.styleNames = LightListPool<string>.Get();
            this.styleNames.EnsureCapacity(bindingList.list.Length);
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
                LightList<UIStyleGroup> groups = LightListPool<UIStyleGroup>.Get();
                string tagName = "<" + element.GetDisplayName() + ">"; // todo get rid of concat allocation
                UIStyleGroup tagGroup = template.ResolveElementStyle(tagName);
                if (tagGroup != default) {
                    groups.Add(tagGroup);    
                }

                for (int i = 0; i < styles.Length; i++) {
                    if (!string.IsNullOrEmpty(styles[i])) {
                        if (template.TryResolveStyleGroup(styles[i], out UIStyleGroup group)) {
                            group.styleType = StyleType.Shared;
                            groups.Add(group);
                        }
                    }
                }
                
                element.style.SetStyleGroups(groups);
            }
        }

        public override bool IsConstant() {
            return bindingList.IsConstant();
        }

    }

}