
using System.Collections.Generic;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers.Style {

    public class StyleSheetCompiler {
        
        private LightList<StyleConstants> constants;
        private LightList<UIStyleGroup> importedGroups;

        public StyleSheetCompiler() {
            constants = LightListPool<StyleConstants>.Get();
            importedGroups = LightListPool<UIStyleGroup>.Get();
        }

        public StyleSheet Compile(List<StyleASTNode> rootNodes) {

            StyleSheet styleSheet = new StyleSheet();
            
            for (int index = 0; index < rootNodes.Count; index++) {
                
                switch (rootNodes[index]) {
                    case StyleRootNode styleRoot:

                        UIStyleGroup styleGroup = new UIStyleGroup();
                        
                        
                        
                        styleSheet.styleGroups.Add(styleGroup);
                        
                        break;
                    case ExportNode exportNode:
                        break;
                    case ImportNode importNode:
                        break;
                    case ConstNode constNode:
                        constants.Add(new StyleConstants());
                        break;
                }
            }

            return null;
        }

        private LightList<UIStyleGroup> CompileStyleGroup(StyleRootNode styleRoot) {

            UIStyleGroup defaultGroup = new UIStyleGroup();
            defaultGroup.name = styleRoot.identifier ?? styleRoot.tagName;
            LightList<UIStyleGroup> result = LightListPool<UIStyleGroup>.Get();
            foreach (var node in styleRoot.children) {
                switch (node) {
                    case PropertyNode propertyNode:
                        // add to normal ui style set

                        // StylePropertyMappers.MapProperty(defaultGroup.normal, propertyNode.propertyName, propertyNode.propertyValue);
                        
                        break;
                    case AttributeGroupContainer attribute:
                        foreach (var uiStyleGroup in result) {
                            // is attribute name already in here?
                            
                        }
                        break;
                    case StyleStateContainer styleContainer:
                        break;
                    
                    default:
                        throw new ParseException($"You cannot have a {node} at this level.");
                }
            }

            return result;
        }
        
        
        private Color CompileRgbaNode(RgbaNode rgbaNode) {
            byte red = (byte) CompileToNumber(rgbaNode.red);
            byte green = (byte) CompileToNumber(rgbaNode.green);
            byte blue = (byte) CompileToNumber(rgbaNode.blue);
            byte alpha = (byte) CompileToNumber(rgbaNode.alpha);

            return new Color32(red, green, blue, alpha);
        }

        private Color CompileRgbNode(RgbNode rgbaNode) {
            byte red = (byte) CompileToNumber(rgbaNode.red);
            byte green = (byte) CompileToNumber(rgbaNode.green);
            byte blue = (byte) CompileToNumber(rgbaNode.blue);

            return new Color32(red, green, blue, 255);
        }

        private StyleASTNode ResolveReference(ReferenceNode reference) {
            foreach (var constant in constants) {
                if (constant.name == reference.referenceName) {
                    // todo this should resolve a ref and figure out type and all that jazz
                }
            }
            
            throw new ParseException($"Could not resolve reference {reference}");
        }

        private int CompileToNumber(StyleASTNode node) {

            if (node is ReferenceNode) {
                node = ResolveReference((ReferenceNode) node);
            }
            
            if (node.type == StyleASTNodeType.NumericLiteral) {
                return int.Parse(((StyleLiteralNode) node).rawValue);
            }

            throw new ParseException($"Expected a numeric value but all I got was this lousy {node}");
        }

    }
}
