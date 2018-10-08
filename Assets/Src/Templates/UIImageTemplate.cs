using System;
using System.Collections.Generic;
using JetBrains.Annotations;
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

            MetaData data = GetCreationData(instance, inputScope.context);

            return data;
        }

        [Pure]
        public static Texture2DAssetReference TextureUrl(string url) {
            return new Texture2DAssetReference(url);
        }

    }

}