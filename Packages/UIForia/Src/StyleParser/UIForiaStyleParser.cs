using System;
using System.Linq;
using System.Runtime.InteropServices;
using UIForia.Compilers;
using UIForia.Rendering;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using UnityEngine;

/*
 
query my-query($tagName) {
    tag = $tagName;
    attr:variant != "value";
    state != hover;
    child-count >= 5;
    #if some constant condition
    #else 
    #end
}

1. what am i selecting?
2. from where?
3. then what do I do with that?

style has-stuff {

    select first-child {

    }

    select animating from descendents {

    }

    select * from children where (tag(<Div>, <Image>)) && child-count(> 5) && ![attr:value="hello"] {

    }

    select * from last-child {

    }

    a where clause can ask any question a when clause can ask 
    a where clause can chain the questions with boolean operators

    select filter from source ? where expressions
    expressions that could have been filters are lifted into filter list (phase 2 optimization probably)

    select .notification-ribbon from descendants {

    }

    select <Image> [attrname] from children {
        
    }
    
    select children {}
    
    select descendents {} get met just stuff in my template
    select descendents deeply {} get me absolutely everything
    select slot:name {}
    select slot:children {}
    
    SomethingWithAChildrenSlot.xml
    <Contents style="x">
        <Group>
            <Group>
                <Group>
                    <define:Children/>
                </Group>
            </Group>
        </Group>
            
    <SomethingWithAChildrenSlot style="x"> select children -> 
        ------- slot:children ------ 
        <Div>
        <Div>
        <Div>
        <Div>
        <Div>
    </SomethingWithAChildrenSlot>
        <define:Children>
        
    TagIndex
        Image -> 4, 5, 6
    
    AttrIndex
        mykey -> 8, 142, 52, 6, 4
        
    4, 5
    ok, 4 is a member, 5 is not
    
    4 
    
    filters -- indexable
        - tag
        - style name
        - attribute key
        - state
        
    where -- not indexable
        - attribute operator
        - child count
        
        
    selector
        has 1 source [children, descendents, slots]
        has n filter sets
            has n filters
            has 0-1 where clause
        has 0-1 where clause
        to pass 
    [when:child-count > 5] {
        
        select [mykey] from descendents where child-count > 5 {}
        
        mixin something()  {
        
            [variables] {
                %fontSize = 12px;
            }
            
            TextFontSize = %fontSize;
        }
             
        style h1 : BaseFont(24px) {
            
        }
        
        style h1 {
            
        }
        
        style h2 {
            FontSize = 22px;
        }
        
        init style a, b, c, d, e {
            
            123
            1
            23
            123
            1
            23
            123
            1
            23
            [hover] {}
        }
        
        style a {
            [hover] {
                
            }
        }

        run all filter parts
        
        filters -> can be indexed to make things zoom
        conditions -> need to be run
        
        select <items> child-count == 5 from children where [is-inventory-item] {} 
        
        select <InventoryItem> [attr contains "boobs"], .large-text [attr contains "boobs"] from descendents  {
        
        }       
        
        hover, focus, active
        
        states
            valid, invalid, is-empty, pristine, dirty, checked, selected, placeholder-shown
             
        style.SetElementState(hover) -> if hover active focus is set, ignore w/ warning
        
        styleSystem.SetStateInternal(hover)
            
        select tag(Input::empty)
        
        select :dirty, :invalid where <Input> {
            
        }

        select <Image> [is-playing] {
            
        }
        
        select styleId(module.styleName) {}
        select type(Input<string>) {}
         
        select <InventoryItem>, .large-
        
        select <InventoryItem> .m-bottom-sp1 child-count > 5 .m [at-capacity] from descendents {
            BorderColor = Blue;
        }
        
        select <Image> [mykey] parent-has-tag(Something),
               <Düv> [myotherkey] not(.style2),
               .style1 [my3rdkey] :hover child-count > 5 // within each filter set support not and and is implicit. no or support
               from descendents where parent-with-attr("hello") {
            
            [when:tag(Image)] {
                BackgroundColor = $color;
            }
            
        }
    }
    
    [when:child-count(> 5)] {

        select           
            :focus <Div> .style-name [attr] without([variant] :focus :hover), 
            :enabled <Div> .style-name [attr] without [variant] :focus :hover 
            from lexical-children {
            from children {
            from lexical-descendants {
            from descendants {
            
            [when:parent-has-attribute] {}

        }

    }

}

select @my-query(Image) deeply from descendents {
 
}

select descendents deeply where state != hover tag(Div) [name contains "value"] {

}

    tag(Div).WithAttr("name")

    from descendents where () => {


    }

select Div[attr], Image[attr] from descendents {}

select Div[attr], Image[attr] from children {

}
 
 
 */

internal unsafe struct PaletteProperty {

    public PropertyType propertyType;
    public fixed byte data[64];

}

internal unsafe struct StylePalette {

    private DataList<PaletteProperty> properties;
    
    public bool TryConvertToByte(ushort propertyIndex, out byte retn) {
        ref PaletteProperty property = ref properties.array[propertyIndex];

        if (property.propertyType == PropertyType.Byte) {
           byte* data = properties.array[propertyIndex].data;
           retn = *(byte*) data;
           return true;
        }

        retn = default;
        return false;

    }

}

namespace UIForia.Parsing {

    internal struct CustomPropertyDefinition {

        public RangeInt name;
        public RangeInt defaultValue;
        public RangeInt enumTypeName;
        public CustomPropertyType type;

    }

    internal struct DeclarationId {

        public int id;

        public DeclarationId(int id) {
            this.id = id;
        }

    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct BlockData {

        [FieldOffset(0)] public ChildCountData childCountData;
        [FieldOffset(0)] public NthChildData nthChildData;
        [FieldOffset(0)] public AttributeData attributeData;
        [FieldOffset(0)] public ConditionData conditionData;
        [FieldOffset(0)] public TagData tagData;
        [FieldOffset(0)] public MixinData mixinData;
        [FieldOffset(0)] public UITimeMeasurement keyFrameTime;

    }

    internal struct MixinData {

        public RangeInt nameRange;

    }

    internal struct TagData {

        public RangeInt tagNameRange;

    }

    internal struct ConditionData {

        public RangeInt conditionRange;

    }

    public struct AttributeData {

        public RangeInt attrKeyRange;
        public RangeInt attrValueRange;
        public AttributeOperator compareOp;

    }

    public struct NthChildData {

        public short stepSize;
        public short offset;

    }

    internal struct ChildCountData {

        public ChildCountOperator op;
        public int number;

    }

    internal struct MixinVariable {

        public RangeInt key;
        public RangeInt value;

    }

    internal class UIForiaStyleParser {

        public const int Version = 3;

        private bool hasCriticalError;
        private bool hasErrors;
        private bool allowMixinValues;
        private bool allowAnimationValues;
        private readonly StructList<MixinVariable> mixinVariables;
        private readonly StructList<ParseBlockNode> blockList;
        private readonly StructList<StyleNode> styleList;
        private readonly StructList<AnimationNode> animationList;
        private readonly StructList<MixinNode> mixinList;
        private readonly StructList<StyleDeclaration> declarationList;
        private readonly StructList<ImportNode> importList;
        private readonly StructList<ConstantNode> constantList;
        private readonly StructList<char> charBuffer;
        private readonly StructList<char> tmpCharBuffer;
        private readonly StructList<TransitionDeclaration> transitionList;
        private readonly StructStack<int> blockStack; // points into the blockList, used to avoid recursion while building a tree of blocks
        private readonly StructList<CustomPropertyDefinition> customPropertyDefinitions;

        private Diagnostics diagnostics;
        private string filePath;

        private static string[] stateNames = Enum.GetNames(typeof(StyleState)).Select(s => s.ToLower()).ToArray();
        private static StyleState[] stateValues = (StyleState[]) Enum.GetValues(typeof(StyleState));

        public UIForiaStyleParser(Diagnostics diagnostics = null) {
            this.diagnostics = diagnostics;
            this.mixinVariables = new StructList<MixinVariable>();
            this.blockList = new StructList<ParseBlockNode>();
            this.animationList = new StructList<AnimationNode>();
            this.declarationList = new StructList<StyleDeclaration>(256);
            this.charBuffer = new StructList<char>(1024);
            this.tmpCharBuffer = new StructList<char>(128);
            this.blockStack = new StructStack<int>();
            this.mixinList = new StructList<MixinNode>();
            this.importList = new StructList<ImportNode>();
            this.transitionList = new StructList<TransitionDeclaration>();
            this.constantList = new StructList<ConstantNode>();
            this.customPropertyDefinitions = new StructList<CustomPropertyDefinition>();
            this.styleList = new StructList<StyleNode>(64);
        }

        public bool TryParse(StyleFileShell result, string contents) {
            this.filePath = result.filePath;
            hasCriticalError = false;
            hasErrors = false;

            importList.size = 0;
            charBuffer.size = 0;
            tmpCharBuffer.size = 0;
            blockStack.size = 0;
            styleList.size = 0;
            mixinList.size = 0;
            constantList.size = 0;
            mixinVariables.size = 0;
            transitionList.size = 0;
            animationList.size = 0;
            customPropertyDefinitions.size = 0;

            // 0 is used as invalid
            blockList.size = 1;
            declarationList.size = 1;

            unsafe {
                fixed (char* ptr = contents) {
                    CharStream stream = new CharStream(ptr, 0, contents.Length);

                    while (!hasCriticalError && stream.HasMoreTokens) {
                        uint start = stream.Ptr;
                        stream.ConsumeWhiteSpaceAndComments();

                        if (stream.TryMatchRange("import")) {
                            ParseImport(ref stream);
                            continue;
                        }

                        if (stream.TryMatchRange("style")) {
                            ParseStyleDefinition(ref stream);
                            continue;
                        }

                        if (stream.TryMatchRange("mixin")) {
                            ParseMixinDefinition(ref stream);
                            continue;
                        }

                        if (stream.TryMatchRange("animation")) {
                            ParseAnimationDefinition(ref stream);
                            continue;
                        }

                        if (stream.TryMatchRange("material")) { }

                        if (stream.TryMatchRange("stylesheet")) {
                            ParseStyleSheetDefinition(ref stream);
                            continue;
                        }

                        if (stream.TryMatchRange("spritesheet")) { }

                        if (stream.TryMatchRange("property ")) {
                            ParseCustomPropertyDefinition(ref stream);
                            continue;
                        }

                        if (stream.TryMatchRange("palette ")) {
                            ParsePaletteDefinition(ref stream);
                            continue;
                        }

                        if (stream.TryMatchRange("texture")) { }

                        if (stream.TryMatchRange("sound")) { }

                        // todo -- not currently part of our api but maybe likely should be
                        if (stream.TryMatchRange("cursor")) { }

                        if (stream.TryMatchRange("const")) {
                            ParseConstantDefinition(ref stream);
                            continue;
                        }

                        if (stream.TryMatchRange("export")) {

                            stream.ConsumeWhiteSpaceAndComments();

                            if (stream.TryMatchRange("style")) {
                                ParseStyleDefinition(ref stream, true);
                                continue;
                            }

                            if (stream.TryMatchRange("mixin")) {
                                ParseMixinDefinition(ref stream, true);
                                continue;
                            }

                            if (stream.TryMatchRange("const")) {
                                ParseConstantDefinition(ref stream, true);
                                continue;
                            }
                        }

                        if (stream.Ptr == start) {
                            throw new NotImplementedException("Error handler fail parse");
                        }
                    }

                }
            }

            if (hasCriticalError) {
                result.hasParseErrors = true;
                result.isValid = false;
                return false;
            }

            // todo -- warn about & remove duplicate constants, mixins, styles, animations, etc

            // todo -- if has errors, maybe return null for everything and copy diagnostics into output instead
            unsafe {
                result.hasParseErrors = hasErrors;
                result.blocks = blockList.ToArray();
                result.animations = animationList.ToArray();
                result.styles = styleList.ToArray();
                result.charBuffer = TypedUnsafe.MallocCopy(charBuffer.array, charBuffer.size);
                result.charBufferSize = charBuffer.size;
                result.propertyDefinitions = declarationList.ToArray();
                result.constants = constantList.ToArray();
                result.imports = importList.ToArray();
                result.mixins = mixinList.ToArray();
                result.mixinVariables = mixinVariables.ToArray();
                result.transitions = transitionList.ToArray();
                result.customProperties = customPropertyDefinitions.ToArray();
                result.isValid = true;
                result.hasParseErrors = false;
            }

            return true;
        }

        private void ParsePaletteDefinition(ref CharStream stream) {

            if (!stream.TryParseIdentifier(out CharSpan paletteName)) {
                LogCriticalError("Expected an identifier after `palette` keyword", stream.GetLineInfo());
                return;
            }

            if (!stream.TryGetSubStream('{', '}', out CharStream paletteContents)) {
                LogCriticalError($"Expected a matching set of `{{` and `}}` after `palette {paletteName}`.", stream.GetLineInfo());
                return;
            }

            paletteContents = paletteContents.Trim();

            while (paletteContents.HasMoreTokens) {

                if (!paletteContents.TryParseIdentifier(out CharSpan typeName, false)) {
                    LogCriticalError("Expected a valid identifier defining custom property type", paletteContents.GetLineInfo());
                    return;
                }

                CustomPropertyType propertyType = CustomPropertyType.Invalid;

                typeName = typeName.Trim();
                RangeInt enumTypeRange = default;

                if (typeName == "enum") {
                    propertyType = CustomPropertyType.Enum;
                    if (!stream.TryGetSubStream('<', '>', out CharStream enumTypeStream)) {
                        LogCriticalError("Expected custom enum property definition to be followed by a type name in angle brackets. `property enum<typename> identifier = value`.", stream.GetLineInfo());
                        return;
                    }

                    enumTypeRange = AddString(new CharSpan(enumTypeStream.Trim()));

                }

                if (StyleDatabase.TryParseCustomPropertyTypeName(typeName, out propertyType)) {
                    
                }
                else {
                    // todo -- list the ones we allow
                    LogCriticalError($"Invalid custom property type `{typeName}`", stream.GetLineInfo());
                    return;
                }
                
                if (!stream.TryParseIdentifier(out CharSpan identifier, false)) {
                    LogCriticalError($"Expected palette property definition to be followed by a valid identifier.", stream.GetLineInfo());
                }

                if (!stream.TryParseCharacter('=')) {
                    LogCriticalError($"Expected palette property definition {identifier} to be followed by `=`", stream.GetLineInfo());
                    return;
                }

                if (!stream.TryGetSubstreamTo(';', out CharStream valueStream)) {
                    LogCriticalError($"Expected palette property definition {identifier} = to be terminated by a `;`", stream.GetLineInfo());
                    return;
                }

                
                customPropertyDefinitions.Add(new CustomPropertyDefinition() {
                    defaultValue = AddString(new CharSpan(valueStream).Trim()),
                    name = AddString(identifier.Trim()),
                    enumTypeName = enumTypeRange,
                    type = propertyType
                });
            }

        }


        struct PaletteProperty {

            public PropertyType propertyType;
            public RangeInt propertyName;
            public RangeInt propertyValue;

        }

        struct StylePalette {

            public RangeInt name;
            public RangeInt propertyBlocks;

        }

        private void ParseCustomPropertyDefinition(ref CharStream stream) {

            CustomPropertyType propertyType = CustomPropertyType.Invalid;

            if (!stream.TryParseIdentifier(out CharSpan typeName, false)) {
                LogCriticalError("Expected a valid identifier defining custom property type", stream.GetLineInfo());
                return;
            }

            RangeInt enumTypeRange = default;

            typeName = typeName.Trim();

            if (typeName == "enum") {
                propertyType = CustomPropertyType.Enum;
                if (!stream.TryGetSubStream('<', '>', out CharStream enumTypeStream)) {
                    LogCriticalError("Expected custom enum property definition to be followed by a type name in angle brackets. `property enum<typename> identifier = value`.", stream.GetLineInfo());
                    return;
                }

                enumTypeRange = AddString(new CharSpan(enumTypeStream.Trim()));

            }
            else if (StyleDatabase.TryParseCustomPropertyTypeName(typeName, out propertyType)) { }
            else {
                // todo -- list the ones we allow
                LogCriticalError($"Invalid custom property type `{typeName}`", stream.GetLineInfo());
                return;
            }

            if (!stream.TryParseIdentifier(out CharSpan identifier, false)) {
                LogCriticalError($"Expected custom property definition to be followed by a valid identifier.", stream.GetLineInfo());
            }

            if (!stream.TryParseCharacter('=')) {
                LogCriticalError($"Expected custom property definition {identifier} to be followed by `=`", stream.GetLineInfo());
                return;
            }

            if (!stream.TryGetSubstreamTo(';', out CharStream valueStream)) {
                LogCriticalError($"Expected custom property definition {identifier} = to be terminated by a `;`", stream.GetLineInfo());
                return;
            }

            customPropertyDefinitions.Add(new CustomPropertyDefinition() {
                defaultValue = AddString(new CharSpan(valueStream).Trim()),
                name = AddString(identifier.Trim()),
                enumTypeName = enumTypeRange,
                type = propertyType
            });

        }

        private void ParseStyleSheetDefinition(ref CharStream stream) {
            // stylesheet identifier { } 
            if (!stream.TryParseIdentifier(out CharSpan sheetName)) {
                LogCriticalError("Invalid stylesheet statement. Expected statement in the form `stylesheet identifier { ...body... };`", stream.GetLineInfo());
                return;
            }

            // todo -- assert not nested in another sheet

            throw new NotImplementedException();

        }

        public bool TryParse(string filePath, string contents, out StyleFileShell result) {
            result = new StyleFileShell(filePath);
            return TryParse(result, contents);
        }

        private void ParseImport(ref CharStream stream) {

            // import "SeedLib/SeedLibTheme.style" as theme;

            // todo -- import {thing1, thing2} from "module"; we know the location of the style file so we know the module it belongs to also

            if (!stream.TryParseDoubleQuotedString(out CharSpan importPath)) {
                LogCriticalError("Invalid import statement. Expected an import statement to follow the form `import \"path/to/your/file.style\" as yourAlias;`", stream.GetLineInfo());
                return;
            }

            if (!stream.TryMatchRange("as")) {
                LogCriticalError("Invalid import statement. Expected an import statement to follow the form `import \"path/to/your/file.style\" as yourAlias;`", stream.GetLineInfo());
                return;
            }

            if (!stream.TryParseIdentifier(out CharSpan identifier)) {
                LogCriticalError("Invalid import statement. Expected an import statement to follow the form `import \"path/to/your/file.style\" as yourAlias;`", stream.GetLineInfo());
                return;
            }

            if (!stream.TryParseCharacter(';')) {
                LogCriticalError($"Invalid import statement. Expected a terminating `;` after {identifier}", identifier.GetLineInfo());
                return;
            }

            importList.Add(new ImportNode() {
                alias = AddString(identifier),
                filePath = AddString(importPath.Trim())
            });

        }

        private void ParseConstantDefinition(ref CharStream stream, bool exported = false) {
            // const value = expression ;
            if (!stream.TryParseIdentifier(out CharSpan identifier)) {
                LogCriticalError("Expected a valid identifier name after `const` declaration.", stream.GetLineInfo());
                return;
            }

            if (!stream.TryParseCharacter('=')) {
                LogCriticalError($"Expected an `=` after `const {identifier}`.", stream.GetLineInfo());
                return;
            }

            if (!stream.TryGetStreamUntil(';', out CharStream valueStream)) {
                LogCriticalError("Expected a terminating `;` after const declaration", stream.GetLineInfo());
                return;
            }

            stream.ConsumeWhiteSpaceAndComments();

            stream.Advance(); // step over ;

            constantList.Add(new ConstantNode() {
                exported = exported,
                identifier = AddString(identifier),
                value = AddString(new CharSpan(valueStream).Trim())
            });

        }

        private void ParseMixinDefinition(ref CharStream stream, bool exported = false) {
            if (!stream.TryParseIdentifier(out CharSpan mixinName)) {
                LogCriticalError(ErrorMessages.ExpectedStyleName(), stream.GetLineInfo());
                return;
            }

            allowMixinValues = true;
            MixinVariableId variableIndex = new MixinVariableId(mixinVariables.size);

            if (stream.Current == '(') {

                if (!stream.TryGetSubStream('(', ')', out CharStream variableStream)) {
                    LogCriticalError(ErrorMessages.UnmatchedParens("mixin " + mixinName), stream.GetLineInfo());
                    return;
                }

                while (variableStream.HasMoreTokens) {
                    if (variableStream.Current != '%') {
                        break;
                    }

                    variableStream.Advance();

                    if (!variableStream.TryParseIdentifier(out CharSpan variableName, true, WhitespaceHandling.ConsumeAfter)) {
                        LogCriticalError("Expected a valid identifier name after `%` in mixin definition.", variableStream.GetLineInfo());
                        return;
                    }

                    if (!variableStream.HasMoreTokens) {
                        mixinVariables.Add(new MixinVariable() {
                            key = AddString(variableName),
                            value = default
                        });
                        break;
                    }

                    if (variableStream.TryParseCharacter('=')) {
                        // read to end
                        CharSpan value = variableStream.GetCharSpanToDelimiterOrEnd(',');
                        mixinVariables.Add(new MixinVariable() {
                            key = AddString(variableName),
                            value = AddString(value)
                        });
                    }
                    else if (variableStream.TryParseCharacter(',')) {
                        mixinVariables.Add(new MixinVariable() {
                            key = AddString(variableName),
                            value = default
                        });
                    }
                    else {
                        LogNonCriticalError("Expected an `=` after mixin variable declaration or a `,` separating multiple arguments", variableStream.GetLineInfo());
                        break;
                    }

                    variableStream.ConsumeWhiteSpace();
                }
            }

            if (!stream.TryGetSubStream('{', '}', out CharStream styleContents)) {
                LogCriticalError(ErrorMessages.UnmatchedBraces("mixin " + mixinName), stream.GetLineInfo());
                return;
            }

            mixinList.Add(new MixinNode() {
                exported = exported,
                nameRange = AddString(mixinName),
                rootBlockIndex = new ParseBlockId(blockList.size),
                variableIndex = variableIndex,
                variableCount = mixinVariables.size - variableIndex.id
            });

            ParseBlockNode node = new ParseBlockNode();
            node.type = BlockNodeType.Root;
            blockList.Add(node);
            blockStack.Push(blockList.size - 1);
            allowMixinValues = true;
            TryParseStyleContents(ref styleContents, default); // todo -- mark that we are in a mixin to allow % syntax for keys
            allowMixinValues = false;
            blockStack.Pop();
        }

        private void ParseAnimationDefinition(ref CharStream stream) {
            if (!stream.TryParseIdentifier(out CharSpan animationName)) {
                LogCriticalError(ErrorMessages.ExpectedStyleName(), stream.GetLineInfo());
                return;
            }

            if (!stream.TryGetSubStream('{', '}', out CharStream animationContents)) {
                LogCriticalError(ErrorMessages.UnmatchedBraces("animation " + animationName), stream.GetLineInfo());
                return;
            }

            AnimationNode animationNode = new AnimationNode() {
                nameRange = AddString(animationName),
                options = default,
                firstKeyFrame = default,
                initBlockRoot = default,
            };

            ParseBlockNode node = new ParseBlockNode();
            node.type = BlockNodeType.AnimationDeclaration;
            blockList.Add(node);
            blockStack.Push(blockList.size - 1);
            TryParseAnimationContents(ref animationContents, ref animationNode);
            blockStack.Pop();
            animationList.Add(animationNode);
        }

        private void TryParseAnimationContents(ref CharStream animationContents, ref AnimationNode animationNode) {

            ref ParseBlockNode parentBlockNode = ref blockList.array[blockStack.Peek()];
            animationContents.ConsumeWhiteSpaceAndComments();

            while (animationContents.HasMoreTokens) {

                animationContents.ConsumeWhiteSpaceAndComments();

                if (!animationContents.HasMoreTokens) {
                    break;
                }

                if (animationContents.TryParseTime(out UITimeMeasurement time)) {

                    if (animationNode.firstKeyFrame.id == 0) {
                        animationNode.firstKeyFrame = new ParseBlockId(blockList.size);
                    }

                    AddChildBlock(ref parentBlockNode, new ParseBlockId(blockList.size));

                    ParseBlockNode keyFrameNode = new ParseBlockNode {
                        blockData = new BlockData {keyFrameTime = time},
                        type = BlockNodeType.AnimationKeyFrame
                    };

                    animationContents.TryGetBraceStream(out CharStream keyFrameContents);
                    blockList.Add(keyFrameNode);
                    blockStack.Push(blockList.size - 1);
                    allowAnimationValues = true;
                    TryParseKeyFrameContents(ref keyFrameContents); // todo -- mark that we are in a mixin to allow % syntax for keys
                    allowAnimationValues = false;
                    blockStack.Pop();

                }
                else if (animationContents.TryGetSubStream('[', ']', out CharStream initContents)) {
                    if (!initContents.TryMatchRange("init")) {
                        // todo fail with a nice message
                        LogCriticalError($"Expected an init block but got {initContents.ToString()} instead", initContents.GetLineInfo());

                        continue;
                    }

                    animationContents.TryGetBraceStream(out CharStream initBlockContents);
                    animationNode.initBlockRoot = new DeclarationId(declarationList.size);

                    ParseBlockNode node = new ParseBlockNode();
                    node.type = BlockNodeType.Root;
                    blockList.Add(node);
                    blockStack.Push(blockList.size - 1);
                    while (initBlockContents.HasMoreTokens) {
                        initBlockContents.ConsumeWhiteSpace();
                        if (!initBlockContents.HasMoreTokens) {
                            break;
                        }

                        if (!initBlockContents.TryGetSubstreamTo('=', out CharStream keyStream)) {
                            LogCriticalError("Expected property name to be followed by an `=` sign", initBlockContents.GetLineInfo());
                            return;
                        }

                        if (!initBlockContents.TryGetSubstreamTo(';', out CharStream valueStream)) {
                            LogCriticalError($"Expected property {keyStream} = to be terminated by a `;`", keyStream.GetLineInfo());
                            return;
                        }

                        PropertyKeyInfoFlag flags = 0; // todo evaluate removal
                        AddProperty(flags, new CharSpan(keyStream).Trim(), new CharSpan(valueStream).Trim());
                    }

                    blockStack.Pop();

                }
                else {

                    // let the compiler handle the property name mapping
                    // we'll pass in everything up to the = sign

                    if (!animationContents.TryGetSubstreamTo('=', out CharStream keyStream)) {
                        LogCriticalError("Expected property name to be followed by an `=` sign", animationContents.GetLineInfo());
                        return;
                    }

                    if (!animationContents.TryGetSubstreamTo(';', out CharStream valueStream)) {
                        LogCriticalError($"Expected property {keyStream} = to be terminated by a `;`", keyStream.GetLineInfo());
                        return;
                    }

                    keyStream.TryParseIdentifier(out CharSpan optionName);

                    // todo -- error checking 

                    switch (optionName.ToLowerString()) {
                        case "duration": {
                            valueStream.TryParseTime(out animationNode.options.duration);
                            if (animationNode.options.duration.unit == UITimeMeasurementUnit.Percentage) {
                                LogNonCriticalError("Animations durations cannot be represented as percentages. Reverting to 1 second duration", valueStream.GetLineInfo());
                                animationNode.options.duration = new UITimeMeasurement(1000);
                            }

                            break;
                        }

                        case "delay":
                            valueStream.TryParseTime(out animationNode.options.startDelay);
                            if (animationNode.options.startDelay.unit == UITimeMeasurementUnit.Percentage) {
                                LogNonCriticalError("Animations delay cannot be repesented as percentages. Reverting to 0 second delay", valueStream.GetLineInfo());
                                animationNode.options.startDelay = default;
                            }

                            break;

                        case "iterations":
                            valueStream.ConsumeWhiteSpace();
                            if (!valueStream.TryParseInt(out animationNode.options.iterations)) {
                                if (valueStream.TryMatchRangeIgnoreCase("infinite")) {
                                    animationNode.options.iterations = -1;
                                }
                            }

                            break;

                        case "forwardstartdelay": {
                            valueStream.TryParseTime(out animationNode.options.forwardStartDelay);
                            if (animationNode.options.forwardStartDelay.unit == UITimeMeasurementUnit.Percentage) {
                                LogNonCriticalError("Animations forward delay cannot be repesented as percentages. Reverting to 0 second delay", valueStream.GetLineInfo());
                                animationNode.options.forwardStartDelay = default;
                            }

                            break;
                        }

                        case "reversestartdelay": {
                            valueStream.TryParseTime(out animationNode.options.reverseStartDelay);
                            if (animationNode.options.reverseStartDelay.unit == UITimeMeasurementUnit.Percentage) {
                                LogNonCriticalError("Animations reverse delay cannot be repesented as percentages. Reverting to 0 second delay", valueStream.GetLineInfo());
                                animationNode.options.reverseStartDelay = default;
                            }

                            break;
                        }

                        case "looptype": {
                            valueStream.ConsumeWhiteSpace();
                            if (valueStream.TryMatchRangeIgnoreCase("pingpong")) {
                                animationNode.options.loopType = AnimationLoopType.PingPong;
                            }
                            else if (valueStream.TryMatchRangeIgnoreCase("constant")) {
                                animationNode.options.loopType = AnimationLoopType.Constant;
                            }
                            else {
                                LogNonCriticalError("Animations loop type can only be one of `PingPong` or `Constant`. Reverting to `Constant`", valueStream.GetLineInfo());
                            }

                            break;
                        }

                        case "direction": {
                            if (valueStream.TryMatchRangeIgnoreCase("forward")) {
                                animationNode.options.direction = AnimationDirection.Forward;
                            }
                            else if (valueStream.TryMatchRangeIgnoreCase("reverse")) {
                                animationNode.options.direction = AnimationDirection.Reverse;
                            }
                            else {
                                animationNode.options.direction = AnimationDirection.Forward;
                                LogNonCriticalError("Animations direction type can only be one of `Forward` or `Reverse`. Reverting to `Forward`", valueStream.GetLineInfo());
                            }

                            break;
                        }

                        case "easing": {

                            if (valueStream.TryMatchRangeIgnoreCase("bezier")) {

                                if (!valueStream.TryGetSubStream('(', ')', out CharStream parenStream)) {
                                    LogNonCriticalError("Failed to parse bezier curve arguments. Expected format `bezier(x1, y1, x2, y2)`", parenStream.GetLineInfo());
                                }

                                if (
                                    parenStream.TryParseFloat(out float x1, true) &&
                                    parenStream.TryParseCharacter(',') &&
                                    parenStream.TryParseFloat(out float y1, true) &&
                                    parenStream.TryParseCharacter(',') &&
                                    parenStream.TryParseFloat(out float x2, true) &&
                                    parenStream.TryParseCharacter(',') &&
                                    parenStream.TryParseFloat(out float y2, true)) {

                                    animationNode.options.easingFunction = EasingFunction.Bezier;
                                    animationNode.options.bezier = new Bezier(x1, y1, x2, y2);

                                }
                                else {
                                    LogNonCriticalError("Failed to parse bezier curve arguments. Expected format `bezier(x1, y1, x2, y2)`", parenStream.GetLineInfo());
                                }

                            }
                            else if (valueStream.TryParseIdentifier(out CharSpan easing)) {
                                string easingStr = easing.ToLowerString();
                                switch (easingStr) {
                                    case "linear":
                                        animationNode.options.easingFunction = EasingFunction.Linear;
                                        break;

                                    case "quadraticeaseout":
                                        animationNode.options.easingFunction = EasingFunction.QuadraticEaseOut;
                                        break;

                                    case "quadraticeasein":
                                        animationNode.options.easingFunction = EasingFunction.QuadraticEaseIn;
                                        break;

                                    case "quadraticeaseinout":
                                        animationNode.options.easingFunction = EasingFunction.QuadraticEaseInOut;
                                        break;

                                    case "cubiceasein":
                                        animationNode.options.easingFunction = EasingFunction.CubicEaseIn;
                                        break;

                                    case "cubiceaseout":
                                        animationNode.options.easingFunction = EasingFunction.CubicEaseOut;
                                        break;

                                    case "cubiceaseinout":
                                        animationNode.options.easingFunction = EasingFunction.CubicEaseInOut;
                                        break;

                                    case "quarticeasein":
                                        animationNode.options.easingFunction = EasingFunction.QuarticEaseIn;
                                        break;

                                    case "quarticeaseout":
                                        animationNode.options.easingFunction = EasingFunction.QuarticEaseOut;
                                        break;

                                    case "quarticeaseinout":
                                        animationNode.options.easingFunction = EasingFunction.QuarticEaseInOut;
                                        break;

                                    case "quinticeasein":
                                        animationNode.options.easingFunction = EasingFunction.QuinticEaseIn;
                                        break;

                                    case "quinticeaseout":
                                        animationNode.options.easingFunction = EasingFunction.QuinticEaseOut;
                                        break;

                                    case "quinticeaseinout":
                                        animationNode.options.easingFunction = EasingFunction.QuinticEaseInOut;
                                        break;

                                    case "sineeasein":
                                        animationNode.options.easingFunction = EasingFunction.SineEaseIn;
                                        break;

                                    case "sineeaseout":
                                        animationNode.options.easingFunction = EasingFunction.SineEaseOut;
                                        break;

                                    case "sineeaseinout":
                                        animationNode.options.easingFunction = EasingFunction.SineEaseInOut;
                                        break;

                                    case "circulareasein":
                                        animationNode.options.easingFunction = EasingFunction.CircularEaseIn;
                                        break;

                                    case "circulareaseout":
                                        animationNode.options.easingFunction = EasingFunction.CircularEaseOut;
                                        break;

                                    case "circulareaseinout":
                                        animationNode.options.easingFunction = EasingFunction.CircularEaseInOut;
                                        break;

                                    case "exponentialeasein":
                                        animationNode.options.easingFunction = EasingFunction.ExponentialEaseIn;
                                        break;

                                    case "exponentialeaseout":
                                        animationNode.options.easingFunction = EasingFunction.ExponentialEaseOut;
                                        break;

                                    case "exponentialeaseinout":
                                        animationNode.options.easingFunction = EasingFunction.ExponentialEaseInOut;
                                        break;

                                    case "elasticeasein":
                                        animationNode.options.easingFunction = EasingFunction.ElasticEaseIn;
                                        break;

                                    case "elasticeaseout":
                                        animationNode.options.easingFunction = EasingFunction.ElasticEaseOut;
                                        break;

                                    case "elasticeaseinout":
                                        animationNode.options.easingFunction = EasingFunction.ElasticEaseInOut;
                                        break;

                                    case "backeasein":
                                        animationNode.options.easingFunction = EasingFunction.BackEaseIn;
                                        break;

                                    case "backeaseout":
                                        animationNode.options.easingFunction = EasingFunction.BackEaseOut;
                                        break;

                                    case "backeaseinout":
                                        animationNode.options.easingFunction = EasingFunction.BackEaseInOut;
                                        break;

                                    case "bounceeasein":
                                        animationNode.options.easingFunction = EasingFunction.BounceEaseIn;
                                        break;

                                    case "bounceeaseout":
                                        animationNode.options.easingFunction = EasingFunction.BounceEaseOut;
                                        break;

                                    case "bounceeaseinout":
                                        animationNode.options.easingFunction = EasingFunction.BounceEaseInOut;
                                        break;

                                    default:
                                        string list = string.Join(", ", Enum.GetNames(typeof(EasingFunction)));
                                        LogNonCriticalError($"Expected animation option `easing` to be one of: {list}. but got `{easing}", easing.GetLineInfo());
                                        animationNode.options.easingFunction = EasingFunction.Linear;
                                        break;
                                }
                            }

                            break;
                        }

                        default:
                            LogNonCriticalError(ErrorMessages.UnknownAnimationOption(optionName.ToString()), keyStream.GetLineInfo());
                            break;
                    }

                }
            }

            if (animationNode.firstKeyFrame.id == -1) {
                LogNonCriticalError("No keyframes present in animation", animationContents.GetLineInfo());
            }
        }

        private void TryParseKeyFrameContents(ref CharStream keyFrameContents) {

            while (!hasCriticalError) {

                keyFrameContents.ConsumeWhiteSpaceAndComments();

                if (!keyFrameContents.HasMoreTokens) {
                    break;
                }

                if (keyFrameContents.TryMatchRange("mixin")) {
                    ParseMixinUsage(ref keyFrameContents);
                    continue;
                }

                ParseProperty(ref keyFrameContents);
            }

        }

        private void ParseStyleDefinition(ref CharStream stream, bool exported = false) {
            if (!stream.TryParseIdentifier(out CharSpan styleName)) {
                LogCriticalError(ErrorMessages.ExpectedStyleName(), stream.GetLineInfo());
                return;
            }

            if (!stream.TryGetSubStream('{', '}', out CharStream styleContents)) {
                LogCriticalError(ErrorMessages.UnmatchedBraces("style " + styleName), stream.GetLineInfo());
                return;
            }

            styleList.Add(new StyleNode() {
                exported = exported,
                nameRange = AddString(styleName),
                rootBlockIndex = new ParseBlockId(blockList.size)
            });

            ParseBlockNode node = new ParseBlockNode();
            node.type = BlockNodeType.Root;
            blockList.Add(node);
            blockStack.Push(blockList.size - 1);
            TryParseStyleContents(ref styleContents, styleName);
            blockStack.Pop();

        }

        private void TryParseStyleContents(ref CharStream stream, CharSpan styleName) {

            while (!hasCriticalError) {

                stream.ConsumeWhiteSpaceAndComments();

                if (!stream.HasMoreTokens) {
                    break;
                }

                char c = stream.Current;
                switch (c) {

                    case '[':
                        ParseBracketBlock(ref stream, styleName);
                        break;

                    case '#':
                        ParseConditionBlock(ref stream);
                        break;

                    default: {

                        if (stream.TryMatchRange("mixin")) {
                            ParseMixinUsage(ref stream);
                            continue;
                        }

                        if (stream.TryMatchRange("transition")) {
                            ParseTransition(ref stream);
                            continue;
                        }

                        if (stream.TryMatchRange("enter ")) {
                            if (stream.TryMatchRange("play ")) {
                                ParsePlay(ref stream, HookEvent.Enter);
                                continue;
                            }
                        }

                        if (stream.TryMatchRange("exit ")) {
                            if (stream.TryMatchRange("play ")) {
                                ParsePlay(ref stream, HookEvent.Exit);
                                continue;
                            }
                        }

                        if (stream.TryMatchRange("pause ")) { }

                        ParseProperty(ref stream);
                        break;
                    }
                }
            }

        }

        private void ParsePlay(ref CharStream stream, HookEvent hookEvent) {
            // expect animation, sound or effect 
            stream.TryParseIdentifier(out CharSpan identifier);
            switch (identifier.ToLowerString()) {
                case "animation":

                    // next: name

                    // todo parse module too: moduleName:animationName

                    stream.TryParseIdentifier(out CharSpan animationName);

                    AnimationActionData animationActionData = new AnimationActionData() {
                        moduleName = AddString(default),
                        animationName = AddString(animationName),
                        hookEvent = hookEvent
                    };

                    if (stream.Current == '{') {
                        if (stream.TryGetBraceStream(out CharStream playAnimationContents)) {
                            // todo parse nested data and put in into the actionData
                            // todo parse out the contents of the animation with all those override options and parameter blocks
                            // actionData.options = 
                        }
                        else {
                            LogCriticalError("Could not find matching brace '}'", playAnimationContents.GetLineInfo());
                        }
                    }
                    else if (!stream.TryParseCharacter(';')) {
                        LogCriticalError("Could not find end of statement ';'", stream.GetLineInfo());
                        return;
                    }

                    ref ParseBlockNode block = ref blockList.array[blockStack.Peek()];

                    declarationList.Add(new StyleDeclaration() {
                        declType = StyleDeclarationType.Action,
                        prevSibling = block.lastDeclaration,
                        declarationData = new DeclarationData() {
                            animationActionData = animationActionData
                        }
                    });

                    block.lastDeclaration = new DeclarationId(declarationList.size - 1);
                    break;

                // todo those
                // case "sound": break;
                // case "effect": break;
                default:
                    LogNonCriticalError($"Unknown play thing {identifier.ToLowerString()}", stream.GetLineInfo());
                    break;
            }
        }

        private void ParseTransition(ref CharStream stream) {

            if (!stream.TryGetDelimitedSubstream('=', out CharStream paramStream)) {
                LogCriticalError("Expected a valid transition string delimited by an `=` after `transition` keyword", stream.GetLineInfo());
                return;
            }

            if (!paramStream.TryParseTime(out UITimeMeasurement durationMeasurement)) {
                LogCriticalError("Expected a valid transition duration after `transition` keyword", stream.GetLineInfo());
                return;
            }

            if (durationMeasurement.unit == UITimeMeasurementUnit.Percentage) {
                LogNonCriticalError("Expected a time unit in ms or sec for transition, % units not supported, falling back to milliseconds", paramStream.GetLineInfo());
                durationMeasurement = new UITimeMeasurement(durationMeasurement.value);
            }

            EasingFunction easingFn = default;
            Bezier bezier = default;

            if (paramStream.TryMatchRangeIgnoreCase("bezier")) {
                throw new NotImplementedException("Transition bezier");
            }
            else if (paramStream.TryMatchRangeIgnoreCase("step")) {
                throw new NotImplementedException("Transition step");
            }
            else if (paramStream.TryParseEnum<EasingFunction>(out int easing)) {
                easingFn = (EasingFunction) easing;
            }
            else {
                LogCriticalError("Expected a valid easing curve definition after the duration of transition", stream.GetLineInfo());
                return;
            }

            paramStream.TryParseTime(out UITimeMeasurement delayMeasurement);

            if (delayMeasurement.unit == UITimeMeasurementUnit.Percentage) {
                LogNonCriticalError("Expected a time unit in ms or sec for transition, % units not supported, falling back to milliseconds", paramStream.GetLineInfo());
                delayMeasurement = new UITimeMeasurement(delayMeasurement.value);
            }

            if (!stream.TryGetDelimitedSubstream(';', out CharStream propertyStream)) {
                LogCriticalError("Expected a valid list of property or shorthandNames in transition after the `=` symbol", stream.GetLineInfo());
                return;
            }

            ref ParseBlockNode block = ref blockList.array[blockStack.Peek()];
            DeclarationId last = block.lastDeclaration;

            int transitionId = transitionList.size;
            transitionList.Add(new TransitionDeclaration() {
                easing = easingFn,
                bezier = bezier,
                delay = (int) delayMeasurement.AsMilliseconds,
                duration = (int) durationMeasurement.AsMilliseconds
            });

            do {
                propertyStream.ConsumeWhiteSpaceAndComments();

                bool customProperty = new CharSpan(propertyStream).Contains("::");

                int propertyIdx = -1;
                int shorthandIdx = -1;
                RangeInt keyRange = default;

                if (customProperty) {
                    if (propertyStream.TryGetDelimitedSubstream(',', out var s)) {
                        keyRange = AddString(new CharSpan(s).Trim());
                    }
                    else {
                        keyRange = AddString(new CharSpan(propertyStream).Trim());
                        propertyStream.AdvanceTo(propertyStream.End);
                    }
                }
                else {
                    if (!propertyStream.TryParseDottedIdentifier(out CharSpan propertyId)) {
                        LogNonCriticalError("Expected a valid identifier in the property list for `transition`", stream.GetLineInfo());
                        return;
                    }

                    tmpCharBuffer.EnsureCapacity(propertyId.Length);
                    tmpCharBuffer.size = propertyId.Length;
                    propertyId.ToLower(tmpCharBuffer.array);

                    if (PropertyParsers.TryResolvePropertyId(tmpCharBuffer.array, 0, tmpCharBuffer.size, out PropertyParseEntry entry)) {
                        propertyIdx = entry.propertyId.index;
                    }
                    else if (PropertyParsers.TryResolveShorthand(tmpCharBuffer.array, 0, out ShorthandEntry shorthandEntry)) {
                        shorthandIdx = shorthandEntry.index;
                    }

                    propertyStream.TryParseCharacter(',');

                }

                declarationList.Add(new StyleDeclaration() {
                    declType = StyleDeclarationType.Transition,
                    prevSibling = last,
                    declarationData = new DeclarationData() {
                        transitionDeclarationData = new TransitionDeclarationData() {
                            transitionId = transitionId,
                            propertyId = propertyIdx,
                            shortHandId = shorthandIdx,
                            customPropertyRange = keyRange
                        }
                    }
                });

                last = new DeclarationId(declarationList.size - 1);

            } while (propertyStream.HasMoreTokens);

            block.lastDeclaration = new DeclarationId(declarationList.size - 1);

        }

        private unsafe void ParseMixinUsage(ref CharStream stream) {
            if (!stream.TryGetSubStream('(', ')', out CharStream parenStream)) {
                LogCriticalError(ErrorMessages.UnmatchedParens("mixin usage"), stream.GetLineInfo());
                return;
            }

            bool hasAtSign = parenStream.TryParseCharacter('@', WhitespaceHandling.ConsumeBefore);

            WhitespaceHandling handling = hasAtSign ? WhitespaceHandling.ConsumeAfter : WhitespaceHandling.ConsumeAll;

            if (!parenStream.TryParseMultiDottedIdentifier(out CharSpan identifier, handling, true)) {
                LogCriticalError("Expected a valid identifier or dotted identifier path inside mixin usage", parenStream.GetLineInfo());
                return;
            }

            MixinVariableId variableStart = new MixinVariableId(mixinVariables.size);
            int variableCount = 0;

            if (stream.Current == '{') {

                if (stream.TryGetSubStream('{', '}', out CharStream variableStream)) {
                    // name = value; name cannot contain % or @ 

                    while (variableStream.HasMoreTokens) {

                        variableStream.ConsumeWhiteSpaceAndComments();

                        if (!variableStream.TryParseIdentifier(out CharSpan variableKey)) {
                            LogNonCriticalError("Unable to parse mixin variable", variableStream.GetLineInfo());
                            variableCount = 0;
                            break;
                        }

                        if (!variableStream.TryParseCharacter('=')) {
                            LogNonCriticalError($"Unable to parse mixin variable {identifier}. Expected an `=` after variable name.", variableStream.GetLineInfo());
                            variableCount = 0;
                            break;
                        }

                        CharSpan span = variableStream.GetCharSpanToDelimiterOrEnd(';').Trim();
                        variableStream.ConsumeWhiteSpaceAndComments();

                        if (!span.HasValue) {
                            LogNonCriticalError($"Unable to parse mixin variable {identifier}. Expected a value after the `=` but value was only whitespace.", variableStream.GetLineInfo());
                            variableCount = 0;
                            break;
                        }

                        mixinVariables.Add(new MixinVariable() {
                            key = AddString(variableKey),
                            value = AddString(span)
                        });
                        variableCount++;
                    }

                }
            }
            else if (stream.TryParseCharacter(';')) {
                // no-op
            }
            else {
                LogCriticalError("Expected a terminating `;` after mixin usage", parenStream.GetLineInfo());
                return;
            }

            if (hasAtSign) {
                identifier = new CharSpan(identifier.data, identifier.rangeStart - 1, identifier.rangeEnd, identifier.baseOffset);
            }

            ref ParseBlockNode block = ref blockList.array[blockStack.Peek()];
            DeclarationId last = block.lastDeclaration;
            block.lastDeclaration = new DeclarationId(declarationList.size);

            declarationList.Add(new StyleDeclaration {
                declType = StyleDeclarationType.MixinUsage,
                prevSibling = last,
                declarationData = new DeclarationData() {
                    mixinDeclarationData = new MixinDeclarationData() {
                        keyRange = AddString(identifier.Trim()),
                        valueRange = default, // todo fill in with mixin parameters eventually
                        mixinVariableCount = variableCount,
                        mixinVariableStart = variableStart,
                    }
                }
            });

        }

        /// <summary>
        /// A condition block must start with the `#if` character sequence followed by the user defined condition
        /// symbol, e.g. #if mobile { ... } or #if !mobile { ... } for inverted conditions.
        /// </summary>
        /// <param name="stream">Stream pointing at the # character</param>
        private void ParseConditionBlock(ref CharStream stream) {

            stream.Advance(); // step over #

            if (!stream.TryMatchRange("if")) {
                LogCriticalError("Conditions must use the `if` keyword. Syntax: `#if condition`", stream.GetLineInfo());
                return;
            }

            bool isInverted = stream.TryParseCharacter('!');

            if (!stream.TryGetStreamUntil('{', out CharStream conditionStream)) {
                LogCriticalError("Conditions must have value expressions following the `#` and be followed by a matching pair of `{` `}`", stream.GetLineInfo());
                return;
            }

            // if (conditionStream.Contains('&') || conditionStream.Contains('|') || conditionStream.Contains('!')) {
            // todo -- warn about this not being supported?    
            //}

            ParseBlockNode node = new ParseBlockNode();
            node.type = BlockNodeType.Condition;
            CharSpan conditionSpan = new CharSpan(conditionStream).Trim();

            if (!conditionSpan.HasValue) {
                LogCriticalError("Conditions must have value expressions following the `#`", conditionStream.GetLineInfo());
                return;
            }

            node.blockData.conditionData.conditionRange = AddString(conditionSpan);
            node.invert = isInverted;

            ParseBlockContents(ref node, ref stream);

        }

        private void ParseBlockContents(ref ParseBlockNode node, ref CharStream stream) {

            if (!stream.TryGetSubStream('{', '}', out CharStream contentStream)) {
                LogCriticalError("Expected a matching pair of `{` and `}`", stream.GetLineInfo());
                return;
            }

            contentStream.ConsumeWhiteSpaceAndComments(CommentMode.DoubleSlash);

            if (!contentStream.HasMoreTokens) {
                return;
            }

            blockList.Add(node);

            ref ParseBlockNode parent = ref blockList.array[blockStack.Peek()];

            AddChildBlock(ref parent, new ParseBlockId(blockList.size - 1));

            blockStack.Push(blockList.size - 1);

            TryParseStyleContents(ref contentStream, default);

            blockStack.Pop();

        }

        private void ParseBracketBlock(ref CharStream stream, CharSpan styleName) {

            if (!stream.TryGetSubStream('[', ']', out CharStream bracketContents)) {
                LogCriticalError("Expected to find a matched bracket", stream.GetLineInfo());
                return;
            }

            ParseBlockNode node = new ParseBlockNode();

            if (bracketContents.TryMatchRangeIgnoreCase("attr:")) {

                // todo -- implement attribute operators != $= ~= ^= *= |=
                if (!ParseAttributeBlock(bracketContents, ref node)) {
                    return;
                }

            }
            else if (bracketContents.TryMatchRangeIgnoreCase("when:")) {

                if (HandleWhenStatement(bracketContents, ref node)) {
                    return;
                }

            }
            else if (bracketContents.TryMatchRangeIgnoreCase("unless:")) {
                if (HandleWhenStatement(bracketContents, ref node)) {
                    return;
                }

                node.invert = true;
            }
            else if (IsStateName(ref bracketContents, out StyleState state)) {
                node.type = BlockNodeType.State;
                node.stateRequirement = state;
            }
            else {
                // todo -- maybe not critical error if structure is ok
                LogCriticalError($"Unknown style block declaration [{bracketContents}]", stream.GetLineInfo());
                return;
            }

            ParseBlockContents(ref node, ref stream);

        }

        private bool HandleWhenStatement(CharStream bracketContents, ref ParseBlockNode node) {
            if (bracketContents.TryMatchRangeIgnoreCase("tag")) {
                if (!bracketContents.TryGetSubStream('(', ')', out CharStream tagStream)) {
                    LogCriticalError("Expected a matching pair of `(` and `)`", bracketContents.GetLineInfo());
                    return true;
                }

                node.type = BlockNodeType.TagName;
                node.blockData.tagData.tagNameRange = AddString(new CharSpan(tagStream));
            }
            else if (bracketContents.TryMatchRangeIgnoreCase("first-child")) {
                node.type = BlockNodeType.FirstChild;
            }
            else if (bracketContents.TryMatchRangeIgnoreCase("last-child")) {
                node.type = BlockNodeType.LastChild;
            }
            else if (bracketContents.TryMatchRangeIgnoreCase("nth-child")) {

                node.type = BlockNodeType.NthChild;
                if (!ParseNthExpression(ref bracketContents, ref node)) return true;

            }
            else if (bracketContents.TryMatchRangeIgnoreCase("child-count")) {
                // accept a number or an operator and a number
                node.type = BlockNodeType.ChildCount;
                if (!bracketContents.TryGetSubStream('(', ')', out CharStream expressionStream)) {
                    LogNonCriticalError("Expected a `child-count` expression", bracketContents.GetLineInfo());
                    return true;
                }

                ChildCountOperator op = 0;
                if (expressionStream.TryMatchRangeIgnoreCase(">=")) {
                    op = ChildCountOperator.GreaterThanEqualTo;
                }
                else if (expressionStream.TryMatchRangeIgnoreCase("<=")) {
                    op = ChildCountOperator.LessThanEqualTo;
                }
                else if (expressionStream.TryMatchRangeIgnoreCase("==")) {
                    op = ChildCountOperator.EqualTo;
                }
                else if (expressionStream.TryMatchRangeIgnoreCase(">")) {
                    op = ChildCountOperator.GreaterThan;
                }
                else if (expressionStream.TryMatchRangeIgnoreCase("<")) {
                    op = ChildCountOperator.LessThan;
                }
                else if (expressionStream.TryMatchRangeIgnoreCase("!=")) {
                    op = ChildCountOperator.NotEqualTo;
                }

                if (!expressionStream.TryParseInt(out int childNumber)) { }

                if (childNumber < 0) {
                    LogNonCriticalError("Expected child count argument to be positive", bracketContents.GetLineInfo());
                }

                node.blockData.childCountData.op = op;
                node.blockData.childCountData.number = childNumber;

            }
            else if (bracketContents.TryMatchRangeIgnoreCase("first-with-tag")) {
                node.type = BlockNodeType.FirstWithTag;
                // should be allowed without an expression
                if (!bracketContents.HasMoreTokens) {
                    node.blockData.tagData.tagNameRange = default;
                }
                else {
                    if (!bracketContents.TryGetSubStream('(', ')', out CharStream tagStream)) {
                        LogNonCriticalError(ErrorMessages.UnmatchedParens("first-with-tag"), bracketContents.GetLineInfo());
                        node.blockData.tagData.tagNameRange = default;
                    }
                    else {
                        node.blockData.tagData.tagNameRange = AddString(new CharSpan(tagStream));
                    }
                }
            }
            else if (bracketContents.TryMatchRangeIgnoreCase("last-with-tag")) {
                node.type = BlockNodeType.LastWithTag;
                // should be allowed without an expression
                if (!bracketContents.HasMoreTokens) {
                    node.blockData.tagData.tagNameRange = default;
                }
                else {
                    if (!bracketContents.TryGetSubStream('(', ')', out CharStream tagStream)) {
                        LogNonCriticalError(ErrorMessages.UnmatchedParens("last-with-tag"), bracketContents.GetLineInfo());
                        node.blockData.tagData.tagNameRange = default;
                    }
                    else {
                        node.blockData.tagData.tagNameRange = AddString(new CharSpan(tagStream));
                    }
                }
            }
            else if (bracketContents.TryMatchRangeIgnoreCase("only-with-tag")) {
                node.type = BlockNodeType.OnlyWithTag;
                // should be allowed without an expression
                if (!bracketContents.HasMoreTokens) {
                    node.blockData.tagData.tagNameRange = default;
                }
                else {
                    if (!bracketContents.TryGetSubStream('(', ')', out CharStream tagStream)) {
                        LogNonCriticalError(ErrorMessages.UnmatchedParens("only-with-tag"), bracketContents.GetLineInfo());
                        node.blockData.tagData.tagNameRange = default;
                    }
                    else {
                        node.blockData.tagData.tagNameRange = AddString(new CharSpan(tagStream));
                    }
                }
            }
            else if (bracketContents.TryMatchRangeIgnoreCase("nth-with-tag")) {

                node.type = BlockNodeType.NthWithTag;
                if (!ParseNthExpression(ref bracketContents, ref node)) return true;

            }
            else if (bracketContents.TryMatchRangeIgnoreCase("only-child")) {
                node.type = BlockNodeType.OnlyChild;
            }
            else if (bracketContents.TryMatchRangeIgnoreCase("empty")) {
                node.type = BlockNodeType.NoChildren;
            }
            else if (bracketContents.TryMatchRangeIgnoreCase("focus-within")) {
                node.type = BlockNodeType.FocusWithin;
            }

            return false;
        }

        private bool ParseNthExpression(ref CharStream bracketContents, ref ParseBlockNode node) {
            // syntax derived from https://developer.mozilla.org/en-US/docs/Web/CSS/:nth-child

            if (!bracketContents.TryGetSubStream('(', ')', out CharStream nthChildExpression)) {
                LogCriticalError("Expected a matching pair of `(` and `)`", bracketContents.GetLineInfo());
                return false;
            }

            if (nthChildExpression.TryMatchRangeIgnoreCase("even")) {
                node.blockData.nthChildData.offset = 0;
                node.blockData.nthChildData.stepSize = 2;
            }
            else if (nthChildExpression.TryMatchRangeIgnoreCase("odd")) {
                node.blockData.nthChildData.offset = 1;
                node.blockData.nthChildData.stepSize = 2;
            }
            else {

                if (nthChildExpression.TryParseCharacter('n')) {
                    node.blockData.nthChildData.stepSize = 1;
                    if (nthChildExpression.HasMoreTokens) {
                        nthChildExpression.TryParseCharacter('+');
                        nthChildExpression.TryParseInt(out int offset);
                        node.blockData.nthChildData.offset = (short) offset;
                    }
                }
                else if (nthChildExpression.TryMatchRange("-n")) {
                    node.blockData.nthChildData.stepSize = -1;
                    if (nthChildExpression.HasMoreTokens) {
                        nthChildExpression.TryParseCharacter('+');
                        nthChildExpression.TryParseInt(out int offset);
                        node.blockData.nthChildData.offset = (short) offset;
                    }
                }
                else {
                    nthChildExpression.TryParseInt(out int intVal);

                    if (!nthChildExpression.HasMoreTokens) {
                        node.blockData.nthChildData.stepSize = 0;
                        node.blockData.nthChildData.offset = (short) intVal;
                    }
                    else {

                        if (!nthChildExpression.TryParseCharacter('n')) {
                            LogNonCriticalError("Expected the character `n` in nth-child expression", nthChildExpression.GetLineInfo());
                            node.blockData.nthChildData.offset = (short) intVal;
                            node.blockData.nthChildData.stepSize = 0;
                        }
                        else {
                            node.blockData.nthChildData.stepSize = (short) intVal;
                            nthChildExpression.ConsumeWhiteSpace();
                            if (nthChildExpression.Current == '+') {
                                nthChildExpression.TryParseCharacter('+');
                                nthChildExpression.TryParseInt(out intVal);
                                node.blockData.nthChildData.offset = (short) intVal;
                            }
                            // todo support 2n - 2 with the whitespace as well
                            else if (nthChildExpression.Current == '-') {
                                nthChildExpression.TryParseInt(out intVal);
                                node.blockData.nthChildData.offset = (short) intVal;
                            }
                            else {
                                LogNonCriticalError("Expected + or - sign in nth-child expression", nthChildExpression.GetLineInfo());
                            }
                        }
                    }
                }

            }

            return true;
        }

        private bool ParseAttributeBlock(CharStream bracketContents, ref ParseBlockNode node) {
            if (!bracketContents.TryParseIdentifier(out CharSpan attrKey)) {
                LogNonCriticalError("Expected to find a valid attribute expression after `[attr:`", bracketContents.GetLineInfo());
                return false;
            }

            node.type = BlockNodeType.Attribute;
            node.blockData.attributeData.attrKeyRange = AddString(attrKey);

            if (!bracketContents.HasMoreTokens) {
                node.blockData.attributeData.attrValueRange = default;
                node.blockData.attributeData.compareOp = AttributeOperator.Exists;
                return true;
            }

            // [attr:x contains "value"]
            // [attr:x starts-with "value"]
            // [attr:x ends-with "value"]
            // [attr:x="value"]
            // [attr:x]

            if (bracketContents.TryMatchRangeIgnoreCase("contains")) {
                node.blockData.attributeData.compareOp = AttributeOperator.Contains;
            }
            else if (bracketContents.TryMatchRange("starts-with")) {
                node.blockData.attributeData.compareOp = AttributeOperator.StartsWith;
            }
            else if (bracketContents.TryMatchRange("ends-with")) {
                node.blockData.attributeData.compareOp = AttributeOperator.EndsWith;
            }
            else if (bracketContents.TryParseCharacter('=')) {
                node.blockData.attributeData.compareOp = AttributeOperator.Equal;
            }
            else if (bracketContents.TryMatchRange("is")) {
                node.blockData.attributeData.compareOp = AttributeOperator.Equal;
            }
            else {
                LogNonCriticalError($"Invalid attribute expression {bracketContents}", bracketContents.GetLineInfo());
                return false;
            }

            if (!bracketContents.TryParseAnyQuotedString(out CharSpan attrValue)) {
                LogNonCriticalError($"Expected to find a valid attribute expression after `[attr:{attrKey}`", bracketContents.GetLineInfo());
                return false;
            }

            node.blockData.attributeData.attrValueRange = AddString(attrValue);

            return true;
        }

        private void AddChildBlock(ref ParseBlockNode parent, ParseBlockId childIndex) {
            if (parent.firstChild.id == 0) {
                parent.firstChild = childIndex;
            }
            else {
                ParseBlockId ptr = parent.firstChild;
                ParseBlockId last = ptr;

                while (ptr.id != 0) {
                    last = ptr;
                    ptr = blockList.array[ptr.id].nextSibling;
                }

                blockList.array[last.id].nextSibling = childIndex;
            }
        }

        private static bool IsStateName(ref CharStream stream, out StyleState styleState) {

            for (int i = 0; i < stateNames.Length; i++) {
                if (stream.TryMatchRangeIgnoreCase(stateNames[i])) {
                    styleState = stateValues[i];
                    return true;
                }
            }

            styleState = default;
            return false;
        }

        private void ParseProperty(ref CharStream stream) {

            PropertyKeyInfoFlag flags = 0;

            stream.ConsumeWhiteSpaceAndComments();

            // let the compiler handle the property name mapping
            // we'll pass in everything up to the = sign

            if (!stream.TryGetSubstreamTo('=', out CharStream keyStream)) {
                LogCriticalError("Expected property name to be followed by an `=` sign", stream.GetLineInfo());
                return;
            }

            if (!stream.TryGetSubstreamTo(';', out CharStream valueStream)) {
                LogCriticalError($"Expected property {keyStream} = to be terminated by a `;`", keyStream.GetLineInfo());
                return;
            }

            AddProperty(flags, new CharSpan(keyStream).Trim(), new CharSpan(valueStream).Trim());

        }

        private unsafe RangeInt AddString(CharSpan span) {
            charBuffer.EnsureAdditionalCapacity(span.Length);
            RangeInt retn = new RangeInt(charBuffer.size, 0);
            fixed (char* charBufferPtr = charBuffer.array) {
                TypedUnsafe.MemCpy(charBufferPtr + charBuffer.size, span.data + span.rangeStart, span.Length);
            }

            charBuffer.size += span.Length;
            retn.length = span.Length;
            return retn;
        }

        private unsafe void AddProperty(PropertyKeyInfoFlag flags, CharSpan keySpan, CharSpan valueSpan) {

            ref ParseBlockNode block = ref blockList.array[blockStack.Peek()];
            StyleDeclaration property = default;
            PropertyDeclarationData propertyData;

            bool isCustomProperty = keySpan.Contains("::");

            bool mightBeMixinValue = false;

            if (isCustomProperty) {
                propertyData.propertyId = default;
                property.declType = StyleDeclarationType.CustomProperty;
                propertyData.keyRange = AddString(keySpan);
            }
            else {

                tmpCharBuffer.EnsureCapacity(keySpan.Length);
                tmpCharBuffer.size = keySpan.Length;
                keySpan.ToLower(tmpCharBuffer.array);

                if (allowMixinValues) {
                    CharStream s = new CharStream(valueSpan);
                    // consider changing mixin syntax to %{value} or similar to be more differentiated from % unit or possible file path escapes 
                    while (s.HasMoreTokens) {
                        int x = s.NextIndexOf('%');
                        if (x < 0) {
                            break;
                        }

                        if (x + 1 < valueSpan.rangeEnd) {
                            if ((valueSpan[x + 1]) == '{') {
                                mightBeMixinValue = true;
                                break;
                            }
                        }

                        s.AdvanceTo(x + 1);
                    }
                }

                if (allowMixinValues && MightContainMixin(tmpCharBuffer)) {
                    propertyData.propertyId = -1;
                    property.declType = StyleDeclarationType.MixinProperty;
                    propertyData.keyRange = AddString(keySpan);
                }
                else if (PropertyParsers.TryResolvePropertyId(tmpCharBuffer.array, 0, tmpCharBuffer.size, out PropertyParseEntry entry)) {
                    propertyData.propertyId = entry.propertyId.index;
                    property.declType = StyleDeclarationType.Property;
                    propertyData.keyRange = default;
                }
                else if (PropertyParsers.TryResolveShorthand(tmpCharBuffer.array, tmpCharBuffer.size, out ShorthandEntry shorthandEntry)) {
                    propertyData.propertyId = shorthandEntry.index;
                    property.declType = StyleDeclarationType.ShortHand;
                    propertyData.keyRange = default;
                }
                else {
                    LogNonCriticalError($"Unable to resolve property name for {keySpan}", keySpan.GetLineInfo());
                    return;
                }
            }

            // else if (StringUtil.EqualsRangeUnsafe(tmpCharBuffer.array, 0, "material:")) {
            //     propertyData.propertyId = -1;
            //     property.declType = StyleDeclarationType.MaterialVar;
            //     propertyData.keyRange = AddString(keySpan);
            // }
            // else if (StringUtil.EqualsRangeUnsafe(tmpCharBuffer.array, 0, "painter:")) {
            //     propertyData.propertyId = -1;
            //     property.declType = StyleDeclarationType.PainterVar;
            //     propertyData.keyRange = AddString(keySpan);
            // }

            if (valueSpan.Contains('@', out int idx)) {
                if (idx + 1 < valueSpan.rangeEnd && !char.IsDigit(valueSpan.data[idx + 1])) {
                    flags |= PropertyKeyInfoFlag.ContainsConst;
                }
            }

            if (valueSpan == "current") {
                if (allowAnimationValues) {
                    flags |= PropertyKeyInfoFlag.AnimationCurrent;
                }
                else {
                    LogNonCriticalError("`current` is only allowed in animation values", valueSpan.GetLineInfo());
                    return;
                }
            }

            if (valueSpan.Contains('$', out idx)) {
                if (idx + 1 < valueSpan.rangeEnd && !char.IsDigit(valueSpan.data[idx + 1])) {
                    flags |= PropertyKeyInfoFlag.ContainsVariable;
                }
            }

            if (mightBeMixinValue) {
                flags |= PropertyKeyInfoFlag.ContainsMixinVariable;
            }

            propertyData.flags = flags;
            propertyData.valueRange = AddString(valueSpan);
            property.prevSibling = block.lastDeclaration;
            property.declarationData.propertyDeclarationData = propertyData;
            block.lastDeclaration = new DeclarationId(declarationList.size);
            declarationList.Add(property);

        }

        private static bool MightContainMixin(StructList<char> structList) {
            for (int i = 0; i < structList.size; i++) {
                if (structList.array[i] == '%' && i + 1 < structList.size) {
                    if (structList.array[i + 1] == '{') {
                        return true;
                    }
                }
            }

            return false;
        }

        private void LogCriticalError(string error, LineInfo lineInfo) {
            if (diagnostics != null) {
                diagnostics.LogError(error, filePath, lineInfo.line, lineInfo.column);
            }
            else {
                Debug.Log(error);
            }

            hasCriticalError = true;
        }

        private void LogNonCriticalError(string error, LineInfo lineInfo) {
            if (diagnostics != null) {
                diagnostics.LogError(error, filePath, lineInfo.line, lineInfo.column);
            }
            else {
                Debug.Log(error);
            }
        }

        internal static class ErrorMessages {

            public static string ExpectedStyleName() {
                return "Expected a valid style name after `style` keyword.";
            }

            public static string UnmatchedBraces(string context) {
                return "Expected a matching set of `{` and `}` after `" + context + "`";
            }

            public static string UnmatchedParens(string context) {
                return "Expected a matching set of `(` and `)` after `" + context + "`";
            }

            public static string UnknownAnimationOption(string context) {
                return "Unknown animation option `" + context + "`";
            }

        }

    }

}