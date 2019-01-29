namespace SVGX {

    public enum LineCapJoinType {

        ButtCap = 1 << 0,
        SquareCap = 1 << 1, 
        RoundCap = 1 << 2,
        MiterJoin = 1 << 3,
        MiterClipJoin = 1 << 4,
        Bevel = 1 << 5,
        Round = 1 << 6

    }
    
    public enum LineCap {

        Butt,
        Square,
        Round

    }

}