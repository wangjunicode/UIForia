using UIForia.Style;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Parsing {
    
    internal struct AnimationNode {
        public RangeInt nameRange;
        public AnimationOptions options;
        public DeclarationId initBlockRoot;
        public ParseBlockId firstKeyFrame;
    }

    internal struct StyleNode {

        public RangeInt nameRange;
        public ParseBlockId rootBlockIndex;
        public bool exported;

    }

    internal struct ImportNode {

        public RangeInt filePath;
        public RangeInt alias;
        public LineInfo lineInfo;

    }

    internal struct ConstantNode {

        public RangeInt identifier;
        public RangeInt value;
        public bool exported;
        public LineInfo lineInfo;

    }

    internal struct MixinNode {

        public RangeInt nameRange;
        public ParseBlockId rootBlockIndex;
        public bool exported;
        public MixinVariableId variableIndex;
        public int variableCount;

    }

    internal enum BlockNodeType : ushort {

        Root,
        Attribute,
        State,
        Mixin,
        Selector,
        Condition,
        FirstChild,
        LastChild,
        OnlyChild,
        NoChildren,
        FocusWithin,
        OnlyWithTag,
        LastWithTag,
        FirstWithTag,
        TagName,
        NthChild,
        NthWithTag,
        ChildCount,
        AnimationDeclaration,
        AnimationKeyFrame,
    }

    internal struct ParseBlockNode {

        public BlockNodeType type;
        public bool invert;
        public DeclarationId lastDeclaration;
        public ParseBlockId nextSibling;
        public ParseBlockId firstChild;
        public StyleState stateRequirement;
        public BlockData blockData;

    }

}