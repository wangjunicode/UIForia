using System;
using System.Runtime.InteropServices;
using UIForia.Rendering;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Text {

    public struct EffectCharInfo {

        public float scale;
        public float colorLerp;
        
        public float3 topLeft;
        public float3 topRight;
        public float3 bottomRight;
        public float3 bottomLeft;

        public float2 uvTopLeft;
        public float2 uvTopRight;
        public float2 uvBottomLeft;
        public float2 uvBottomRight;

        public Color32 colorTopLeft;
        public Color32 colorTopRight;
        public Color32 colorBottomRight;
        public Color32 colorBottomLeft;

    }

    [Flags]
    public enum CharacterFlags : ushort {

        UnderlineStart = 1 << 0,
        UnderlineMid =  1 << 1,
        UnderLineEnd =  1 << 2,
        Renderable = 1 << 3,
        
        Italic = 1 << 4,
        Bold = 1 << 5,
        Subscript = 1 << 6,
        SuperScript = 1 << 7,
        
        UseEffectInfo = 1 << 8
        
    }

    public struct TextMat {

        public byte outlineWidth;
        public byte outlineSoftness;
        public byte underlayDilate;
        public byte underlaySoftness;
        
        public byte glowOffset;
        public byte glowInner;
        public byte glowOuter;
        public byte glowPower;

        public Color32 faceColor;
        public Color32 outlineColor;
        public Color32 underlayColor;
        public Color32 glowColor;
        
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct BurstCharInfo {

        public int character;
        public int wordIndex;
        public ushort materialIndex;
        public CharacterFlags flags;
        public ushort effectIdx;
        public ushort renderBufferIndex;
        public ushort glyphIndex; // dont need?
        public ushort lineIndex;
        
        // can probab
        public int vertexIdx;

        // maybe dont need to store this
        // public float shearTop;
        // public float shearBottom;
        
        // can just store a glyph index here
        public float2 position;
        // public float2 bottomRight;
        // public float2 topLeftUv;
        // public float2 bottomRightUv;

    }

    public struct TextSpan2 {

        public TextStyle style;
        
        public int symbolStart;
        public int symbolEnd;
        public int renderStart;
        public int renderEnd;

    }

    public struct TextEffectInfo {

        public float layoutWidthScale;
        public float layoutHeightScale;

        public float2 offset;
        public float rotation;
        public float vertexInsetX;
        public float vertexInsetY;
        public float opacity;
        public float scaleMultiplier;
        public float colorFade;

        public float2 topLeft;
        public float2 bottomRight;
        public float2 topRight;
        public float2 bottomLeft;
        
        

    }
    
    public class TextTypeWriter {

        public virtual void OnCharacterClicked() {
            
        }

        public virtual void OnSelection() { 
        
        }

        public virtual void OnTextSet(string text) {
            
        }

        // span.AddEffect(effect, id);
        // span.AddRevealEvent
        
        public void OnUpdate() {
      
        }
        
        public virtual void OnCharacterAdded(ManagedTextSpanInfo span, int charIdx) {
       
            
        }

        public enum TextRemoveState {

            Remove,
            DeferredRemove,
            Keep

        }

        public virtual void OnCharacterRemoved() { }

    }

}