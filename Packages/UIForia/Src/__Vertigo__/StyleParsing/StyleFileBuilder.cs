using System;
using UIForia.Style;
using UIForia.Util;
using UnityEngine;

namespace UIForia.NewStyleParsing {

    [Serializable]
    public struct StaticPropertyKey {

        public PropertyId propertyId;
        public ushort conditionDepth;
        public ushort conditionId;

    }

    public class StyleFileBuilder {

        public StructList<StyleASTNode> nodeList;
        public StructList<StyleNode> styleNodeList;
        public StructList<PropertyDefinition> propertyDefinitions;

        public StyleFileBuilder() {
            this.nodeList = new StructList<StyleASTNode>(64);
            this.styleNodeList = new StructList<StyleNode>(32);
            this.propertyDefinitions = new StructList<PropertyDefinition>(256);
        }

        public ParsedStyleFile Build() {
            
            ParsedStyleFile retn = new ParsedStyleFile {
                nodeList = nodeList.ToArray(),
                styleNodeList = styleNodeList.ToArray(),
                propertyDefinitions = propertyDefinitions.ToArray()
            };

            nodeList.size = 0;
            styleNodeList.size = 0;
            propertyDefinitions.size = 0;

            return retn;
        }

        public void AddPropertyShorthandNode(int parentIndex, in CharSpan propertyIdSpan, in CharSpan valueSpan) {
            int nodeId = CreateNode(parentIndex, StyleNodeType.Property, propertyIdSpan.GetContentRange());

            propertyDefinitions.Add(new PropertyDefinition() {
                nodeIndex = nodeId,
                propertyId = -1,
                propertyName = propertyIdSpan.ToString(),
                propertyValue = valueSpan.ToString(),
                valueContentRange = valueSpan.GetContentRange()
            });

        }

        public void AddPropertyNode(int parentIndex, in PropertyParseInfo parseInfo) {

            StyleNodeType nodeType = StyleNodeType.Property;

            if (parseInfo.isShorthand) {
                nodeType = StyleNodeType.ShorthandProperty;
            }

            int nodeId = CreateNode(parentIndex, nodeType, parseInfo.identifierSpan.GetContentRange());

            propertyDefinitions.Add(new PropertyDefinition() {
                nodeIndex = nodeId,
                propertyId = parseInfo.propertyId,
                propertyName = parseInfo.propertyName,
                propertyValue = parseInfo.valueSpan.ToString(),
                valueContentRange = parseInfo.valueSpan.GetContentRange()
            });

        }

        public NodeRef AddStyleNode(int parentIndex, CharSpan styleNameSpan, CharSpan extendNameSpan = default) {

            int nodeId = CreateNode(parentIndex, StyleNodeType.Style, styleNameSpan.GetContentRange());

            styleNodeList.Add(new StyleNode() {
                index = nodeId,
                extendName = default,
                styleName = styleNameSpan.ToString()
            });

            return new NodeRef(nodeId, this);
        }

        public NodeRef AddStyleStateNode(int parentIndex, StyleState2 state) {
            StyleNodeType nodeType;

            switch (state) {

                case StyleState2.Normal:
                    nodeType = StyleNodeType.State_Normal;
                    break;

                case StyleState2.Hover:
                    nodeType = StyleNodeType.State_Hover;
                    break;

                case StyleState2.Focused:
                    nodeType = StyleNodeType.State_Focused;
                    break;

                case StyleState2.Active:
                    nodeType = StyleNodeType.State_Active;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            int nodeId = CreateNode(parentIndex, nodeType, default); // line info?
            return new NodeRef(nodeId, this);
        }

        private int CreateNode(int parentIndex, StyleNodeType nodeType, RangeInt contentRange) {
            int id = nodeList.size;
            nodeList.Add(new StyleASTNode() {
                parentIndex = -1,
                index = id,
                childCount = 0,
                nodeType = nodeType,
                contentRange = contentRange,
                firstChildIndex = -1,
                nextSiblingIndex = -1
            });
            AddChildNode(parentIndex, id);
            return id;
        }

        private void AddChildNode(int parentIndex, int childIndex) {
            ref StyleASTNode parentNode = ref nodeList.array[parentIndex];

            if (parentNode.childCount == 0) {
                parentNode.firstChildIndex = childIndex;
            }
            else {

                int ptr = parentNode.firstChildIndex;
                // could also keep last child index and not have to loop here. 
                while (true) {
                    ref StyleASTNode child = ref nodeList.array[ptr];
                    if (child.nextSiblingIndex < 0) {
                        child.nextSiblingIndex = childIndex;
                        break;
                    }

                    ptr = child.nextSiblingIndex;
                }

            }

            parentNode.childCount++;
        }

        public NodeRef CreateRoot() {
            nodeList.Add(new StyleASTNode() {
                parentIndex = -1,
                index = 0,
                childCount = 0,
                nodeType = StyleNodeType.Root,
                contentRange = default,
                firstChildIndex = -1,
                nextSiblingIndex = -1
            });
            return new NodeRef(0, this);
        }

    }

}