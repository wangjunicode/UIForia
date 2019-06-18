using System;
using System.Collections.Generic;
using System.Reflection;
using UIForia.Bindings;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UIForia.Editor {

    public class BindingNodeTreeItem : TreeViewItem {

        public BindingNode node;

        public BindingNodeTreeItem(BindingNode node) {
            this.node = node;
            if (node == null) {
                this.depth = -1;
                this.id = -9999;
                this.displayName = "Root";
            }
            else {
                this.id = node.UniqueId;
                this.displayName = node.element.GetDisplayName() + " id: " + node.element.id;
            }
        }

    }

    public class SkipTreeView : TreeView {

        public bool needsReload;

        public event Action<BindingNode> onSelectionChanged;

        public SkipTreeView(TreeViewState state) : base(state) {
        }

        public SkipTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader) {
        }

        protected override TreeViewItem BuildRoot() {
            SkipTree<BindingNode> bindingTree = UIForiaTreeDebuggerWindow.s_SelectedApplication.BindingSystem.GetReadTree();
            SkipTree<BindingNode>.TreeNode treeRoot = bindingTree.GetTraversableTree(true);

            BindingNodeTreeItem Recurse(SkipTree<BindingNode>.TreeNode n) {
                BindingNodeTreeItem retn = new BindingNodeTreeItem(n.item);
                for (int i = 0; i < n.children.Length; i++) {
                    retn.AddChild(Recurse(n.children[i]));
                }

                return retn;
            }

            BindingNodeTreeItem root = Recurse(treeRoot);
            SetupDepthsFromParentsAndChildren(root);
            needsReload = false;
            return root;
        }

        public bool RunGUI() {
            OnGUI(GUILayoutUtility.GetRect(0, 10000, 0, 10000));
            return needsReload;
        }

        protected override void SelectionChanged(IList<int> selectedIds) {
            if (selectedIds.Count == 0) {
                onSelectionChanged?.Invoke(null);
                return;
            }

            int id = selectedIds[0];

            var tree = UIForiaTreeDebuggerWindow.s_SelectedApplication.BindingSystem.GetReadTree();
            BindingNode node = tree.GetItem(id);
            onSelectionChanged?.Invoke(node);
        }

    }

    public class UIForiaTreeDebuggerWindow : EditorWindow {

        public const string k_InspectedAppKey = "UIForia.Inspector.ApplicationName";

        public TreeViewState state;
        public SkipTreeView treeView;
        private bool needsReload;
        private string inspectedAppId;
        private bool firstLoad;
        private BindingNode selectedNode;

        private void OnInspectorUpdate() {
            Repaint();
        }

        private static MethodInfo s_GameWindowSizeMethod;

        public static int s_SelectedElementId;
        public static Application s_SelectedApplication;

        public void OnEnable() {
            firstLoad = true;
            state = new TreeViewState();
            autoRepaintOnSceneChange = true;
            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
        }

        private void OnElementCreatedOrDestroyed(UIElement element) {
            needsReload = true;
        }

        public void OnRefresh() {
            s_SelectedElementId = -1;
            ///treeView?.Destroy();

            Application app = Application.Find(inspectedAppId);

            if (app == null) return;

            treeView = new SkipTreeView(state);
            treeView.onSelectionChanged += OnSelectionChanged;
            // treeView.view = app.GetView(0);
        }

        public void OnSelectionChanged(BindingNode node) {
            selectedNode = node;
        }

        private void Update() {
            if (!EditorApplication.isPlaying) {
                return;
            }

            Repaint();
        }

        private void SetApplication(string appId) {
            Application oldApp = Application.Find(inspectedAppId);

            if (oldApp != null) {
                oldApp.onElementCreated -= OnElementCreatedOrDestroyed;
                oldApp.onElementDestroyed -= OnElementCreatedOrDestroyed;
                oldApp.onRefresh -= OnRefresh;
            }

            // treeView?.Destroy();

            inspectedAppId = appId;
            EditorPrefs.SetString(k_InspectedAppKey, appId);

            Application app = Application.Find(appId);

            if (app != null) {
                needsReload = true;

                treeView = new SkipTreeView(state);
                treeView.onSelectionChanged += OnSelectionChanged;
                app.onElementCreated += OnElementCreatedOrDestroyed;
                app.onElementDestroyed += OnElementCreatedOrDestroyed;
                app.onRefresh += OnRefresh;
            }

            s_SelectedApplication = app;
            s_SelectedElementId = -1;
        }

        public void OnGUI() {
            if (!EditorApplication.isPlaying) {
                EditorGUILayout.LabelField("Enter play mode to inspect a UIForia Application");
                return;
            }

            EditorGUILayout.BeginVertical();
            string[] names = new string[Application.Applications.Count + 1];
            names[0] = "None";

            int oldIdx = 0;

            for (int i = 1; i < names.Length; i++) {
                names[i] = Application.Applications[i - 1].id;
                if (names[i] == inspectedAppId) {
                    oldIdx = i;
                }
            }

            int idx = EditorGUILayout.Popup(new GUIContent("Application"), oldIdx, names);
            if (firstLoad || idx != oldIdx) {
                SetApplication(names[idx]);
                if (firstLoad) {
                    s_SelectedApplication = Application.Find(names[idx]);
                    s_SelectedElementId = -1;
                    firstLoad = false;
                }
            }

            if (s_SelectedApplication == null) {
                //    treeView?.Destroy();
                treeView = null;
            }

            if (treeView == null) {
                EditorGUILayout.EndVertical();
                return;
            }

            //treeView.showChildrenAndId = EditorGUILayout.Toggle("Show Meta Data", treeView.showChildrenAndId);

            if (needsReload) {
                needsReload = false;
                treeView.Reload();
                treeView.ExpandAll();
            }

            if (selectedNode != null) {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Selected Binding -> " + selectedNode.element.GetDisplayName() + " id: " + selectedNode.UniqueId);
                EditorGUI.indentLevel++;
                for (int i = 0; i < selectedNode.bindings.Length; i++) {
                    
                    EditorGUILayout.LabelField(selectedNode.bindings[i].GetType().ToString().Substring("UIForia.Bindings.".Length) + " " + selectedNode.bindings[i].bindingId);
                    
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            needsReload = treeView.RunGUI();
            EditorGUILayout.EndVertical();
        }

        private static Vector2 GetMainGameViewSize() {
            if (s_GameWindowSizeMethod == null) {
                Type windowType = Type.GetType("UnityEditor.GameView,UnityEditor");
                s_GameWindowSizeMethod = windowType.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
            }

            return (Vector2) s_GameWindowSizeMethod.Invoke(null, null);
        }

    }

}
