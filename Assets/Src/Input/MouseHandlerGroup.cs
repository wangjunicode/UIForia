using System;
using System.Reflection;
using Src;
using Src.Compilers;
using Src.Input;

public struct MouseHandlerGroup {

    public readonly UITemplateContext context;
    public readonly MouseEventHandler[] handlers;
    public readonly InputEventType handledEvents;

    public MouseHandlerGroup(UITemplateContext context, MouseEventHandler[] bindings, InputEventType handledEvents) {
        this.context = context;
        this.handlers = bindings;
        this.handledEvents = handledEvents;
    }

}

[Flags]
public enum DragEventType {

    Create = 1 << 0,
    Start = 1 << 1,
    Update = 1 << 2,
    Drop = 1 << 3,
    Cancel = 1 << 4,
    End = 1 << 5,
    Enter = 1 << 6,
    Exit = 1 << 7,
    Move = 1 << 8,
    Hover = 1 << 9

}