using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Style;
using UIForia.Util;

namespace UIForia.Style2 {

    // todo the size of this struct is 32 bytes. can get it down to 24 or so by not including a the char * pointer and switch to ReflessCharSpan
    // but then we need a way to get the data to hookup the references, which is annoyoing

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "(), nq}")]
    [StructLayout(LayoutKind.Explicit)]
    internal struct StyleBodyPart {

        [FieldOffset(0)] public readonly BodyPartType type;
        [FieldOffset(2)] public ushort sourceId;
        [FieldOffset(4)] public readonly Part_ConditionBlock conditionBlock;
        [FieldOffset(4)] public readonly Part_EnterState enterState;
        [FieldOffset(4)] public readonly Part_Property property;
        [FieldOffset(4)] public readonly Part_VariableProperty variableProperty;
        [FieldOffset(4)] public readonly Part_Style style;
        [FieldOffset(4)] public readonly Part_ConstantBranch constantBranch;
        [FieldOffset(4)] public readonly Part_ExtendStyle extendStyle;
        [FieldOffset(4)] public readonly Part_VariablePropertyShorthand variableShorthand;

        public StyleBodyPart(in Part_ConstantBranch constantBranch) : this() {
            this.type = BodyPartType.ConstantBranch;
            this.constantBranch = constantBranch;
        }

        public StyleBodyPart(in Part_ConditionBlock conditionBlock) : this() {
            this.type = BodyPartType.ConditionBlock;
            this.conditionBlock = conditionBlock;
        }

        public StyleBodyPart(in Part_EnterState enterState) : this() {
            this.type = BodyPartType.EnterState;
            this.enterState = enterState;
        }

        public StyleBodyPart(in Part_ExtendStyle extendStyle) : this() {
            this.type = BodyPartType.ExtendStyle;
            this.extendStyle = extendStyle;
        }

        public StyleBodyPart(in Part_Style style) : this() {
            this.type = BodyPartType.Style;
            this.style = style;
        }

        public StyleBodyPart(in Part_Property property) : this() {
            this.type = BodyPartType.Property;
            this.property = property;
        }

        public StyleBodyPart(in Part_VariableProperty property) : this() {
            this.type = BodyPartType.VariableProperty;
            this.variableProperty = property;
        }

        public StyleBodyPart(in Part_VariablePropertyShorthand variableShorthand) : this() {
            this.type = BodyPartType.VariablePropertyShorthand;
            this.variableShorthand = variableShorthand;
        }

        public static implicit operator StyleBodyPart(Part_ConstantBranch c) {
            return new StyleBodyPart(c);
        }

        public static implicit operator StyleBodyPart(Part_ConditionBlock c) {
            return new StyleBodyPart(c);
        }

        public static implicit operator StyleBodyPart(Part_EnterState c) {
            return new StyleBodyPart(c);
        }

        public static implicit operator StyleBodyPart(Part_ExtendStyle c) {
            return new StyleBodyPart(c);
        }

        public static implicit operator StyleBodyPart(Part_Style c) {
            return new StyleBodyPart(c);
        }

        public static implicit operator StyleBodyPart(Part_Property c) {
            return new StyleBodyPart(c);
        }

        public static implicit operator StyleBodyPart(Part_VariableProperty c) {
            return new StyleBodyPart(c);
        }

        public static implicit operator StyleBodyPart(Part_VariablePropertyShorthand c) {
            return new StyleBodyPart(c);
        }

#if DEBUG
        private string DebuggerDisplay() {
            switch (type) {
                case BodyPartType.Property:
                    return property.ToString();

                case BodyPartType.ExtendStyle:
                    return extendStyle.ToString();

                case BodyPartType.Style:
                    return style.ToString();

                case BodyPartType.VariableProperty:
                    return variableProperty.ToString();

                case BodyPartType.ConstantBranch:
                    return constantBranch.ToString();

                case BodyPartType.EnterState:
                    return enterState.ToString();

                case BodyPartType.ConditionBlock:
                    return conditionBlock.ToString();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
#endif

    }

    internal struct Part_ConstantBranch {

        public int conditionId;
        public CharSpan value;

        public Part_ConstantBranch(int conditionId, in CharSpan value) {
            this.conditionId = conditionId;
            this.value = value;
        }

        public override string ToString() {
            return "ConditionBranch = " + value;
        }

    }

    internal struct Part_ExtendStyle {

        public readonly ReflessCharSpan baseStyleName;
        public readonly ushort baseStyleSourceId;
        public readonly ushort baseStyleId;

        public Part_ExtendStyle(CharSpan baseStyleName, ushort baseStyleSourceId, ushort baseStyleId) {
            this.baseStyleName = new ReflessCharSpan(baseStyleName);
            this.baseStyleSourceId = baseStyleSourceId;
            this.baseStyleId = baseStyleId;
        }

        public override string ToString() {
            return "Extend " + baseStyleName;
        }

    }

    internal struct Part_ConditionBlock {

        public readonly ReflessCharSpan conditionName;
        public readonly ushort rangeStart;
        public readonly ushort conditionId;

        public Part_ConditionBlock(CharSpan conditionName, int conditionId, int rangeStart) {
            this.conditionName = new ReflessCharSpan(conditionName);
            this.conditionId = (ushort) conditionId;
            this.rangeStart = (ushort) rangeStart;
        }

        public override string ToString() {
            return "Condition #" + conditionName + "(end at " + rangeStart + ")";
        }

    }

    internal struct Part_Property {

        public readonly StyleProperty2 value;

        public Part_Property(StyleProperty2 value) {
            this.value = value;
        }

        public override string ToString() {
            return value.propertyId.ToString(); //declaration.ToString();
        }

    }

    internal struct Part_VariableProperty {

        public readonly CharSpan declaration;
        public readonly PropertyId propertyId;

        public Part_VariableProperty(PropertyId propertyId, CharSpan declaration) {
            this.propertyId = propertyId;
            this.declaration = declaration;
        }

        public override string ToString() {
            return "Variable Property (" + propertyId + ") = " + declaration;
        }

    }

    internal struct Part_VariablePropertyShorthand {

        public readonly CharSpan declaration;
        public readonly int shorthandIndex;

        public Part_VariablePropertyShorthand(int shorthandIndex, CharSpan declaration) {
            this.shorthandIndex = shorthandIndex;
            this.declaration = declaration;
        }

        public override string ToString() {
            return $"Variable Shorthand Property ({PropertyParsers.s_ShorthandNames[shorthandIndex]}) = {declaration}";
        }

    }

    internal struct Part_EnterState {

        public readonly ushort stateIndex;
        public readonly ushort rangeStart;

        public Part_EnterState(int stateIndex, int rangeStart) {
            this.stateIndex = (ushort) stateIndex;
            this.rangeStart = (ushort) rangeStart;
        }

        public override string ToString() {
            string stateName = null;
            switch (stateIndex) {
                case StyleStateIndex.Normal:
                    stateName = "Normal";
                    break;
                case StyleStateIndex.Hover:
                    stateName = "Hover";
                    break;
                case StyleStateIndex.Active:
                    stateName = "Active";
                    break;
                case StyleStateIndex.Focus:
                    stateName = "Focus";
                    break;
                default:
                    stateName = "--Invalid--";
                    break;
            }

            return "Enter State = " + stateName;
        }

    }

    internal struct Part_Style {

        public readonly CharSpan styleName;
        public readonly int rangeStart;

        public Part_Style(in CharSpan styleName, int rangeStart) {
            this.styleName = styleName;
            this.rangeStart = rangeStart;
        }

        public override string ToString() {
            return "style " + styleName + "(end at " + rangeStart + ")";
        }

    }

}