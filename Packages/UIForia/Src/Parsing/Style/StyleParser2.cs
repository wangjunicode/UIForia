using System;
using System.Collections.Generic;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Parsing.Style.Tokenizer;
using UIForia.Util;

namespace UIForia.Parsing.Style {
    public struct StyleParser2 {

        private StyleTokenStream tokenStream;

        /// <summary>
        /// Contains all top level nodes that are in the style file. 
        /// </summary>
        private LightList<StyleASTNode> nodes;

        private Stack<StyleASTNode> expressionStack;
        private Stack<StyleOperatorNode> operatorStack;
        private Stack<AttributeGroupContainer> groupExpressionStack;
        private Stack<StyleOperatorType> groupOperatorStack;

        private StyleParser2(StyleTokenStream stream) {
            tokenStream = stream;
            nodes = LightListPool<StyleASTNode>.Get();
            operatorStack = StackPool<StyleOperatorNode>.Get();
            expressionStack = StackPool<StyleASTNode>.Get();
            groupExpressionStack = StackPool<AttributeGroupContainer>.Get();
            groupOperatorStack = StackPool<StyleOperatorType>.Get();
        }

        private static StyleTokenStream FromString(string input) {
            return new StyleTokenStream(StyleTokenizer.Tokenize(input, ListPool<StyleToken>.Get()));
        }

        public static LightList<StyleASTNode> Parse(string input) {
            return new StyleParser2(FromString(input)).Parse();
        }

        private void Release() {
            tokenStream.Release();
            StackPool<StyleOperatorNode>.Release(operatorStack);
            StackPool<StyleASTNode>.Release(expressionStack);
        }

        private LightList<StyleASTNode> Parse() {
            if (!tokenStream.HasMoreTokens) {
                return nodes;
            }

            ParseLoop();
            Release();

            return nodes;
        }

        private void ParseLoop() {
            while (tokenStream.HasMoreTokens) {
                ParseNextRoot();
            }
        }

        private void ParseNextRoot() {
            switch (tokenStream.Current.styleTokenType) {
                case StyleTokenType.Style:
                    tokenStream.Advance();
                    ParseStyle();
                    break;

                case StyleTokenType.Audio:
                    tokenStream.Advance();
                    // ParseAudio
                    AssertTokenType(StyleTokenType.BracesClose);
                    tokenStream.Advance();
                    break;

                case StyleTokenType.Animation:
                    tokenStream.Advance();
                    // ParseAnimation
                    AssertTokenType(StyleTokenType.BracesClose);
                    tokenStream.Advance();
                    break;

                case StyleTokenType.Import:
                    tokenStream.Advance();
                    break;

                case StyleTokenType.Export:
                    ParseExportNode();
                    break;

                case StyleTokenType.Const:
                    tokenStream.Advance();
                    break;

                case StyleTokenType.Cursor:
                    tokenStream.Advance();
                    break;
                default:
                    throw new ParseException(tokenStream.Current, $"Did not expect token {tokenStream.Current.value} of type {tokenStream.Current.styleTokenType} here at line");
            }
        }

        /// <summary>
        /// Takes on all the things after a 'style' keyword on the root level of a style file.
        /// </summary>
        /// <exception cref="ParseException"></exception>
        private void ParseStyle() {
            string identifier = null;
            string tagName = null;
            switch (tokenStream.Current.styleTokenType) {
                // <TagName> { ... }
                case StyleTokenType.LessThan:
                    tokenStream.Advance();
                    AssertTokenType(StyleTokenType.Identifier);
                    tagName = tokenStream.Current.value;
                    tokenStream.Advance();
                    AssertTokenTypeAndAdvance(StyleTokenType.GreaterThan);
                    break;
                // styleId { ... }
                case StyleTokenType.Identifier:
                    identifier = tokenStream.Current.value;
                    tokenStream.Advance();
                    break;
                default:
                    throw new ParseException(tokenStream.Current, $"Expected style definition or tag name but found {tokenStream.Current.styleTokenType}");
            }

            StyleRootNode styleRootNode = StyleASTNode.StyleRootNode(identifier, tagName);
            nodes.Add(styleRootNode);

            // we just read an element name or style name
            // now move forward and expect an open curly brace

            // next there should be one of those:
            // - property
            // - state
            // - attribute with or without boolean modifier
            // - expression with constants
            ParseStyleGroupBody(styleRootNode);
        }

        private void ParseExportNode() {
            tokenStream.Advance();
            // export statement must be followed by const keyword
            AssertTokenTypeAndAdvance(StyleTokenType.Const);
            // const name
            string variableName = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);
            // type declaration (add supported types in the StyleTokenizer)
            AssertTokenTypeAndAdvance(StyleTokenType.Colon);
            string variableType = AssertTokenTypeAndAdvance(StyleTokenType.VariableType);

            AssertTokenTypeAndAdvance(StyleTokenType.Equal);

            // now let's find out which value we're assigning
            nodes.Add(StyleASTNode.ExportNode(variableName, variableType, ParsePropertyValue()));
        }

        private void ParseStyleGroupBody(StyleGroupContainer styleRootNode) {
            AssertTokenTypeAndAdvance(StyleTokenType.BracesOpen);

            while (tokenStream.HasMoreTokens && !AdvanceIfTokenType(StyleTokenType.BracesClose)) {
                switch (tokenStream.Current.styleTokenType) {
                    case StyleTokenType.Not: {
                        groupOperatorStack.Push(StyleOperatorType.Not);
                        tokenStream.Advance();
                        ParseAttributeOrExpressionGroup();
                        break;
                    }

                    case StyleTokenType.And:
                        tokenStream.Advance();
                        if (AdvanceIfTokenType(StyleTokenType.Not)) {
                            groupOperatorStack.Push(StyleOperatorType.Not);
                        }

                        ParseAttributeOrExpressionGroup();
                        break;

                    case StyleTokenType.BracketOpen:
                        tokenStream.Advance();
                        ParseStateOrAttributeGroup(styleRootNode);

                        break;

                    case StyleTokenType.Identifier:
                        ParseProperty(styleRootNode);
                        break;

                    case StyleTokenType.BracesOpen: {
                        // At this point only unconsumed attribute/expression group bodies are allowed

                        if (groupExpressionStack.Count > 1) {
                            throw new ParseException(tokenStream.Current, "There was a problem, I somehow made an error parsing a combined style group...");
                        }

                        if (groupExpressionStack.Count == 1) {
                            AttributeGroupContainer attributeGroupContainer = groupExpressionStack.Pop();
                            ParseStyleGroupBody(attributeGroupContainer);
                            styleRootNode.AddChildNode(attributeGroupContainer);
                        }
                        else {
                            throw new ParseException(tokenStream.Current, "Expected an attribute style group body. Braces are in a weird position!");
                        }

                        break;
                    }
                    default:
                        throw new ParseException(tokenStream.Current, "Expected either a boolean group operator (not / and), the start" +
                                                                      " of a group (an open bracket) or a regular property identifier but found " +
                                                                      tokenStream.Current.styleTokenType + " with value " + tokenStream.Current.value);
                }
            }
        }

        private void ParseStateOrAttributeGroup(StyleGroupContainer styleRootNode) {
            switch (tokenStream.Current.styleTokenType) {
                // this is the state group
                case StyleTokenType.Identifier:
                    StyleStateContainer stateGroupRootNode = StyleASTNode.StateGroupRootNode(tokenStream.Current.value);

                    tokenStream.Advance();
                    AssertTokenTypeAndAdvance(StyleTokenType.BracketClose);
                    AssertTokenTypeAndAdvance(StyleTokenType.BracesOpen);

                    ParseProperties(stateGroupRootNode);

                    AssertTokenTypeAndAdvance(StyleTokenType.BracesClose);

                    styleRootNode.AddChildNode(stateGroupRootNode);

                    break;
                case StyleTokenType.AttributeSpecifier:
                    ParseAttributeGroup();
                    break;
                default:
                    throw new ParseException(tokenStream.Current, "Expected either a group state identifier (hover etc.)" +
                                                                  " or an attribute identifier (attr:...) but found " +
                                                                  tokenStream.Current.styleTokenType);
            }
        }

        private void ParseProperties(StyleGroupContainer styleRootNode) {
            while (tokenStream.HasMoreTokens && tokenStream.Current.styleTokenType != StyleTokenType.BracesClose) {
                ParseProperty(styleRootNode);
            }
        }

        private void ParseProperty(StyleGroupContainer styleRootNode) {
            string propertyName = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);
            AssertTokenTypeAndAdvance(StyleTokenType.Equal);

            PropertyNode propertyNode = StyleASTNode.PropertyNode(propertyName, ParsePropertyValue());
            styleRootNode.AddChildNode(propertyNode);
        }

        private StyleASTNode ParsePropertyValue() {
            StyleASTNode propertyValue = null;
            switch (tokenStream.Current.styleTokenType) {
                case StyleTokenType.Number:
                    StyleASTNode value = StyleASTNode.NumericLiteralNode(tokenStream.Current.value);
                    StyleASTNode unit = null;
                    tokenStream.Advance();
                    if (tokenStream.Current.styleTokenType != StyleTokenType.EndStatement) {
                        unit = StyleASTNode.UnitNode(AssertTokenTypeAndAdvance(StyleTokenType.Identifier));
                    }

                    propertyValue = StyleASTNode.MeasurementNode(value, unit);

                    break;
                case StyleTokenType.String:
                    propertyValue = StyleASTNode.StringLiteralNode(tokenStream.Current.value);
                    tokenStream.Advance();
                    break;
                case StyleTokenType.Identifier:
                    propertyValue = StyleASTNode.IdentifierNode(tokenStream.Current.value);
                    tokenStream.Advance();
                    break;
                case StyleTokenType.Rgba:
                    propertyValue = ParseRgba();
                    break;
                case StyleTokenType.Rgb:
                    propertyValue = ParseRgb();
                    break;
                case StyleTokenType.HashColor:
                    propertyValue = StyleASTNode.ColorNode(tokenStream.Current.value);
                    tokenStream.Advance();
                    break;
                case StyleTokenType.Url:
                    tokenStream.Advance();
                    AssertTokenTypeAndAdvance(StyleTokenType.ParenOpen);

                    StyleASTNode url = ParseLiteralOrReference(StyleTokenType.Identifier);
                    while (tokenStream.HasMoreTokens && !AdvanceIfTokenType(StyleTokenType.ParenClose)) {
                        StyleIdentifierNode urlIdentifier = (StyleIdentifierNode) url;
                        // advancing tokens no matter the type. We want to concatenate all identifiers and slashes of a path again.
                        urlIdentifier.name += tokenStream.Current.value;
                        tokenStream.Advance();
                    }

                    propertyValue = StyleASTNode.UrlNode(url);
                    break;
                case StyleTokenType.At:
                    propertyValue = ParseVariableReference();
                    break;
            }

            AssertTokenTypeAndAdvance(StyleTokenType.EndStatement);
            return propertyValue;
        }

        private StyleASTNode ParseVariableReference() {
            AdvanceIfTokenType(StyleTokenType.At);
            ReferenceNode referenceNode = StyleASTNode.ReferenceNode(AssertTokenTypeAndAdvance(StyleTokenType.Identifier));

            while (tokenStream.HasMoreTokens && AdvanceIfTokenType(StyleTokenType.Dot)) {
                referenceNode.AddChildNode(
                    StyleASTNode.DotAccessNode(
                        AssertTokenTypeAndAdvance(StyleTokenType.Identifier)
                    )
                );
            }

            return referenceNode;
        }

        private StyleASTNode ParseRgba() {
            AssertTokenTypeAndAdvance(StyleTokenType.Rgba);
            AssertTokenTypeAndAdvance(StyleTokenType.ParenOpen);

            StyleASTNode red = ParseLiteralOrReference(StyleTokenType.Number);
            AssertTokenTypeAndAdvance(StyleTokenType.Comma);

            StyleASTNode green = ParseLiteralOrReference(StyleTokenType.Number);
            AssertTokenTypeAndAdvance(StyleTokenType.Comma);

            StyleASTNode blue = ParseLiteralOrReference(StyleTokenType.Number);
            AssertTokenTypeAndAdvance(StyleTokenType.Comma);

            StyleASTNode alpha = ParseLiteralOrReference(StyleTokenType.Number);
            AssertTokenTypeAndAdvance(StyleTokenType.ParenClose);

            return StyleASTNode.RgbaNode(red, green, blue, alpha);
        }

        private StyleASTNode ParseRgb() {
            AssertTokenTypeAndAdvance(StyleTokenType.Rgb);
            AssertTokenTypeAndAdvance(StyleTokenType.ParenOpen);

            StyleASTNode red = ParseLiteralOrReference(StyleTokenType.Number);
            AssertTokenTypeAndAdvance(StyleTokenType.Comma);

            StyleASTNode green = ParseLiteralOrReference(StyleTokenType.Number);
            AssertTokenTypeAndAdvance(StyleTokenType.Comma);

            StyleASTNode blue = ParseLiteralOrReference(StyleTokenType.Number);
            AssertTokenTypeAndAdvance(StyleTokenType.ParenClose);

            return StyleASTNode.RgbNode(red, green, blue);
        }

        private StyleASTNode ParseLiteralOrReference(StyleTokenType literalType) {
            if (AdvanceIfTokenType(StyleTokenType.At)) {
                return ParseVariableReference();
            }

            string value = AssertTokenTypeAndAdvance(literalType);
            switch (literalType) {
                case StyleTokenType.String:
                    return StyleASTNode.StringLiteralNode(value);
                case StyleTokenType.Number:
                    return StyleASTNode.NumericLiteralNode(value);
                case StyleTokenType.Boolean:
                    return StyleASTNode.BooleanLiteralNode(value);
                case StyleTokenType.Identifier:
                    return StyleASTNode.IdentifierNode(value);
            }

            throw new ParseException(tokenStream.Current, $"Please add support for this type: {literalType}!");
        }

        private void AssertTokenType(StyleTokenType styleTokenType) {
            if (tokenStream.Current.styleTokenType != styleTokenType) {
                throw new ParseException(tokenStream.Current, $"Parsing stylesheet failed. Expected token '{styleTokenType}' but got '{tokenStream.Current.styleTokenType}'");
            }
        }

        private string AssertTokenTypeAndAdvance(StyleTokenType styleTokenType) {
            if (tokenStream.Current.styleTokenType != styleTokenType) {
                throw new ParseException(tokenStream.Current, $"Parsing stylesheet failed. Expected token '{styleTokenType}' but got '{tokenStream.Current.styleTokenType}'");
            }

            tokenStream.Advance();
            return tokenStream.Previous.value;
        }

        private bool AdvanceIfTokenType(StyleTokenType styleTokenType) {
            if (tokenStream.Current.styleTokenType == styleTokenType) {
                tokenStream.Advance();
                return true;
            }

            return false;
        }

        private void ParseAttributeOrExpressionGroup() {
            AssertTokenTypeAndAdvance(StyleTokenType.BracketOpen);

            switch (tokenStream.Current.styleTokenType) {
                case StyleTokenType.AttributeSpecifier:
                    ParseAttributeGroup();
                    break;
                case StyleTokenType.Dollar:
                    ParseStyleExpression();
                    // todo add style expression
                    break;
            }
        }

        private void ParseAttributeGroup() {
            AssertTokenTypeAndAdvance(StyleTokenType.AttributeSpecifier);
            AssertTokenTypeAndAdvance(StyleTokenType.Colon);
            string attributeIdentifier = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);
            string attributeValue = null;

            if (AdvanceIfTokenType(StyleTokenType.Equal)) {
                attributeValue = tokenStream.Current.value;
                tokenStream.Advance();
            }

            bool invert = groupOperatorStack.Count > 0 && groupOperatorStack.Pop() == StyleOperatorType.Not;

            AttributeGroupContainer andAttribute = groupExpressionStack.Count > 0 ? groupExpressionStack.Pop() : null;
            AttributeGroupContainer attributeGroupContainer =
                StyleASTNode.AttributeGroupRootNode(attributeIdentifier, attributeValue, invert, andAttribute);
            groupExpressionStack.Push(attributeGroupContainer);

            AssertTokenTypeAndAdvance(StyleTokenType.BracketClose);
        }

        private void ParseStyleExpression() {
            switch (tokenStream.Current.styleTokenType) {
                case StyleTokenType.Plus:
                    StyleASTNode.OperatorNode(StyleOperatorType.Plus);
                    break;
            }

            AssertTokenTypeAndAdvance(StyleTokenType.BracketClose);
        }

        private bool ParseOperatorExpression(out StyleOperatorNode operatorNode) {
            tokenStream.Save();

            if (!tokenStream.Current.IsOperator) {
                tokenStream.Restore();
                operatorNode = default;
                return false;
            }

            tokenStream.Advance();

            switch (tokenStream.Previous.styleTokenType) {
                case StyleTokenType.Plus:
                    operatorNode = StyleASTNode.OperatorNode(StyleOperatorType.Plus);
                    return true;

                case StyleTokenType.Minus:
                    operatorNode = StyleASTNode.OperatorNode(StyleOperatorType.Minus);
                    return true;

                case StyleTokenType.Times:
                    operatorNode = StyleASTNode.OperatorNode(StyleOperatorType.Times);
                    return true;

                case StyleTokenType.Divide:
                    operatorNode = StyleASTNode.OperatorNode(StyleOperatorType.Divide);
                    return true;

                case StyleTokenType.Mod:
                    operatorNode = StyleASTNode.OperatorNode(StyleOperatorType.Mod);
                    return true;

                case StyleTokenType.BooleanAnd:
                    operatorNode = StyleASTNode.OperatorNode(StyleOperatorType.BooleanAnd);
                    return true;

                case StyleTokenType.BooleanOr:
                    operatorNode = StyleASTNode.OperatorNode(StyleOperatorType.BooleanOr);
                    return true;

                case StyleTokenType.Equals:
                    operatorNode = StyleASTNode.OperatorNode(StyleOperatorType.Equals);
                    return true;

                case StyleTokenType.NotEquals:
                    operatorNode = StyleASTNode.OperatorNode(StyleOperatorType.NotEquals);
                    return true;

                case StyleTokenType.GreaterThan:
                    operatorNode = StyleASTNode.OperatorNode(StyleOperatorType.GreaterThan);
                    return true;

                case StyleTokenType.GreaterThanEqualTo:
                    operatorNode = StyleASTNode.OperatorNode(StyleOperatorType.GreaterThanEqualTo);
                    return true;

                case StyleTokenType.LessThan:
                    operatorNode = StyleASTNode.OperatorNode(StyleOperatorType.LessThan);
                    return true;

                case StyleTokenType.LessThanEqualTo:
                    operatorNode = StyleASTNode.OperatorNode(StyleOperatorType.LessThanEqualTo);
                    return true;

                case StyleTokenType.QuestionMark:
                    operatorNode = StyleASTNode.OperatorNode(StyleOperatorType.TernaryCondition);
                    return true;

                case StyleTokenType.Colon:
                    operatorNode = StyleASTNode.OperatorNode(StyleOperatorType.TernarySelection);
                    return true;

                case StyleTokenType.As: {
                    operatorNode = StyleASTNode.OperatorNode(StyleOperatorType.As);
                    return true;
                }

                default:
                    throw new Exception("Unknown op type");
            }
        }
    }

}
