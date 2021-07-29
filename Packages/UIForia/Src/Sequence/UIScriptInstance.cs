using UIForia.Util;

namespace UIForia {

    public class UIScriptInstance {

        internal UIElement root;
        internal SequenceContext context;
        internal IScriptNode rootScript;
        internal bool isPlaying;
        internal bool needsReset;
        internal string scriptName;

        public UIElement element => root;
        
        public bool IsComplete { get; internal set; }
        
        public bool IsRunning => isPlaying;
        
        public void EnsureRunning() {
            if (!isPlaying) {
                Run();
            }
        }

        public void Run() {
            if (isPlaying) {
                context.properties.size = 0;
                rootScript.Reset();
                needsReset = false;
                return;
            }

            isPlaying = true;
            if (needsReset) {
                context.properties.size = 0;
                rootScript.Reset();
            }
            needsReset = true;
            root.application.RunScript(this);
        }

        public void Reset() {
            if (isPlaying || needsReset) {
                rootScript.Reset();
                needsReset = false;
            }
        }

        internal void Update(int deltaTimeMS, StructList<ElementId> dummyInstanceList) {
            dummyInstanceList.size = 1;
            dummyInstanceList.array[0] = root.elementId;
            
            if (context.elapsedTime == 0) {
                context.deltaTime = 0;
                rootScript.Update(context, dummyInstanceList);
                context.elapsedTime += deltaTimeMS;
            }
            else {
                context.deltaTime = deltaTimeMS;
                context.elapsedTime += deltaTimeMS;
                rootScript.Update(context, dummyInstanceList);
            }
        }

        public void SetBoolProperty(string name, bool value) {
            context.SetBoolProperty(name, value);
        }

        public bool GetBoolProperty(string name) {
            return context.GetBoolProperty(name);
        }

        public float GetFloatProperty(string name) {
            return context.GetFloatProperty(name);
        }

        public int GetIntProperty(string name) {
            return context.GetIntProperty(name);
        }

        public string GetStringProperty(string name) {
            return context.GetStringProperty(name);
        }

        public void OnComplete() {
            isPlaying = false;
            IsComplete = true;
        }

    }

}