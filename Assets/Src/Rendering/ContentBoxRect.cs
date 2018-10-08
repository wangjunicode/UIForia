using System.Diagnostics;
using Rendering;
using Src;
using UnityEngine;

[DebuggerDisplay("{top}, {right}, {bottom}, {left}")]
public struct ContentBoxRect {

    public UIMeasurement top;
    public UIMeasurement right;
    public UIMeasurement bottom;
    public UIMeasurement left;

    public static readonly ContentBoxRect Unset = new ContentBoxRect(FloatUtil.UnsetValue);

    public ContentBoxRect(float value) {
        this.top = value;
        this.right = value;
        this.bottom = value;
        this.left = value;
    }

    public ContentBoxRect(float top, float right, float bottom, float left) {
        this.top = top;
        this.right = right;
        this.bottom = bottom;
        this.left = left;
    }

    public ContentBoxRect(UIMeasurement top, UIMeasurement right, UIMeasurement bottom, UIMeasurement left) {
        this.top = top;
        this.right = right;
        this.bottom = bottom;
        this.left = left;
    }
   
    public bool Equals(ContentBoxRect other) {
        return top == other.top
               && right == other.right
               && bottom == other.bottom
               && left == other.left;
    }

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        return obj is ContentBoxRect && Equals((ContentBoxRect) obj);
    }

    [DebuggerStepThrough]
    public static bool operator ==(ContentBoxRect self, ContentBoxRect other) {
        return self.top == other.top
               && self.left == other.left
               && self.right == other.right
               && self.bottom == other.bottom;
    }

    [DebuggerStepThrough]
    public static bool operator !=(ContentBoxRect self, ContentBoxRect other) {
        return !(self == other);
    }

}


[DebuggerDisplay("{top}, {right}, {bottom}, {left}")]
public struct PaddingBox {

    public UIFixedLength top;
    public UIFixedLength right;
    public UIFixedLength bottom;
    public UIFixedLength left;

    public static readonly ContentBoxRect Unset = new ContentBoxRect(FloatUtil.UnsetValue);

    public PaddingBox(float value) {
        this.top = value;
        this.right = value;
        this.bottom = value;
        this.left = value;
    }

    public PaddingBox(float top, float right, float bottom, float left) {
        this.top = top;
        this.right = right;
        this.bottom = bottom;
        this.left = left;
    }

    public PaddingBox(UIFixedLength top, UIFixedLength right, UIFixedLength bottom, UIFixedLength left) {
        this.top = top;
        this.right = right;
        this.bottom = bottom;
        this.left = left;
    }
   
    public bool Equals(PaddingBox other) {
        return top == other.top
               && right == other.right
               && bottom == other.bottom
               && left == other.left;
    }

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        return obj is PaddingBox && Equals((PaddingBox) obj);
    }

    [DebuggerStepThrough]
    public static bool operator ==(PaddingBox self, PaddingBox other) {
        return self.top == other.top
               && self.left == other.left
               && self.right == other.right
               && self.bottom == other.bottom;
    }

    [DebuggerStepThrough]
    public static bool operator !=(PaddingBox self, PaddingBox other) {
        return !(self == other);
    }

}