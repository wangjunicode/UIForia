using System;
using System.Collections.Generic;
using Rendering;
using Src;
using Src.Input;
using UnityEngine;

public abstract partial class InputSystem {

    public bool IsMouseLeftDown => m_MouseState.isLeftMouseDown;
    public bool IsMouseLeftDownThisFrame => m_MouseState.isLeftMouseDownThisFrame;
    public bool IsMouseLeftUpThisFrame => m_MouseState.isLeftMouseUpThisFrame;

    public bool IsMouseRightDown => m_MouseState.isRightMouseDown;
    public bool IsMouseRightDownThisFrame => m_MouseState.isRightMouseDownThisFrame;
    public bool IsMouseRightUpThisFrame => m_MouseState.isRightMouseUpThisFrame;

    public bool IsMouseMiddleDown => m_MouseState.isMiddleMouseDown;
    public bool IsMouseMiddleDownThisFrame => m_MouseState.isMiddleMouseDownThisFrame;
    public bool IsMouseMiddleUpThisFrame => m_MouseState.isMiddleMouseUpThisFrame;

    public Vector2 ScrollDelta => m_MouseState.scrollDelta;

    public Vector2 MousePosition => m_MouseState.mousePosition;
    public Vector2 MouseDownPosition => m_MouseState.mouseDownPosition;

    public bool IsDragging { get; protected set; }

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
            mouseHandlerGroup.context.SetObjectAlias(k_EventAlias, boxedEvent);

            for (int j = 0; j < handlers.Length; j++) {
                MouseEventHandler handler = handlers[j];
                if (handler.eventType != eventType) {
                    continue;
                }

                if (handler.eventPhase != EventPhase.Bubble) {
                    m_MouseEventCaptureList.Add(ValueTuple.Create(handler, element, mouseHandlerGroup.context));
                    continue;
                }

                handler.Invoke(element, mouseHandlerGroup.context, mouseEvent);
                if (m_EventPropagator.shouldStopPropagation) {
                    break;
                }
            }

            mouseHandlerGroup.context.RemoveObjectAlias(k_EventAlias);
            if (m_EventPropagator.shouldStopPropagation) {
                m_MouseEventCaptureList.Clear();
                return;
            }
        }

        for (int i = 0; i < m_MouseEventCaptureList.Count; i++) {
            MouseEventHandler handler = m_MouseEventCaptureList[i].Item1;
            UIElement element = m_MouseEventCaptureList[i].Item2;
            UITemplateContext context = m_MouseEventCaptureList[i].Item3;
            context.SetObjectAlias(k_EventAlias, boxedEvent);

            handler.Invoke(element, context, mouseEvent);

            context.RemoveObjectAlias(k_EventAlias);

            if (m_EventPropagator.shouldStopPropagation) {
                m_MouseEventCaptureList.Clear();
                return;
            }
        }

        m_MouseEventCaptureList.Clear();
    }


    private void ProcessMouseEvents() {

        RunMouseEvents(m_ExitedElements, InputEventType.MouseExit);
        RunMouseEvents(m_EnteredElements, InputEventType.MouseEnter);

        if (m_MouseState.isLeftMouseDownThisFrame) {
            RunMouseEvents(m_ElementsThisFrame, InputEventType.MouseDown);
        }
        else if (m_MouseState.isLeftMouseUpThisFrame) {
            RunMouseEvents(m_ElementsThisFrame, InputEventType.MouseUp);
        }

        RunMouseEvents(m_ElementsThisFrame, m_MouseState.DidMove ? InputEventType.MouseMove : InputEventType.MouseHover);

    }

   

}