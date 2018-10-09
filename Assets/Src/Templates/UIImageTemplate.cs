using System;
using System.Collections.Generic;
using System.Linq;
using Src.Compilers.AliasSource;

namespace Src {

    public class UIImageTemplate : UITemplate {

        public static readonly MethodAliasSource s_UrlSource;

        public UIImageTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }

        static UIImageTemplate() {
           s_UrlSource = new MethodAliasSource("url", typeof(UIImageTemplate).GetMethod(nameof(TextureUrl)));
        }

        public override Type elementType => typeof(UIImageElement);

        public override bool Compile(ParsedTemplate template) {
            template.contextDefinition.AddConstAliasSource(s_UrlSource);
            base.Compile(template);
            template.contextDefinition.RemoveConstAliasSource(s_UrlSource);
            return true;
        }

        public override MetaData CreateScoped(TemplateScope inputScope) {
            UIImageElement instance = new UIImageElement();

            MetaData instanceData = GetCreationData(instance, inputScope.context);
            instanceData.constantBindings = constantBindings;
            instanceData.conditionalBindings = conditionalBindings;
            instanceData.bindings = bindings;
            instanceData.context = inputScope.context;
            instanceData.inputBindings = inputBindings;
            instanceData.constantStyleBindings = constantStyleBindings;
            instanceData.element.templateAttributes = templateAttributes;
            instanceData.baseStyles = baseStyles;
            instanceData.mouseEventHandlers = mouseEventHandlers;
            instanceData.dragEventCreators = dragEventCreators;
            instanceData.dragEventHandlers = dragEventHandlers;
            instanceData.keyboardEventHandlers = keyboardEventHandlers;
            instanceData.element.templateChildren = inputScope.inputChildren.Select(c => c.element).ToArray();
            instanceData.element.ownChildren = instanceData.children.Select(c => c.element).ToArray();
            return instanceData;
        }

        public static Texture2DAssetReference TextureUrl(string url) {
            return new Texture2DAssetReference(url);
        }

    }

}