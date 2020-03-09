using System;

namespace UIForia.Attributes {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ResolveGenericTemplateArguments : Attribute { }

}