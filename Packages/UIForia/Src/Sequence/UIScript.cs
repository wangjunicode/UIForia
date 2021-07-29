using System;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public struct UIScript {

        public readonly string scriptName;
        private Func<UIElement, IScriptNode> src;

        public UIScript(Func<UIElement, IScriptNode> src) {
            this.scriptName = default;
            this.src = src;
        }

        public UIScript(string name, Func<UIElement, IScriptNode> src) {
            this.scriptName = name;
            this.src = src;
        }

        public UIScriptInstance Run(UIElement element) {
            UIScriptInstance instance = Instantiate(element);
            instance?.Run();
            return instance;
        }

        public UIScriptInstance Instantiate(UIElement element) {

            if (src == null) {
                return null;
            }

            try {

                UIScriptInstance instance = new UIScriptInstance {
                    root = element,
                    scriptName = scriptName,
                    rootScript = src.Invoke(element),

                    context = new SequenceContext() {
                        rootElementId = element,
                    }

                };

                return instance;
            }
            catch (Exception e) {
                Debug.LogException(e);
                return null;
            }
        }

    }

}