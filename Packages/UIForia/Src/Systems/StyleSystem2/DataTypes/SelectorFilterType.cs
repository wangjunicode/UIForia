using System;

namespace UIForia {

    [Flags]
    public enum SelectorFilterType {

        ElementsWithTag = 1 << 0,
        ElementsWithState = 1 << 1,
        ElementsWithStyle = 1 << 2,
        ElementsWithAttribute = 1 << 3,
        ElementsWithAttribute_ValueContains = 1 << 4,
        ElementsWithAttribute_ValueEquals = 1 << 5,
        ElementsWithAttribute_ValueStartsWith = 1 << 6,
        ElementsWithAttribute_ValueEndsWith = 1 << 7,
        Inverted = 1 << 15
    }

}