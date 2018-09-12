using System;
using System.Collections.Generic;
using Src;
using Src.Input;
using Src.Systems;
using UnityEngine;

public abstract partial class InputSystem : IInputSystem, IInputProvider {

    private const string EventAlias = "$event";

    private readonly IStyleSystem m_StyleSystem;
    private readonly ILayoutSystem m_LayoutSystem;

    private List<UIElement> m_ElementsThisFrame;
    private List<UIElement> m_ElementsLastFrame;
    
    private readonly List<UIElement> m_EnteredElements;
    private readonly List<UIElement> m_ExitedElements;
    private readonly Dictionary<KeyCode, KeyState> m_KeyStates;
    private readonly Dictionary<int, MouseHandlerGroup> m_MouseHandlerMap;
    private readonly SkipTree<KeyboardEventTreeNode> keyboardEventTree;

    private int m_LayoutResultCount;
    private LayoutResult[] m_LayoutQueryResults;

    private readonly List<KeyCode> downThisFrame;
    private readonly List<KeyCode> upThisFrame;

    private KeyboardModifiers modifiersThisFrame;

    private int m_FocusedId;
    protected MouseState m_MouseState;

    private readonly EventPropagator m_EventPropagator;
    private readonly List<ValueTuple<MouseEventHandler, UIElement, UITemplateContext>> m_CaptureList;

    protected static readonly UIElement.DepthComparerAscending s_DepthComparer = new UIElement.DepthComparerAscending();

    protected InputSystem(ILayoutSystem layoutSystem, IStyleSystem styleSystem) {
        this.m_LayoutSystem = layoutSystem;
        this.m_StyleSystem = styleSystem;


        this.m_ElementsThisFrame = new List<UIElement>();
        this.m_ElementsLastFrame = new List<UIElement>();
        this.m_EnteredElements = new List<UIElement>();
        this.m_ExitedElements = new List<UIElement>();

        this.m_MouseHandlerMap = new Dictionary<int, MouseHandlerGroup>();
        this.m_LayoutQueryResults = new LayoutResult[16];

        this.upThisFrame = new List<KeyCode>();
        this.downThisFrame = new List<KeyCode>();
        this.m_KeyStates = new Dictionary<KeyCode, KeyState>();
        this.keyboardEventTree = new SkipTree<KeyboardEventTreeNode>();
        this.m_EventPropagator = new EventPropagator();
        this.m_CaptureList = new List<ValueTuple<MouseEventHandler, UIElement, UITemplateContext>>();
        this.m_FocusedId = -1;
    }

    protected abstract MouseState GetMouseState();

    public void OnUpdate() {
        m_MouseState = GetMouseState();

        ProcessKeyboardEvents();
        ProcessMouseEvents();

        List<UIElement> temp = m_ElementsLastFrame;
        m_ElementsLastFrame = m_ElementsThisFrame;
        m_ElementsThisFrame = temp;

        m_ElementsThisFrame.Clear();
    }

    private void RunMouseEvents(List<UIElement> elements, InputEventType eventType) {
        m_EventPropagator.Reset(m_MouseState);
        MouseInputEvent mouseEvent = new MouseInputEvent(m_EventPropagator, eventType, modifiersThisFrame);
        object boxedEvent = mouseEvent;
        for (int i = 0; i < elements.Count; i++) {
            UIElement element = elements[i];
            MouseHandlerGroup mouseHandlerGroup;

            if (!m_MouseHandlerMap.TryGetValue(element.id, out mouseHandlerGroup)) {
                continue;
            }

            if ((mouseHandlerGroup.handledEvents & eventType) == 0) {
                continue;
            }

            MouseEventHandler[] handlers = mouseHandlerGroup.handlers;
            mouseHandlerGroup.context.SetObjectAlias(EventAlias, boxedEvent);

            for (int j = 0; j < handlers.Length; j++) {
                MouseEventHandler handler = handlers[j];
                if (handler.eventType != eventType) {
                    continue;
                }

                if (handler.eventPhase != EventPhase.Bubble) {
                    m_CaptureList.Add(ValueTuple.Create(handler, element, mouseHandlerGroup.context));
                    continue;
                }

                handler.Invoke(element, mouseHandlerGroup.context, mouseEvent);
                if (m_EventPropagator.shouldStopPropagation) {
                    break;
                }
            }

            mouseHandlerGroup.context.RemoveObjectAlias(EventAlias);
            if (m_EventPropagator.shouldStopPropagation) {
                m_CaptureList.Clear();
                return;
            }
        }

        for (int i = 0; i < m_CaptureList.Count; i++) {
            MouseEventHandler handler = m_CaptureList[i].Item1;
            UIElement element = m_CaptureList[i].Item2;
            UITemplateContext context = m_CaptureList[i].Item3;
            context.SetObjectAlias(EventAlias, boxedEvent);

            handler.Invoke(element, context, mouseEvent);

            context.RemoveObjectAlias(EventAlias);

            if (m_EventPropagator.shouldStopPropagation) {
                m_CaptureList.Clear();
                return;
            }
        }

        m_CaptureList.Clear();
    }

}