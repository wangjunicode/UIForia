using System;
using System.Linq.Expressions;
using UIForia.Elements;
using UIForia.Parsing.Expression;
using UIForia.Systems;
using UIForia.Util;

namespace UIForia.Compilers {

    public class CompiledTemplate {

        internal Expression<Func<UIElement, TemplateScope2, CompiledTemplate, UIElement>> buildExpression;
        internal Func<UIElement, TemplateScope2, CompiledTemplate, UIElement> createFn;
        internal LightList<LinqBinding> sharedBindings = new LightList<LinqBinding>();
        internal ProcessedType elementType;
        internal AttributeDefinition2[] attributes;

        public int templateId;
        public SlotDefinition[] slotDefinitions;

        internal Func<UIElement, TemplateScope2, CompiledTemplate, UIElement> Compile() {
            return buildExpression.Compile();
        }

        internal UIElement Create(UIElement parent, TemplateScope2 scope) {
            if (createFn == null) {
                createFn = buildExpression.Compile();
            }

            return createFn(parent, scope, null);
        }

        public int GetSlotId(string slotName) {
            
            if (slotDefinitions == null) {
                throw new ArgumentOutOfRangeException(slotName, $"Slot name {slotName} was not registered");
            }
            
            for (int i = 0; i < slotDefinitions.Length; i++) {
                if (slotDefinitions[i].tagName == slotName) {
                    return i;
                }
            }

            throw new ArgumentOutOfRangeException(slotName, $"Slot name {slotName} was not registered");
        }

    }

}