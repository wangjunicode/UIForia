using System;

namespace UIForia.NewStyleParsing {

    [Flags]
    public enum StyleNodeType {

        Style = 1 << 0,
        Mixin = 1 << 1,
        Animation = 1 << 2,
        Selector = 1 << 3,
        Package = 1 << 4,
        ConstVariable = 1 << 5,
        Property = 1 << 6,
        ShorthandProperty = 1 << 7,
        State_Active = 1 << 8,
        State_Focused = 1 << 9,
        State_Hover = 1 << 10,
        State_Normal = 1 << 11,
        ConditionBlock = 1 << 12,
        Root = 1 << 13,

        StyleState = State_Active | State_Focused | State_Hover | State_Normal,


    }

}