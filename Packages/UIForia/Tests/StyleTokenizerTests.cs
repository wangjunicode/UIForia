using System.Collections.Generic;
using NUnit.Framework;
using UIForia.Parsing.Style.Tokenizer;

[TestFixture]
public class StyleTokenizerTests {
    [Test]
    public void TokenizeExport() {
        List<StyleToken> tokens = StyleTokenizer.Tokenize("export const color0 : Color = rgba(1, 0, 0, 1);");
        
        AssertTokenTypes(new List<StyleTokenType>() {
            StyleTokenType.Export,
            StyleTokenType.Const,
            StyleTokenType.Identifier,
            StyleTokenType.Colon,
            StyleTokenType.VariableType,
            StyleTokenType.Equal,
            StyleTokenType.Rgba,
            StyleTokenType.ParenOpen,
            StyleTokenType.Number,
            StyleTokenType.Comma,
            StyleTokenType.Number,
            StyleTokenType.Comma,
            StyleTokenType.Number,
            StyleTokenType.Comma,
            StyleTokenType.Number,
            StyleTokenType.ParenClose,
            StyleTokenType.EndStatement,
        }, tokens);
    }

    [Test]
    public void TokenizeHashColor() {
        List<StyleToken> tokens = StyleTokenizer.Tokenize(@"BackgrundColor = #1A2B3C4D");
        AssertTokenTypes(new List<StyleTokenType>() {
                StyleTokenType.Identifier,
                StyleTokenType.Equal,
                StyleTokenType.HashColor,
        }, tokens);
    }

    [Test]
    public void TokenizeImport() {
        List<StyleToken> tokens = StyleTokenizer.Tokenize(@"import vars as Constants from ""file"";");
        
        AssertTokenTypes(new List<StyleTokenType>() {
            StyleTokenType.Import,
            StyleTokenType.Identifier,
            StyleTokenType.As,
            StyleTokenType.Identifier,
            StyleTokenType.From,
            StyleTokenType.String,
            StyleTokenType.EndStatement,
        }, tokens);
    }

    [Test]
    public void SkipCommentsAndParseStyleKeyword() {
        List<StyleToken> tokens = StyleTokenizer.Tokenize(@"
            // just a comment, ignore me please
            style goodStyle {  }
        ");

        AssertTokenTypes(new List<StyleTokenType>() {
            StyleTokenType.Style,
            StyleTokenType.Identifier,
            StyleTokenType.BracesOpen,
            StyleTokenType.BracesClose,
        }, tokens);
    }

    [Test]
    public void AttributeBlockTest() {
        List<StyleToken> tokens = StyleTokenizer.Tokenize(@"
            [attr:attrName=""value""]
        ");

        AssertTokenTypes(new List<StyleTokenType>() {
            StyleTokenType.BracketOpen,
            StyleTokenType.AttributeSpecifier,
            StyleTokenType.Colon,
            StyleTokenType.Identifier,
            StyleTokenType.Equal,
            StyleTokenType.String,
            StyleTokenType.BracketClose,
        }, tokens);
    }

    [Test]
    public void TagGroupWithAttributeTest() {
        List<StyleToken> tokens = StyleTokenizer.Tokenize(@"
            <TagName> and [attr:other] {}
        ");

        AssertTokenTypes(new List<StyleTokenType>() {
            StyleTokenType.LessThan,
            StyleTokenType.Identifier,
            StyleTokenType.GreaterThan,
            StyleTokenType.And,
            StyleTokenType.BracketOpen,
            StyleTokenType.AttributeSpecifier,
            StyleTokenType.Colon,
            StyleTokenType.Identifier,
            StyleTokenType.BracketClose,
            StyleTokenType.BracesOpen,
            StyleTokenType.BracesClose,
        }, tokens);
    }
    
    [Test]
    public void GroupWithExpression() {
        List<StyleToken> tokens = StyleTokenizer.Tokenize(@"
              not [$siblingIndex % 2 == 0] {
                  @use styleNameHere;
              }
        ");

        AssertTokenTypes(new List<StyleTokenType>() {
            StyleTokenType.Not,
            StyleTokenType.BracketOpen,
            StyleTokenType.Identifier,
            StyleTokenType.Mod,
            StyleTokenType.Number,
            StyleTokenType.Equals,
            StyleTokenType.Number,
            StyleTokenType.BracketClose,
            StyleTokenType.BracesOpen,
            StyleTokenType.At,
            StyleTokenType.Use,
            StyleTokenType.Identifier,
            StyleTokenType.EndStatement,
            StyleTokenType.BracesClose,
        }, tokens);
    }

    [Test]
    public void StyleWithProperty() {
        List<StyleToken> tokens = StyleTokenizer.Tokenize(@"
              style mystyle {
                  MarginTop = 10px;
              }
        ");
        
        AssertTokenTypes(new List<StyleTokenType>() {
            StyleTokenType.Style,
            StyleTokenType.Identifier,
            StyleTokenType.BracesOpen,
            StyleTokenType.Identifier,
            StyleTokenType.Equal,
            StyleTokenType.Number,
            StyleTokenType.Identifier,
            StyleTokenType.EndStatement,
            StyleTokenType.BracesClose,
        }, tokens);
    }

    private static void AssertTokenTypes(List<StyleTokenType> expected, List<StyleToken> actual) {
        Assert.AreEqual(expected.Count, actual.Count);
        for (int i = 0; i < expected.Count; i++) {
            Assert.AreEqual(expected[i], actual[i].styleTokenType);
        }
    }
}
