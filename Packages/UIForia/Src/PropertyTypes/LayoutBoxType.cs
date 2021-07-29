namespace UIForia.Layout {

    internal enum LayoutBoxType : byte {

        // important: identical to LayoutType values for external values 
        FlexVertical = 0,
        FlexHorizontal = 1,
        GridHorizontal = 2 ,
        GridVertical = 3,
        Radial = 4,
        Stack = 5,
        
        // internal, cannot set from style system 
        TextHorizontal = 6,
        TextVertical = 7,
        Scroll = 8,
        Image = 9, // does this need to be a layout box really? maybe because it defaults to a different size? 

        Ignored = 10,
        
        // always last
        __COUNT__

    }

}