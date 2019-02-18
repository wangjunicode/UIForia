using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {
    
    internal static partial class StyleASTNodeFactory {
    
        internal static readonly ObjectPool<ExpressionGroupContainer> s_ExpressionGroupContainerNodePool = new ObjectPool<ExpressionGroupContainer>();

        internal static ExpressionGroupContainer ExpressionGroupRootNode(string identifier, bool invert, AttributeGroupContainer next) {
            ExpressionGroupContainer rootNode = s_ExpressionGroupContainerNodePool.Get();
            rootNode.type = StyleASTNodeType.ExpressionGroup;
            rootNode.identifier = identifier;
            return rootNode;
        }
    }
    
    public class ExpressionGroupContainer : ChainableGroupContainer {

        // TODO add expression node
        
        public override void Release() {
            base.Release();
            StyleASTNodeFactory.s_ExpressionGroupContainerNodePool.Release(this);
        }
    }
}