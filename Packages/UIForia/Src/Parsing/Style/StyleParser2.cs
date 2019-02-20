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
                    AssertTokenTypeAndAdvance(StyleTokenType.BracesOpen);
                    AssertTokenTypeAndAdvance(StyleTokenType.BracesClose);
                    break;

                case StyleTokenType.Animation:
                    tokenStream.Advance();
                    // ParseAnimation
                    AssertTokenTypeAndAdvance(StyleTokenType.BracesOpen);
                    AssertTokenTypeAndAdvance(StyleTokenType.BracesClose);
                    tokenStream.Advance();
                    break;

                case StyleTokenType.Import:
                    ParseImportNode();
                    break;

                case StyleTokenType.Export:
                    ParseExportNode();
                    break;

                case StyleTokenType.Const:
                    nodes.Add(ParseConstNode());
                    AdvanceIfTokenType(StyleTokenType.EndStatement);
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
            StyleToken initialStyleToken = tokenStream.Current;
            switch (initialStyleToken.styleTokenType) {
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
                    throw new ParseException(initialStyleToken, $"Expected style definition or tag name but found {initialStyleToken.styleTokenType}");
            }

            StyleRootNode styleRootNode = StyleASTNodeFactory.StyleRootNode(identifier, tagName);
            styleRootNode.WithLocation(initialStyleToken);
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
            StyleToken exportToken = tokenStream.Current;
            tokenStream.Advance();
            // export statement must be followed by const keyword

            // now let's find out which value we're assigning
            nodes.Add(StyleASTNodeFactory.ExportNode(ParseConstNode()).WithLocation(exportToken));
            AdvanceIfTokenType(StyleTokenType.EndStatement);
        }

        private ConstNode ParseConstNode() {
            StyleToken constToken = tokenStream.Current;
            AssertTokenTypeAndAdvance(StyleTokenType.Const);
            // const name
            string variableName = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);
            AssertTokenTypeAndAdvance(StyleTokenType.Equal);

            ConstNode constNode = StyleASTNodeFactory.ConstNode(variableName, ParsePropertyValue());
            constNode.WithLocation(constToken);
            return constNode;
        }

        private void ParseImportNode() {
            StyleToken importToken = tokenStream.Current;
            AssertTokenTypeAndAdvance(StyleTokenType.Import);
            string source = AssertTokenTypeAndAdvance(StyleTokenType.String);
            AssertTokenTypeAndAdvance(StyleTokenType.As);

            string alias = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);
            AssertTokenTypeAndAdvance(StyleTokenType.EndStatement);

            nodes.Add(StyleASTNodeFactory.ImportNode(alias, source).WithLocation(importToken));
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
                    StyleStateContainer stateGroupRootNode = StyleASTNodeFactory.StateGroupRootNode(tokenStream.Current);

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
                                                                  " or an attribute identifier (attr:...)");
            }
        }

        private void ParseProperties(StyleGroupContainer styleRootNode) {
            while (tokenStream.HasMoreTokens && tokenStream.Current.styleTokenType != StyleTokenType.BracesClose) {
                ParseProperty(styleRootNode);
            }
        }

        private void ParseProperty(StyleGroupContainer styleRootNode) {
            StyleToken propertyNodeToken = tokenStream.Current;
            string propertyName = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);
            AssertTokenTypeAndAdvance(StyleTokenType.Equal);

            PropertyNode propertyNode = StyleASTNodeFactory.PropertyNode(propertyName);
            propertyNode.WithLocation(propertyNodeToken);

            while (tokenStream.HasMoreTokens && !AdvanceIfTokenType(StyleTokenType.EndStatement)) {
                propertyNode.AddChildNode(ParsePropertyValue());
                // we just ignore the comma for now
                AdvanceIfTokenType(StyleTokenType.Comma);
            }

            styleRootNode.AddChildNode(propertyNode);
        }

        private StyleASTNode ParsePropertyValue() {
            StyleToken propertyToken = tokenStream.Current;
            StyleASTNode propertyValue;

            switch (tokenStream.Current.styleTokenType) {
                case StyleTokenType.Number:
                    StyleLiteralNode value = StyleASTNodeFactory.NumericLiteralNode(tokenStream.Current.value).WithLocation(propertyToken) as StyleLiteralNode;
                    tokenStream.Advance();
                    if (tokenStream.Current.styleTokenType != StyleTokenType.EndStatement) {
                        UnitNode unit = ParseUnit().WithLocation(tokenStream.Previous) as UnitNode;
                        propertyValue = StyleASTNodeFactory.MeasurementNode(value, unit);
                    }
                    else {
                        propertyValue = value;
                    }

                    break;
                case StyleTokenType.String:
                    propertyValue = StyleASTNodeFactory.StringLiteralNode(tokenStream.Current.value);
                    tokenStream.Advance();
                    break;
                case StyleTokenType.Identifier:
                    propertyValue = StyleASTNodeFactory.IdentifierNode(tokenStream.Current.value);
                    tokenStream.Advance();
                    break;
                case StyleTokenType.Rgba:
                    propertyValue = ParseRgba();
                    break;
                case StyleTokenType.Rgb:
                    propertyValue = ParseRgb();
                    break;
                case StyleTokenType.HashColor:
                    propertyValue = StyleASTNodeFactory.ColorNode(tokenStream.Current.value);
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

                    propertyValue = StyleASTNodeFactory.UrlNode(url);
                    break;
                case StyleTokenType.At:
                    propertyValue = ParseVariableReference();
                    break;
                default:
                    throw new ParseException(propertyToken, "Expected a property value but found no valid token.");
            }

            return propertyValue.WithLocation(propertyToken);
        }

        private UnitNode ParseUnit() {
            StyleToken styleToken = tokenStream.Current;
            string value = styleToken.value;

            tokenStream.Advance();

            switch (styleToken.styleTokenType) {
                case StyleTokenType.Identifier:
                    return StyleASTNodeFactory.UnitNode(value);
                case StyleTokenType.Mod:
                    return StyleASTNodeFactory.UnitNode(value);
                default:
                    throw new ParseException(styleToken, "Expected a token that looks like a unit but this didn't.");
            }
        }

        private StyleASTNode ParseVariableReference() {
            AdvanceIfTokenType(StyleTokenType.At);
            ReferenceNode referenceNode = StyleASTNodeFactory.ReferenceNode(AssertTokenTypeAndAdvance(StyleTokenType.Identifier));

            while (tokenStream.HasMoreTokens && AdvanceIfTokenType(StyleTokenType.Dot)) {
                referenceNode.AddChildNode(
                    StyleASTNodeFactory.DotAccessNode(
                        AssertTokenTypeAndAdvance(StyleTokenType.Identifier)
                    ).WithLocation(tokenStream.Previous)
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

            return StyleASTNodeFactory.RgbaNode(red, green, blue, alpha);
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

            return StyleASTNodeFactory.RgbNode(red, green, blue);
        }

        private StyleASTNode ParseLiteralOrReference(StyleTokenType literalType) {
            StyleToken currentToken = tokenStream.Current;
            if (AdvanceIfTokenType(StyleTokenType.At)) {
                return ParseVariableReference().WithLocation(currentToken);
            }

            string value = AssertTokenTypeAndAdvance(literalType);
            switch (literalType) {
                case StyleTokenType.String:
                    return StyleASTNodeFactory.StringLiteralNode(value).WithLocation(currentToken);
                case StyleTokenType.Number:
                    return StyleASTNodeFactory.NumericLiteralNode(value).WithLocation(currentToken);
                case StyleTokenType.Boolean:
                    return StyleASTNodeFactory.BooleanLiteralNode(value).WithLocation(currentToken);
                case StyleTokenType.Identifier:
                    return StyleASTNodeFactory.IdentifierNode(value).WithLocation(currentToken);
            }

            throw new ParseException(currentToken, $"Please add support for this type: {literalType}!");
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
            StyleToken attributeToken = tokenStream.Current;
            string attributeIdentifier = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);
            string attributeValue = null;

            if (AdvanceIfTokenType(StyleTokenType.Equal)) {
                attributeValue = tokenStream.Current.value;
                tokenStream.Advance();
            }

            bool invert = groupOperatorStack.Count > 0 && groupOperatorStack.Pop() == StyleOperatorType.Not;

            AttributeGroupContainer andAttribute = groupExpressionStack.Count > 0 ? groupExpressionStack.Pop() : null;
            AttributeGroupContainer attributeGroupContainer =
                StyleASTNodeFactory.AttributeGroupRootNode(attributeIdentifier, attributeValue, invert, andAttribute);
            attributeGroupContainer.WithLocation(attributeToken);

            groupExpressionStack.Push(attributeGroupContainer);

            AssertTokenTypeAndAdvance(StyleTokenType.BracketClose);
        }

        private void ParseStyleExpression() {
            switch (tokenStream.Current.styleTokenType) {
                case StyleTokenType.Plus:
                    StyleASTNodeFactory.OperatorNode(StyleOperatorType.Plus);
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
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.Plus);
                    return true;

                case StyleTokenType.Minus:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.Minus);
                    return true;

                case StyleTokenType.Times:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.Times);
                    return true;

                case StyleTokenType.Divide:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.Divide);
                    return true;

                case StyleTokenType.Mod:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.Mod);
                    return true;

                case StyleTokenType.BooleanAnd:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.BooleanAnd);
                    return true;

                case StyleTokenType.BooleanOr:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.BooleanOr);
                    return true;

                case StyleTokenType.Equals:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.Equals);
                    return true;

                case StyleTokenType.NotEquals:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.NotEquals);
                    return true;

                case StyleTokenType.GreaterThan:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.GreaterThan);
                    return true;

                case StyleTokenType.GreaterThanEqualTo:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.GreaterThanEqualTo);
                    return true;

                case StyleTokenType.LessThan:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.LessThan);
                    return true;

                case StyleTokenType.LessThanEqualTo:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.LessThanEqualTo);
                    return true;

                case StyleTokenType.QuestionMark:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.TernaryCondition);
                    return true;

                case StyleTokenType.Colon:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.TernarySelection);
                    return true;

                case StyleTokenType.As: {
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.As);
                    return true;
                }

                default:
                    throw new Exception("Unknown op type");
            }
        }
    }

}
