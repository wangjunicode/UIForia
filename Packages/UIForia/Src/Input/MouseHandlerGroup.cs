using UIForia;
using UIForia.Input;

public struct MouseHandlerGroup {

    public readonly ExpressionContext context;
    public readonly MouseEventHandler[] handlers;
    public readonly InputEventType handledEvents;
   
    public MouseHandlerGroup(ExpressionContext context, MouseEventHandler[] bindings, InputEventType handledEvents) {
        this.context = context;
        this.handlers = bindings;
        this.handledEvents = handledEvents;
    }

}
