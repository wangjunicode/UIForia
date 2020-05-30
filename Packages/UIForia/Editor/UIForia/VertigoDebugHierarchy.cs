using System;
using System.Collections.Generic;
using UIForia;
using UIForia.Editor;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace VertigoEditor {

    public class VertigoHierarchyWindow : EditorWindow {

        public TreeViewState state;
        public HierarchyView2 treeView;
        private bool needsReload;

        public VertigoApplication selectedApplication;

        public void OnEnable() {
            state = new TreeViewState();
            autoRepaintOnSceneChange = true;
            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
            treeView = new HierarchyView2(this, state);
        }

        private void OnInspectorUpdate() {
            Repaint();
        }

        public void OnGUI() {
            EditorGUIUtility.labelWidth += 50;
            DrawHierarchyInfo();
            EditorGUIUtility.labelWidth -= 50;
        }

        private void DrawHierarchyInfo() {

            EditorGUILayout.BeginVertical();

            IReadOnlyList<VertigoApplication> applications = VertigoApplication.GetActiveApplications();

            string[] names = new string[applications.Count + 1];
            names[0] = "None";

            int oldIdx = 0;

            for (int i = 0; i < applications.Count; i++) {
                names[i + 1] = applications[i].name;
                if (applications[i] == selectedApplication) {
                    oldIdx = i + 1;
                }
            }

            int idx = EditorGUILayout.Popup(new GUIContent("Application"), oldIdx, names);

            if (idx != oldIdx && idx > 0) {

                SetApplication(applications[idx - 1]);

            }

            if (idx == 0) {
                SetApplication(null);
            }

            if (selectedApplication == null) {
                EditorGUILayout.EndVertical();
                return;
            }

            if (needsReload) {
                needsReload = false;
                treeView.Reload();
                treeView.ExpandAll();
            }

            needsReload = treeView.RunGUI();

            EditorGUILayout.EndVertical();
        }

        private void SetApplication(VertigoApplication application) {
            treeView.needsReload = true;
            needsReload = true;
            selectedApplication = application;
        }

        private void Update() {
            if (selectedApplication == null) {
                return;
            }

            Repaint();
        }

    }

    public class HierarchyView2 : TreeView {

        private struct ViewState {

            public bool showTemplateContents;

        }

        private readonly ManagedIntMap<ViewState> m_ViewState;

        public bool needsReload;
        public event Action<UIElement> onSelectionChanged;

        private static readonly GUIStyle s_ElementNameStyle;
        private static readonly GUIStyle s_ElementTemplateRootStyle;
        private static readonly GUIContent s_Content = new GUIContent();

        public bool showChildrenAndId = false;
        public bool showDisabled = false;
        private VertigoHierarchyWindow editorWindow;

        static HierarchyView2() {
            s_ElementNameStyle = new GUIStyle();
            GUIStyleState elementNameNormal = new GUIStyleState();
            GUIStyleState elementStyleNormal = new GUIStyleState();
            elementNameNormal.textColor = UIForiaEditorTheme.elementNameNormal;
            elementStyleNormal.textColor = UIForiaEditorTheme.elementStyleNormal;
            s_ElementNameStyle.normal = elementNameNormal;
        }

        public HierarchyView2(VertigoHierarchyWindow editorWindow, TreeViewState state) : base(state) {
            this.editorWindow = editorWindow;
            m_ViewState = new ManagedIntMap<ViewState>();
            needsReload = true;
        }

        public void Destroy() {
            onSelectionChanged = null;
        }

        protected override TreeViewItem BuildRoot() {
            LightStack<ElementTreeItem> stack = LightStack<ElementTreeItem>.Get();

            TreeViewItem root = new TreeViewItem(-9999, -1);

            List<UIElement> children = new List<UIElement>(32);

            VertigoApplication application = editorWindow.selectedApplication;

            if (application == null) {
                return root;
            }

            RootWindow rootWindow = application.windowManager.rootWindow;

            UIElement rootElement = rootWindow.GetRootElement();

            ElementTreeItem firstChild = new ElementTreeItem(rootElement);
            firstChild.displayName = rootElement.ToString();
            stack.Push(firstChild);

            while (stack.Count > 0) {
                ElementTreeItem current = stack.Pop();
                // if (current.element.isDisabled && !showDisabled) {
                //     continue;
                // }

                UIElement element = current.element;

                element.GetChildren(children);

                if (children.Count == 0) {
                    continue;
                }

                for (int i = 0; i < children.Count; i++) {
                    ElementTreeItem childItem = new ElementTreeItem(children[i]);
                    // if (childItem.element.isDisabled && !showDisabled) {
                    //     continue;
                    // }

                    childItem.displayName = children[i].ToString();
                    current.AddChild(childItem);
                    stack.Push(childItem);
                }
            }

            root.AddChild(firstChild);

            root.displayName = "ROOT";
            SetupDepthsFromParentsAndChildren(root);
            needsReload = false;
            if (root.children == null) {
                root.children = new List<TreeViewItem>();
            }

            LightStack<ElementTreeItem>.Release(ref stack);
            return root;
        }

        public bool RunGUI() {
            OnGUI(GUILayoutUtility.GetRect(0, 10000, 0, 10000));
            return needsReload;
        }

        protected override void RowGUI(RowGUIArgs args) {
            ElementTreeItem item = (ElementTreeItem) args.item;
            GUIStyleState textStyle = s_ElementNameStyle.normal;

            bool isTemplateRoot = (item.element.flags & UIElementFlags.TemplateRoot) != 0;

            Color mainColor = isTemplateRoot
                ? UIForiaEditorTheme.mainColorTemplateRoot
                : UIForiaEditorTheme.mainColorRegularChild;

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

            List<string> names = ListPool<string>.Get();

            // item.element.style.GetStyleNameList(names);
            string styleName = string.Empty;

            for (int i = 0; i < names.Count; i++) {
                styleName += names[i] + " ";
            }

            ListPool<string>.Release(ref names);

            if (styleName.Length > 0) {
                styleName = '[' + styleName.TrimEnd() + "] ";
            }

            s_Content.text = styleName; // + "(children: " + box.children.Count + ", id: " + item.element.id + ")";

            if (showChildrenAndId) {
                s_Content.text += "(id: " + item.element?.id + ")";
            }

            textStyle.textColor = AdjustColor(UIForiaEditorTheme.elementStyleNormal, item.element);

            GUI.Label(r, s_Content, s_ElementNameStyle);

            v = s_ElementNameStyle.CalcSize(s_Content);
            r.x += v.x + 5f;
            r.width -= v.x + 5f;

            r = DrawAdditionalInfo(item.element, r);

            if (!isTemplateRoot) {
                return;
            }

            ViewState viewState;
            m_ViewState.TryGetValue((int) item.element.id, out viewState);

            r.x = rowWidth - 16;
            r.width = 16;
            s_Content.text = viewState.showTemplateContents ? "+" : "-";
            GUI.Label(r, s_Content);

            if (Event.current.type == EventType.MouseDown) {
                if (r.Contains(Event.current.mousePosition)) {
                    viewState.showTemplateContents = !viewState.showTemplateContents;
                    m_ViewState[(int) item.element.id] = viewState;
                    needsReload = true;
                }
            }
        }

        private static Rect DrawAdditionalInfo(UIElement element, Rect rect) {
            if (element is UITextElement textElement) {
                if (!string.IsNullOrEmpty(textElement.text)) {
                    if (textElement.text.Length <= 20) {
                        s_Content.text = '"' + textElement.text.Trim() + '"';
                    }
                    else {
                        s_Content.text = '"' + textElement.text.Substring(0, 20).Trim() + "...\"";
                    }

                    s_ElementNameStyle.normal.textColor = AdjustColor(UIForiaEditorTheme.elementNameNormal, element);
                    GUI.Label(rect, s_Content, s_ElementNameStyle);
                    Vector2 size = s_ElementNameStyle.CalcSize(s_Content);
                    rect.x += size.x;
                    rect.width -= size.x;
                }
            }

            return rect;
        }

        private static Color AdjustColor(Color color, UIElement element) {
            return element.isEnabled ? color : new Color(color.r, color.g, color.b, 0.25f);
        }

        protected override void SelectionChanged(IList<int> selectedIds) {

            // if (selectedIds.Count == 0) {
            //     onSelectionChanged?.Invoke(null);
            //     return;
            // }
            //
            // int id = selectedIds[0];
            // UIElement element = UIForiaHierarchyWindow.s_SelectedApplication.GetElement(id);
            // onSelectionChanged?.Invoke(element);
        }

    }

}