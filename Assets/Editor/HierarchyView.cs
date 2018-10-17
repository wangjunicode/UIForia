using System;
using Src.Util;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ElementTreeItem = System.ValueTuple<UIElement, UnityEditor.IMGUI.Controls.TreeViewItem>;

public class HierarchyView : TreeView {

    public UIView view;
    public UIElement rootElement;
    public RectTransform viewTransform;

    public HierarchyView(UIElement rootElement, TreeViewState state) : base(state) {
        this.rootElement = rootElement;
    }

    public HierarchyView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader) { }

    protected override TreeViewItem BuildRoot() {
        Stack<ElementTreeItem> stack = StackPool<ElementTreeItem>.Get();

        TreeViewItem root = new TreeViewItem(-9999, -1);
        TreeViewItem firstChild = new TreeViewItem(rootElement.id, rootElement.depth);
        firstChild.displayName = rootElement.ToString();
        stack.Push(ValueTuple.Create(rootElement, firstChild));

        while (stack.Count > 0) {
            ElementTreeItem current = stack.Pop();
            UIElement element = current.Item1;
            TreeViewItem item = current.Item2;

            UIElement[] ownChildren = element.ownChildren;

            if (ownChildren == null) {
                continue;
            }

            for (int i = 0; i < ownChildren.Length; i++) {
                TreeViewItem childItem = new TreeViewItem(ownChildren[i].id);
                childItem.displayName = ownChildren[i].ToString();

                item.AddChild(childItem);
                stack.Push(ValueTuple.Create(ownChildren[i], childItem));
            }
        }

        root.displayName = rootElement.ToString();
        root.AddChild(firstChild);

        SetupDepthsFromParentsAndChildren(root);

        return root;
    }

    protected override void SelectionChanged(IList<int> selectedIds) {
        int id = selectedIds[0];
        UIElement element = view.GetElement(id);
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