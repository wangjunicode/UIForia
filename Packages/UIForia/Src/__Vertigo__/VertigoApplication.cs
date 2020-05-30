using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Src.Systems;
using UIForia.Elements;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public enum CompilationType {

        Dynamic,
        Precompiled

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RequireDynamicTemplateAttribute : Attribute {

        public readonly Type requiredType;

        public RequireDynamicTemplateAttribute(Type type) {
            this.requiredType = type;
        }

    }

    public class UIWindow {

        public readonly int id;
        protected UIElement rootElement;

        protected UIWindow host;
        protected LightList<UIWindow> subWindows;
        public readonly VertigoApplication application;

        public UIWindow(int id, UIWindow host, VertigoApplication application) {
            this.application = application;
            this.host = host;
            this.id = id;
        }

        public void SetRootElement(UIElement rootElement) {
            this.rootElement = rootElement;
        }

    }

    public class RootWindow : UIWindow {

        public RootWindow(int id, UIWindow host, VertigoApplication application) : base(id, host, application) { }

        public UIElement GetRootElement() {
            return rootElement;
        }

    }

    public sealed class WindowManager {

        public readonly RootWindow rootWindow;

        internal WindowManager(VertigoApplication application) {
            this.rootWindow = new RootWindow(0, null, application);
        }

    }

    public class VertigoApplication : IDisposable {

        public readonly string name;

        internal StyleDatabase styleDatabase;
        internal AttributeSystem attributeSystem;
        internal StringInternSystem internSystem;
        internal AwesomeLayoutSystem layoutSystem;
        internal VertigoRenderSystem renderSystem;
        internal LinqBindingSystem bindingSystem;
        internal InputSystem inputSystem;
        internal WindowManager windowManager;

        internal ElementSystem elementSystem;
        internal TemplateSystem templateSystem;
        internal StyleSystem2 styleSystem;

        public ApplicationType ApplicationType { get; private set; }
        public CompilationType CompilationType { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsRunning { get; private set; }
        
        internal Type entryPoint;

        private static LightList<VertigoApplication> s_RunningApplications = new LightList<VertigoApplication>();

        public VertigoApplication(string name, ApplicationType applicationType, CompilationType compilationType, Type entryPoint) {
            ApplicationType = applicationType;
            CompilationType = compilationType;
            this.name = name;
            this.entryPoint = entryPoint;
            this.elementSystem = new ElementSystem(1024);
            this.templateSystem = new TemplateSystem(this);
            this.windowManager = new WindowManager(this);
            s_RunningApplications.Add(this);
        }

        private bool wasInitialized;
        private bool isValid;

        public void Destroy() {
            s_RunningApplications.Remove(this);
        }

        private void InitializeSystems(in CompileResult compileResult) {
            if (wasInitialized) {
                // some systems are complex with allocators and memory, safer to just dispose and re-create them
                styleDatabase.Dispose();
                styleSystem.Dispose();
                attributeSystem.Dispose();
                internSystem.Dispose();
            }

            styleDatabase = compileResult.styleDatabase;
            internSystem = compileResult.internSystem;
            attributeSystem = new AttributeSystem(internSystem, elementSystem);
            styleSystem = new StyleSystem2(1024, styleDatabase);
            elementSystem.Initialize();
            templateSystem.Initialize(compileResult.templateDataMap, elementSystem, styleSystem, attributeSystem);
            wasInitialized = true;

        }

        // todo -- [RequireDynamicTemplate(typeof(Window<>))]
        public void Initialize(Type currentType) {

            switch (CompilationType) {

                case CompilationType.Dynamic:

                    if (VertigoLoader.Compile(entryPoint, out CompileResult compileResult)) {
                        isValid = true;
                        IsRunning = true;
                        InitializeSystems(compileResult);
                        templateSystem.CreateAppEntryPoint(windowManager.rootWindow, currentType);
                        // how do I handle elements?
                        // windows seem to make sense as root containers
                        // what is a window? stackable, spawnable, despawnable, binding behavior, layout & render parallel 
                    }
                    else {
                        IsRunning = false;
                        isValid = false; // maybe just keep running old version if we had one
                        compileResult.diagnostics.Dump();
                    }

                    break;

                case CompilationType.Precompiled:
                    VertigoLoader.LoadPrecompiled(this, entryPoint); // new Precompiled<T>(); ??? need to get at the template data somehow, ideally w/o reflection
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public void Refresh(Type currentType) {
            if (!IsInitialized) {
                Initialize(currentType);
                return;
            }

            if (currentType != entryPoint) {
                entryPoint = currentType;
                // 
            }

            switch (CompilationType) {

                case CompilationType.Dynamic:
                    VertigoLoader.Compile(entryPoint, out CompileResult result);
                    break;

                case CompilationType.Precompiled:
                    Debug.Log("Cannot refresh a precompiled application.");
                    return;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void RunFrame() {

            if (!IsInitialized) return;

            switch (ApplicationType) {

                case ApplicationType.Game:
                    RunGameFrame();
                    return;

                case ApplicationType.Editor:
                    RunEditorFrame();
                    return;

                case ApplicationType.Test:
                    RunTestFrame();
                    return;

                default:
                    return;
            }

        }

        private void RunTestFrame() {
            throw new NotImplementedException();
        }

        private void RunEditorFrame() {
            throw new NotImplementedException();
        }

        private void RunGameFrame() {
            throw new NotImplementedException();
        }

        public void Dispose() {
            styleDatabase?.Dispose();
            attributeSystem?.Dispose();
            elementSystem?.Dispose();
        }

        public static IReadOnlyList<VertigoApplication> GetActiveApplications() {
            return s_RunningApplications;
        }

    }

}