using Src;
using Src.Input;
using Src.Systems;

public abstract partial class InputSystem {

    public void OnReady() { }

    public void OnInitialize() { }

    public void OnReset() {
        // don't clear key states
        m_FocusedId = -1;
        m_LayoutResultCount = 0;
        m_ElementsLastFrame.Clear();
        m_ElementsThisFrame.Clear();
        m_MouseDownElements.Clear();
        m_KeyboardEventTree.Clear();
        m_MouseHandlerMap.Clear();
        m_DragCreatorMap.Clear();
        m_DragHandlerMap.Clear();
    }

    public void OnDestroy() { }

    public void OnElementEnabled(UIElement element) { }

    public void OnElementDisabled(UIElement element) { }

    public void OnElementDestroyed(UIElement element) {
        m_ElementsLastFrame.Remove(element);
        m_ElementsThisFrame.Remove(element);
        m_MouseDownElements.Remove(element);
        m_KeyboardEventTree.RemoveHierarchy(element);
        // todo -- clear child handlers
        m_MouseHandlerMap.Remove(element.id);
        m_DragCreatorMap.Remove(element.id);
        m_DragHandlerMap.Remove(element.id);
    }

    public void OnElementShown(UIElement element) { }

    public void OnElementHidden(UIElement element) { }

    public void OnElementCreated(MetaData elementData) {
        
        MouseEventHandler[] mouseHandlers = elementData.mouseEventHandlers;
        DragEventCreator[] dragEventCreators = elementData.dragEventCreators;
        DragEventHandler[] dragEventHandlers = elementData.dragEventHandlers;
        KeyboardEventHandler[] keyboardHandlers = elementData.keyboardEventHandlers;
        
        if (mouseHandlers != null && mouseHandlers.Length > 0) {
            InputEventType handledEvents = 0;

            for (int i = 0; i < mouseHandlers.Length; i++) {
                handledEvents |= mouseHandlers[i].eventType;
            }

            m_MouseHandlerMap[elementData.elementId] = new MouseHandlerGroup(elementData.context, mouseHandlers, handledEvents);
        }

        if (dragEventHandlers != null && dragEventHandlers.Length > 0) {
            InputEventType handledEvents = 0;

            for (int i = 0; i < dragEventHandlers.Length; i++) {
                handledEvents |= dragEventHandlers[i].eventType;
            }
            
            m_DragHandlerMap[elementData.elementId] = new DragHandlerGroup(elementData.context, dragEventHandlers, handledEvents);
        }
        
        if (keyboardHandlers != null && keyboardHandlers.Length > 0) {
            m_KeyboardEventTree.AddItem(new KeyboardEventTreeNode(elementData.element, keyboardHandlers));
        }

        if (dragEventCreators != null) {
            m_DragCreatorMap[elementData.elementId] = new DragCreatorGroup(elementData.context, dragEventCreators);
        }
        
        for (int i = 0; i < elementData.children.Count; i++) {
            OnElementCreated(elementData.children[i]);
        }
    }

}