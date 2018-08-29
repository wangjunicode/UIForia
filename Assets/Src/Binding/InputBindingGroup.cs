using Src;
using Src.Input;
using Src.InputBindings;

public struct InputBindingGroup {

    public readonly UITemplateContext context;
    public readonly InputBinding[] bindings;
    public readonly InputEventType handledEvents;

    public InputBindingGroup(UITemplateContext context, InputBinding[] bindings, InputEventType handledEvents) {
        this.context = context;
        this.bindings = bindings;
        this.handledEvents = handledEvents;
    }

}