using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {
        
        internal static readonly ObjectPool<PropertyNode> s_PropertyNodePool = new ObjectPool<PropertyNode>();
        
        internal static PropertyNode PropertyNode(string propertyName) {
            PropertyNode propertyNode = s_PropertyNodePool.Get();
            propertyNode.identifier = propertyName;
            return propertyNode;
        }
    }
    
    public class PropertyNode : StyleNodeContainer {

        public PropertyNode() {
            type = StyleASTNodeType.Property;
        }

        public override void Release() {
            base.Release();
            StyleASTNodeFactory.s_PropertyNodePool.Release(this);
        }
    }
}