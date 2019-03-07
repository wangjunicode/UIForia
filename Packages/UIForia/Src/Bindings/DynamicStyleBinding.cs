using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Templates;
using UIForia.Util;

namespace UIForia.Bindings {

    public class DynamicStyleBinding : Binding {

        private readonly ParsedTemplate template;
        public readonly ArrayLiteralExpression<string> bindingList;

        //when bindings support multi-threading: add [ThreadStatic] 
        private static readonly LightList<string> boundStyles;
        private static readonly LightList<UIStyleGroupContainer> containers;

        public DynamicStyleBinding(ParsedTemplate template, ArrayLiteralExpression<string> bindingList) : base("style") {
            this.template = template;
            this.bindingList = bindingList.Clone();
        }

        static DynamicStyleBinding() {
            containers = new LightList<UIStyleGroupContainer>(12);
            boundStyles = new LightList<string>(12);
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            string[] bindingStyles = bindingList.Evaluate(context);
            
            containers.QuickClear();
            boundStyles.QuickClear();
            
            containers.EnsureCapacity(bindingStyles.Length);
            boundStyles.EnsureCapacity(bindingStyles.Length);
            
            string[] boundStyleArray = boundStyles.Array;

            for (int i = 0; i < bindingStyles.Length; i++) {
                string style = bindingStyles[i];
                if (string.IsNullOrWhiteSpace(style) || style == string.Empty) {
                    continue;
                }

                bool add = true;
                int cnt = boundStyles.Count;
                for (int j = 0; j < cnt; j++) {
                    if (boundStyleArray[j] == style) {
                        add = false;
                        break;
                    }
                }

                if (add) {
                    UIStyleGroupContainer c = template.GetSharedStyle(style);
                    if (c != null) {
                        containers.Add(c);
                    }
                }
            }

            element.style.UpdateSharedStyles(containers);
        }

        public override bool IsConstant() {
            return bindingList.IsConstant();
        }

    }

}