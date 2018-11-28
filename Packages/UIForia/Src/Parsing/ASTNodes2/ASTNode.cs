using System.Collections.Generic;
using UIForia.Util;

namespace UIForia.Parsing {

    public abstract class ASTNode {

        protected static readonly ObjectPool<LiteralNode> s_LiteralPool = new ObjectPool<LiteralNode>();
        protected static readonly ObjectPool<OperatorNode> s_OperatorPool = new ObjectPool<OperatorNode>();
        protected static readonly ObjectPool<IdentifierNode> s_IdentifierPool = new ObjectPool<IdentifierNode>();
        protected static readonly ObjectPool<TypeOfNode> s_TypeOfPool = new ObjectPool<TypeOfNode>();
        protected static readonly ObjectPool<MethodCallNode> s_MethodCallPool = new ObjectPool<MethodCallNode>();
        protected static readonly ObjectPool<ParenNode> s_ParenPool = new ObjectPool<ParenNode>();
        protected static readonly ObjectPool<DotAccessNode> s_DotAccessPool = new ObjectPool<DotAccessNode>();
        protected static readonly ObjectPool<MemberAccessExpressionNode> s_MemberAccessExpressionPool = new ObjectPool<MemberAccessExpressionNode>();
        protected static readonly ObjectPool<IndexExpressionNode> s_IndexExpressionPool = new ObjectPool<IndexExpressionNode>();
        protected static readonly ObjectPool<InvokeNode> s_InvokeNodePool = new ObjectPool<InvokeNode>();
        protected static readonly ObjectPool<UnaryExpressionNode> s_UnaryNodePool = new ObjectPool<UnaryExpressionNode>();
        protected static readonly ObjectPool<TypePathNode> s_TypePathNodePool = new ObjectPool<TypePathNode>();

        public ASTNodeType type;

        public bool IsCompound {
            get {
                if (type == ASTNodeType.Operator) {
                    return true;
                }

                return false;
            }
        }

        public abstract void Release();

        public static LiteralNode NullLiteralNode(string value) {
            LiteralNode retn = s_LiteralPool.Get();
            retn.type = ASTNodeType.NullLiteral;
            retn.rawValue = value;
            return retn;
        }

        public static LiteralNode StringLiteralNode(string value) {
            LiteralNode retn = s_LiteralPool.Get();
            retn.type = ASTNodeType.StringLiteral;
            retn.rawValue = value;
            return retn;
        }

        public static LiteralNode BooleanLiteralNode(string value) {
            LiteralNode retn = s_LiteralPool.Get();
            retn.type = ASTNodeType.BooleanLiteral;
            retn.rawValue = value;
            return retn;
        }

        public static LiteralNode NumericLiteralNode(string value) {
            LiteralNode retn = s_LiteralPool.Get();
            retn.type = ASTNodeType.NumericLiteral;
            retn.rawValue = value;
            return retn;
        }

        public static LiteralNode DefaultLiteralNode(string value) {
            LiteralNode retn = s_LiteralPool.Get();
            retn.type = ASTNodeType.DefaultLiteral;
            retn.rawValue = value;
            return retn;
        }

        public static OperatorNode OperatorNode(OperatorType operatorType) {
            OperatorNode operatorNode = s_OperatorPool.Get();
            operatorNode.type = ASTNodeType.Operator;
            operatorNode.operatorType = operatorType;
            return operatorNode;
        }

        public static IdentifierNode IdentifierNode(string name) {
            IdentifierNode idNode = s_IdentifierPool.Get();
            idNode.name = name;
            idNode.type = name[0] == '$' ? ASTNodeType.Alias : ASTNodeType.Identifier;
            return idNode;
        }

        public static TypeOfNode TypeOfNode(ASTNode expression) {
            TypeOfNode typeOfNode = s_TypeOfPool.Get();
            typeOfNode.expression = expression;
            return typeOfNode;
        }

        public static MethodCallNode MethodCallNode(string methodName, ASTNode[] signature = null) {
            MethodCallNode methodCallNode = s_MethodCallPool.Get();
            methodCallNode.methodName = methodName;
            methodCallNode.signature = signature;
            return methodCallNode;
        }

        public static ParenNode ParenNode(ASTNode expression) {
            ParenNode parenNode = s_ParenPool.Get();
            parenNode.expression = expression;
            return parenNode;
        }

        public static DotAccessNode DotAccessNode(string propertyName) {
            DotAccessNode dotAccessNode = s_DotAccessPool.Get();
            dotAccessNode.propertyName = propertyName;
            return dotAccessNode;
        }

        public static InvokeNode InvokeNode(List<ASTNode> parameters) {
            InvokeNode invokeNode = s_InvokeNodePool.Get();
            invokeNode.parameters = parameters;
            return invokeNode;
        }

        public static MemberAccessExpressionNode MemberAccessExpressionNode(string identifier, List<ASTNode> parts) {
            MemberAccessExpressionNode accessExpressionNode = s_MemberAccessExpressionPool.Get();
            accessExpressionNode.identifier = identifier;
            accessExpressionNode.parts = parts;
            return accessExpressionNode;
        }
        
        public static NewExpressionNode NewExpressionNode(TypeNode typeNode, InvokeNode invokeNode) {
            return null;
        }
        
        public static ListInitializerNode ListInitializerNode(ASTNode[] list) {
            return null;
        }

        public static IndexExpressionNode IndexExpressionNode(ASTNode expression) {
            IndexExpressionNode indexExpressionNode = s_IndexExpressionPool.Get();
            indexExpressionNode.expression = expression;
            return indexExpressionNode;
        }

        public static UnaryExpressionNode UnaryExpressionNode(ASTNodeType nodeType, ASTNode expr) {
            UnaryExpressionNode unaryNode = s_UnaryNodePool.Get();
            unaryNode.type = nodeType;
            unaryNode.expression = expr;
            return unaryNode;
        }

        public static UnaryExpressionNode DirectCastNode(TypePath typePath, ASTNode expression) {
            UnaryExpressionNode unaryNode = s_UnaryNodePool.Get();
            unaryNode.type = ASTNodeType.DirectCast;
            unaryNode.typePath = typePath;
            unaryNode.expression = expression;
            return unaryNode;
        }
        
    }

    public struct TypePath {

        public List<string> path;
        public List<TypePath> genericArguments;

        public void Release() {
            ListPool<string>.Release(ref path);
            ReleaseGenerics();
        }

        public void ReleaseGenerics() {
            if (genericArguments != null && genericArguments.Count > 0) {
                for (int i = 0; i < genericArguments.Count; i++) {
                    genericArguments[i].Release();
                }
                ListPool<TypePath>.Release(ref genericArguments);
                genericArguments = null;
            }
        }

    }
    
    public class TypePathNode : ASTNode {

        public TypePath typePath;
        
        public TypePathNode() {
            type = ASTNodeType.TypePath;
        }
        
        public override void Release() {
            typePath.Release();
        }

    }

    public class UnaryExpressionNode : ASTNode {

        public ASTNode expression;
        public TypePath typePath;
        
        public override void Release() {
            typePath.Release();
            expression?.Release();
            s_UnaryNodePool.Release(this);
        }

    }

    public class MemberAccessExpressionNode : ASTNode {

        public string identifier;
        public List<ASTNode> parts;

        public MemberAccessExpressionNode() {
            type = ASTNodeType.AccessExpression;
        }
        
        public override void Release() {
            s_MemberAccessExpressionPool.Release(this);
            for (int i = 0; i < parts.Count; i++) {
                parts[i].Release();
            }
            ListPool<ASTNode>.Release(ref parts);
        }

    }

    public class ParenNode : ASTNode {

        public ASTNode expression;
        
        public override void Release() {
            expression?.Release();
            s_ParenPool.Release(this);
        }

    }

    public class TypeNode : ASTNode {

        public override void Release() {
            
        }

    }

    public class InvokeNode : ASTNode {

        public List<ASTNode> parameters;
        
        public override void Release() {
            for (int i = 0; i < parameters.Count; i++) {
                parameters[i].Release();
            }
            ListPool<ASTNode>.Release(ref parameters);
            s_InvokeNodePool.Release(this);
        }

    }

    public class NewExpressionNode : ASTNode {

        public override void Release() {
            
        }

    }

    public class ListInitializerNode : ASTNode {

        public override void Release() {
            
        }

    }
    
    public class IndexExpressionNode : ASTNode {

        public ASTNode expression;

        public IndexExpressionNode() {
            type = ASTNodeType.IndexExpression;
        }
        
        public override void Release() {
            expression?.Release();
            s_IndexExpressionPool.Release(this);
        }

    }

    public class DotAccessNode : ASTNode {

        public string propertyName;
        
        public DotAccessNode() {
            type = ASTNodeType.DotAccess;
        }
        
        public override void Release() {
            s_DotAccessPool.Release(this);
        }

    }
    
    public class MethodCallNode : ASTNode {

        public string methodName;
        public ASTNode[] signature;

        public MethodCallNode() {
            type = ASTNodeType.MethodCall;
        }

        public override void Release() {
            s_MethodCallPool.Release(this);
            if (signature != null && signature.Length > 0) {
                ArrayPool<ASTNode>.Release(ref signature);
            }
        }

    }

    public class TypeOfNode : ASTNode {

        public ASTNode expression;

        public TypeOfNode() {
            type = ASTNodeType.TypeOf;
        }

        public override void Release() {
            expression.Release();
            s_TypeOfPool.Release(this);
        }

    }

}