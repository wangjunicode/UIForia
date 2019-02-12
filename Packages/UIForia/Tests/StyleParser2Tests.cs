using NUnit.Framework;
using UIForia.Parsing.Style;
using UIForia.Style.Parsing;

[TestFixture]
public class StyleParser2Tests {

    [Test]
    public void ParseSimpleStyle() {
        var nodes = StyleParser2.Parse(@"
            style simple {
                MarginTop = 10px;
            }
        ");

        Assert.AreEqual(1, nodes.Count);
        var rootNode = ((StyleRootNode) nodes[0]);
        Assert.AreEqual("simple", rootNode.identifier);
        Assert.AreEqual(null, rootNode.tagName);
        Assert.AreEqual(1, rootNode.children.Count);

        var propertyNode = rootNode.children[0];
        Assert.AreEqual(StyleASTNodeType.Property, propertyNode.type);
        
        var typedPropertyNode = (((PropertyNode) propertyNode));
        Assert.AreEqual("MarginTop", typedPropertyNode.propertyName);
        Assert.AreEqual(2, typedPropertyNode.propertyValueParts.Count);
        Assert.AreEqual(StyleASTNodeType.NumericLiteral, typedPropertyNode.propertyValueParts[0].type);
        Assert.AreEqual(StyleASTNodeType.Unit, typedPropertyNode.propertyValueParts[1].type);
    }

    [Test]
    public void ParseColorProperty() {
        var nodes = StyleParser2.Parse(@"
            style withBg {
                BackgroundColor = rgba(10, 20, 30, 40);
            }
        ");
        
        Assert.AreEqual(1, nodes.Count);
        var rootNode = ((StyleRootNode) nodes[0]);
        Assert.AreEqual("withBg", rootNode.identifier);
        Assert.AreEqual(null, rootNode.tagName);
        Assert.AreEqual(1, rootNode.children.Count);

        var property = (((PropertyNode) rootNode.children[0]));
        Assert.AreEqual("BackgroundColor", property.propertyName);
        Assert.AreEqual(1, property.propertyValueParts.Count);
        Assert.AreEqual(StyleASTNodeType.Rgba, property.propertyValueParts[0].type);

        var rgbaNode = (RgbaNode) property.propertyValueParts[0];
        Assert.AreEqual(StyleASTNodeType.Rgba, rgbaNode.type);
        Assert.AreEqual(StyleASTNode.NumericLiteralNode("10"), rgbaNode.red);
        Assert.AreEqual(StyleASTNode.NumericLiteralNode("20"), rgbaNode.green);
        Assert.AreEqual(StyleASTNode.NumericLiteralNode("30"), rgbaNode.blue);
        Assert.AreEqual(StyleASTNode.NumericLiteralNode("40"), rgbaNode.alpha);
    }

    [Test]
    public void ParseRgbColorProperty() {
        var nodes = StyleParser2.Parse(@"
            style withBg {
                BackgroundColor = rgb(10, 20, 30);
            }
        ");
        
        Assert.AreEqual(1, nodes.Count);
        var rootNode = ((StyleRootNode) nodes[0]);
        Assert.AreEqual("withBg", rootNode.identifier);
        Assert.AreEqual(null, rootNode.tagName);
        Assert.AreEqual(1, rootNode.children.Count);

        var property = (((PropertyNode) rootNode.children[0]));
        Assert.AreEqual("BackgroundColor", property.propertyName);
        Assert.AreEqual(1, property.propertyValueParts.Count);
        Assert.AreEqual(StyleASTNodeType.Rgb, property.propertyValueParts[0].type);

        var rgbNode = (RgbNode) property.propertyValueParts[0];
        Assert.AreEqual(StyleASTNodeType.Rgb, rgbNode.type);
        Assert.AreEqual(StyleASTNode.NumericLiteralNode("10"), rgbNode.red);
        Assert.AreEqual(StyleASTNode.NumericLiteralNode("20"), rgbNode.green);
        Assert.AreEqual(StyleASTNode.NumericLiteralNode("30"), rgbNode.blue);
    }

    [Test]
    public void ParseUrl() {
        var nodes = StyleParser2.Parse(@"
            style withBg {
                Background = url(path);
            }
        ");
        
        Assert.AreEqual(1, nodes.Count);
        var rootNode = ((StyleRootNode) nodes[0]);
        Assert.AreEqual("withBg", rootNode.identifier);
        Assert.AreEqual(null, rootNode.tagName);
        Assert.AreEqual(1, rootNode.children.Count);

        var property = (((PropertyNode) rootNode.children[0]));
        Assert.AreEqual("Background", property.propertyName);
        Assert.AreEqual(1, property.propertyValueParts.Count);
        Assert.AreEqual(StyleASTNodeType.Url, property.propertyValueParts[0].type);

        var urlNode = (UrlNode) property.propertyValueParts[0];
        Assert.AreEqual(StyleASTNodeType.Url, urlNode.type);
        Assert.AreEqual(StyleASTNode.IdentifierNode("path"), urlNode.url);
    }

    [Test]
    public void ParsePropertyWithReference() {
        var nodes = StyleParser2.Parse(@"
            style hasReferenceToBackgroundImagePath {
                Background = url(@pathRef);
            }
        ");

        Assert.AreEqual(1, nodes.Count);
        var property = (((PropertyNode) ((StyleRootNode) nodes[0]).children[0]));
        Assert.AreEqual("Background", property.propertyName);
        Assert.AreEqual(1, property.propertyValueParts.Count);
        Assert.AreEqual(StyleASTNodeType.Url, property.propertyValueParts[0].type);

        var urlNode = (UrlNode) property.propertyValueParts[0];
        Assert.AreEqual(StyleASTNodeType.Url, urlNode.type);
        Assert.AreEqual(StyleASTNode.ReferenceNode("pathRef"), urlNode.url);
    }

    [Test]
    public void ParseStyleState() {
        var nodes = StyleParser2.Parse(@"
            style hasBackgroundOnHover {
                [hover] { Background = url(@pathRef); }
            }
        ");

        Assert.AreEqual(1, nodes.Count);
        var stateGroupContainer = (((StyleContainer) ((StyleRootNode) nodes[0]).children[0]));
        Assert.AreEqual("hover", stateGroupContainer.identifier);
        
        var property = (PropertyNode) stateGroupContainer.children[0];
        Assert.AreEqual("Background", property.propertyName);
        Assert.AreEqual(1, property.propertyValueParts.Count);
        Assert.AreEqual(StyleASTNodeType.Url, property.propertyValueParts[0].type);

        var urlNode = (UrlNode) property.propertyValueParts[0];
        Assert.AreEqual(StyleASTNodeType.Url, urlNode.type);
        Assert.AreEqual(StyleASTNode.ReferenceNode("pathRef"), urlNode.url);
    }

    [Test]
    public void ParseAttributeGroup() {
        var nodes = StyleParser2.Parse(@"
            style hasBackgroundOnHover {
                [attr:attrName] { Background = url(@pathRef); }
            }
        ");

        Assert.AreEqual(1, nodes.Count);
        var attributeGroupContainer = (((AttributeGroupContainer) ((StyleRootNode) nodes[0]).children[0]));
        Assert.AreEqual("attrName", attributeGroupContainer.identifier);
        
        var property = (PropertyNode) attributeGroupContainer.children[0];
        Assert.AreEqual("Background", property.propertyName);
        Assert.AreEqual(1, property.propertyValueParts.Count);
        Assert.AreEqual(StyleASTNodeType.Url, property.propertyValueParts[0].type);

        var urlNode = (UrlNode) property.propertyValueParts[0];
        Assert.AreEqual(StyleASTNodeType.Url, urlNode.type);
        Assert.AreEqual(StyleASTNode.ReferenceNode("pathRef"), urlNode.url);
    }
    
    [Test]
    public void ParseEmptyGroups() {
        var nodes = StyleParser2.Parse(@"
            style hasBackgroundOnHover {
                [attr:attrName] { }
                [hover] {}
            }
        ");

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual(2, ((StyleRootNode) nodes[0]).children.Count);
        
        var attributeGroupContainer = (((AttributeGroupContainer) ((StyleRootNode) nodes[0]).children[0]));
        var stateGroupContainer = (((StyleContainer) ((StyleRootNode) nodes[0]).children[1]));
        Assert.AreEqual("attrName", attributeGroupContainer.identifier);
        Assert.AreEqual(0, attributeGroupContainer.children.Count);
        
        Assert.AreEqual("hover", stateGroupContainer.identifier);
        Assert.AreEqual(0, stateGroupContainer.children.Count);
    }

    [Test]
    public void ParseAttributeGroupWithStateGroup() {
        var nodes = StyleParser2.Parse(@"
            style mixingItAllUp {
                TextColor = green;
                [attr:attrName] { 
                    Background = url(@pathRef); 
                    [hover] {
                        TextColor = red;
                        TextColor = yellow;
                    }
                    TextColor = blue;
                }
                MarginTop = 10px;
            }
            style mixingItAllUp2 {
                TextColor = green;
                [attr:attrName] { 
                    Background = url(@pathRef); 
                    [hover] {
                        TextColor = red;
                    }
                    TextColor = blue;
                }
                MarginTop = 10px;
            }
        ");

        // there should be two style nodes
        Assert.AreEqual(2, nodes.Count);
        
        // ...3 nodes in a style
        var styleChildren = ((StyleRootNode) nodes[0]).children;
        Assert.AreEqual(3, styleChildren.Count);

        
        // first node is the property color = green
        var property1 = (PropertyNode) styleChildren[0];
        Assert.AreEqual("TextColor", property1.propertyName);
        Assert.AreEqual(StyleASTNodeType.Identifier, property1.propertyValueParts[0].type);

        // next the attribute group that in turn has 3 children
        var attributeGroupContainer = (((AttributeGroupContainer) styleChildren[1]));
        Assert.AreEqual("attrName", attributeGroupContainer.identifier);
        
        // and the trailing margin property is the third of the style's properties 
        var property2 = (PropertyNode) styleChildren[2];
        Assert.AreEqual("MarginTop", property2.propertyName);
        Assert.AreEqual(2, property2.propertyValueParts.Count);
        Assert.AreEqual(StyleASTNodeType.NumericLiteral, property2.propertyValueParts[0].type);
        Assert.AreEqual(StyleASTNodeType.Unit, property2.propertyValueParts[1].type);

        
        // now assert the existence of the three attribute group children
        Assert.AreEqual(3, attributeGroupContainer.children.Count);
        var attrProperty1 = (PropertyNode) attributeGroupContainer.children[0];
        var stateGroup = (StyleContainer) attributeGroupContainer.children[1];
        var attrProperty2 = (PropertyNode) attributeGroupContainer.children[2];

        // assert values for attr property 1
        Assert.AreEqual("Background", attrProperty1.propertyName);
        Assert.AreEqual(1, attrProperty1.propertyValueParts.Count);
        Assert.AreEqual(StyleASTNodeType.Url, attrProperty1.propertyValueParts[0].type);

        var urlNode = (UrlNode) attrProperty1.propertyValueParts[0];
        Assert.AreEqual(StyleASTNodeType.Url, urlNode.type);
        Assert.AreEqual(StyleASTNode.ReferenceNode("pathRef"), urlNode.url);
        
        // assert values for attr property 2
        Assert.AreEqual("TextColor", attrProperty2.propertyName);
        Assert.AreEqual(StyleASTNodeType.Identifier, attrProperty2.propertyValueParts[0].type);
        
        // assert the state group
        Assert.AreEqual("hover", stateGroup.identifier);
        // just asserting that multiple properties in a state group can be a thing
        Assert.AreEqual(2, stateGroup.children.Count);
        var stateGroupChild = (PropertyNode) stateGroup.children[0];
        Assert.AreEqual("TextColor", stateGroupChild.propertyName);
        Assert.AreEqual(StyleASTNodeType.Identifier, stateGroupChild.propertyValueParts[0].type);
    }


    [Test]
    public void ParseExportKeyword() {
        var nodes = StyleParser2.Parse(@"
            export const col1 : Color = rgba(100, 100, 100, 100);
        ");
        
        // there should be two style nodes
        Assert.AreEqual(1, nodes.Count);
        var styleChildren = ((StyleRootNode) nodes[0]).children;
        Assert.AreEqual(3, styleChildren.Count);
    }
}
