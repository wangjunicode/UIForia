using System;
using System.Collections.Generic;
using Rendering;
using Src;
using Src.Input;
using Src.Systems;
using UnityEngine;

public abstract partial class InputSystem : IInputSystem, IInputProvider {

    private const float k_DragThreshold = 5f;
    private const string k_EventAlias = "$event";

    private readonly IStyleSystem m_StyleSystem;
    private readonly ILayoutSystem m_LayoutSystem;

    private List<UIElement> m_ElementsThisFrame;
    private List<UIElement> m_ElementsLastFrame;

    private int m_FocusedId;
    private DragEvent m_CurrentDragEvent;
    private KeyboardModifiers modifiersThisFrame;

    protected MouseState m_MouseState;

    private readonly List<UIElement> m_ExitedElements;
    private readonly List<UIElement> m_EnteredElements;
    private readonly List<UIElement> m_MouseDownElements;
    private readonly Dictionary<KeyCode, KeyState> m_KeyStates;
    private readonly Dictionary<int, MouseHandlerGroup> m_MouseHandlerMap;
    private readonly Dictionary<int, DragHandlerGroup> m_DragHandlerMap;
    private readonly Dictionary<int, DragCreatorGroup> m_DragCreatorMap;
    private readonly SkipTree<KeyboardEventTreeNode> m_KeyboardEventTree;

    private int m_LayoutResultCount;
    private LayoutResult[] m_LayoutQueryResults;

    private readonly List<KeyCode> m_DownThisFrame;
    private readonly List<KeyCode> m_UpThisFrame;

    private readonly EventPropagator m_EventPropagator;
    private readonly List<ValueTuple<MouseEventHandler, UIElement, UITemplateContext>> m_MouseEventCaptureList;
    private readonly List<ValueTuple<DragEventHandler, UIElement, UITemplateContext>> m_DragEventCaptureList;
    protected static readonly UIElement.DepthComparerAscending s_DepthComparer = new UIElement.DepthComparerAscending();

    protected InputSystem(ILayoutSystem layoutSystem, IStyleSystem styleSystem) {
        this.m_LayoutSystem = layoutSystem;
        this.m_StyleSystem = styleSystem;

        this.m_MouseDownElements = new List<UIElement>();
        this.m_ElementsThisFrame = new List<UIElement>();
        this.m_ElementsLastFrame = new List<UIElement>();
        this.m_EnteredElements = new List<UIElement>();
        this.m_ExitedElements = new List<UIElement>();

        this.m_MouseHandlerMap = new Dictionary<int, MouseHandlerGroup>();
        this.m_DragCreatorMap = new Dictionary<int, DragCreatorGroup>();
        this.m_DragHandlerMap = new Dictionary<int, DragHandlerGroup>();
        this.m_LayoutQueryResults = new LayoutResult[16];

        this.m_UpThisFrame = new List<KeyCode>();
        this.m_DownThisFrame = new List<KeyCode>();
        this.m_KeyStates = new Dictionary<KeyCode, KeyState>();
        this.m_KeyboardEventTree = new SkipTree<KeyboardEventTreeNode>();
        this.m_EventPropagator = new EventPropagator();
        this.m_MouseEventCaptureList = new List<ValueTuple<MouseEventHandler, UIElement, UITemplateContext>>();
        this.m_DragEventCaptureList = new List<ValueTuple<DragEventHandler, UIElement, UITemplateContext>>();
        this.m_FocusedId = -1;
    }

    protected abstract MouseState GetMouseState();

    public virtual void OnUpdate() {
        m_MouseState = GetMouseState();

        ProcessKeyboardEvents();
        ProcessMouseInput();

        if (!IsDragging) {
            ProcessMouseEvents();
        }

        ProcessDragEvents();

        List<UIElement> temp = m_ElementsLastFrame;
        m_ElementsLastFrame = m_ElementsThisFrame;
        m_ElementsThisFrame = temp;

        m_ElementsThisFrame.Clear();
        m_EnteredElements.Clear();
        m_ExitedElements.Clear();

        if (IsMouseLeftUpThisFrame) {
            m_MouseDownElements.Clear();
        }
    }

    private void ProcessMouseInput() {
        m_LayoutResultCount = m_LayoutSystem.QueryPoint(m_MouseState.mousePosition, ref m_LayoutQueryResults);
        
        for (int i = 0; i < m_LayoutResultCount; i++) {
            int elementId = m_LayoutQueryResults[i].element.id;

            UIElement element = m_LayoutQueryResults[i].element;

            m_ElementsThisFrame.Add(element);

            if (!m_ElementsLastFrame.Contains(element)) {
                m_EnteredElements.Add(element);
                m_StyleSystem.EnterState(elementId, StyleState.Hover);
            }
        }

        for (int i = 0; i < m_ElementsLastFrame.Count; i++) {
            if (!m_ElementsThisFrame.Contains(m_ElementsLastFrame[i])) {
                m_ExitedElements.Add(m_ElementsLastFrame[i]);
                m_StyleSystem.ExitState(m_ElementsLastFrame[i].id, StyleState.Hover);
            }
        }

        m_EnteredElements.Sort(s_DepthComparer);
        m_ElementsThisFrame.Sort(s_DepthComparer);
        
        if (m_MouseState.isLeftMouseDownThisFrame) {
            m_MouseDownElements.AddRange(m_ElementsThisFrame);
        }
        
    }

}