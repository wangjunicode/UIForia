using UnityEngine;

namespace UIForia {

    [CreateAssetMenu(fileName = "UIApplication", menuName = "UIForia/Application", order = 1)]
    public class Compilation : ScriptableObject {

        public string compiledAppPath;

        [HideInInspector] [SerializeField] public string guid;
        [HideInInspector] [SerializeField] public byte[] styleDatabase;
        [HideInInspector] [SerializeField] internal CompilationType compilationType;
        [HideInInspector] [SerializeField] internal ApplicationType applicationType;

    }

}