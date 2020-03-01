using System;
using System.ComponentModel;
using System.IO;
using UIForia.Elements;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace UIForia {

    public class UIViewBehavior : MonoBehaviour {

        public Type type;
        public string typeName;
        public new Camera camera;
        public Application application;
        public bool usePreCompiledTemplates;
        
        [HideInInspector] public string applicationName = "Game App 2";

        [BurstCompile]
        public struct DummyJob : IJob {

            [Unity.Collections.ReadOnly] public NativeArray<float> values;

            [Unity.Collections.WriteOnly] public NativeArray<float> output;
            
            public void Execute() {
                float sum = 0;
                for (int i = 0; i < values.Length; i++) {
                    sum += 555;
                }

                output[0] = sum;
            }

        }

        public TemplateSettings GetTemplateSettings(Type type) {
            TemplateSettings settings = new TemplateSettings();
            settings.rootType = type;
            settings.applicationName = applicationName;
            settings.assemblyName = "Assembly-CSharp";
            settings.outputPath = Path.Combine(UnityEngine.Application.dataPath, "UIForiaGenerated2");
            settings.codeFileExtension = "generated.cs";
            settings.preCompiledTemplatePath = "Assets/UIForia_Generated2/" + applicationName;
            settings.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath);
            return settings;
        }

        public void Start() {
            type = Type.GetType(typeName);
            if (type == null) return;
            
            NativeArray<float> array = new NativeArray<float>(32, Allocator.TempJob);
            NativeArray<float> output = new NativeArray<float>(1, Allocator.TempJob);

            for (int i = 0; i < array.Length; i++) {
                array[i] = i;
            }
            
            DummyJob job = new DummyJob() {
                values = array,
                output = output
            };
                job.Run();
                Debug.Log(output[0]);
                output.Dispose();
            
 
            
            array.Dispose();
            
            // TemplateSettings settings = GetTemplateSettings(type);
            //
            // application = usePreCompiledTemplates
            //     ? GameApplication.CreateFromPrecompiledTemplates(settings, camera, DoDependencyInjection)
            //     : GameApplication.CreateFromRuntimeTemplates(settings, camera, DoDependencyInjection);

        }

        // optional!
        private void DoDependencyInjection(UIElement element) {
            // DiContainer.Inject(element);
        }

        private void Update() {
            // if (type == null) return;
            // application?.Update();
            // application?.GetView(0).SetSize((int)application.Width, (int)application.Height);
        }

    }

}