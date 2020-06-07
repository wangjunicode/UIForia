using System.Diagnostics;
using UnityEngine;

namespace UIForia.Text {

    [DebuggerDisplay("Char = {(char)character}")]
    public struct CharInfo {

        public Vector2 topLeft;
        public Vector2 bottomRight;
        
        public float scale;

        public int character;

        public TextGlyph glyph;

        // for sampling the font texture 
        public Vector2 topLeftUV;
        public Vector2 bottomRightUV;

        // italic style
        public float topShear;
        public float bottomShear;

        // local space, relative to parent element 
        public float wordLayoutX;
        public float wordLayoutY;

        public int lineIndex;
        public bool visible;
        public float kerningAdvance;

        public float LayoutX => wordLayoutX + topLeft.x;
        public float LayoutY => wordLayoutY + topLeft.y;
        
        public float MaxX => wordLayoutX + topLeft.x + (bottomRight.x - topLeft.x);
    }

}