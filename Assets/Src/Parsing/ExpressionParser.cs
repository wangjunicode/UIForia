using System.Collections.Generic;

namespace Src {
    public class ExpressionParser {
        private readonly TokenStream tokenStream;

        public ExpressionParser(TokenStream tokenStream) {
            this.tokenStream = tokenStream;
        }

        public ASTNode Parse(List<TokenDefinition> tokens) {
            ConstantExpressionNode node = new ConstantExpressionNode();
            ExpressionNode expressionNode = new ExpressionNode();
            if (TryParseConstantExpression(ref node)) {
                return node;
            }
            else if (TryParseExpression(ref expressionNode)) { }

            return null;
        }

        private bool TryParseExpression(ref ExpressionNode node) {
            if (TryParseValueExpression()) { }
        }

        private bool TryParseValueExpression() {
            return true;
        }

        private bool TryParseLookupExpression(ref ExpressionNode node) {
            
            if (TryParseIdentifier(ref node)) {
                
            }
            return true;
        }

        private bool TryParsePropertyAccessExpression() {
            return true;
        }

        private bool TryParseArrayAccessExpression() {
            return true;
        }

        private bool TryParseConstantExpression(ref ConstantExpressionNode node) {
            tokenStream.Save();
            if ((tokenStream.Current.tokenType & TokenType.Constant) == 0) {
                return false;
            }

            node.value = tokenStream.Current.value;
            tokenStream.Consume(1);
            return true;
        }

        private bool TryParseExpressionStatement() {
            tokenStream.Save();

            // ExpressionStatementNode node = new ExpressionStatementNode;

            if (tokenStream.Current.tokenType != TokenType.ExpressionOpen) {
                return false;
            }

            tokenStream.Consume(1);

            ConstantExpressionNode node = new ConstantExpressionNode();
            if (!TryParseConstantExpression(ref node)) {
                tokenStream.Restore();
                return false;
            }

            if (tokenStream.Current.tokenType != TokenType.ExpressionClose) {
                tokenStream.Consume(1);
                return false;
            }

            return true;
        }

        private bool TryParseIdentifier(ref IdentifierNode identifierNode) {
            tokenStream.Save();
            if (tokenStream.Current.tokenType == TokenType.AnyIdentifier &&
                tokenStream.Next.tokenType == TokenType.WhiteSpace) {
                identifierNode = new IdentifierNode(token.current.value);
                return true;
            }
            return true;
        }

        private bool TryParseConstant() {
            tokenStream.Save();
            if ((tokenStream.Current.tokenType & TokenType.Constant) == 0) {
                return false;
            }

            tokenStream.Consume(1);
            //expressionParts.Add(new ConstantExpression());
            return true;
        }

        private bool TryParseDot() {
            if (tokenStream.Current.tokenType == TokenType.PropertyAccess) {
                tokenStream.Consume(1);
                return true;
            }

            return false;
        }

        private bool TryParsePropertyAccess() {
            if (TryParseIdentifier() && TryParseDot()) {
                //tree.Add(new PropertyAccessNode() { identifier = ParseIdentifier, property = ParseIdentifier2)
            }
        }

        private bool TryParseUnaryExpression() {
            return true;
        }

        private bool TryParseOperator() {
            bool isOperator = (tokenStream.Current.tokenType & TokenType.Operator) != 0;
        }

        private bool TryParsePropertyAccessChain() {
            if (TryParseDot() && TryParseIdentifier()) {
                if (TryParsePropertyAccessChain()) { }

                return true;
            }

            return false;
        }
    }
}