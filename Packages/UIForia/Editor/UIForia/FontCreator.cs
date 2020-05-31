using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using UIForia.Util;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace UIForia.Editor.FontLoading {

    [Serializable]
    public struct UIForiaFontAssetCreationSettings {

        public string sourceFontFileName;
        public string sourceFontFileGUID;
        public int pointSizeSamplingMode;
        public int pointSize;
        public int padding;
        public int packingMode;
        public int atlasWidth;
        public int atlasHeight;
        public int characterSetSelectionMode;
        public string characterSequence;
        public string referencedFontAssetGUID;
        public string referencedTextAssetGUID;
        public int fontStyle;
        public float fontStyleModifier;
        public int renderMode;
        public bool includeFontFeatures;

    }

    public class FontCreator {

        public class FontCreatorCreatorWindow : EditorWindow {

            [MenuItem("Window/UIForia/Font Asset Creator", false, 2025)]
            public static void ShowFontAtlasCreatorWindow() {
                FontCreatorCreatorWindow window = GetWindow<FontCreatorCreatorWindow>();
                window.titleContent = new GUIContent("Font Asset Creator");
                window.Focus();
            }

            int m_FontAssetCreationSettingsCurrentIndex = 0;

            const float k_TwoColumnControlsWidth = 335f;

            Stopwatch m_StopWatch;

            string[] m_FontSizingOptions = {"Auto Sizing", "Custom Size"};
            int m_PointSizeSamplingMode;
            string[] m_FontResolutionLabels = {"16", "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192"};
            int[] m_FontAtlasResolutions = {16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192};
            string[] m_FontCharacterSets = {"ASCII", "Extended ASCII", "ASCII Lowercase", "ASCII Uppercase", "Numbers + Symbols", "Custom Range", "Unicode Range (Hex)", "Custom Characters", "Characters from File"};

            enum FontPackingModes {

                Fast = 0,
                Optimum = 4

            };

            FontPackingModes m_PackingMode = FontPackingModes.Fast;

            int m_CharacterSetSelectionMode;

            string m_CharacterSequence = "";
            string m_OutputFeedback = "";
            string m_WarningMessage;
            const string k_OutputNameLabel = "Font: ";
            const string k_OutputSizeLabel = "Pt. Size: ";
            const string k_OutputCountLabel = "Characters packed: ";
            int m_CharacterCount;
            Vector2 m_ScrollPosition;
            Vector2 m_OutputScrollPosition;

            bool m_IsRepaintNeeded;

            float m_RenderingProgress;
            bool m_IsRenderingDone;
            bool m_IsProcessing;
            bool m_IsGenerationDisabled;
            bool m_IsGenerationCancelled;

            Object m_SourceFontFile;
            FontAsset m_SelectedFontAsset;
            FontAsset m_LegacyFontAsset;
            FontAsset m_ReferencedFontAsset;

            TextAsset m_CharacterList;
            int m_PointSize;

            int m_Padding = 5;
            FontLoader.FaceStyles m_FontStyle = FontLoader.FaceStyles.Normal;
            float m_FontStyleValue = 2;
            FontLoader.RenderModes m_RenderMode = FontLoader.RenderModes.DistanceField16;
            int m_AtlasWidth = 512;
            int m_AtlasHeight = 512;

            FontLoader.FT_FaceInfo m_FontFaceInfo;
            FontLoader.FT_GlyphInfo[] m_FontGlyphInfo;
            byte[] m_TextureBuffer;
            Texture2D m_FontAtlas;
            Texture2D m_SavedFontAtlas;

            bool m_IncludeKerningPairs;
            int[] m_KerningSet;

            bool m_Locked;
            bool m_IsFontAtlasInvalid;

            public GUIStyle label;
            public GUIStyle textAreaBoxWindow;

            /// <summary>
            /// Function which returns a string containing a sequence of Unicode (Hex) character ranges.
            /// </summary>
            /// <param name="characterSet"></param>
            /// <returns></returns>
            public static string GetUnicodeCharacterSequence(int[] characterSet) {
                string characterSequence = string.Empty;
                int count = characterSet.Length;
                int first = characterSet[0];
                int last = first;

                for (int i = 1; i < count; i++) {
                    if (characterSet[i - 1] + 1 == characterSet[i]) {
                        last = characterSet[i];
                    }
                    else {
                        if (first == last)
                            characterSequence += first.ToString("X2") + ",";
                        else
                            characterSequence += first.ToString("X2") + "-" + last.ToString("X2") + ",";

                        first = last = characterSet[i];
                    }

                }

                // handle the final group
                if (first == last)
                    characterSequence += first.ToString("X2");
                else
                    characterSequence += first.ToString("X2") + "-" + last.ToString("X2");

                return characterSequence;
            }

            public void OnEnable() {
                minSize = new Vector2(315, minSize.y);
                m_StopWatch = new Stopwatch();
            }

            public void OnDisable() {

                // Cancel font asset generation just in case one is in progress.
                FontLoader.TMPro_FontPlugin.SendCancellationRequest(FontLoader.CancellationRequestType.WindowClosed);

                // Destroy Engine only if it has been initialized already
                FontLoader.TMPro_FontPlugin.Destroy_FontEngine();

                if (m_FontAtlas != null && EditorUtility.IsPersistent(m_FontAtlas) == false) {
                    DestroyImmediate(m_FontAtlas);
                }

                Resources.UnloadUnusedAssets();
            }

            public void OnGUI() {
                label = label ?? new GUIStyle(EditorStyles.label) {richText = true, wordWrap = true, stretchWidth = true};

                GUILayout.BeginHorizontal();
                DrawControls();
                if (position.width > position.height && position.width > k_TwoColumnControlsWidth) {
                    DrawPreview();
                }

                GUILayout.EndHorizontal();
            }

            public void Update() {
                if (m_IsRepaintNeeded) {
                    m_IsRepaintNeeded = false;
                    Repaint();
                }

                // Update Progress bar is we are Rendering a Font.
                if (m_IsProcessing) {
                    m_RenderingProgress = FontLoader.TMPro_FontPlugin.Check_RenderProgress();

                    m_IsRepaintNeeded = true;
                }

                // Update Feedback Window & Create Font Texture once Rendering is done.
                if (m_IsRenderingDone) {
                    // Stop StopWatch
                    m_StopWatch.Stop();
                    Debug.Log("Font Atlas generation completed in: " + m_StopWatch.Elapsed.TotalMilliseconds.ToString("0.000 ms."));
                    m_StopWatch.Reset();

                    m_IsProcessing = false;
                    m_IsRenderingDone = false;

                    if (m_IsGenerationCancelled == false) {
                        UpdateRenderFeedbackWindow();
                        CreateFontTexture();
                    }

                    Repaint();
                }
            }

            /// <summary>
            /// Method which returns the character corresponding to a decimal value.
            /// </summary>
            /// <param name="sequence"></param>
            /// <returns></returns>
            static int[] ParseNumberSequence(string sequence) {
                List<int> unicodeList = new List<int>();
                string[] sequences = sequence.Split(',');

                foreach (string seq in sequences) {
                    string[] s1 = seq.Split('-');

                    if (s1.Length == 1)
                        try {
                            unicodeList.Add(int.Parse(s1[0]));
                        }
                        catch {
                            Debug.Log("No characters selected or invalid format.");
                        }
                    else {
                        for (int j = int.Parse(s1[0]); j < int.Parse(s1[1]) + 1; j++) {
                            unicodeList.Add(j);
                        }
                    }
                }

                return unicodeList.ToArray();
            }

            /// <summary>
            /// Method which returns the character (decimal value) from a hex sequence.
            /// </summary>
            /// <param name="sequence"></param>
            /// <returns></returns>
            static int[] ParseHexNumberSequence(string sequence) {
                List<int> unicodeList = new List<int>();
                string[] sequences = sequence.Split(',');

                foreach (string seq in sequences) {
                    string[] s1 = seq.Split('-');

                    if (s1.Length == 1)
                        try {
                            unicodeList.Add(int.Parse(s1[0], NumberStyles.AllowHexSpecifier));
                        }
                        catch {
                            Debug.Log("No characters selected or invalid format.");
                        }
                    else {
                        for (int j = int.Parse(s1[0], NumberStyles.AllowHexSpecifier); j < int.Parse(s1[1], NumberStyles.AllowHexSpecifier) + 1; j++) {
                            unicodeList.Add(j);
                        }
                    }
                }

                return unicodeList.ToArray();
            }

            /// <summary>
            /// Function which returns a string containing a sequence of Decimal character ranges.
            /// </summary>
            /// <param name="characterSet"></param>
            /// <returns></returns>
            public static string GetDecimalCharacterSequence(int[] characterSet) {
                string characterSequence = string.Empty;
                int count = characterSet.Length;
                int first = characterSet[0];
                int last = first;

                for (int i = 1; i < count; i++) {
                    if (characterSet[i - 1] + 1 == characterSet[i]) {
                        last = characterSet[i];
                    }
                    else {
                        if (first == last)
                            characterSequence += first + ",";
                        else
                            characterSequence += first + "-" + last + ",";

                        first = last = characterSet[i];
                    }

                }

                // handle the final group
                if (first == last)
                    characterSequence += first;
                else
                    characterSequence += first + "-" + last;

                return characterSequence;
            }

            void DrawControls() {
                GUILayout.Space(5f);

                if (position.width > position.height && position.width > k_TwoColumnControlsWidth) {
                    m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, GUILayout.Width(315));
                }
                else {
                    m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
                }

                GUILayout.Space(5f);

                GUILayout.Label(m_SelectedFontAsset != null ? string.Format("Creation Settings ({0})", m_SelectedFontAsset.name) : "Font Settings", EditorStyles.boldLabel);

                EditorGUIUtility.labelWidth = 125f;
                EditorGUIUtility.fieldWidth = 5f;

                EditorGUI.BeginDisabledGroup(m_IsProcessing);
                {
                    // FONT TTF SELECTION
                    EditorGUI.BeginChangeCheck();
                    m_SourceFontFile = EditorGUILayout.ObjectField("Source Font File", m_SourceFontFile, typeof(Font), false) as Font;
                    if (EditorGUI.EndChangeCheck()) {
                        m_SelectedFontAsset = null;
                        m_IsFontAtlasInvalid = true;
                    }

                    // FONT SIZING
                    EditorGUI.BeginChangeCheck();
                    if (m_PointSizeSamplingMode == 0) {
                        m_PointSizeSamplingMode = EditorGUILayout.Popup("Sampling Point Size", m_PointSizeSamplingMode, m_FontSizingOptions);
                    }
                    else {
                        GUILayout.BeginHorizontal();
                        m_PointSizeSamplingMode = EditorGUILayout.Popup("Sampling Point Size", m_PointSizeSamplingMode, m_FontSizingOptions, GUILayout.Width(225));
                        m_PointSize = EditorGUILayout.IntField(m_PointSize);
                        GUILayout.EndHorizontal();
                    }

                    if (EditorGUI.EndChangeCheck()) {
                        m_IsFontAtlasInvalid = true;
                    }

                    // FONT PADDING
                    EditorGUI.BeginChangeCheck();
                    m_Padding = EditorGUILayout.IntField("Padding", m_Padding);
                    m_Padding = (int) Mathf.Clamp(m_Padding, 0f, 64f);
                    if (EditorGUI.EndChangeCheck()) {
                        m_IsFontAtlasInvalid = true;
                    }

                    // FONT PACKING METHOD SELECTION
                    EditorGUI.BeginChangeCheck();
                    m_PackingMode = (FontPackingModes) EditorGUILayout.EnumPopup("Packing Method", m_PackingMode);
                    if (EditorGUI.EndChangeCheck()) {
                        m_IsFontAtlasInvalid = true;
                    }

                    // FONT ATLAS RESOLUTION SELECTION
                    GUILayout.BeginHorizontal();
                    GUI.changed = false;

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PrefixLabel("Atlas Resolution");
                    m_AtlasWidth = EditorGUILayout.IntPopup(m_AtlasWidth, m_FontResolutionLabels, m_FontAtlasResolutions); //, GUILayout.Width(80));
                    m_AtlasHeight = EditorGUILayout.IntPopup(m_AtlasHeight, m_FontResolutionLabels, m_FontAtlasResolutions); //, GUILayout.Width(80));
                    if (EditorGUI.EndChangeCheck()) {
                        m_IsFontAtlasInvalid = true;
                    }

                    GUILayout.EndHorizontal();

                    // FONT CHARACTER SET SELECTION
                    EditorGUI.BeginChangeCheck();
                    bool hasSelectionChanged = false;
                    m_CharacterSetSelectionMode = EditorGUILayout.Popup("Character Set", m_CharacterSetSelectionMode, m_FontCharacterSets);
                    if (EditorGUI.EndChangeCheck()) {
                        m_CharacterSequence = "";
                        hasSelectionChanged = true;
                        m_IsFontAtlasInvalid = true;

                        //Debug.Log("Resetting Sequence!");
                    }

                    switch (m_CharacterSetSelectionMode) {
                        case 0: // ASCII
                            //characterSequence = "32 - 126, 130, 132 - 135, 139, 145 - 151, 153, 155, 161, 166 - 167, 169 - 174, 176, 181 - 183, 186 - 187, 191, 8210 - 8226, 8230, 8240, 8242 - 8244, 8249 - 8250, 8252 - 8254, 8260, 8286";
                            m_CharacterSequence = "32 - 126, 160, 8203, 8230, 9633";
                            break;

                        case 1: // EXTENDED ASCII
                            m_CharacterSequence = "32 - 126, 160 - 255, 8192 - 8303, 8364, 8482, 9633";
                            // Could add 9632 for missing glyph
                            break;

                        case 2: // Lowercase
                            m_CharacterSequence = "32 - 64, 91 - 126, 160";
                            break;

                        case 3: // Uppercase
                            m_CharacterSequence = "32 - 96, 123 - 126, 160";
                            break;

                        case 4: // Numbers & Symbols
                            m_CharacterSequence = "32 - 64, 91 - 96, 123 - 126, 160";
                            break;

                        case 5: // Custom Range
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            GUILayout.Label("Enter a sequence of decimal values to define the characters to be included in the font asset or retrieve one from another font asset.", label);
                            GUILayout.Space(10f);

                            EditorGUI.BeginChangeCheck();
                            m_ReferencedFontAsset = EditorGUILayout.ObjectField("Select Font Asset", m_ReferencedFontAsset, typeof(FontAsset), false) as FontAsset;
                            if (EditorGUI.EndChangeCheck() || hasSelectionChanged) {
                                if (m_ReferencedFontAsset != null) {
                                    m_CharacterSequence = GetDecimalCharacterSequence(FontAsset.TMPConversionUtil.GetCharactersArray(m_ReferencedFontAsset));
                                }

                                m_IsFontAtlasInvalid = true;
                            }

                            // Filter out unwanted characters.
                            char chr = Event.current.character;
                            if ((chr < '0' || chr > '9') && (chr < ',' || chr > '-')) {
                                Event.current.character = '\0';
                            }

                            GUILayout.Label("Character Sequence (Decimal)", EditorStyles.boldLabel);
                            EditorGUI.BeginChangeCheck();
                            m_CharacterSequence = EditorGUILayout.TextArea(m_CharacterSequence, textAreaBoxWindow, GUILayout.Height(120), GUILayout.ExpandWidth(true));
                            if (EditorGUI.EndChangeCheck()) {
                                m_IsFontAtlasInvalid = true;
                            }

                            EditorGUILayout.EndVertical();
                            break;

                        case 6: // Unicode HEX Range
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            GUILayout.Label("Enter a sequence of Unicode (hex) values to define the characters to be included in the font asset or retrieve one from another font asset.", label);
                            GUILayout.Space(10f);

                            EditorGUI.BeginChangeCheck();
                            m_ReferencedFontAsset = EditorGUILayout.ObjectField("Select Font Asset", m_ReferencedFontAsset, typeof(FontAsset), false) as FontAsset;
                            if (EditorGUI.EndChangeCheck() || hasSelectionChanged) {
                                if (m_ReferencedFontAsset != null)
                                    m_CharacterSequence = GetUnicodeCharacterSequence(FontAsset.TMPConversionUtil.GetCharactersArray(m_ReferencedFontAsset));
                                m_IsFontAtlasInvalid = true;
                            }

                            // Filter out unwanted characters.
                            chr = Event.current.character;
                            if ((chr < '0' || chr > '9') && (chr < 'a' || chr > 'f') && (chr < 'A' || chr > 'F') && (chr < ',' || chr > '-')) {
                                Event.current.character = '\0';
                            }

                            GUILayout.Label("Character Sequence (Hex)", EditorStyles.boldLabel);
                            EditorGUI.BeginChangeCheck();
                            m_CharacterSequence = EditorGUILayout.TextArea(m_CharacterSequence, textAreaBoxWindow, GUILayout.Height(120), GUILayout.ExpandWidth(true));
                            if (EditorGUI.EndChangeCheck()) {
                                m_IsFontAtlasInvalid = true;
                            }

                            EditorGUILayout.EndVertical();
                            break;

                        case 7: // Characters from Font Asset
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            GUILayout.Label("Type the characters to be included in the font asset or retrieve them from another font asset.", label);
                            GUILayout.Space(10f);

                            EditorGUI.BeginChangeCheck();
                            m_ReferencedFontAsset = EditorGUILayout.ObjectField("Select Font Asset", m_ReferencedFontAsset, typeof(FontAsset), false) as FontAsset;
                            if (EditorGUI.EndChangeCheck() || hasSelectionChanged) {
                                if (m_ReferencedFontAsset != null)
                                    m_CharacterSequence = FontAsset.TMPConversionUtil.GetCharacters(m_ReferencedFontAsset);
                                m_IsFontAtlasInvalid = true;
                            }

                            EditorGUI.indentLevel = 0;

                            GUILayout.Label("Custom Character List", EditorStyles.boldLabel);
                            EditorGUI.BeginChangeCheck();
                            m_CharacterSequence = EditorGUILayout.TextArea(m_CharacterSequence, textAreaBoxWindow, GUILayout.Height(120), GUILayout.ExpandWidth(true));
                            if (EditorGUI.EndChangeCheck()) {
                                m_IsFontAtlasInvalid = true;
                            }

                            EditorGUILayout.EndVertical();
                            break;

                        case 8: // Character List from File
                            EditorGUI.BeginChangeCheck();
                            m_CharacterList = EditorGUILayout.ObjectField("Character File", m_CharacterList, typeof(TextAsset), false) as TextAsset;
                            if (EditorGUI.EndChangeCheck()) {
                                m_IsFontAtlasInvalid = true;
                            }

                            if (m_CharacterList != null) {
                                m_CharacterSequence = m_CharacterList.text;
                            }

                            break;
                    }

                    // FONT STYLE SELECTION
                    GUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    m_FontStyle = (FontLoader.FaceStyles) EditorGUILayout.EnumPopup("Font Style", m_FontStyle, GUILayout.Width(225));
                    m_FontStyleValue = EditorGUILayout.IntField((int) m_FontStyleValue);
                    if (EditorGUI.EndChangeCheck()) {
                        m_IsFontAtlasInvalid = true;
                    }

                    GUILayout.EndHorizontal();

                    // Render Mode Selection
                    EditorGUI.BeginChangeCheck();
                    m_RenderMode = (FontLoader.RenderModes) EditorGUILayout.EnumPopup("Render Mode", m_RenderMode);
                    if (EditorGUI.EndChangeCheck()) {
                        //m_availableShaderNames = UpdateShaderList(font_renderMode, out m_availableShaders);
                        m_IsFontAtlasInvalid = true;
                    }

                    m_IncludeKerningPairs = EditorGUILayout.Toggle("Get Kerning Pairs", m_IncludeKerningPairs);

                    EditorGUILayout.Space();
                }

                EditorGUI.EndDisabledGroup();

                if (!string.IsNullOrEmpty(m_WarningMessage)) {
                    EditorGUILayout.HelpBox(m_WarningMessage, MessageType.Warning);
                }

                GUI.enabled = m_SourceFontFile != null && !m_IsProcessing && !m_IsGenerationDisabled; // Enable Preview if we are not already rendering a font.
                if (GUILayout.Button("Generate Font Atlas") && m_CharacterSequence.Length != 0 && GUI.enabled) {
                    if (!m_IsProcessing && m_SourceFontFile != null) {
                        DestroyImmediate(m_FontAtlas);
                        m_FontAtlas = null;
                        m_OutputFeedback = string.Empty;
                        m_SavedFontAtlas = null;
                        int errorCode;

                        errorCode = FontLoader.TMPro_FontPlugin.Initialize_FontEngine(); // Initialize Font Engine
                        if (errorCode != 0) {
                            if (errorCode == 0xF0) {
                                //Debug.Log("Font Library was already initialized!");
                                errorCode = 0;
                            }
                            else
                                Debug.Log("Error Code: " + errorCode + "  occurred while Initializing the FreeType Library.");
                        }

                        string fontPath = AssetDatabase.GetAssetPath(m_SourceFontFile); // Get file path of TTF Font.

                        if (errorCode == 0) {
                            errorCode = FontLoader.TMPro_FontPlugin.Load_TrueType_Font(fontPath); // Load the selected font.

                            if (errorCode != 0) {
                                if (errorCode == 0xF1) {
                                    //Debug.Log("Font was already loaded!");
                                    errorCode = 0;
                                }
                                else
                                    Debug.Log("Error Code: " + errorCode + "  occurred while Loading the [" + m_SourceFontFile.name + "] font file. This typically results from the use of an incompatible or corrupted font file.");
                            }
                        }

                        if (errorCode == 0) {
                            if (m_PointSizeSamplingMode == 0) m_PointSize = 72; // If Auto set size to 72 pts.

                            errorCode = FontLoader.TMPro_FontPlugin.FT_Size_Font(m_PointSize); // Load the selected font and size it accordingly.
                            if (errorCode != 0)
                                Debug.Log("Error Code: " + errorCode + "  occurred while Sizing the font.");
                        }

                        // Define an array containing the characters we will render.
                        if (errorCode == 0) {
                            int[] characterSet;
                            if (m_CharacterSetSelectionMode == 7 || m_CharacterSetSelectionMode == 8) {
                                List<int> charList = new List<int>();

                                for (int i = 0; i < m_CharacterSequence.Length; i++) {
                                    // Check to make sure we don't include duplicates
                                    if (charList.FindIndex(item => item == m_CharacterSequence[i]) == -1)
                                        charList.Add(m_CharacterSequence[i]);
                                    else {
                                        //Debug.Log("Character [" + characterSequence[i] + "] is a duplicate.");
                                    }
                                }

                                characterSet = charList.ToArray();
                            }
                            else if (m_CharacterSetSelectionMode == 6) {
                                characterSet = ParseHexNumberSequence(m_CharacterSequence);
                            }
                            else {
                                characterSet = ParseNumberSequence(m_CharacterSequence);
                            }

                            m_CharacterCount = characterSet.Length;

                            m_TextureBuffer = new byte[m_AtlasWidth * m_AtlasHeight];

                            m_FontFaceInfo = new FontLoader.FT_FaceInfo();

                            m_FontGlyphInfo = new FontLoader.FT_GlyphInfo[m_CharacterCount];

                            int padding = m_Padding;

                            bool autoSizing = m_PointSizeSamplingMode == 0;

                            float strokeSize = m_FontStyleValue;
                            if (m_RenderMode == FontLoader.RenderModes.DistanceField16) strokeSize = m_FontStyleValue * 16;
                            if (m_RenderMode == FontLoader.RenderModes.DistanceField32) strokeSize = m_FontStyleValue * 32;

                            m_IsProcessing = true;
                            m_IsGenerationCancelled = false;

                            // Start Stop Watch
                            m_StopWatch = Stopwatch.StartNew();

                            ThreadPool.QueueUserWorkItem(someTask => {
                                m_IsRenderingDone = false;

                                errorCode = FontLoader.TMPro_FontPlugin.Render_Characters(m_TextureBuffer, m_AtlasWidth, m_AtlasHeight, padding, characterSet, m_CharacterCount, m_FontStyle, strokeSize, autoSizing, m_RenderMode, (int) m_PackingMode, ref m_FontFaceInfo, m_FontGlyphInfo);
                                m_IsRenderingDone = true;
                            });

                        }

                    }
                }

                // FONT RENDERING PROGRESS BAR
                GUILayout.Space(1);

                Rect progressRect = EditorGUILayout.GetControlRect(false, 20);

                bool isEnabled = GUI.enabled;
                GUI.enabled = true;
                EditorGUI.ProgressBar(progressRect, m_IsProcessing ? m_RenderingProgress : 0, "Generation Progress");
                progressRect.x = progressRect.x + progressRect.width - 20;
                progressRect.y += 1;
                progressRect.width = 20;
                progressRect.height = 16;

                GUI.enabled = m_IsProcessing;
                if (GUI.Button(progressRect, "X")) {
                    FontLoader.TMPro_FontPlugin.SendCancellationRequest(FontLoader.CancellationRequestType.CancelInProgress);
                    m_RenderingProgress = 0;
                    m_IsProcessing = false;
                    m_IsGenerationCancelled = true;
                }

                GUI.enabled = isEnabled;

                // FONT STATUS & INFORMATION
                GUISkin skin = GUI.skin;

                //GUI.skin = TMP_UIStyleManager.TMP_GUISkin;
                GUI.enabled = true;

                GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(145));
                m_OutputScrollPosition = EditorGUILayout.BeginScrollView(m_OutputScrollPosition);
                EditorGUILayout.LabelField(m_OutputFeedback, label);
                EditorGUILayout.EndScrollView();
                GUILayout.EndVertical();

                GUI.skin = skin;

                // SAVE TEXTURE & CREATE and SAVE FONT XML FILE
                GUI.enabled = m_FontAtlas != null && !m_IsProcessing; // Enable Save Button if font_Atlas is not Null.

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Save") && GUI.enabled) {
                    if (m_SelectedFontAsset == null) {
                        if (m_LegacyFontAsset != null)
                            SaveNewFontAssetWithSameName(m_LegacyFontAsset);
                        else
                            SaveNewFontAsset(m_SourceFontFile);
                    }
                    else {
                        // Save over exiting Font Asset
                        string filePath = Path.GetFullPath(AssetDatabase.GetAssetPath(m_SelectedFontAsset)).Replace('\\', '/');

                        Save_SDF_FontAsset(filePath);
                    }
                }

                if (GUILayout.Button("Save as...") && GUI.enabled) {
                    if (m_SelectedFontAsset == null) {
                        SaveNewFontAsset(m_SourceFontFile);
                    }
                    else {
                        SaveNewFontAssetWithSameName(m_SelectedFontAsset);
                    }
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                GUI.enabled = true; // Re-enable GUI

                GUILayout.Space(5);

                if (position.height > position.width || position.width < k_TwoColumnControlsWidth) {
                    DrawPreview();
                    GUILayout.Space(5);
                }

                EditorGUILayout.EndScrollView();

                if (m_IsFontAtlasInvalid)
                    ClearGeneratedData();
            }

            /// <summary>
            /// Clear the previously generated data.
            /// </summary>
            void ClearGeneratedData() {
                m_IsFontAtlasInvalid = false;

                if (m_FontAtlas != null) {
                    DestroyImmediate(m_FontAtlas);
                    m_FontAtlas = null;
                }

                m_SavedFontAtlas = null;

                m_OutputFeedback = string.Empty;
                m_WarningMessage = string.Empty;
            }

            /// <summary>
            /// Function to update the feedback window showing the results of the latest generation.
            /// </summary>
            void UpdateRenderFeedbackWindow() {
                m_PointSize = m_FontFaceInfo.pointSize;

                string colorTag = m_FontFaceInfo.characterCount == m_CharacterCount ? "<color=#C0ffff>" : "<color=#ffff00>";
                string colorTag2 = "<color=#C0ffff>";

                var missingGlyphReport = k_OutputNameLabel + "<b>" + colorTag2 + m_FontFaceInfo.name + "</color></b>";

                if (missingGlyphReport.Length > 60)
                    missingGlyphReport += "\n" + k_OutputSizeLabel + "<b>" + colorTag2 + m_FontFaceInfo.pointSize + "</color></b>";
                else
                    missingGlyphReport += "  " + k_OutputSizeLabel + "<b>" + colorTag2 + m_FontFaceInfo.pointSize + "</color></b>";

                missingGlyphReport += "\n" + k_OutputCountLabel + "<b>" + colorTag + m_FontFaceInfo.characterCount + "/" + m_CharacterCount + "</color></b>";

                // Report missing requested glyph
                missingGlyphReport += "\n\n<color=#ffff00><b>Missing Characters</b></color>";
                missingGlyphReport += "\n----------------------------------------";

                m_OutputFeedback = missingGlyphReport;

                for (int i = 0; i < m_CharacterCount; i++) {
                    if (m_FontGlyphInfo[i].x == -1) {
                        missingGlyphReport += "\nID: <color=#C0ffff>" + m_FontGlyphInfo[i].id + "\t</color>Hex: <color=#C0ffff>" + m_FontGlyphInfo[i].id.ToString("X") + "\t</color>Char [<color=#C0ffff>" + (char) m_FontGlyphInfo[i].id + "</color>]";

                        if (missingGlyphReport.Length < 16300)
                            m_OutputFeedback = missingGlyphReport;
                    }
                }

                if (missingGlyphReport.Length > 16300)
                    m_OutputFeedback += "\n\n<color=#ffff00>Report truncated.</color>\n<color=#c0ffff>See</color> \"TextMesh Pro\\Glyph Report.txt\"";

                // Save Missing Glyph Report file
                if (Directory.Exists("Assets/TextMesh Pro")) {
                    missingGlyphReport = Regex.Replace(missingGlyphReport, @"<[^>]*>", string.Empty);
                    File.WriteAllText("Assets/TextMesh Pro/Glyph Report.txt", missingGlyphReport);
                    AssetDatabase.Refresh();
                }
            }

            void CreateFontTexture() {
                m_FontAtlas = new Texture2D(m_AtlasWidth, m_AtlasHeight, TextureFormat.Alpha8, false, true);

                Color32[] colors = new Color32[m_AtlasWidth * m_AtlasHeight];

                for (int i = 0; i < (m_AtlasWidth * m_AtlasHeight); i++) {
                    byte c = m_TextureBuffer[i];
                    colors[i] = new Color32(c, c, c, c);
                }

                // Clear allocation of 
                m_TextureBuffer = null;

                if (m_RenderMode == FontLoader.RenderModes.Raster || m_RenderMode == FontLoader.RenderModes.RasterHinted)
                    m_FontAtlas.filterMode = FilterMode.Point;

                m_FontAtlas.SetPixels32(colors, 0);
                m_FontAtlas.Apply(false, true);
            }

            /// <summary>
            /// Open Save Dialog to provide the option save the font asset using the name of the source font file. This also appends SDF to the name if using any of the SDF Font Asset creation modes.
            /// </summary>
            /// <param name="sourceObject"></param>
            void SaveNewFontAsset(Object sourceObject) {
                string filePath;

                // Save new Font Asset and open save file requester at Source Font File location.
                string saveDirectory = new FileInfo(AssetDatabase.GetAssetPath(sourceObject)).DirectoryName;

                filePath = EditorUtility.SaveFilePanel("Save TextMesh Pro! Font Asset File", saveDirectory, sourceObject.name + " SDF", "asset");

                if (filePath.Length == 0) {
                    return;
                }

                Save_SDF_FontAsset(filePath);

            }

            /// <summary>
            /// Open Save Dialog to provide the option to save the font asset under the same name.
            /// </summary>
            /// <param name="sourceObject"></param>
            void SaveNewFontAssetWithSameName(Object sourceObject) {
                string filePath;

                // Save new Font Asset and open save file requester at Source Font File location.
                string saveDirectory = new FileInfo(AssetDatabase.GetAssetPath(sourceObject)).DirectoryName;

                filePath = EditorUtility.SaveFilePanel("Save TextMesh Pro! Font Asset File", saveDirectory, sourceObject.name, "asset");

                if (filePath.Length == 0) {
                    return;
                }

                Save_SDF_FontAsset(filePath);

            }

            private void Save_SDF_FontAsset(string filePath) {
                filePath = filePath.Substring(0, filePath.Length - 6); // Trim file extension from filePath.

                string dataPath = UnityEngine.Application.dataPath;

                if (filePath.IndexOf(dataPath, StringComparison.InvariantCultureIgnoreCase) == -1) {
                    Debug.LogError("You're saving the font asset in a directory outside of this project folder. This is not supported. Please select a directory under \"" + dataPath + "\"");
                    return;
                }

                string relativeAssetPath = filePath.Substring(dataPath.Length - 6);
                string tex_DirName = Path.GetDirectoryName(relativeAssetPath);
                string tex_FileName = Path.GetFileNameWithoutExtension(relativeAssetPath);
                string tex_Path_NoExt = tex_DirName + "/" + tex_FileName;

                FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<FontAsset>(tex_Path_NoExt + ".asset");

                if (fontAsset == null) {
                    fontAsset = CreateInstance<FontAsset>();
                    AssetDatabase.CreateAsset(fontAsset, tex_Path_NoExt + ".asset");
                }

                fontAsset.faceInfo = GetFaceInfo(m_FontFaceInfo);
                fontAsset.textGlyphList = GetGlyphInfo(m_FontGlyphInfo);
                fontAsset.kerningPairs = GetKerningTable(AssetDatabase.GetAssetPath(m_SourceFontFile), (int) fontAsset.faceInfo.PointSize);

                fontAsset.atlas = m_FontAtlas;
                m_FontAtlas.name = tex_FileName + " Atlas";

                AssetDatabase.AddObjectToAsset(m_FontAtlas, fontAsset);

                fontAsset.gradientScale = m_Padding + 1;

                // Save Font Asset creation settings
                m_SelectedFontAsset = fontAsset;
                m_LegacyFontAsset = null;
                // fontAsset.creationSettings = SaveFontCreationSettings();

                AssetDatabase.SaveAssets();

                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(fontAsset)); // Re-import font asset to get the new updated version.

                AssetDatabase.Refresh();

                m_FontAtlas = null;

            }

            void DrawPreview() {
                Rect pixelRect;
                if (position.width > position.height && position.width > k_TwoColumnControlsWidth) {
                    float minSide = Mathf.Min(position.height - 15f, position.width - k_TwoColumnControlsWidth);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(minSide));

                    pixelRect = GUILayoutUtility.GetRect(minSide, minSide, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
                }
                else {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    pixelRect = GUILayoutUtility.GetAspectRect(1f);
                }

                if (m_FontAtlas != null) {
                    EditorGUI.DrawTextureAlpha(pixelRect, m_FontAtlas, ScaleMode.StretchToFill);
                }
                else if (m_SavedFontAtlas != null) {
                    EditorGUI.DrawTextureAlpha(pixelRect, m_SavedFontAtlas, ScaleMode.StretchToFill);
                }

                EditorGUILayout.EndVertical();
            }

            // Convert from FT_FaceInfo to FaceInfo
            private static FaceInfo GetFaceInfo(FontLoader.FT_FaceInfo ftFace) {
                FaceInfo face = new FaceInfo();

                //face.Name = ftFace.name;
                face.PointSize = ftFace.pointSize;
                face.Padding = ftFace.padding;
                face.LineHeight = ftFace.lineHeight;
                face.CapHeight = 0;
                face.Baseline = 0;
                face.Ascender = ftFace.ascender;
                face.Descender = ftFace.descender;
                face.CenterLine = ftFace.centerLine;
                face.Underline = ftFace.underline;
                face.UnderlineThickness = ftFace.underlineThickness == 0 ? 5 : ftFace.underlineThickness; // Set Thickness to 5 if TTF value is Zero.
                face.strikethrough = (face.Ascender + face.Descender) / 2.75f;
                face.strikethroughThickness = face.UnderlineThickness;
                face.SuperscriptOffset = face.Ascender;
                face.SubscriptOffset = face.Underline;
                face.SubSize = 0.5f;
                //face.CharacterCount = ft_face.characterCount;
                face.AtlasWidth = ftFace.atlasWidth;
                face.AtlasHeight = ftFace.atlasHeight;

                return face;
            }

            // Convert from FT_GlyphInfo[] to GlyphInfo[]
            private DEPRECATE_TMP_Glyph[] GetGlyphInfo(FontLoader.FT_GlyphInfo[] ftGlyphs) {
                List<DEPRECATE_TMP_Glyph> glyphs = new List<DEPRECATE_TMP_Glyph>(ftGlyphs.Length);

                for (int i = 0; i < ftGlyphs.Length; i++) {

                    // Filter out characters with missing glyphs.
                    if (ftGlyphs[i].x == -1) {
                        continue;
                    }

                    DEPRECATE_TMP_Glyph g = new DEPRECATE_TMP_Glyph {
                        id = ftGlyphs[i].id,
                        x = ftGlyphs[i].x,
                        y = ftGlyphs[i].y,
                        width = ftGlyphs[i].width,
                        height = ftGlyphs[i].height,
                        xOffset = ftGlyphs[i].xOffset,
                        yOffset = ftGlyphs[i].yOffset,
                        xAdvance = ftGlyphs[i].xAdvance,
                        scale = 1
                    };

                    glyphs.Add(g);
                }

                return glyphs.ToArray();
            }

            public TextKerningPair[] GetKerningTable(string fontFilePath, int pointSize) {

                FontLoader.FT_KerningPair[] kerningPairs = new FontLoader.FT_KerningPair[7500];

                int kpCount = FontLoader.TMPro_FontPlugin.FT_GetKerningPairs(fontFilePath, m_KerningSet, m_KerningSet.Length, kerningPairs);

                TextKerningPair[] retn = new TextKerningPair[kpCount];
                for (int i = 0; i < kpCount; i++) {
                    retn[i] = new TextKerningPair() {
                        firstGlyph = (uint) kerningPairs[i].ascII_Left,
                        secondGlyph = (uint) kerningPairs[i].ascII_Right,
                        advance = kerningPairs[i].xAdvanceOffset * pointSize
                    };
                }

                return retn;
            }

        }

    }

}