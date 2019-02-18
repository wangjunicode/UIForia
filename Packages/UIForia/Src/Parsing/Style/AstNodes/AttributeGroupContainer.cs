using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {
        
        internal static readonly ObjectPool<AttributeGroupContainer> s_AttributeGroupContainerNodePool = new ObjectPool<AttributeGroupContainer>();

        internal static AttributeGroupContainer AttributeGroupRootNode(string identifier, string value, bool invert, AttributeGroupContainer next) {
            AttributeGroupContainer rootNode = s_AttributeGroupContainerNodePool.Get();
            rootNode.type = StyleASTNodeType.AttributeGroup;
            rootNode.identifier = identifier;
            rootNode.value = value;
            rootNode.invert = invert;
            rootNode.next = next;
            return rootNode;
        }
    }

    public class AttributeGroupContainer : ChainableGroupContainer {

        public override void Release() {
            base.Release();
            StyleASTNodeFactory.s_AttributeGroupContainerNodePool.Release(this);
        }
    }
}