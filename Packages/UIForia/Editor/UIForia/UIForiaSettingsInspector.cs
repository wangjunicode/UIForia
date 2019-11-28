
using UnityEngine;
#if UNITY_EDITOR
using UIForia;
using UnityEditor;

namespace UIForia.Editor {

    [CustomEditor(typeof(UIForiaSettings))]
    public class LevelScriptEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
        }

    }

}
#endif