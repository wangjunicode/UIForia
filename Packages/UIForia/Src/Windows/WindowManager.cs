using System;
using System.Collections.Generic;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Layout;
using UIForia.Systems;

namespace UIForia.Windows {
    public class WindowManager {
        private Dictionary<string, IWindowSpawner> spawners;
        private readonly string defaultWindowId = "Default";

        internal IWindowSpawner defaultSpawner;

        internal List<UIWindow> windows;

        public event Action<UIWindow[]> onWindowsSorted;
        public event Action<UIWindow> onWindowRemoved;
        public event Action<UIWindow> onWindowAdded;
 
        private readonly Application application;
        
        public WindowManager(IWindowSpawner defaultSpawner, Application application) {
            this.application = application;
            this.defaultSpawner = defaultSpawner;
            spawners = new Dictionary<string, IWindowSpawner> {
                {"default", defaultSpawner}
            };
            windows = new List<UIWindow>(16);
        }

        // Register a window spawner by id. It must be unique.
        public IWindowSpawner RegisterWindowSpawner(string id, IWindowSpawner spawner) {
            if (spawners.TryGetValue(id, out IWindowSpawner registeredSpawner)) {
                return registeredSpawner;
            }

            spawners.Add(id, spawner);

            return spawner;
        }

        internal void SpawnDefaultWindow(UIElement rootElement) {
            UIWindow rootWindow = new UIWindow(defaultWindowId, defaultSpawner, application, rootElement, new Size(application.Width, application.Height));
            windows.Add(rootWindow);
            onWindowAdded?.Invoke(rootWindow);
        }

        // Retrieve a window spawner by id.
        public IWindowSpawner GetWindowSpawner(string id) {
            return spawners[id];
        }

        // Show a window of Type TWindowType with the given id and place it in 'host'. Use the default spawner for that type.
        public UIWindow Show<TWindowType>(string windowId = null, UIWindow host = null) where TWindowType : UIWindow {
            return Show<TWindowType>(windowId, defaultSpawner, host);
        }

        // Show a window of Type TWindowType with the given id and given spawner. Also call `setup` action before first Enable()
        public UIWindow Show<TWindowType>(string windowId, IWindowSpawner spawner, UIWindow host = null, Action<TWindowType> setup = null)
            where TWindowType : UIWindow {

            for (int i = 0; i < windows.Count; i++) {
                if (windows[i].windowId == windowId) {
                    UIWindow window = windows[i];
                    window.SetEnabled(true);
                    return window;
                }
            }

            if (application.templateData.TryGetTemplate<TWindowType>(out DynamicTemplate dynamicTemplate)) {
                UIElement element = application.templateData.templates[dynamicTemplate.templateId].Invoke(null, new TemplateScope(application));
                UIWindow window = new UIWindow(windowId, spawner ?? defaultSpawner, application, element, new Size(application.Width, application.Height));

                windows.Add(window);
                onWindowAdded?.Invoke(window);
                return window;
            }

            throw new TemplateNotFoundException($"Unable to find a template for {typeof(TWindowType)}. This is probably because you are trying to load this template dynamically and did include the type in the {nameof(TemplateSettings.dynamicallyCreatedTypes)} list.");
        }

        // Hide a window by reference. Invoke `afterHide` after spawner finishes hiding this window.
        public UIWindow Hide<TWindowType>(TWindowType window, Action<TWindowType> afterHide = null) where TWindowType : UIWindow {

            for (int i = 0; i < windows.Count; i++) {
                if (windows[i].RootElement == window) {
                    window.SetEnabled(false);
                    afterHide?.Invoke(window);
                    return window;
                }
            }

            return null;
        }

        // Hide a window by of TWindowType where id matches. Invoke `afterHide` after spawner finishes hiding this window.
        public UIWindow Hide<TWindowType>(string id, Action<TWindowType> afterHide = null) where TWindowType : UIWindow {
            for (int i = 0; i < windows.Count; i++) {
                if (windows[i].windowId == id) {
                    windows[i].SetEnabled(false);
                    afterHide?.Invoke((TWindowType) windows[i]);
                    return windows[i];
                }
            }
            return null;
        }

        // Toggle a window of TWindowType with id from hidden to shown or vice versa. Invokes `beforeShow` before spawner shows and `afterHide` after it is hidden.
        public TWindowType Toggle<TWindowType>(string id, Action<TWindowType> beforeShow = null, Action<TWindowType> afterHide = null)
            where TWindowType : UIWindow {
            return null;
        }

        // Toggle a window reference from hidden to shown or vice versa. Invokes `beforeShow` before spawner shows and `afterHide` after it is hidden.
        public TWindowType Toggle<TWindowType>(TWindowType window, Action<TWindowType> beforeShow = null, Action<TWindowType> afterHide = null)
            where TWindowType : UIWindow {
            return null;
        }

        // Focuses `window` and invokes `focusReady` after the spawner finishes focusing the window
        public virtual void Focus<TWindowType>(TWindowType window, Action<TWindowType> focusReady = null) where TWindowType : UIWindow {
        }

        // Find the window by id if it exists. Focuses window and invokes `focusReady` after the spawner finishes focusing the window. Returns false if window doesn't exist or rejects focus.
        public virtual bool Focus<TWindowType>(string id, Action<TWindowType> focusReady = null) where TWindowType : UIWindow {
            return false;
        }

        // Blur the currently focused window if there is one. Call `afterBlur` after it's spawner has blurred it
        public virtual bool Blur(Action<UIWindow> afterBlur = null) {
            return false;
        }

        public virtual UIWindow GetDefaultWindow() {
            return GetWindow(defaultWindowId);
        }

        public virtual UIWindow GetWindow(string windowId) {
            for (int i = 0; i < windows.Count; i++) {
                if (windows[i].windowId == windowId) {
                    return windows[i];
                }
            }

            return null;
        }

        // find the first window of TWindowType return return it. Null if not found
        public virtual TWindowType GetWindow<TWindowType>() where TWindowType : UIWindow {
            return null;
        }

        // find the window of type TWindowType with id and return it. Null if not found
        public virtual TWindowType GetWindow<TWindowType>(string id) where TWindowType : UIWindow {
            return null;
        }

        // invokes spawner.Maximize(window) on every active window
        public virtual void MaxmimizeAll(Action<UIWindow> afterMaximize = null) {   
            for (int i = 0; i < windows.Count; i++) {
                windows[i].spawner.Maxmimize(windows[i]);
            }
        }

        // invokes spawner.Minimize(window) on every active window
        public virtual void MinimizeAll(Action<UIWindow> afterMinimize = null) {
            for (int i = 0; i < windows.Count; i++) {
                windows[i].spawner.Minimize(windows[i]);
            }
        }

        // brings the target window all the way forward. No effect on the root
        public virtual bool BringToFront(UIWindow window) {
            window.Depth = 0;
            SortWindows();
            return false;
        }

        // push the target window all the way to the back just before root. No effect on the root
        public virtual bool PushToBack(UIWindow window) {
            return false;
        }

        // pull the target window one step forward
        public virtual bool PullForward(UIWindow window) {
            return false;
        }

        // push the target window one step backwards
        public virtual bool PushBack(UIWindow window) {
            return false;
        }

        // Set the default spawner for all window types
        public void SetDefaultSpawner(IWindowSpawner spawner) {
        }

        // Set the default spawner for all window of TWindowType
        public void SetDefaultSpawnerForType<TWindowType>(IWindowSpawner spawner) {
        }

        public void Destroy() {
            for (int i = 0; i < windows.Count; i++) {
                windows[i].Destroy();
                onWindowRemoved?.Invoke(windows[i]);
            }

            windows.Clear();
        }

        public void Destroy(UIWindow window) {
            window.Destroy();
            windows.Remove(window);
            onWindowRemoved?.Invoke(window);
        }
 
        public void UpdateBindings(LinqBindingSystem linqBindingSystem) {
            for (int i = 0; i < windows.Count; i++) {
                linqBindingSystem.NewUpdateFn(windows[i]);
            }
        }
        
        private void SortWindows() {
            // let's bubble sort the views since only once view is out of place
            for (int i = (windows.Count - 1); i > 0; i--) {
                for (int j = 1; j <= i; j++) {
                    if (windows[j - 1].Depth > windows[j].Depth) {
                        UIWindow tWindow = windows[j - 1];
                        windows[j - 1] = windows[j];
                        windows[j] = tWindow;
                    }
                }
            }

            onWindowsSorted?.Invoke(windows.ToArray());
        }
    }
}