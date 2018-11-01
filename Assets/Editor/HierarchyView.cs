using System;
using Src.Util;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using Src;
using Src.Editor;
using UnityEditor;
using UnityEngine;

public class HierarchyView : TreeView {

    private struct ViewState {

        public bool showTemplateContents;

    }

    public UIView view;
    public UIElement rootElement;

    public event Action<UIElement> onSelectionChanged;
    private bool needsReload;
    private IntMap<ViewState> m_ViewState;

    public class ElementTreeItem : TreeViewItem {

        public UIElement element;

        public ElementTreeItem(UIElement element) : base(element.id, element.depth) {
            this.element = element;
        }

    }

    private static readonly GUIStyle s_ElementNameStyle;
    private static readonly GUIStyle s_ElementStyleStyle;
    private static readonly GUIStyle s_ElementTemplateRootStyle;

    static HierarchyView() {
        s_ElementNameStyle = new GUIStyle();
        s_ElementStyleStyle = new GUIStyle();
        GUIStyleState elementNameNormal = new GUIStyleState();
        GUIStyleState elementStyleNormal = new GUIStyleState();
        elementNameNormal.textColor = Color.white;
        elementStyleNormal.textColor = Color.yellow;
        s_ElementNameStyle.normal = elementNameNormal;
        s_ElementStyleStyle.normal = elementStyleNormal;
    }

    public HierarchyView(UIElement rootElement, TreeViewState state) : base(state) {
        this.rootElement = rootElement;
        m_ViewState = new IntMap<ViewState>();
        needsReload = true;
    }

    public HierarchyView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader) {
        throw new NotImplementedException();
    }

    protected override TreeViewItem BuildRoot() {
        Stack<ElementTreeItem> stack = StackPool<ElementTreeItem>.Get();

        TreeViewItem root = new TreeViewItem(-9999, -1);
        ElementTreeItem firstChild = new ElementTreeItem(rootElement);
        firstChild.displayName = rootElement.ToString();
        stack.Push(firstChild);

        while (stack.Count > 0) {
            ElementTreeItem current = stack.Pop();
            UIElement element = current.element;

            UIElement[] ownChildren = element.ownChildren;

            if (ownChildren == null) {
                continue;
            }

            if ((element.flags & UIElementFlags.TemplateRoot) != 0) {
                ViewState viewState;
                if (m_ViewState.TryGetValue(element.id, out viewState) && viewState.showTemplateContents) {
                    UIElement[] templateChildren = element.templateChildren;

                    if (templateChildren != null) {
                        for (int i = 0; i < templateChildren.Length; i++) {
                            ElementTreeItem childItem = new ElementTreeItem(templateChildren[i]);
                            childItem.displayName = childItem.element.ToString();
                            current.AddChild(childItem);
                            stack.Push(childItem);
                        }
                    }

                    continue;
                }
            }

            for (int i = 0; i < ownChildren.Length; i++) {
                ElementTreeItem childItem = new ElementTreeItem(ownChildren[i]);
                childItem.displayName = ownChildren[i].ToString();
                current.AddChild(childItem);
                stack.Push(childItem);
            }
        }

        root.displayName = rootElement.ToString();
        root.AddChild(firstChild);

        SetupDepthsFromParentsAndChildren(root);
        needsReload = false;
        return root;
    }

    private static readonly GUIContent s_Content = new GUIContent();

    public bool RunGUI() {
        OnGUI(GUILayoutUtility.GetRect(0, 10000, 0, 10000));
        return needsReload;
    }
    
    protected override void RowGUI(RowGUIArgs args) {
        ElementTreeItem item = (ElementTreeItem) args.item;
        GUIStyleState textStyle = s_ElementNameStyle.normal;

        bool isTemplateRoot = (item.element.flags & UIElementFlags.TemplateRoot) != 0;
        
        Color mainColor = isTemplateRoot ? Color.green : Color.white;
        
        textStyle.textColor = AdjustColor(mainColor, item.element);

        float indent = GetContentIndent(args.item);
        float rowWidth = args.rowRect.width;
        args.rowRect.x += indent;
        args.rowRect.width -= indent;
        s_Content.text = item.element.GetDisplayName();
        Vector2 v = s_ElementNameStyle.CalcSize(s_Content);
        Rect r = new Rect(args.rowRect);
        GUI.Label(args.rowRect, s_Content, s_ElementNameStyle);
        r.x += v.x + 5f;
        r.width -= v.x + 5f;
        s_Content.text = item.element.style.BaseStyleNames;

        textStyle.textColor = AdjustColor(Color.yellow, item.element);

        GUI.Label(r, s_Content, s_ElementNameStyle);

        if (!isTemplateRoot) {
            return;
        }

        ViewState viewState;
        m_ViewState.TryGetValue(item.element.id, out viewState);
        
        r.x = rowWidth - 16;
        r.width = 16;
        s_Content.text = viewState.showTemplateContents ? "+" : "-";
        GUI.Label(r, s_Content);

        if (Event.current.type == EventType.MouseDown) {
            if (r.Contains(Event.current.mousePosition)) {
                viewState.showTemplateContents = !viewState.showTemplateContents;
                m_ViewState[item.element.id] = viewState;
                needsReload = true;
            }
        }
    }

    protected static Color AdjustColor(Color color, UIElement element) {
        return element.isEnabled ? color : new Color(color.r, color.g, color.b, 0.25f);
    }

    protected override void SelectionChanged(IList<int> selectedIds) {
        int id = selectedIds[0];
        UIElement element = view.GetElement(id);
        onSelectionChanged?.Invoke(element);
    }

    public void OnSceneGUI() {
        if (view == null) {
            return;
        }

//        Camera camera;
//        camera.targetTexture = new RenderTexture(128, 128, 0);
//        camera.Render();
//        Handles.cam
//        if (Event.current.type == EventType.MouseMove) {
//            Vector3 mouse = Event.current.mousePosition;
////            mouse.y = Camera.current.pixelHeight - mouse.y;
//            Rect sceneViewRect = SceneView.currentDrawingSceneView.position;
////            Vector3 sceneViewPosition = new Vector2(sceneViewRect.x, sceneViewRect.y);
//            Vector2 rectpos;
//            Vector2 localpoint;
//            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewTransform, sceneViewRect.min, SceneView.currentDrawingSceneView.camera, out localpoint);
//            Debug.Log(localpoint);
//            


//        Handles.BeginGUI();
//        Handles.DrawSolidRectangleWithOutline(new Rect(0, 0, 400, 400), new Color32(255, 0, 0, 75), Color.black);
//        Handles.EndGUI();
    }

}