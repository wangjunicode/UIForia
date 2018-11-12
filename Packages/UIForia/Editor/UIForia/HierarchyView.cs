using System;
using UIForia.Util;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using UIForia;
using UIForia.Editor;
using UIForia.Rendering;
using UIForia.Systems;
using UnityEditor;
using UnityEngine;
using Application = UIForia.Application;

public class HierarchyView : TreeView {

    private struct ViewState {

        public bool showTemplateContents;

    }

    public UIView view;
    public UIElement rootElement;

    public event Action<UIElement> onSelectionChanged;
    public bool needsReload;
    private readonly IntMap<ViewState> m_ViewState;

    public class ElementTreeItem : TreeViewItem {

        public readonly UIElement element;

        public ElementTreeItem(UIElement element) : base(element.id, element.depth) {
            this.element = element;
        }

    }

    private static readonly GUIStyle s_ElementNameStyle;
    private static readonly GUIStyle s_ElementTemplateRootStyle;

    static HierarchyView() {
        s_ElementNameStyle = new GUIStyle();
        GUIStyleState elementNameNormal = new GUIStyleState();
        GUIStyleState elementStyleNormal = new GUIStyleState();
        elementNameNormal.textColor = Color.white;
        elementStyleNormal.textColor = Color.yellow;
        s_ElementNameStyle.normal = elementNameNormal;
    }

    public HierarchyView(UIElement rootElement, TreeViewState state) : base(state) {
        this.rootElement = rootElement;
        m_ViewState = new IntMap<ViewState>();
        needsReload = true;
    }

    public HierarchyView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader) {
        throw new NotImplementedException();
    }

    public void SetRootElement(UIElement element) {
        this.rootElement = element;
        m_ViewState.Clear();
        Reload();
        SetExpandedRecursive(rootItem.id, true);
    }

    protected override TreeViewItem BuildRoot() {
        Stack<ElementTreeItem> stack = StackPool<ElementTreeItem>.Get();
        
        // todo -- maybe pool tree items
        
        TreeViewItem root = new TreeViewItem(-9999, -1);
        ElementTreeItem firstChild = new ElementTreeItem(rootElement);
        firstChild.displayName = rootElement.ToString();
        stack.Push(firstChild);

        while (stack.Count > 0) {
            ElementTreeItem current = stack.Pop();
            UIElement element = current.element;

            UIElement[] ownChildren = element.children;

            if (ownChildren == null) {
                continue;
            }

//            if ((element.flags & UIElementFlags.TemplateRoot) != 0) {
//                ViewState viewState;
//                if (m_ViewState.TryGetValue(element.id, out viewState) && viewState.showTemplateContents) {
//                    UIElement[] templateChildren = element.templateChildren;
//
//                    if (templateChildren != null) {
//                        for (int i = 0; i < templateChildren.Length; i++) {
//                            ElementTreeItem childItem = new ElementTreeItem(templateChildren[i]);
//                            childItem.displayName = childItem.element.ToString();
//                            current.AddChild(childItem);
//                            stack.Push(childItem);
//                        }
//                    }
//
//                    continue;
//                }
//            }

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

    private string GetCullText(CullResult result) {
        switch (result) {
            
            case CullResult.NotCulled:
                return string.Empty;
            
            case CullResult.ClipRectIsZero:
                return "[Culled - Fully Clipped]";
            
            case CullResult.ActualSizeZero:
                return "[Culled - Size is zero]";
            
            case CullResult.OpacityZero:
                return "[Culled - Opacity is zero]";
                
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }
    
    protected override void RowGUI(RowGUIArgs args) {
        ElementTreeItem item = (ElementTreeItem) args.item;
        GUIStyleState textStyle = s_ElementNameStyle.normal;

        bool isTemplateRoot = (item.element.flags & UIElementFlags.TemplateRoot) != 0;

        bool isChildren = item.element is UIChildrenElement;
        
        Color mainColor = isTemplateRoot ? Color.green : Color.white;
        if (isChildren) {
            mainColor = new Color32(255, 0, 99, 255);
        }
        
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

        RenderData renderData = Application.Game.RenderSystem.GetRenderData(item.element);

        if (renderData != null && renderData.CullResult != CullResult.NotCulled) {
            v = s_ElementNameStyle.CalcSize(s_Content);
            r.x += v.x + 5f;
            r.width -= v.x + 5f;
            s_Content.text = GetCullText(renderData.CullResult);
            textStyle.textColor = AdjustColor(Color.red, item.element);
            GUI.Label(r, s_Content, s_ElementNameStyle);
        }
        
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

    private static Color AdjustColor(Color color, UIElement element) {
        return element.isEnabled ? color : new Color(color.r, color.g, color.b, 0.25f);
    }

    protected override void SelectionChanged(IList<int> selectedIds) {
        int id = selectedIds[0];
        UIElement element = Application.Game.GetElement(id);
        onSelectionChanged?.Invoke(element);
    }

}