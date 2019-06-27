using System;
using System.Collections.Generic;
using UIForia.Parsing.Expression.Tokenizer;
using UIForia.Util;

namespace UIForia.Parsing.Expression.AstNodes {

    public abstract class ASTNode {

        protected static readonly ObjectPool<LiteralNode> s_LiteralPool = new ObjectPool<LiteralNode>();
        protected static readonly ObjectPool<OperatorNode> s_OperatorPool = new ObjectPool<OperatorNode>();
        protected static readonly ObjectPool<IdentifierNode> s_IdentifierPool = new ObjectPool<IdentifierNode>();
        protected static readonly ObjectPool<TypeNode> s_TypeNodePool = new ObjectPool<TypeNode>();
        protected static readonly ObjectPool<ParenNode> s_ParenPool = new ObjectPool<ParenNode>();
        protected static readonly ObjectPool<DotAccessNode> s_DotAccessPool = new ObjectPool<DotAccessNode>();
        protected static readonly ObjectPool<MemberAccessExpressionNode> s_MemberAccessExpressionPool = new ObjectPool<MemberAccessExpressionNode>();
        protected static readonly ObjectPool<IndexNode> s_IndexExpressionPool = new ObjectPool<IndexNode>();
        protected static readonly ObjectPool<InvokeNode> s_InvokeNodePool = new ObjectPool<InvokeNode>();
        protected static readonly ObjectPool<UnaryExpressionNode> s_UnaryNodePool = new ObjectPool<UnaryExpressionNode>();
        protected static readonly ObjectPool<ListInitializerNode> s_ListInitializerPool = new ObjectPool<ListInitializerNode>();

        public ASTNodeType type;

        public bool IsCompound {
            get {
                if (type == ASTNodeType.Operator) {
                    return true;
                }

                return false;
            }
        }

        public int line;
        public int column;

        public ASTNode WithLocation(ExpressionToken token) {
            this.line = token.line;
            this.column = token.column;
            return this;
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
            idNode.type = ASTNodeType.Identifier;
            return idNode;
        }

        public static TypeNode TypeOfNode(TypePath typePath) {
            TypeNode typeOfNode = s_TypeNodePool.Get();
            typeOfNode.typePath = typePath;
            typeOfNode.type = ASTNodeType.TypeOf;
            return typeOfNode;
        }

        public static TypeNode NewExpressionNode(TypePath typePath) {
            TypeNode typeOfNode = s_TypeNodePool.Get();
            typeOfNode.typePath = typePath;
            typeOfNode.type = ASTNodeType.New;
            return typeOfNode;
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

        public static ListInitializerNode ListInitializerNode(List<ASTNode> list) {
            ListInitializerNode listInitializerNode = s_ListInitializerPool.Get();
            listInitializerNode.list = list;
            return listInitializerNode;
        }

        public static IndexNode IndexExpressionNode(ASTNode expression) {
            IndexNode indexNode = s_IndexExpressionPool.Get();
            indexNode.expression = expression;
            return indexNode;
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

        public override string ToString() {
            return $"{identifier} with parts: {string.Join(".", parts)}";
        }

    }

    public class ParenNode : ASTNode {

        public ASTNode expression;

        public ParenNode() {
            type = ASTNodeType.Paren;
        }

        public override void Release() {
            expression?.Release();
            s_ParenPool.Release(this);
        }

    }

    public class TypeNode : ASTNode {

        public TypePath typePath;

        public override void Release() {
            s_TypeNodePool.Release(this);
            typePath.Release();
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

        public TypePath typePath;

        public NewExpressionNode() {
            type = ASTNodeType.New;
        }

        public override void Release() {
            throw new NotImplementedException();
        }

    }

    public class ListInitializerNode : ASTNode {

        public List<ASTNode> list;

        public ListInitializerNode() {
            type = ASTNodeType.ListInitializer;
        }

        public override void Release() {
            for (int i = 0; i < list.Count; i++) {
                list[i].Release();
            }

            ListPool<ASTNode>.Release(ref list);
            s_ListInitializerPool.Release(this);
        }

    }

    public class IndexNode : ASTNode {

        public ASTNode expression;

        public IndexNode() {
            type = ASTNodeType.IndexExpression;
        }

        public override void Release() {
            expression?.Release();
            s_IndexExpressionPool.Release(this);
        }

        public override string ToString() {
            return expression.ToString();
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

        public override string ToString() {
            return propertyName;
        }

    }

}