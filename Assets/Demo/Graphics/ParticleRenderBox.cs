using UIForia;
using UIForia.Rendering;
using UnityEngine;

namespace Demo.Graphics {

    [CustomPainter("DemoParticles")]
    public class ParticleRenderBox : RenderBox {

        public struct ParticleData {

            public Mesh mesh;
            public ParticleSystem system;
            public ParticleSystemRenderer renderer;

        }
        
        public override Rect RenderBounds { get; }

        protected GameObject particleRoot;
        protected ParticleData[] particleData;

        public override void OnInitialize() {
            base.OnInitialize();
            particleRoot = Object.Instantiate(Resources.Load<GameObject>("Particles/Halo"));
            particleRoot.name = "ParticleRenderBox";
            ParticleSystem[] systems = particleRoot.GetComponentsInChildren<ParticleSystem>();
            particleData = new ParticleData[systems.Length];
            for (int i = 0; i < systems.Length; i++) {
                particleData[i].renderer = systems[i].GetComponent<ParticleSystemRenderer>();
                particleData[i].renderer.enabled = false;
                particleData[i].mesh = new Mesh();
                particleData[i].mesh.MarkDynamic();
                particleData[i].system = systems[i];
            }
        }

        public override void PaintBackground(RenderContext ctx) {
            // todo -- solve the 'off screen doesn't render problem
            for (int i = 0; i < particleData.Length; i++) {
                ref ParticleData data = ref particleData[i];
                data.system.Simulate(Time.unscaledDeltaTime, false, false, true);
                data.renderer.BakeMesh(data.mesh);
                ctx.DrawMesh(data.mesh, data.renderer.material, element.layoutResult.matrix.ToMatrix4x4());
            }
        }

    }

}