using System;
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

    public class VertigoApplication {

        internal ElementSystem elementSystem;

        public ApplicationType ApplicationType { get; private set; }
        public CompilationType CompilationType { get; private set; }

        public readonly string name;
        public readonly Type entryPoint;
        
        public VertigoApplication(string name, ApplicationType applicationType, CompilationType compilationType, Type entryPoint) {
            this.name = name;
            ApplicationType = applicationType;
            CompilationType = compilationType;
            this.entryPoint = entryPoint;
        }

        public bool IsCompiling { get; private set; }
        public bool HasCompilationErrors { get; private set; }
        public bool IsInitialized { get; private set; }

        private CompilationId compilationId;
        private Diagnostics diagnostics = new Diagnostics();
        
        public void Initialize() {
            if (IsInitialized) return;

            compilationId = VertigoLoader.Compile(entryPoint, diagnostics);

          //  if (compilationId.id == -1) {
                for (int i = 0; i < diagnostics.diagnosticList.Count; i++) {
                    Debug.Log(diagnostics.diagnosticList[i].message);
                }
             //   Debug.Log("Failed");
           // }
            // applicationLoader.LoadEntryPoint(ApplicationType, systems, entryType);
            // 
            // ideally we can run some / all of the init steps in parallel

            // int compileId = applicationLoader.LoadEntryPoint(entry, systems);

            // how do I handle other entry points?
            // other window requirements etc?

            // AppDescription {
            //    entrypoint
            //    also compile all other entry points in all referenced modules by default? 
            //    cant, still need to figure out generics
            //    entry, dynamics[] -> window types, any other generic element type
            //    thats not part of a module though, or is it? 
            //    kind of makes sense, entry point doesnt / shouldnt know all dynamic usages like windows
            //    can load dynamic / generics from depdendency modules
            //    ie IncludeType(typeof(KlangWindow<Chat>));
            //    part of entry point? part of element definition?
            //    
            //    [RequireDynamicTemplate(typeof(Window<>))]
            //    

        }
        
        public void RunFrame() {
            
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

    }

}