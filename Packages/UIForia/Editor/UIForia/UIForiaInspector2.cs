using System;
using System.Collections.Generic;
using UIForia;
using UIForia.Src;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

public class UIForiaDebugUI : EditorWindow {

    [MenuItem("Window/UIForia/Inspector 2")]
    public static void ShowExample() {
        UIForiaDebugUI wnd = GetWindow<UIForiaDebugUI>();
        wnd.titleContent = new GUIContent("UIForia Inspector 2");
    }

    private VisualElement container;
    private GUIContent guidContent = new GUIContent();

    // [DidReloadScripts]
    // private static void OnScriptsReloaded() {
    //     Debug.Log("Reloaded");
    // }
    private Texture2D errorIcon;
    private Texture2D warningIcon;
    private Texture2D infoIcon;
    private Rect lowerPanel;
    private Rect resizer;
    private Rect menuBar;
    private Rect statsRect;

    private Vector2 moduleLogScroll;
    private bool showDetails;
    private Rect upperPanel;
    private float menuBarHeight = 20f;
    private float sizeRatio = 0.5f;
    private Vector2 upperPanelScroll;
    private Vector2 lowerPanelScroll;
    private int selectedLogIndex;
    private GUIStyle textAreaStyle;
    private float resizerHeight = 5f;
    private bool isResizing;
    private GUIStyle resizerStyle;
    private GUIStyle boxStyle;
    private Texture2D boxBgSelected;
    private Texture2D boxBgOdd;
    private Texture2D boxBgEven;

    public void OnEnable() {

        VisualElement root = rootVisualElement;

        // root.RegisterCallback<MouseDownEvent>(evt => Debug.Log(evt.mousePosition));
        // root.RegisterCallback<MouseMoveEvent>(evt => Debug.Log(evt.mousePosition));
        // root.RegisterCallback<MouseUpEvent>(evt => Debug.Log(evt.mousePosition));

        container = new VisualElement() {
            name = "Container"
        };
        selectedLogIndex = -1;
        root.Add(container);

        boxStyle = new GUIStyle();
        boxStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        resizerStyle = new GUIStyle();
        resizerStyle.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;
        boxBgOdd = EditorGUIUtility.Load("builtin skins/darkskin/images/cn entrybackodd.png") as Texture2D;
        boxBgEven = EditorGUIUtility.Load("builtin skins/darkskin/images/cnentrybackeven.png") as Texture2D;
        boxBgSelected = EditorGUIUtility.Load("builtin skins/darkskin/images/menuitemhover.png") as Texture2D;
        errorIcon = EditorGUIUtility.Load("icons/console.erroricon.png") as Texture2D;
        warningIcon = EditorGUIUtility.Load("icons/console.warnicon.png") as Texture2D;
        infoIcon = EditorGUIUtility.Load("icons/console.infoicon.png") as Texture2D;
        resizerHeight = 5f;
        textAreaStyle = new GUIStyle();
        textAreaStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
        textAreaStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/projectbrowsericonareabg.png") as Texture2D;

    }

    private void OnGUI() {

        if (ModuleSystem.IsLoading) {
            return;
        }

        if (ModuleSystem.FailedToLoad) {
            GUILayout.Label("Module System failed to load");
            DrawModuleStats(position);
            DrawUpperLogPanel();
        }
        else {
            DrawFilterBar();
            DrawModuleStats(position);
            DrawUpperLogPanel();
            DrawLowerLogPanel();
            DrawResizer();
            ProcessEvents(Event.current);
            Repaint();

            // container.style.width = new StyleLength(position.width);
            // container.style.height = new StyleLength(position.height);
            // container.style.backgroundColor = new StyleColor(new Color32(242, 246, 250, 255));
        }
    }

    private void DrawFilterBar() { }

    private void DrawUpperLogPanel() {
        upperPanel = new Rect(0, statsRect.height, position.width, (position.height * sizeRatio) - statsRect.height);

        GUILayout.BeginArea(upperPanel);
        upperPanelScroll = GUILayout.BeginScrollView(upperPanelScroll);

        List<Diagnostic> logs = ModuleSystem.GetDiagnosticLogs();

        for (int i = 0; i < logs.Count; i++) {
            if (DrawBoxLog(logs[i], i)) {
                selectedLogIndex = i;
                GUI.changed = true;
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawLowerLogPanel() {
        lowerPanel = new Rect(0, (position.height * sizeRatio) + resizerHeight, position.width, (position.height * (1 - sizeRatio)) - resizerHeight);

        GUILayout.BeginArea(lowerPanel);
        lowerPanelScroll = GUILayout.BeginScrollView(lowerPanelScroll);

        List<Diagnostic> logs = ModuleSystem.GetDiagnosticLogs();
        if (selectedLogIndex != -1 && selectedLogIndex < logs.Count) {
            GUILayout.TextArea(ModuleSystem.GetDiagnosticLogs()[selectedLogIndex].message, textAreaStyle);
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawResizer() {
        resizer = new Rect(0, (position.height * sizeRatio) - resizerHeight, position.width, resizerHeight * 2);

        GUILayout.BeginArea(new Rect(resizer.position + (Vector2.up * resizerHeight), new Vector2(position.width, 2)), resizerStyle);
        GUILayout.EndArea();

        EditorGUIUtility.AddCursorRect(resizer, MouseCursor.ResizeVertical);
    }

    private void ProcessEvents(Event e) {
        switch (e.type) {
            case EventType.MouseDown:
                if (e.button == 0 && resizer.Contains(e.mousePosition)) {
                    isResizing = true;
                }

                break;

            case EventType.MouseUp:
                isResizing = false;
                break;
        }

        Resize(e);
    }

    private void Resize(Event e) {
        if (isResizing) {
            sizeRatio = e.mousePosition.y / position.height;
            Repaint();
        }
    }

    private bool DrawBoxLog(Diagnostic log, int index) {
        bool isSelected = selectedLogIndex == index;
        bool isOdd = index % 2 != 0;

        if (isSelected) {
            boxStyle.normal.background = boxBgSelected;
        }
        else {
            boxStyle.normal.background = isOdd ? boxBgOdd : boxBgEven;
        }

        switch (log.diagnosticType) {

            case DiagnosticType.ModuleLog:
                guidContent.image = infoIcon;
                break;

            case DiagnosticType.ModuleError:
                guidContent.image = errorIcon;
                break;

            case DiagnosticType.ModuleException:
                guidContent.image = errorIcon;
                break;

            case DiagnosticType.ParseError:
                guidContent.image = errorIcon;
                break;

            case DiagnosticType.ParseWarning:
                guidContent.image = warningIcon;
                break;

        }

        guidContent.text = log.message;
        return GUILayout.Button(guidContent, boxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(30));

    }

    private void DrawModuleStats(Rect logLocation) {

        ModuleSystem.Stats stats = ModuleSystem.GetStats();
        GUILayout.BeginVertical();

        DrawTiming("Total Module Load Time", stats.totalModuleLoadTime);
        DrawTiming("Namespace Scan Time", stats.typeResolverStats.namespaceScanTime);
        guidContent.text = "Show Detailed Timing";

        showDetails = GUILayout.Toggle(showDetails, guidContent);
        if (showDetails) {
            DrawCount("Module Count", stats.moduleCount);
            DrawCount("Type Count", stats.elementCount);
            DrawTiming("Get Module Types Time", stats.getModuleTypesTime);
            DrawTiming("Create Modules Time", stats.createModulesTime);
            DrawTiming("Validate Dependencies Time", stats.validateModuleDependenciesTime);
            DrawTiming("Validate Paths Time", stats.validateModulePathsTime);
            DrawTiming("Assign Types to Modules Time", stats.assignElementsToModulesTime);
            GUILayout.Space(12);

            DrawCount("Total Assembly Count", stats.typeResolverStats.assemblyCount);
            DrawCount("Total Type Count", stats.typeResolverStats.typeCount);
            DrawCount("Total Namespace Count", stats.typeResolverStats.namespaceCount);
        }

        GUILayout.EndVertical();
        statsRect = GUILayoutUtility.GetLastRect();
    }

    private void DrawCount(string label, int count) {
        GUILayout.BeginHorizontal();
        guidContent.text = label;
        guidContent.image = null;
        GUILayout.Label(guidContent);
        GUILayout.FlexibleSpace();
        guidContent.text = count.ToString();
        GUILayout.Label(guidContent);
        GUILayout.EndHorizontal();
    }

    private void DrawTiming(string label, double time) {
        GUILayout.BeginHorizontal();
        guidContent.image = null;
        guidContent.text = label;
        GUILayout.Label(guidContent);
        GUILayout.FlexibleSpace();
        guidContent.text = time.ToString("F3");
        GUILayout.Label(guidContent);
        GUILayout.EndHorizontal();
    }

}